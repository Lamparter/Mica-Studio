using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using WinUIEditor;
using WinUIEx;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using System.Xml.Linq;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio
{
	public record TreeNode(string Name, string Path, bool isFolder, ObservableCollection<TreeNode> Children);
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : WindowEx
    {
		private ObservableCollection<TreeNode> files = new();
        public MainWindow()
        {
            this.InitializeComponent(); 
            this.ExtendsContentIntoTitleBar = true;
			this.SetTitleBar(AppTitleBar);
		}

		private async void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FileOpenPicker();
			var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			InitializeWithWindow.Initialize(picker, hwnd);
			picker.FileTypeFilter.Add("*");
			var file = await picker.PickSingleFileAsync();

			if (file != null)
			{

			}
		}

		private async void OpenFolder_Click(object sender, RoutedEventArgs e)
		{
			var picker = new FolderPicker();
			var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
			InitializeWithWindow.Initialize(picker, hwnd);
			picker.FileTypeFilter.Add("*");
			var folder = await picker.PickSingleFolderAsync();

			if (folder != null)
			{
				Editor.clear();
				files.Clear();
				var rootFolderNode = new TreeNode(folder.Name, folder.Path, true, new ObservableCollection<TreeNode>());
				files.Add(rootFolderNode);
			}
		}

		private async Task LoadFolderContentAsync(TreeNode node)
		{
			string currentFolderPath = node.Path;

			try
			{
				// Get subfolders in the current directory
				var subFolders = Directory.GetDirectories(currentFolderPath);
				foreach (var subFolder in subFolders)
				{
					var subFolderName = Path.GetFileName(subFolder);
					node.Children.Add(new TreeNode(subFolderName, subFolder, true, new ObservableCollection<TreeNode>()));
				}

				// Get files in the current directory
				var files = Directory.GetFiles(currentFolderPath);
				foreach (var file in files)
				{
					var fileName = Path.GetFileName(file);
					node.Children.Add(new TreeNode(fileName, file, false, new ObservableCollection<TreeNode>()));
				}
			}
			catch (Exception ex)
			{
				// Handle exceptions like unauthorized access, path not found, etc.
				// You can log the exception or show a message to the user
				Debug.WriteLine(ex.Message);
			}
		}

		/*private async Task LoadFolderContentAsyncWinRT(TreeNode node)
		{
			if (!node.isFolder) return;
			var currentFolder = node.StorageItem as StorageFolder;
			var subFolders = await currentFolder.GetFoldersAsync();
			var files = await currentFolder.GetFilesAsync();
			foreach (var subFolder in subFolders)
			{
				node.Children.Add(new TreeNode(subFolder.Name, subFolder, true, new ObservableCollection<TreeNode>()));
			}

			// Add files one by one
			foreach (var file in files)
			{
				node.Children.Add(new TreeNode(file.Name, file, false, new ObservableCollection<TreeNode>()));
			}
		}*/

		// Load items when TreeViewNode expanding
		private async void ExplorerTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
		{
			var node = args.Item as TreeNode;
			if (node.isFolder && node.Children.Count == 0)
			{
				await LoadFolderContentAsync(args.Item as TreeNode);
			}
		}

		private async void ExplorerTreeView_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
		{
			if(args.AddedItems.Count > 0)
			{
				var node = args.AddedItems[0] as TreeNode;
				if (!node.isFolder)
				{
					Editor.OpenFile(await StorageFile.GetFileFromPathAsync(node.Path));
				}
			}
		}
	}
}
