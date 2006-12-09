using System;
using Gtk;
using CsBoard.Plugin;

namespace CsBoard
{
	namespace Viewer
	{
		public class PluginManagerDialog:Dialog
		{
			PluginManager manager;
			TreeView tree;
			ListStore store;

			public PluginManagerDialog (Window parent,
						    PluginManager
						    manager):base ("Plugins",
								   parent,
								   DialogFlags.
								   Modal,
								   "Close",
								   ResponseType.
								   None)
			{
				this.manager = manager;
				SetupTree ();
				foreach (PluginInfo info in manager.Plugins)
				{
					if (info.Plugin == null)
					  {
						  continue;
					  }
					store.AppendValues (info);
				}

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Automatic;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Child = tree;
				  VBox.PackStart (win, true, true, 4);
				  SetSizeRequest (400, 400);
				  VBox.ShowAll ();
			}

			private void SetupTree ()
			{
				tree = new TreeView ();
				this.store = new ListStore (typeof (object));
				tree.Model = store;

				TreeViewColumn col = new TreeViewColumn ();
				col.Sizing = TreeViewColumnSizing.Fixed;
				col.Spacing = 4;

				CellRendererText text_renderer =
					new CellRendererText ();
				text_renderer.WrapMode = Pango.WrapMode.Word;
				col.PackStart (text_renderer, true);
				col.SetCellDataFunc (text_renderer,
						     new
						     TreeCellDataFunc
						     (PluginInfoCellDataFunc));

				CellRendererToggle loaded_renderer =
					new CellRendererToggle ();
				loaded_renderer.Activatable = false;
				col.PackStart (loaded_renderer, false);
				col.SetCellDataFunc (loaded_renderer,
						     new
						     TreeCellDataFunc
						     (LoadedStatusCellDataFunc));

				col.Title = "Plugins Details";
				tree.AppendColumn (col);
			}

			private void PluginInfoCellDataFunc (TreeViewColumn
							     col,
							     CellRenderer
							     cell,
							     TreeModel model,
							     TreeIter iter)
			{
				CellRendererText r = (CellRendererText) cell;
				PluginInfo info =
					(PluginInfo) model.GetValue (iter, 0);
				r.Markup =
					String.Format ("<b>{0}</b>\n{1}",
						       info.Plugin.Name,
						       info.Plugin.
						       Description);
			}


			private void LoadedStatusCellDataFunc (TreeViewColumn
							       col,
							       CellRenderer
							       cell,
							       TreeModel
							       model,
							       TreeIter iter)
			{
				CellRendererToggle r =
					(CellRendererToggle) cell;
				PluginInfo info =
					(PluginInfo) model.GetValue (iter, 0);
				r.Active = info.Loaded;
			}
		}
	}
}
