using CommunityToolkit.Mvvm.Messaging.Messages;
using MicaStudio.Core.Interfaces.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicaStudio.Core.Messages.Explorer
{
	/*
	 * This message notifies when a new node has been selected in the Explorer
	 * Listeners can use this for tasks
	 * An example is the tabview might listen to change the selected tab or add a new tab
	 * The message passes the IExplorerNode that was clicked
	 * v0
	 */
	public class ExplorerSelectionChangedMessage : ValueChangedMessage<IExplorerNode>
	{
		public ExplorerSelectionChangedMessage(IExplorerNode node) : base(node)
		{
		}
	}
}
