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

using Chess.Parser;
using Chess.Game;
using System.IO;
using Gtk;
using GLib;
using Mono.Unix;
using System.Collections;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameViewerUI:VBox
		{
			protected Statusbar statusBar;
			protected ViewerMenuBar menubar;


			protected GameViewerWidget gameViewerWidget;

			public ChessGameView ChessGameView
			{
				get
				{
					return gameViewerWidget.
						ChessGameWidget.NotationView;
				}
			}

			public SearchableGamesListWidget GamesListWidget
			{
				get
				{
					return gameViewerWidget.
						GamesListWidget;
				}
			}

			public Gtk.Statusbar StatusBar
			{
				get
				{
					return statusBar;
				}
			}

			public Gtk.MenuBar MenuBar
			{
				get
				{
					return menubar;
				}
			}

			public bool AddToViewMenu (Gtk.MenuItem item)
			{
				return AddToMenu (menubar.viewMenuItem, item,
						  null);
			}

			public bool RemoveFromViewMenu (Gtk.MenuItem item)
			{
				Menu menu =
					(Menu) menubar.viewMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}

			public bool AddToFileMenu (Gtk.MenuItem item)
			{
				return AddToMenu (menubar.fileMenuItem, item,
						  menubar.saveAsSeparator);
			}

			public bool RemoveFromFileMenu (Gtk.MenuItem item)
			{
				Menu menu =
					(Menu) menubar.fileMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}


			protected bool AddToMenu (Gtk.MenuItem parentMenu,
						  Gtk.MenuItem itemToBeAdded,
						  Gtk.MenuItem beforeThis)
			{
				Gtk.Menu menu = (Gtk.Menu) parentMenu.Submenu;
				if (menu == null)
				  {
					  menu = new Menu ();
					  menu.Show ();
					  parentMenu.Submenu = menu;
				  }
				if (beforeThis == null)
				  {
					  menu.Append (itemToBeAdded);
					  return true;
				  }

				// find the index
				int index = 0;
				foreach (Gtk.MenuItem item in menu.
					 AllChildren)
				{
					if (beforeThis.Equals (item))
					  {
						  menu.Insert (itemToBeAdded,
							       index);
						  return true;
					  }
					index++;
				}
				return false;
			}

			ArrayList games;

			public ArrayList Games
			{
				get
				{
					return games;
				}
			}
			public event GamesLoadedEventHandler GamesLoadedEvent;

			protected void SetGames (ArrayList list)
			{
				games = list;
				if (GamesLoadedEvent != null)
					GamesLoadedEvent (this,
							  System.EventArgs.
							  Empty);

				menubar.moveCommentMenuItem.Sensitive =
					games != null;
			}

			public GameViewerUI ():base ()
			{
				menubar = new ViewerMenuBar ();
				// this will be enabled as and when
				menubar.moveCommentMenuItem.Sensitive = false;
				gameViewerWidget =
					new GameViewerWidget (this);

				PackStart (gameViewerWidget, true, true, 2);
				statusBar = new Statusbar ();
				PackStart (statusBar, false, true, 2);
			}
		}
	}
}
