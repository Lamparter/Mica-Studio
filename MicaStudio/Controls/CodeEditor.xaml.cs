using CommunityToolkit.Mvvm.Messaging;
using MicaStudio.Core.Interfaces.Tabs;
using MicaStudio.Core.Messages.Commands.Files;
using MicaStudio.Core.Messages.Explorer;
using MicaStudio.Panels;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using MicaStudio.Utilities;
using WinUIEditor;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio.Controls
{
	public sealed partial class CodeEditor : UserControl, ITabContent
	{
		public CodeEditor()
		{
			this.InitializeComponent();
		}

		private void addLine(int line, string text)
		{
			ScintillaEditor.Editor.GotoLine(line);
			ScintillaEditor.Editor.AddText(text.Length, text);
			ScintillaEditor.Editor.NewLine();
		}


		public async void OpenFile(StorageFile file)
		{
			if (file is null) return;
			Stopwatch stopwatch = Stopwatch.StartNew();

			/*ScintillaEditor.Editor.Modified -= Editor_Modified;
			ScintillaEditor.Editor.ClearAll();
			ScintillaEditor.Editor.AnnotationClearAll();
						//TEMPORARY::
			keyCount = 190;
			colorToScintillaStyle.Clear();*/
			using (IRandomAccessStream stream = await file.OpenReadAsync())
			{
				using (DataReader reader = new DataReader(stream))
				{
					await reader.LoadAsync((uint)stream.Size);
					// Read raw bytes from file into a byte array
					byte[] fileBytes = new byte[reader.UnconsumedBufferLength];
					reader.ReadBytes(fileBytes);

					// Try to convert bytes to a displayable string
					string fileContent = System.Text.Encoding.Default.GetString(fileBytes);

					// Fallback: If Encoding.Default fails to render properly, display hex values
					if (fileContent.Contains("\0")) // Detect if there's binary data
					{
						fileContent = BitConverter.ToString(fileBytes).Replace("-", " ");
					}

					ScintillaEditor.Editor.SetText(fileContent);
				}
			}
			ScintillaEditor.ResetLexer();
			ScintillaEditor.ApplyDefaultsToDocument();
			//ScintillaEditor.Editor.SetFoldFlags(FoldFlag.LineAfterExpanded); // ENABLE FOLDING



			stopwatch.Stop();
			Debug.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

			await Task.Run(async() => await SyntaxHighlight(file.FileType));
		}

		private void Editor_Modified(Editor sender, ModifiedEventArgs args)
		{
			string line = ScintillaEditor.Editor.GetLine(args.Position);
			ITokenizeLineResult result = grammar.TokenizeLine(line);
			parseTokens(result, line, args.Position, registry);
			/*int startLine = (int)ScintillaEditor.Editor.LineFromPosition(args.Position); // gets position of first line modified
			int endLine = (int)ScintillaEditor.Editor.LineFromPosition(args.Position + args.Length); // gets position of last line modified
			// gets all lines between range start and end including end line
			int[] linePositionsModified = Enumerable.Range(startLine, endLine - startLine + 1).ToArray();

			// loop through all modified lines and re syntax highlight them
			foreach (int linePosition in linePositionsModified)
			{
				string line = ScintillaEditor.Editor.GetLine(linePosition);
				ITokenizeLineResult result = grammar.TokenizeLine(line);
				parseTokens(result, line, linePosition, registry);
			}*/
		}

		// Maps a textmate colour to a scintilla style key
		private Dictionary<int, int> colorToScintillaStyle = new();
		private IGrammar grammar;
		private Registry registry;

		public async Task SyntaxHighlight(string extension)
		{
			try
			{
				Stopwatch stopwatch = Stopwatch.StartNew();

				RegistryOptions options = new RegistryOptions(ThemeName.DarkPlus);
				registry = new Registry(options);
				grammar = registry.LoadGrammar(options.GetScopeByExtension(extension)); // parameter is initial scope name

				if (grammar is null) return; // no syntax highlighter available

				IStateStack? ruleStack = null; // important for state of token, without it multi line comments wont work

				for (int i = 0; i < ScintillaEditor.Editor.LineCount; i++)
				{
					string line = ScintillaEditor.Editor.GetLine(i);
					ITokenizeLineResult result = grammar.TokenizeLine(line, ruleStack, TimeSpan.MaxValue);
					ruleStack = result.RuleStack;

					if (result.Tokens.Count() == 0) break; // return if no tokens
					parseTokens(result, line, i, registry);
				}

				stopwatch.Stop();
				Debug.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

				ScintillaEditor.Editor.Modified += Editor_Modified; // start dynamic highlighting
			}
			catch
			{
				ScintillaEditor.Editor.Modified -= Editor_Modified;
			}
		}

		private int keyCount = 190;
		private void parseTokens(ITokenizeLineResult result, string line, long linePosition, Registry registry)
		{
			Theme theme = registry.GetTheme();

			foreach (IToken token in result.Tokens)
			{
				int startIndex = (token.StartIndex > line.Length) ? line.Length : token.StartIndex;
				int endIndex = (token.EndIndex > line.Length) ? line.Length : token.EndIndex;
				foreach(var scope in token.Scopes)
				{
					List<ThemeTrieElementRule> themeRules = theme.Match(new string[] { scope });

					foreach (ThemeTrieElementRule themeRule in themeRules)
					{
						DispatcherQueue.TryEnqueue(() =>
						{
							// get position of current line i
							long linePos = ScintillaEditor.Editor.PositionFromLine(linePosition);

							// Register foreground colour to a scintilla style if it does not exist
							if (!colorToScintillaStyle.ContainsKey(themeRule.foreground))
							{
								var color = ColourUtilities.HexToByte(theme.GetColor(themeRule.foreground));
								keyCount++;
								// IMPORTANT: Define a style with a unique KEY mapped to a colour, we will use it to highlight tokens
								ScintillaEditor.Editor.StyleSetFore(keyCount, color);

								// add it to hashmap so we can retrieve it
								colorToScintillaStyle[themeRule.foreground] = keyCount;
							}

							// start styling from token position by using line position and index of token
							ScintillaEditor.Editor.StartStyling(linePos + startIndex, 0);
							// USE the style which we defined earlier for foreground on the token
							// the scintilla style KEYS are in the hashmap
							ScintillaEditor.Editor.SetStyling(endIndex - startIndex, colorToScintillaStyle[themeRule.foreground]);
							/*Debug.WriteLine(
								"      - Matched theme rule: " +
								"[bg: {0}, fg:{1}, fontStyle: {2}]",
								theme.GetColor(themeRule.background),
								theme.GetColor(themeRule.foreground),
								themeRule.fontStyle);*/
						});
	
					}
				}
			}
		}
		int HexToColor(string hexString)
		{
			// Remove # if it exists
			if (hexString.IndexOf('#') != -1)
				hexString = hexString.Replace("#", "");

			byte r = byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
			byte g = byte.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
			byte b = byte.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

			// Combine into an integer in RGB format
			int color = r | (g << 8) | (b << 16);

			return color;
		}

		/*
		 * Find { instances, use a stack to track these
		 * If } is encountered then pop from stack and create a folding range from the line of { to the line for }
		 */
		private Stack<long> foldStarts = new(); // keeps track of line positions where { was encountered
		private int level = 3;
		private void foldLine(string line, long linePosition)
		{
			if(line.Contains('{'))
			{
				foldStarts.Push(linePosition);
				level++;
			}
			else if(line.Contains('}'))
			{
				long startPosition = foldStarts.Pop();
				ScintillaEditor.Editor.SetMarginWidthN(2, 16);
				ScintillaEditor.Editor.SetFoldLevel(startPosition, FoldLevel.HeaderFlag);
				for (long i = startPosition + 1; i <= linePosition; i++)
				{
					ScintillaEditor.Editor.SetFoldLevel(i, (FoldLevel)level);
				}

				level--;
			}
		}


		public void clear() => ScintillaEditor.Editor.ClearAll();

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ScintillaEditor.Editor.AnnotationSetText(0, "test");
			ScintillaEditor.Editor.AnnotationVisible = AnnotationVisible.Indented;

			ScintillaEditor.Editor.IndicSetStyle(8, IndicatorStyle.FullBox);
			ScintillaEditor.Editor.IndicSetFore(8, 0x0000ff);
			ScintillaEditor.Editor.IndicatorCurrent = 8;
			ScintillaEditor.Editor.IndicatorFillRange(0, 7);
		}

		public void Dispose()
		{
			ScintillaEditor.Editor.Modified -= Editor_Modified;
			colorToScintillaStyle.Clear();
			ScintillaEditor.Editor.ClearAll();
			ScintillaEditor.Editor.ClearDocumentStyle();
			ScintillaEditor.Editor.EmptyUndoBuffer();
			ScintillaEditor.Editor.AnnotationClearAll();
		}
	}
}
