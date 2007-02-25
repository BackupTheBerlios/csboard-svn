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
			  Gtk.MenuItem saveItem, openDbItem;

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

				openDbItem = new MenuItem (Catalog.
							   GetString
							   ("Games Database"));
				openDbItem.Activated +=
					on_open_games_db_activate;
				openDbItem.Show ();
			}

			private void on_open_games_db_activate (object
								o,
								EventArgs
								args)
			{
				GameDbBrowser br = new GameDbBrowser ();
				  br.Window.Show ();
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
				if (games == null)
				  {
					  dlg.Respond (ResponseType.Ok);
					  return false;
				  }
				double totalgames = games.Count;
				int ngames = 0;
				// Dont use 'foreach' since the list is going to change
				for(int i = 0; i < games.Count; i++) {
					ChessGame game = (ChessGame) games[i];
					PGNGameDetails updated;
					if(!(game is PGNGameDetails)) {
						GameDb.Instance.AddGame (game, out updated);
						viewer.UpdateGame(game, updated);
					}
					
					ngames++;
					dlg.UpdateProgress (ngames /
							    totalgames);
				}
				if(ngames > 0)
					GameDb.Instance.Commit();

				dlg.Respond (ResponseType.Ok);
				return false;
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				viewer.AddToFileMenu (saveItem);
				viewer.AddToViewMenu (openDbItem);

				viewer.ChessGameWidget.Widget.
					ButtonPressEvent +=
					new
					ButtonPressEventHandler
					(OnButtonPressEvent);
				viewer.ChessGameWidget.Widget.PopupMenu +=
					new PopupMenuHandler (PopupMenuCb);

				GameViewer.GameDb = GameDb.Instance;
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.RemoveFromViewMenu (saveItem);
				viewer.RemoveFromViewMenu (openDbItem);
				return true;
			}

			private void PopupMenuCb (object o,
						  PopupMenuArgs args)
			{
				Menu menu = new RatingPopup (viewer);
				menu.ShowAll ();
				menu.Popup ();
			}

			[GLib.ConnectBefore]
				public void OnButtonPressEvent (object o,
								ButtonPressEventArgs
								args)
			{
				if (args.Event.Button != 3)
					return;
				Menu menu = new RatingPopup (viewer);
				menu.ShowAll ();
				menu.Popup ();
			}
		}

		class RatingPopup:Gtk.Menu
		{
			GameViewer viewer;
			Hashtable map;

			public RatingPopup (GameViewer viewer)
			{
				this.viewer = viewer;
				map = new Hashtable ();
				CheckMenuItem[] ratingItems =
				{
				new CheckMenuItem (Catalog.
							   GetString
							   ("Not interested")),
						new
						CheckMenuItem
						(Catalog.
							 GetString
							 ("Average Game")),
						new
						CheckMenuItem
						(Catalog.
							 GetString
							 ("Good Game")),
						new
						CheckMenuItem
						(Catalog.
							 GetString
							 ("Excellent Game")),
						new
						CheckMenuItem
						(Catalog.
							 GetString
							 ("Must Have"))};
				GameRating[]ratings =
				{
				GameRating.Ignore,
						GameRating.Average,
						GameRating.Good,
						GameRating.Excellent,
						GameRating.MustHave};

				int i = 0;
				foreach (CheckMenuItem item in ratingItems)
				{
					Append (item);
					item.Activated += OnRatingActivated;
					map[item] = ratings[i++];
					item.Show ();
				}
			}

			private void OnRatingActivated (object o,
							EventArgs args)
			{
				GameRating rating = (GameRating) map[o];
				ChessGame game = viewer.CurrentGame;
				if (game == null)
					return;
				PGNGameDetails updated;
				GameDb.Instance.AddGame (game, rating, out updated);
				GameDb.Instance.Commit();
				viewer.UpdateCurrentGame(updated);
			}
		}

	}
}
