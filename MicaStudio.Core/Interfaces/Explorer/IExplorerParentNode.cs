using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicaStudio.Core.Interfaces.Explorer
{
	/*
	 * Interface for a TreeView item shown in the Explorer panel represented as a Tree with nodes
	 * This interface can have child nodes of type IExplorerNode and can be expanded/collapsed
	 * 
	 * If the node is expanding then the Expanding() method is called which can be used to load child nodes
	 */
	public interface IExplorerParentNode : IExplorerNode
	{
		public bool IsExpanded { get; set; }
		public ObservableCollection<IExplorerNode> Children { get; }

		void Expanding(); // Called when node is expanded in TreeView
	}
}
