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
			ToolButton dbToolButton;

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
							 ("Add Games to _Database"));
				saveItem.Activated += on_add_to_db_activate;
				saveItem.Show ();

				openDbItem = new MenuItem (Catalog.
							   GetString
							   ("Games _Database"));

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

			AddToDbDialog dbDlg;
			private void on_add_to_db_activate (object
							    o, EventArgs args)
			{
				dbDlg = new AddToDbDialog (viewer.Window);
				if (dbDlg.Run () != (int) ResponseType.Ok)
				  {
					  dbDlg.Hide ();
					  dbDlg.Dispose ();
					  return;
				  }

				dbDlg.Hide ();
				dlg = new ProgressDialog (viewer.Window,
							  Catalog.
							  GetString
							  ("Saving to database..."));
				GLib.Idle.Add (AddGamesIdleHandler);
				dlg.Run ();
				dlg.Hide ();
				dlg.Dispose ();
				dlg = null;
				dbDlg.Dispose ();
				dbDlg = null;
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
				GameCollection collection = null;
				string[]tags = dbDlg.Tags;
				if (dbDlg.AddCollection
				    && dbDlg.CollectionTitle != null)
				  {
					  collection =
						  new GameCollection (dbDlg.
								      CollectionTitle,
								      dbDlg.
								      Description,
								      new
								      ArrayList
								      ());
				  }
				// Dont use 'foreach' since the list is going to change
				for (int i = 0; i < games.Count; i++)
				  {
					  ChessGame game =
						  (ChessGame) games[i];
					  PGNGameDetails updated;
					  if (!(game is PGNGameDetails))
					    {
						    GameDb.Instance.
							    FindOrCreateGame
							    (game,
							     out updated);
					    }
					  else
						  updated =
							  game as
							  PGNGameDetails;

					  foreach (string tag in tags)
					  {
						  updated.AddTag (tag);
					  }
					  if (collection != null)
						  collection.
							  AddGame (updated);
					  GameDb.Instance.DB.Set (updated);
					  viewer.UpdateGame (game, updated);

					  ngames++;
					  dlg.UpdateProgress (ngames /
							      totalgames);
				  }
				if (collection != null)
					GameDb.Instance.DB.Set (collection);

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

				Image img =
					new Image (new
						   IconSet (Gdk.Pixbuf.
							    LoadFromResource
							    ("dbicon.png")),
						   viewer.Toolbar.IconSize);
				img.Show ();
				dbToolButton =
					new ToolButton (img,
							Catalog.
							GetString
							("Games DB"));
				dbToolButton.Clicked +=
					on_open_games_db_activate;
				dbToolButton.Show ();
				viewer.Toolbar.Insert (dbToolButton,
						       viewer.Toolbar.NItems);

				GameViewer.GameDb = GameDb.Instance;
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.ChessGameDetailsBox.Remove (editor);
				viewer.RemoveFromViewMenu (saveItem);
				viewer.RemoveFromViewMenu (openDbItem);
				viewer.Toolbar.Remove (dbToolButton);
				return true;
			}
		}

		class AddToDbDialog:Dialog
		{
			public Entry tagsEntry, collectionEntry;
			public TextView description;
			CheckButton addCollectionToggle;

			public bool AddCollection
			{
				get
				{
					return addCollectionToggle.Active;
				}
			}

			public string CollectionTitle
			{
				get
				{
					string title =
						collectionEntry.Text.Trim ();
					  return title.Length ==
						0 ? null : title;
				}
			}

			public string Description
			{
				get
				{
					return description.Buffer.Text;
				}
			}

			public string[] Tags
			{
				get
				{
					string tagstr =
						tagsEntry.Text.Trim ();
					if (tagstr.Length == 0)
						return new string[]
					  {
					  };
					System.Text.StringBuilder buf =
						new System.Text.
						StringBuilder ();
					foreach (char ch in tagstr)
					{
						if (ch == ' ')
							continue;
						buf.Append (ch);
					}
					return buf.ToString ().Split (',');
				}
			}

			public AddToDbDialog (Window parent):base (Catalog.
								   GetString
								   ("Add games to database"),
								   parent,
								   DialogFlags.
								   Modal,
								   Stock.
								   Cancel,
								   ResponseType.
								   Cancel,
								   Stock.Ok,
								   ResponseType.
								   Ok)
			{
				tagsEntry = new Entry ();

				Label label;
				uint row = 0;
				Table table = new Table (3, 2, false);
				table.RowSpacing = table.ColumnSpacing = 4;

				label = new Label ();
				label.Markup =
					Catalog.GetString ("<b>Tags</b>");
				label.Xalign = 0;
				table.Attach (label, 0, 1, row, row + 1);
				table.Attach (tagsEntry, 1, 2, row, row + 1);

				row++;
				label = new Label ();
				label.Markup =
					Catalog.
					GetString
					("<i><small>Comma separated list of tags</small></i>");
				label.Xalign = 0;
				table.Attach (label, 0, 2, row, row + 1);

				row++;
				table.Attach (GetCollectionDetailsFrame (), 0,
					      2, row, row + 1);
				table.ShowAll ();
				VBox.PackStart (table, true, true, 2);
			}

			private Frame GetCollectionDetailsFrame ()
			{
				collectionEntry = new Entry ();
				ScrolledWindow scroll = new ScrolledWindow ();
				scroll.HscrollbarPolicy = PolicyType.Never;
				scroll.VscrollbarPolicy =
					PolicyType.Automatic;

				description = new TextView ();
				description.WrapMode = WrapMode.Word;
				scroll.Add (description);

				addCollectionToggle =
					new CheckButton (Catalog.
							 GetString
							 ("Create a collection"));
				addCollectionToggle.Toggled += OnToggled;
				addCollectionToggle.Active = false;
				addCollectionToggle.Toggle ();

				Frame frame = new Frame ();
				Table table = new Table (4, 2, false);
				Label label;
				uint row = 0;

				table.RowSpacing = table.ColumnSpacing = 4;

				table.Attach (addCollectionToggle, 0, 2, row,
					      row + 1);

				row++;
				label = new Label ();
				label.Markup =
					Catalog.GetString ("<b>Title</b>");
				label.Xalign = 0;
				table.Attach (label, 0, 1, row, row + 1);
				table.Attach (collectionEntry, 1, 2, row,
					      row + 1);
				row++;
				label = new Label ();
				label.Xalign = 0;
				label.Markup =
					Catalog.
					GetString
					("<i><small>Create a collection with this title</small></i>");
				table.Attach (label, 0, 2, row, row + 1);

				label = new Label ();
				label.Markup =
					Catalog.
					GetString ("<b>Description</b>");
				label.Xalign = 0;
				row++;
				table.Attach (label, 0, 2, row, row + 1);

				row++;
				table.Attach (scroll, 0, 2, row, row + 1);
				frame.Add (table);
				return frame;
			}

			private void OnToggled (object o, EventArgs args)
			{
				collectionEntry.Sensitive =
					description.Sensitive =
					addCollectionToggle.Active;
			}
		}

		class GameEditor:Expander
		{

			GameViewer viewer;
			ComboBox combo;
			ComboBoxEntry tagsCombo;
			ListStore tagsStore;
			Button save;
			  GameRating[] ratings;

			public GameEditor (GameViewer viewer):base (Catalog.
								    GetString
								    ("Rating"))
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

				Table table = new Table (3, 2, false);
				table.RowSpacing = 2;
				table.ColumnSpacing = 2;
				uint row = 0, col = 0;
				Label label =
					new Label (Catalog.
						   GetString ("My Rating"));
				label.Xalign = 0;
				label.Yalign = 0;
				table.Attach (label, col, col + 1, row,
					      row + 1);
				col++;
				table.Attach (combo, col, col + 1, row,
					      row + 1);

				label = new Label (Catalog.
						   GetString ("Tags"));
				label.Xalign = 0;
				label.Yalign = 0;
				col = 0;
				row++;
				table.Attach (label, col, col + 1, row,
					      row + 1);
				col++;
				table.Attach (tagsCombo, col, col + 1, row,
					      row + 1);

				col = 1;
				row++;
				Alignment align = new Alignment (1, 0, 0, 0);
				align.Add (save);
				table.Attach (align, col, col + 1, row,
					      row + 1);

				Add (table);

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
