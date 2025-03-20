using CommunityToolkit.Mvvm.ComponentModel;
using MicaStudio.Core.Interfaces.Explorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicaStudio.Core.Classes.Explorer
{
	/*
	 * Represents a File on the Explorer Panel TreeView
	 * Contains a path variable for the file path this represents
	 */
	public partial class FileNode : ObservableObject, IExplorerNode
	{
		[ObservableProperty]
		private string displayName = "";

		public string FilePath { get; } = "";

		public FileNode(string filePath)
		{
			FilePath = filePath;
			DisplayName = Path.GetFileName(filePath);
		}
	}
}
