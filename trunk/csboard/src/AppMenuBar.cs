//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Library General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
//
// Copyright (C) 2006 Ravi Kiran UVS

using Gtk;
using System;
using Mono.Unix;

namespace CsBoard
{
	public class AppMenuBar:MenuBar
	{
		public MenuItem fileMenuItem, editMenuItem, viewMenuItem,
			helpMenuItem, quitMenuItem, fullscreenMenuItem,
			unfullscreenMenuItem;
		AccelGroup accel;
		public AccelGroup AccelGroup
		{
			get
			{
				return accel;
			}
		}

		public AppMenuBar ():base ()
		{
			/* File menu */
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
			  item.Activated += OnQuit;
			  quitMenuItem = item;

			  menu.Append (item);

			/* Edit menu */
			  editMenuItem =
				new MenuItem (Catalog.GetString ("_Edit"));
			  editMenuItem.Submenu = new Menu ();
			  Append (editMenuItem);

			/* View menu */
			  viewMenuItem =
				new MenuItem (Catalog.GetString ("_View"));
			  viewMenuItem.Submenu = new Menu ();
			  Append (viewMenuItem);

			  menu = new Menu ();
			  viewMenuItem.Submenu = menu;

			  fullscreenMenuItem = item =
				new ImageMenuItem (Catalog.
						   GetString ("_Fullscreen"));
			  item.Activated += OnFullscreenActivated;
			  item.Image =
				new Image (Stock.Fullscreen, IconSize.Menu);
			  menu.Append (item);

			  unfullscreenMenuItem = item =
				new ImageMenuItem (Catalog.
						   GetString
						   ("_Leave Fullscreen"));
			  item.Activated += OnUnfullscreenActivated;
			  item.Image =
				new Image (Stock.LeaveFullscreen,
					   IconSize.Menu);
			  item.Show ();
			//menu.Append(item);                    

			/* Help menu */
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
			  item.Activated += OnContents;
			  item.Image = new Image (Stock.Help, IconSize.Menu);
			  menu.Append (item);

			  ShowAll ();

			  CsBoardApp.Instance.Window.WindowStateEvent +=
				OnWindowStateEvent;
			  accel = new AccelGroup ();
			  set_accel_group (accel);
		}

		void set_accel_group (AccelGroup accel)
		{
			quitMenuItem.
				AddAccelerator ("activate", accel,
						new AccelKey (Gdk.Key.
							      q,
							      Gdk.
							      ModifierType.
							      ControlMask,
							      AccelFlags.
							      Visible));

			fullscreenMenuItem.
				AddAccelerator ("activate", accel,
						new AccelKey (Gdk.Key.
							      F11,
							      Gdk.
							      ModifierType.
							      None,
							      AccelFlags.
							      Visible));
			unfullscreenMenuItem.
				AddAccelerator ("activate", accel,
						new AccelKey (Gdk.Key.
							      F11,
							      Gdk.
							      ModifierType.
							      None,
							      AccelFlags.
							      Visible));
		}

		protected void AppendAfter (MenuItem item, MenuItem itemToAdd)
		{
			int i = 0;
			foreach (Widget child in AllChildren)
			{
				if (child.Equals (item))
				  {
					  Insert (itemToAdd, i + 1);
					  return;
				  }
				i++;
			}

			Append (itemToAdd);
		}

		protected virtual void OnQuit (object o, EventArgs args)
		{
			CsBoardApp.Instance.Quit ();
		}

		protected virtual void OnAbout (object o, EventArgs args)
		{
			CsBoardApp.ShowAboutDialog (null);
		}

		protected virtual void OnContents (object o, EventArgs args)
		{
			CsBoardApp.ShowHelpContents ();
		}

		protected virtual void OnFullscreenActivated (object o,
							      EventArgs args)
		{
			CsBoardApp.Instance.Window.Fullscreen ();
		}

		protected virtual void OnUnfullscreenActivated (object o,
								EventArgs
								args)
		{
			CsBoardApp.Instance.Window.Unfullscreen ();
		}

		protected virtual void OnWindowStateEvent (object o,
							   WindowStateEventArgs
							   args)
		{
			if ((args.Event.ChangedMask & Gdk.WindowState.
			     Fullscreen) == 0)
				return;

			if ((args.Event.NewWindowState & Gdk.WindowState.
			     Fullscreen) != 0)
			  {
				  replace_viewitem (fullscreenMenuItem,
						    unfullscreenMenuItem);
			  }
			else
			  {
				  replace_viewitem (unfullscreenMenuItem,
						    fullscreenMenuItem);
			  }
		}

		private void replace_viewitem (MenuItem item,
					       MenuItem newitem)
		{
			Menu menu = viewMenuItem.Submenu as Menu;
			int i = 0;
			foreach (MenuItem child in menu.Children)
			{
				if (child.Equals (item))
				  {
					  break;
				  }
				i++;
			}

			menu.Remove (item);
			menu.Insert (newitem, i);
		}
	}
}
