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
			[Glade.Widget] private Gtk.TreeView searchTreeView;
			[Glade.Widget] private Gtk.Entry searchEntry;
			[Glade.Widget] private Gtk.Entry tagSearchEntry;

			[Glade.Widget] private Gtk.ComboBox colorOption;

			[Glade.Widget] private Gtk.ComboBox ratingMatchOption;
			[Glade.Widget] private Gtk.ComboBox ratingChoice;

			[Glade.Widget] private Gtk.Statusbar statusbar;

			private Gtk.ListStore searchStore;

			GamesList searchGamesList;

			Hashtable ratingMap;

			ObjectSet results;

			ArrayList modifiedGames;

			public GameDbBrowser ()
			{
				modifiedGames = new ArrayList ();

				ratingMap = new Hashtable ();
				ratingMap["Average"] = GameRating.Average;
				ratingMap["Good"] = GameRating.Good;
				ratingMap["Excellent"] = GameRating.Excellent;
				ratingMap["Must Have"] = GameRating.MustHave;

				Glade.XML xml =
					Glade.XML.
					FromAssembly ("gamedb.glade",
						      "gameDbWindow", null);
				xml.Autoconnect (this);

				colorOption.Active = 0;
				ratingMatchOption.Active = 0;

				searchTreeView.Selection.Mode =
					SelectionMode.Multiple;
				searchGamesList =
					new GamesList (searchTreeView);

				searchStore =
					new
					ListStore (typeof (PGNGameDetails));

				searchTreeView.Model = searchStore;

				searchEntry.Activated += OnSearch;
				tagSearchEntry.Activated += OnSearch;

				int width, height;
				  GameViewer.Instance.Window.
					GetSize (out width, out height);

				  gameDbWindow.Resize ((int) Math.
						       Round (0.9 * width),
						       (int) Math.Round (0.9 *
									 height));


				  searchTreeView.RowActivated +=
					OnRowActivated;
			}

			public void on_window_delete_event (System.Object b,
							    DeleteEventArgs e)
			{
				gameDbWindow.Hide ();
				gameDbWindow.Dispose ();
			}


			protected void OnSearch (object o, EventArgs args)
			{
				HandleSearch ();
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
			}

			private void OnSearchButtonClicked (object o,
							    EventArgs args)
			{
				HandleSearch ();
			}

			void OnRowActivated (object obj,
					     RowActivatedArgs args)
			{
				LoadSelectedGames ();
			}

			void OnLoadGamesButtonClicked (object obj,
						       EventArgs args)
			{
				LoadSelectedGames ();
			}

			private void LoadSelectedGames ()
			{
				ArrayList list = new ArrayList ();
				GetSelectedGames (list);

				GameViewer.Instance.LoadGames (list);
				GameViewer.Instance.Window.Present ();
			}

			private void GetSelectedGames (ArrayList list)
			{
				TreePath[]selected =
					searchTreeView.Selection.
					GetSelectedRows ();
				if (selected == null || selected.Length == 0)
					return;
				foreach (TreePath path in selected)
				{
					TreeIter iter;
					searchTreeView.Model.
						GetIter (out iter, path);
					PGNGameDetails info =
						(PGNGameDetails)
						searchTreeView.Model.
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
					  string color =
						  colorOption.ActiveText;
					  if (color.Equals ("Any"))
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
					  if (color.Equals ("White"))
					    {
						    Query white =
							    query.
							    Descend ("white");
						    white.Constrain (search).
							    Like ();
					    }
					  else if (color.Equals ("Black"))
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
							    ActiveText];
					  switch (ratingMatchOption.
						  ActiveText)
					    {
					    case "Equals":
						    query.Descend ("rating").
							    Constrain
							    (rating).Equal ();
						    break;
					    case "Above":
						    Query q1 =
							    query.
							    Descend
							    ("rating");
						    q1.Constrain (rating -
								  1).
							    Greater ();
						    q1.OrderDescending ();
						    break;
					    case "Below":
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
