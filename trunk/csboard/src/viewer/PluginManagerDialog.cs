using System;
using Gtk;
using CsBoard.Plugin;
using Mono.Unix;

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
						    manager):base (Catalog.GetString("Plugins"),
								   parent,
								   DialogFlags.
								   Modal,
								   Catalog.GetString("Close"),
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

				col.Title = Catalog.GetString("Plugins Details");
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

		public class PluginViewerPlugin:CsPlugin
		{
			GameViewer viewer;
			MenuItem toolsItem;

			public PluginViewerPlugin ():base ("plugin-viewer",
							   Catalog.GetString("Plugin Viewer"),
							   Catalog.GetString("A plugin to show the status of other plugins!"))
			{
			}

			public override bool Initialize ()
			{
				if ((viewer = GameViewer.Instance) == null)
					return false;
				Menu menu = new Menu ();
				toolsItem = new MenuItem (Catalog.GetString("Tools"));
				toolsItem.Submenu = menu;
				MenuItem pluginsItem =
					new MenuItem (Catalog.GetString("Plugins"));
				menu.Add (pluginsItem);

				pluginsItem.Activated += on_plugins_activate;

				int idx = viewer.MenuBar.Children.Length - 1;
				viewer.MenuBar.Insert (toolsItem, idx);
				toolsItem.ShowAll ();
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.MenuBar.Remove (toolsItem);
				return true;
			}

			public void on_plugins_activate (System.Object b,
							 EventArgs e)
			{
				Dialog dlg =
					new
					PluginManagerDialog (viewer.Window,
							     CsBoard.Plugin.
							     PluginManager.
							     Instance);
				dlg.Run ();
				dlg.Hide ();
				dlg.Dispose ();
			}
		}
	}
}
