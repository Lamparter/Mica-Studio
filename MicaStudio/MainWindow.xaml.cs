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
using CommunityToolkit.Mvvm.Messaging;
using Windows.System;
using MicaStudio.Core.Messages.Commands.Files;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            this.InitializeComponent(); 
            this.ExtendsContentIntoTitleBar = true;
			this.SetTitleBar(AppTitleBar);
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
				WeakReferenceMessenger.Default.Send(new FolderOpenedMessage((folder.Name, folder.Path)));
			}
		}
	}
}
