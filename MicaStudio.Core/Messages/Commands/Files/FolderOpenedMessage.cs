using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicaStudio.Core.Messages.Commands.Files
{
	/*
	 * This message notifies when a folder has been opened. 
	 * The folder name and path is sent as a tuple parameter
	 */
	public class FolderOpenedMessage : ValueChangedMessage<(string, string)>
	{
		public FolderOpenedMessage((string, string) file) : base(file)
		{

		}
	}
}
