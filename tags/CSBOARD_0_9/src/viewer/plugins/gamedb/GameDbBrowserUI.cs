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
using System.Collections;
using System;
using Mono.Unix;

using com.db4o;
using com.db4o.query;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameDbBrowserUI:VBox
		{

			protected Gtk.VBox gamesListBox;
			protected Gtk.Entry searchEntry, tagSearchEntry;

			protected ComboBox colorOption;

			protected ComboBox ratingMatchOption;
			protected Gtk.ComboBox ratingChoice;

			protected Gtk.Statusbar statusbar;

			protected Gtk.TreeView gamesCollectionTree;
			protected Gtk.Entry gamesCollectionFilterEntry;
			protected Gtk.Button gamesCollectionFilterButton;

			protected Gtk.Button newCollectionButton,
				addGamesToCollectionButton,
				editGameCollectionButton,
				deleteGameCollectionButton;

			protected ListStore gamesCollectionStore;

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

			Notebook book;

			private void CreateAllGamesPage ()
			{
				VBox box = new VBox ();
				HBox hbox;

				  hbox = new HBox ();
				  hbox.PackStart (new
						  Label (Catalog.
							 GetString
							 ("Filter")), false,
						  false, 4);
				  searchEntry = new Entry ();
				  hbox.PackStart (searchEntry, false, true,
						  2);
				Button button =
					new Button (Catalog.GetString ("Go"));
				  button.Clicked += OnSearch;
				  hbox.PackStart (button, false, false, 2);

				HBox hbox1 = new HBox ();
				  hbox1.PackStart (hbox, true, true, 2);
				Button showAllButton =
					new Button (Catalog.
						    GetString ("Show All"));
				  showAllButton.Clicked +=
					OnShowAllButtonClicked;
				  hbox1.PackStart (showAllButton, false,
						   false, 2);

				  box.PackStart (hbox1, false, true, 2);

				Expander advanced =
					new Expander (Catalog.
						      GetString ("Advanced"));
				Table table = new Table (3, 3, false);

				  colorOption = new ComboBox (new string[]
							      {
							      Catalog.
							      GetString
							      ("Any"),
							      Catalog.
							      GetString
							      ("White"),
							      Catalog.
							      GetString
							      ("Black")});
				uint x = 0, y = 0;
				  table.Attach (new
						Label (Catalog.
						       GetString ("Color")),
						x, x + 1, y, y + 1,
						AttachOptions.Shrink,
						AttachOptions.Shrink, 2, 2);
				  x++;
				Alignment align =
					new Alignment (0, 0.5f, 0, 0);
				  align.Add (colorOption);
				  table.Attach (align, x, x + 1, y,
						y + 1, AttachOptions.Shrink,
						AttachOptions.Shrink, 2, 2);
				  x = 0;
				  y++;

				  ratingMatchOption =
					new ComboBox (new string[]
						      {
						      "",
						      Catalog.
						      GetString ("Equals"),
						      Catalog.
						      GetString ("Above"),
						      Catalog.
						      GetString ("Below")});

				  ratingChoice = new ComboBox (new string[]
							       {
							       "",
							       Catalog.
							       GetString
							       ("Average"),
							       Catalog.
							       GetString
							       ("Good"),
							       Catalog.
							       GetString
							       ("Excellent"),
							       Catalog.
							       GetString
							       ("Must Have")});

				table.Attach (new
					      Label (Catalog.
						     GetString ("Rating")), x,
					      x + 1, y, y + 1,
					      AttachOptions.Shrink,
					      AttachOptions.Shrink, 2, 2);
				x++;
				align = new Alignment (0, 0.5f, 0, 0);
				align.Add (ratingMatchOption);
				table.Attach (align, x, x + 1, y,
					      y + 1, AttachOptions.Shrink,
					      AttachOptions.Shrink, 2, 2);
				x++;
				align = new Alignment (0, 0.5f, 0, 0);
				align.Add (ratingChoice);
				table.Attach (align, x, x + 1, y, y + 1);
				x = 0;
				y++;

				table.Attach (new
					      Label (Catalog.
						     GetString ("Tags")), x,
					      x + 1, y, y + 1,
					      AttachOptions.Shrink,
					      AttachOptions.Shrink, 2, 2);
				x++;
				tagSearchEntry = new Entry ();
				table.Attach (tagSearchEntry, x, x + 2, y,
					      y + 1);

				advanced.Child = table;
				advanced.Expanded = false;
				box.PackStart (advanced, false, true, 2);

				gamesListBox = new VBox ();
				ScrolledWindow win = new ScrolledWindow ();
				win.HscrollbarPolicy = win.VscrollbarPolicy =
					PolicyType.Automatic;
				win.AddWithViewport (gamesListBox);
				box.PackStart (win, true, true, 2);

				hbox = new HBox ();
				button = new Button (Catalog.
						     GetString
						     ("Load Games"));
				button.Clicked += OnLoadGamesButtonClicked;
				button.Image =
					new Image (Stock.Open,
						   IconSize.Button);
				hbox.PackStart (button, false, false, 2);

				button = new Button (Catalog.
						     GetString
						     ("Edit Games"));
				button.Clicked += OnEditGamesButtonClicked;
				button.Image =
					new Image (Stock.Edit,
						   IconSize.Button);
				hbox.PackStart (button, false, false, 2);

				button = new Button (Catalog.
						     GetString
						     ("Save Modified Games"));
				button.Clicked +=
					OnSaveModifiedGamesButtonClicked;
				button.Image =
					new Image (Stock.Save,
						   IconSize.Button);
				hbox.PackStart (button, false, false, 2);

				box.PackStart (hbox, false, true, 2);

				book.AppendPage (box,
						 new Label (Catalog.
							    GetString
							    ("All Games")));
				ShowAll ();
			}

			private void CreateGameCollectionPage ()
			{
				VBox box = new VBox ();
				HBox hbox = new HBox ();
				hbox.PackStart (new
						Label (Catalog.
						       GetString ("Filter")),
						false, false, 2);
				gamesCollectionFilterEntry = new Entry ();
				gamesCollectionFilterButton =
					new Button (Catalog.GetString ("Go"));

				hbox.PackStart (gamesCollectionFilterEntry,
						false, false, 2);
				hbox.PackStart (gamesCollectionFilterButton,
						false, false, 2);

				box.PackStart (hbox, false, true, 2);

				///////////////
				gamesCollectionTree = new TreeView ();
				ScrolledWindow win = new ScrolledWindow ();
				win.HscrollbarPolicy = win.VscrollbarPolicy =
					PolicyType.Automatic;
				win.Child = gamesCollectionTree;
				box.PackStart (win, true, true, 2);

				///////////////
				HButtonBox buttonbox = new HButtonBox ();
				buttonbox.Layout = ButtonBoxStyle.Start;
				newCollectionButton =
					new Button (Catalog.
						    GetString
						    ("New Collection"));
				newCollectionButton.Image =
					new Image (Stock.New,
						   IconSize.Button);
				editGameCollectionButton =
					new Button (Catalog.
						    GetString
						    ("Edit Collection"));
				editGameCollectionButton.Image =
					new Image (Stock.Edit,
						   IconSize.Button);
				deleteGameCollectionButton =
					new Button (Catalog.
						    GetString
						    ("Delete Collection"));
				deleteGameCollectionButton.Image =
					new Image (Stock.Delete,
						   IconSize.Button);
				addGamesToCollectionButton =
					new Button (Catalog.
						    GetString
						    ("Add Selected Games"));
				addGamesToCollectionButton.Image =
					new Image (Stock.Add,
						   IconSize.Button);
				buttonbox.PackStart (newCollectionButton,
						     false, false, 2);
				buttonbox.PackStart (editGameCollectionButton,
						     false, false, 2);
				buttonbox.
					PackStart (deleteGameCollectionButton,
						   false, false, 2);
				buttonbox.
					PackStart (addGamesToCollectionButton,
						   false, false, 2);

				box.PackStart (buttonbox, false, true, 2);
				book.AppendPage (box,
						 new Label (Catalog.
							    GetString
							    ("Game Collections")));
			}

			private void CreateBaseUI ()
			{
				book = new Notebook ();
				PackStart (book, true, true, 2);
				statusbar = new Statusbar ();
				PackStart (statusbar, false, true, 2);

				CreateAllGamesPage ();
				CreateGameCollectionPage ();
			}
			public GameDbBrowserUI ():base ()
			{
				CreateBaseUI ();
				modifiedGames = new ArrayList ();

				ratingMap = new Hashtable ();
				ratingMap[RATING_AVERAGE] =
					GameRating.Average;
				ratingMap[RATING_GOOD] = GameRating.Good;
				ratingMap[RATING_EXCELLENT] =
					GameRating.Excellent;
				ratingMap[RATING_MUST_HAVE] =
					GameRating.MustHave;

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

				searchGamesList.GameSelectionEvent +=
					OnGameSelectionEvent;
				book.ShowAll ();
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

				GameViewer.Instance.LoadGames (list);
				//GameViewer.Instance.Window.Present ();
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
				CsBoardApp.Instance.Window.GetSize (out width,
								    out
								    height);
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
				CsBoardApp.Instance.Window.GetSize (out width,
								    out
								    height);
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

				GameViewer.Instance.LoadGames (list);
				//GameViewer.Instance.Window.Present ();
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
