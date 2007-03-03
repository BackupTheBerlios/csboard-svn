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
			GameEditor editor;

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
				for (int i = 0; i < games.Count; i++)
				  {
					  ChessGame game =
						  (ChessGame) games[i];
					  PGNGameDetails updated;
					  if (!(game is PGNGameDetails))
					    {
						    GameDb.Instance.
							    AddGame (game,
								     out
								     updated);
						    viewer.UpdateGame (game,
								       updated);
					    }

					  ngames++;
					  dlg.UpdateProgress (ngames /
							      totalgames);
				  }
				if (ngames > 0)
					GameDb.Instance.Commit ();

				dlg.Respond (ResponseType.Ok);
				return false;
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				editor = new GameEditor (viewer);
				viewer.ChessGameDetailsBox.PackStart (editor,
								      false,
								      true,
								      2);

				viewer.AddToFileMenu (saveItem);
				viewer.AddToViewMenu (openDbItem);

				GameViewer.GameDb = GameDb.Instance;
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.ChessGameDetailsBox.Remove (editor);
				viewer.RemoveFromViewMenu (saveItem);
				viewer.RemoveFromViewMenu (openDbItem);
				return true;
			}
		}

		class GameEditor:HBox
		{

			GameViewer viewer;
			ComboBox combo;
			ComboBoxEntry tagsCombo;
			ListStore tagsStore;
			Button save;
			  GameRating[] ratings;

			public GameEditor (GameViewer viewer):base ()
			{
				this.viewer = viewer;
				viewer.GameLoadedEvent += OnGameLoaded;
				combo = new ComboBox (new string[]
						      {
						      Catalog.
						      GetString
						      ("Not interested"),
						      Catalog.
						      GetString
						      ("Average Game"),
						      Catalog.
						      GetString ("Good Game"),
						      Catalog.
						      GetString
						      ("Excellent Game"),
						      Catalog.
						      GetString ("Must Have")}
				);
				save = new Button (Stock.Save);
				save.Sensitive = false;
				save.Clicked += OnSave;
				combo.Changed +=
					delegate (object o, EventArgs args)
				{
					save.Sensitive = true;
				};

				ratings = new GameRating[]
				{
				GameRating.Ignore,
						GameRating.Average,
						GameRating.Good,
						GameRating.Excellent,
						GameRating.MustHave};
				tagsStore = new ListStore (typeof (string));
				tagsCombo = new ComboBoxEntry (tagsStore, 0);
				tagsCombo.Entry.Activated +=
					OnTagsComboActivated;

				PackStart (new
					   Label (Catalog.
						  GetString ("My Rating")),
					   false, false, 2);
				PackStart (combo, false, false, 2);
				PackStart (tagsCombo, false, false, 2);
				PackStart (save, false, false, 2);

				ShowAll ();
			}

			private void OnGameLoaded (object o, EventArgs args)
			{
				combo.Active = -1;
				PGNGameDetails details =
					viewer.CurrentGame as PGNGameDetails;
				if (details != null
				    && details.Rating != GameRating.Unknown)
					combo.Active =
						details.Rating ==
						GameRating.
						Ignore ? 0 : (int) details.
						Rating;
				UpdateTagDetails (details);
				save.Sensitive = false;
			}

			private void UpdateTagDetails (PGNGameDetails details)
			{
				tagsStore.Clear ();
				if (details == null)
					return;
				if (details.Tags != null)
				  {
					  foreach (string tag in details.Tags)
					  {
						  tagsStore.
							  AppendValues (tag);
					  }
				  }
			}

			private void OnTagsComboActivated (object o,
							   EventArgs args)
			{
				string str = tagsCombo.Entry.Text.Trim ();
				if (str.Length > 0)
				  {
					  tagsStore.AppendValues (str);
					  save.Sensitive = true;
				  }
				tagsCombo.Entry.Text = "";
			}

			private void OnSave (object o, EventArgs args)
			{
				save.Sensitive = false;
				ChessGame game = viewer.CurrentGame;
				if (game == null)
					return;
				PGNGameDetails updated;
				bool newobj =
					GameDb.Instance.
					FindOrCreateGame (game, out updated);
				if (combo.Active >= 0)
					updated.Rating =
						ratings[combo.Active];
				tagsCombo.Model.
					Foreach (delegate
						 (TreeModel model,
						  TreePath path,
						  TreeIter iter)
						 {
						 string tag =
						 (string) model.
						 GetValue (iter, 0);
						 updated.AddTag (tag);
						 return false;}
				);
				if (newobj)
					viewer.UpdateCurrentGame (updated);
				GameDb.Instance.SaveGame (updated);
				UpdateTagDetails (updated);
				GameDb.Instance.Commit ();
			}
		}
	}
}
