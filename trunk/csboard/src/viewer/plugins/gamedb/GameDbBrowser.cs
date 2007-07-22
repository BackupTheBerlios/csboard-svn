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

using Glade;
using Gtk;
using System.Collections;
using System;

using com.db4o;
using com.db4o.query;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameDbBrowser
		{

			[Glade.Widget] private Gtk.Window gameDbWindow;
			[Glade.Widget] private Gtk.VBox gamesListBox;
			[Glade.Widget] private Gtk.Entry searchEntry;
			[Glade.Widget] private Gtk.Entry tagSearchEntry;

			[Glade.Widget] private Gtk.ComboBox colorOption;

			[Glade.Widget] private Gtk.ComboBox ratingMatchOption;
			[Glade.Widget] private Gtk.ComboBox ratingChoice;

			[Glade.Widget] private Gtk.Statusbar statusbar;

			[Glade.Widget] private Gtk.
				TreeView gamesCollectionTree;
			[Glade.Widget] private Gtk.
				Entry gamesCollectionFilterEntry;
			[Glade.Widget] private Gtk.
				Button gamesCollectionFilterButton;

			[Glade.Widget] private Gtk.Button newCollectionButton,
				addGamesToCollectionButton,
				editGameCollectionButton,
				deleteGameCollectionButton;

			private Gtk.ListStore gamesCollectionStore;

			GamesListWidget searchGamesList;

			Hashtable ratingMap;

			ObjectSet results;

			ArrayList modifiedGames;

			const int ANY_IDX = 0;
			const int WHITE_IDX = 1;
			const int BLACK_IDX = 2;

			const int RATING_AVERAGE = 1;
			const int RATING_GOOD = 2;
			const int RATING_EXCELLENT = 3;
			const int RATING_MUST_HAVE = 4;

			const int MATCH_EQUALS = 1;
			const int MATCH_ABOVE = 2;
			const int MATCH_BELOW = 3;

			public GameDbBrowser ()
			{
				modifiedGames = new ArrayList ();

				ratingMap = new Hashtable ();
				ratingMap[RATING_AVERAGE] =
					GameRating.Average;
				ratingMap[RATING_GOOD] = GameRating.Good;
				ratingMap[RATING_EXCELLENT] =
					GameRating.Excellent;
				ratingMap[RATING_MUST_HAVE] =
					GameRating.MustHave;

				Glade.XML xml =
					Glade.XML.
					FromAssembly ("gamedb.glade",
						      "gameDbWindow", null);
				xml.Autoconnect (this);

				colorOption.Active = 0;
				ratingMatchOption.Active = 0;

				searchGamesList = new GamesListWidget ();	// this will add the column and other details
				searchGamesList.View.SelectionMode =
					SelectionMode.Multiple;
				gamesListBox.PackStart (searchGamesList, true,
							true, 2);

				gamesCollectionStore =
					new
					ListStore (typeof (string),
						   typeof (GameCollection));

				gamesCollectionTree.Model =
					gamesCollectionStore;

				gamesCollectionTree.RowActivated +=
					OnGamesCollectionRowActivated;

				CellRendererText renderer =
					new CellRendererText ();
				  gamesCollectionTree.
					AppendColumn (new
						      TreeViewColumn
						      ("Collections",
						       renderer, "markup",
						       0));

				  searchEntry.Activated += OnSearch;
				  tagSearchEntry.Activated += OnSearch;

				  gamesCollectionFilterEntry.Activated +=
					OnFilterGamesCollection;
				  gamesCollectionFilterButton.Clicked +=
					OnFilterGamesCollection;

				  newCollectionButton.Clicked +=
					OnNewCollection;
				  addGamesToCollectionButton.Clicked +=
					OnAddGamesToCollection;
				  editGameCollectionButton.Clicked +=
					OnEditGameCollection;
				  deleteGameCollectionButton.Clicked +=
					OnDeleteGameCollection;
				/*
				   int width, height;
				   GameViewer.Instance.Window.
				   GetSize (out width, out height);

				   gameDbWindow.Resize ((int) Math.
				   Round (0.9 * width),
				   (int) Math.Round (0.9 *
				   height));
				 */
				  searchGamesList.GameSelectionEvent +=
					OnGameSelectionEvent;
			}

			public Window Window
			{
				get
				{
					return gameDbWindow;
				}
			}

			public void on_window_delete_event (System.Object b,
							    DeleteEventArgs e)
			{
				gameDbWindow.Hide ();
			}


			protected void OnSearch (object o, EventArgs args)
			{
				HandleSearch ();
			}

			protected void OnGamesCollectionRowActivated (object
								      o,
								      RowActivatedArgs
								      args)
			{
				TreeModel model = ((TreeView) o).Model;
				TreeIter iter;
				model.GetIter (out iter, args.Path);
				GameCollection col =
					(GameCollection) model.GetValue (iter,
									 1);
				ArrayList list = new ArrayList ();
				col.LoadGames (list);

				GameViewer.Instance.GameViewerWidget.
					LoadGames (list);
				GameViewer.Instance.Window.Present ();
			}

			private void OnFilterGamesCollection (object o,
							      EventArgs args)
			{
				ArrayList list = new ArrayList ();
				string filter =
					gamesCollectionFilterEntry.Text.
					Trim ();
				GameDb.Instance.GetGameCollections (list,
								    filter.
								    Length ==
								    0 ? null :
								    filter);
				gamesCollectionStore.Clear ();
				foreach (GameCollection collection in list)
				{
					gamesCollectionStore.
						AppendValues
						(GenerateGameCollectionCellValue
						 (collection), collection);
				}
			}

			private void OnNewCollection (object o,
						      EventArgs args)
			{
				GamesCollectionDialog dlg =
					GamesCollectionDialog.CreateEmpty ();
				if (dlg.Dialog.Run () ==
				    (int) ResponseType.Ok)
				  {
					  GameCollection collection =
						  new GameCollection (dlg.
								      Title,
								      dlg.
								      Description,
								      new
								      ArrayList
								      ());
					  GameDb.Instance.
						  AddCollection (collection);
				  }

				dlg.Dialog.Hide ();
				dlg.Dialog.Dispose ();
			}

			private void OnAddGamesToCollection (object o,
							     EventArgs args)
			{
				ArrayList list = new ArrayList ();
				GetSelectedGames (list);
				if (list.Count == 0)
					return;

				TreeViewColumn col;
				TreePath path;
				gamesCollectionTree.GetCursor (out path,
							       out col);
				if (path == null)
					return;

				TreeIter iter;
				gamesCollectionTree.Model.
					GetIter (out iter, path);
				GameCollection collection =
					(GameCollection)
					gamesCollectionTree.Model.
					GetValue (iter, 1);

				foreach (PGNGameDetails info in list)
				{
					collection.AddGame (info);
				}

				string updated_details =
					GenerateGameCollectionCellValue
					(collection);
				gamesCollectionTree.Model.SetValue (iter, 0,
								    updated_details);

				GameDb.Instance.AddCollection (collection);
			}

			private void OnDeleteGameCollection (object o,
							     EventArgs args)
			{
				TreeViewColumn col;
				TreePath path;
				gamesCollectionTree.GetCursor (out path,
							       out col);
				if (path == null)
					return;

				TreeIter iter;
				gamesCollectionTree.Model.GetIter (out iter,
								   path);
				GameCollection collection =
					(GameCollection) gamesCollectionTree.
					Model.GetValue (iter, 1);
				GameDb.Instance.DeleteCollection (collection);
				((ListStore) gamesCollectionTree.Model).
					Remove (ref iter);
			}

			private void OnEditGameCollection (object o,
							   EventArgs args)
			{
				TreeViewColumn col;
				TreePath path;
				gamesCollectionTree.GetCursor (out path,
							       out col);
				if (path == null)
					return;

				TreeIter iter;
				gamesCollectionTree.Model.GetIter (out iter,
								   path);
				GameCollection collection =
					(GameCollection) gamesCollectionTree.
					Model.GetValue (iter, 1);
				EditGamesCollectionDialog dlg =
					new
					EditGamesCollectionDialog
					(collection);

				int width, height;
				gameDbWindow.GetSize (out width, out height);
				width = (int) Math.Round (0.9 * width);
				height = (int) Math.Round (0.9 * height);
				dlg.Dialog.Resize (width, height);
				dlg.Dialog.Show ();

				dlg.Dialog.Run ();
				dlg.Dialog.Hide ();
				dlg.Dialog.Dispose ();
			}

			static string
				GenerateGameCollectionCellValue
				(GameCollection col)
			{
				return String.
					Format
					("<b>{0}</b>\n<small>{1} games</small>",
					 col.Title, col.Games.Count);
			}

			protected void OnShowAllButtonClicked (object o,
							       EventArgs args)
			{
				HandleBrowse ();
			}

			protected void OnEditGamesButtonClicked (object o,
								 EventArgs
								 args)
			{
				ArrayList list = new ArrayList ();
				GetSelectedGames (list);

				GamesEditorDialog editor =
					new GamesEditorDialog (list,
							       modifiedGames);
				int width, height;
				gameDbWindow.GetSize (out width, out height);
				width = (int) Math.Round (0.9 * width);
				height = (int) Math.Round (0.9 * height);
				editor.Dialog.Resize (width, height);
				editor.Dialog.Show ();

				editor.SplitPane.Position =
					(int) Math.Round (0.40 * width);
				editor.Dialog.Run ();
				editor.Dialog.Hide ();
			}

			protected void
				OnSaveModifiedGamesButtonClicked (object o,
								  EventArgs
								  args)
			{
				foreach (PGNGameDetails info in modifiedGames)
				{
					GameDb.Instance.DB.Set (info);
				}
				GameDb.Instance.DB.Commit ();
			}

			protected void OnSearchButtonClicked (object o,
							      EventArgs args)
			{
				HandleSearch ();
			}

			void OnGameSelectionEvent (ChessGame game)
			{
				LoadSelectedGames ();
			}

			protected void OnLoadGamesButtonClicked (object obj,
								 EventArgs
								 args)
			{
				LoadSelectedGames ();
			}

			private void LoadSelectedGames ()
			{
				ArrayList list = new ArrayList ();
				GetSelectedGames (list);

				GameViewer.Instance.GameViewerWidget.
					LoadGames (list);
				GameViewer.Instance.Window.Present ();
			}

			private void GetSelectedGames (ArrayList list)
			{
				TreePath[]selected =
					searchGamesList.View.SelectedItems;
				if (selected == null || selected.Length == 0)
					return;
				foreach (TreePath path in selected)
				{
					TreeIter iter;
					searchGamesList.Model.
						GetIter (out iter, path);
					PGNGameDetails info =
						(PGNGameDetails)
						searchGamesList.Model.
						GetValue (iter, 0);
					list.Add (info);
				}
			}

			private void HandleBrowse ()
			{
				searchEntry.Text = "";
				ratingChoice.Active = 0;
				tagSearchEntry.Text = "";
				HandleSearch ();
			}

			private void HandleSearch ()
			{
				string search = searchEntry.Text.Trim ();

				Query query = GameDb.Instance.DB.Query ();
				query.Constrain (typeof (PGNGameDetails));
				if (search.Length > 0)
				  {
					  int idx = colorOption.Active;
					  if (idx == ANY_IDX)
					    {
						    Query white =
							    query.
							    Descend ("white");
						    Query black =
							    query.
							    Descend ("black");

						    Constraint whiteConstraint
							    =
							    white.
							    Constrain
							    (search).Like ();
						    Constraint blackConstraint
							    =
							    black.
							    Constrain
							    (search).Like ();

						    whiteConstraint.
							    Or
							    (blackConstraint);
					    }
					  if (idx == WHITE_IDX)
					    {
						    Query white =
							    query.
							    Descend ("white");
						    white.Constrain (search).
							    Like ();
					    }
					  else if (idx == BLACK_IDX)
					    {
						    Query black =
							    query.
							    Descend ("black");
						    black.Constrain (search).
							    Like ();
					    }

				  }

				if (ratingMatchOption.Active >= 0
				    && ratingChoice.Active >= 0
				    && !ratingChoice.ActiveText.Equals (""))
				  {
					  int rating =
						  (int)
						  ratingMap[ratingChoice.
							    Active];
					  switch (ratingMatchOption.Active)
					    {
					    case MATCH_EQUALS:
						    query.Descend ("rating").
							    Constrain
							    (rating).Equal ();
						    break;
					    case MATCH_ABOVE:
						    Query q1 =
							    query.
							    Descend
							    ("rating");
						    q1.Constrain (rating -
								  1).
							    Greater ();
						    q1.OrderDescending ();
						    break;
					    case MATCH_BELOW:
						    Query q2 =
							    query.
							    Descend
							    ("rating");
						    q2.Constrain (rating +
								  1).
							    Smaller ();
						    q2.OrderDescending ();
						    break;
					    }
				  }

				HandleTagSearchOptions (query);
				results = query.Execute ();

				statusbar.Pop (1);
				statusbar.Push (1,
						String.Format ("{0} games",
							       results.
							       Size ()));
				ArrayList list = new ArrayList ();
				while (results.HasNext ())
				  {
					  PGNGameDetails details
						  =
						  (PGNGameDetails) results.
						  Next ();
					  list.Add (details);
				  }
				searchGamesList.SetGames (list);
			}

			private void HandleTagSearchOptions (Query query)
			{
				string tagstr = tagSearchEntry.Text.Trim ();
				if (tagstr.Length == 0)
					return;
				// TODO: split
				ArrayList tags = new ArrayList ();
				tags.Add (tagstr);
				Constraint prev = null;
				foreach (string tag in tags)
				{
					if (prev != null)
						query.Descend ("tags").
							Constrain (tag).
							Equal ().Or (prev);
					else
						prev = query.Descend ("tags").
							Constrain (tag).
							Equal ();
				}
			}
		}
	}
}
