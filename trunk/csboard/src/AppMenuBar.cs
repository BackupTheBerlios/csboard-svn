using Gtk;
using System;
using Mono.Unix;

namespace CsBoard
{
	public class AppMenuBar:MenuBar
	{
		protected MenuItem fileMenuItem, helpMenuItem;
		public AppMenuBar ():base ()
		{

			fileMenuItem =
				new MenuItem (Catalog.GetString ("_File"));
			Append (fileMenuItem);

			Menu menu = new Menu ();
			  fileMenuItem.Submenu = menu;
			  menu.Append (new SeparatorMenuItem ());
			ImageMenuItem item =
				new ImageMenuItem (Catalog.
						   GetString ("_Quit"));
			  item.Image = new Image (Stock.Quit, IconSize.Menu);
			  Console.WriteLine (Stock.Quit);
			  item.Activated += OnQuit;
			  item.AddAccelerator ("activate",
					       ChessWindow.Instance.
					       AccelGroup,
					       new AccelKey (Gdk.Key.q,
							     Gdk.ModifierType.
							     ControlMask,
							     AccelFlags.
							     Visible));

			  menu.Append (item);


			  helpMenuItem =
				new MenuItem (Catalog.GetString ("_Help"));
			  Append (helpMenuItem);

			  menu = new Menu ();
			  helpMenuItem.Submenu = menu;

			  item = new ImageMenuItem (Catalog.
						    GetString ("_About"));
			  item.Image = new Image (Stock.About, IconSize.Menu);
			  item.Activated += OnAbout;
			  menu.Append (item);

			  item = new ImageMenuItem (Catalog.
						    GetString ("_Contents"));
			  item.Image = new Image (Stock.Help, IconSize.Menu);
			  menu.Append (item);

			  ShowAll ();
		}

		protected virtual void OnQuit (object o, EventArgs args)
		{
			Application.Quit ();
		}

		protected virtual void OnAbout (object o, EventArgs args)
		{
			ChessWindow.ShowAboutDialog (null);
		}
	}
}
