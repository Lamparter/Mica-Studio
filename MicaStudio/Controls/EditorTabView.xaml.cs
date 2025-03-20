using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MicaStudio.Core.Classes.Explorer;
using MicaStudio.Core.Interfaces.Tabs;
using MicaStudio.Core.Messages.Explorer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MicaStudio.Controls
{
	public record Tab(string Name, ITabContent Content);
	[INotifyPropertyChanged]
	public sealed partial class EditorTabView : UserControl
	{
		private ObservableCollection<Tab> Tabs = new();
		[ObservableProperty]
		private Tab? selectedTab;
		public EditorTabView()
		{
			this.InitializeComponent();
			WeakReferenceMessenger.Default.Register<ExplorerSelectionChangedMessage>(this, async (r, m) =>
			{
				if(m.Value is FileNode)
				{
					var file = m.Value as FileNode;
					var editor = new CodeEditor();
					var tab = new Tab(file.DisplayName, editor);
					Tabs.Add(tab);
					SelectedTab = tab;
					editor.OpenFile(await StorageFile.GetFileFromPathAsync(file.FilePath));
				}
			});
		}

		private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Debug.WriteLine("tabs");
		}

		private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
		{
			Tab tab = (Tab)args.Item;
			if (tab == SelectedTab)
				SelectedTab = Tabs.Count > 1 ? Tabs[Tabs.Count - 2] : null;

			Tabs.Remove(tab);
			tab.Content.Dispose();
			tab = null;
		}
	}
}
