using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicaStudio.Core.Interfaces.Explorer;

namespace MicaStudio.Selectors
{
	/*
	 * Select the Item Templates to show nodes in the Explorer TreeView
	 * IExplorerParentNode can have children so it should have a template with:
	 * - HasUnrealizedChildren = True, in order to show the expand/collapse arrow glyph
	 * - ItemsSource = set to the child nodes of the IExplorerParentNode
	 */
	public partial class ExplorerItemSelector : DataTemplateSelector
	{
		public DataTemplate? ExplorerNodeTemplate { get; set; }
		public DataTemplate? ExplorerParentNodeTemplate { get; set; }

		// Important to use SelectTemplateCore(object item) and NOT SelectTemplateCore(object item, DependencyObject Container)
		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item is IExplorerParentNode)
			{
				return ExplorerParentNodeTemplate ?? base.SelectTemplateCore(item);
			}
			else if (item is IExplorerNode)
			{
				return ExplorerNodeTemplate ?? base.SelectTemplateCore(item);
			}
			return base.SelectTemplateCore(item);
		}
	}
}
