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

using System;
using System.Collections;
using System.IO;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;
using Gtk;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameDBPlugin:CsPlugin
		{
			GameViewer viewer;
			  Gtk.MenuItem saveItem, loadItem;

			ProgressDialog dlg;

			public GameDBPlugin ():base ("gamedb",
						     Catalog.
						     GetString
						     ("Games Database Plugin"),
						     Catalog.
						     GetString
						     ("Game database"))
			{
				saveItem = new MenuItem (Catalog.
							 GetString
							 ("Add Games to Database"));
				saveItem.Activated += on_add_to_db_activate;
				saveItem.Show ();

				loadItem = new MenuItem (Catalog.
							 GetString
							 ("Load Games from Database"));
				loadItem.Activated +=
					on_load_from_db_activate;
				loadItem.Show ();
			}

			private void on_add_to_db_activate (object
							    o, EventArgs args)
			{
				dlg = new ProgressDialog (viewer.Window,
							  Catalog.
							  GetString
							  ("Saving to database..."));
				GLib.Idle.Add (AddGamesIdleHandler);
				dlg.Run ();
				dlg.Hide ();
				dlg.Dispose ();
				dlg = null;
			}

			private bool AddGamesIdleHandler ()
			{
				IList games = viewer.Games;
				if (games == null) {
					dlg.Respond (ResponseType.Ok);
					return false;
				}
				double totalgames = games.Count;
				int ngames = 0;
				foreach (PGNChessGame game in games) {
					GameDb.Instance.AddGame (game);
					ngames++;
					dlg.UpdateProgress (ngames /
							    totalgames);
				}

				dlg.Respond (ResponseType.Ok);
				return false;
			}

			private void on_load_from_db_activate (object
							       o,
							       EventArgs args)
			{
				ArrayList list = new ArrayList ();
				GameDb.Instance.LoadGames (list);
				viewer.LoadGames (list);
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				viewer.AddToFileMenu (saveItem);
				viewer.AddToFileMenu (loadItem);

				return true;
			}

			public override bool Shutdown ()
			{
				viewer.RemoveFromViewMenu (saveItem);
				viewer.RemoveFromViewMenu (loadItem);
				return true;
			}
		}
	}
}
