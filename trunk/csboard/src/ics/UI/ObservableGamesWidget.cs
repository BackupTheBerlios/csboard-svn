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
using Gdk;
using System.Reflection;


namespace CsBoard
{
	namespace ICS
	{
		public class ObservableGamesView:Notebook
		{
			RelayTournamentsView relayTournamentsView;
			ObservableGamesWidget observableGamesWidget;
			public ObservableGamesView (GameObservationManager
						    observer,
						    ICSClient client)
			{
				observableGamesWidget =
					new ObservableGamesWidget (observer);
				relayTournamentsView =
					new RelayTournamentsView (client);
				TabPos = PositionType.Left;
				AppendPage (relayTournamentsView,
					    new Label (Catalog.
						       GetString
						       ("Tournaments")));
				AppendPage (observableGamesWidget,
					    new Label (Catalog.
						       GetString ("Games")));
				ShowAll ();
			}

			public void ShowRelayTournamentsPage ()
			{
				CurrentPage = 0;	// relay tournaments page
			}
		}

		public class ObservableGamesWidget:VBox
		{
			private Gtk.TreeView gamesView;

			TreeStore store;

			Label infoLabel;
			int ngames;

			GameObservationManager obManager;

			static protected Pixbuf ComputerPixbuf =
				Gdk.Pixbuf.LoadFromResource ("computer.png");

			Entry filterEntry;

			TreeModelFilter filter;
			  TreeIter[,] iters;	// [rated,unrated,examined, lightning,blitz,standard,others
			const int LIGHTNING_CATEGORY_IDX = 2;
			const int OTHER_CATEGORY_IDX = 3;

			public ObservableGamesWidget (GameObservationManager
						      observer)
			{
				obManager = observer;
				iters = new TreeIter[3, 4];
				gamesView = new TreeView ();
				infoLabel = new Label ();
				infoLabel.Xalign = 0;
				infoLabel.Xpad = 4;
				observer.ObservableGameEvent +=
					OnObservableGameEvent;

				store = new TreeStore (typeof (string),	// used for filtering
						       typeof (int),	// gameid
						       typeof (string),	// markup
						       typeof (string),	// 
						       typeof (string));

				  gamesView.HeadersVisible = true;
				  gamesView.HeadersClickable = true;

				  gamesView.AppendColumn (Catalog.
							  GetString ("Games"),
							  new
							  CellRendererText (),
							  "markup", 2);
				  gamesView.AppendColumn (Catalog.
							  GetString ("Time"),
							  new
							  CellRendererText (),
							  "markup", 3);
				  gamesView.AppendColumn (Catalog.
							  GetString
							  ("Category"),
							  new
							  CellRendererText (),
							  "markup", 4);

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy =
					win.VscrollbarPolicy =
					PolicyType.Automatic;
				  win.Add (gamesView);

				  UpdateInfoLabel ();

				  filterEntry = new Entry ();
				  filterEntry.Changed += OnFilter;

				  filter = new TreeModelFilter (store, null);
				  filter.VisibleFunc = FilterFunc;
				  gamesView.Model = filter;

				  AddParentIters ();

				  infoLabel.UseMarkup = true;
				Button refreshButton =
					new Button (Stock.Refresh);
				  refreshButton.Clicked +=
					delegate (object o, EventArgs args)
				{
					Clear ();
					obManager.GetGames ();
				};
				Alignment align = new Alignment (0, 1, 0, 0);
				align.Add (refreshButton);

				HBox hbox = new HBox ();
				hbox.PackStart (infoLabel, true, true, 4);
				hbox.PackStart (align, false, false, 4);

				PackStart (hbox, false, true, 4);

				Label tipLabel = new Label ();
				tipLabel.Xalign = 0;
				tipLabel.Xpad = 4;
				tipLabel.Markup =
					String.
					Format ("<small><i>{0}</i></small>",
						Catalog.
						GetString
						("Press the refresh button to get an updated list of games.\nDouble click on a game to observe it."));
				PackStart (tipLabel, false, true, 4);
				PackStart (filterEntry, false, true, 4);
				PackStart (win, true, true, 4);

				gamesView.RowActivated += OnRowActivated;
				SetSizeRequest (600, 400);
				ShowAll ();
			}

			private void AddCategoryIters (int type,
						       TreeIter parent)
			{
				string[]names = new string[]
				{
				Catalog.GetString ("Blitz"),
						Catalog.
						GetString
						("Standard"),
						Catalog.
						GetString
						("Lightning"),
						Catalog.GetString ("Others")};

				for (int i = 0; i < names.Length; i++)
				  {
					  iters[type, i] =
						  store.AppendValues (parent,
								      "", 0,
								      String.
								      Format
								      ("<b><i>{0}</i></b>",
								       names
								       [i]),
								      "", "");
					  if (i < names.Length - 1)
					    {
						    gamesView.
							    ExpandRow (store.
								       GetPath
								       (iters
									[type,
									 i]),
								       false);
					    }
				  }
			}

			private void AddParentIters ()
			{
				TreeIter iter;
				iter = store.AppendValues ("",
							   0,
							   String.
							   Format
							   ("<b>{0}</b>",
							    Catalog.
							    GetString
							    ("Rated Games")),
							   "", "");
				AddCategoryIters (0, iter);
				gamesView.ExpandRow (store.GetPath (iter),
						     false);

				iter = store.AppendValues ("", 0,
							   String.
							   Format
							   ("<b>{0}</b>",
							    Catalog.
							    GetString
							    ("Unrated Games")),
							   "", "");
				AddCategoryIters (1, iter);
				gamesView.ExpandRow (store.GetPath (iter),
						     false);

				iter = store.AppendValues ("", 0,
							   String.
							   Format
							   ("<b>{0}</b>",
							    Catalog.
							    GetString
							    ("Examined Games")),
							   "", "");
				AddCategoryIters (2, iter);
			}

			private void OnFilter (object o, EventArgs args)
			{
				filter.Refilter ();
			}

			protected bool FilterFunc (TreeModel model,
						   TreeIter iter)
			{
				TreePath path = model.GetPath (iter);
				if (path.Depth <= 2)
				  {
					  return true;
				  }
				if (path.Indices[1] != OTHER_CATEGORY_IDX
				    && path.Indices[1] !=
				    LIGHTNING_CATEGORY_IDX)
				  {
					  path.Up ();
					  gamesView.ExpandRow (path, false);
				  }

				string filterstr = filterEntry.Text.Trim ();
				if (filterstr.Length == 0)
					return true;

				string str =
					model.GetValue (iter, 0) as string;

				if (str.IndexOf (filterstr.ToLower ()) >= 0)
					return true;

				return false;
			}

			private void OnRowActivated (object o,
						     RowActivatedArgs args)
			{
				TreeIter iter;
				TreeView tree = o as TreeView;
				tree.Model.GetIter (out iter, args.Path);
				int gameId =
					(int) tree.Model.GetValue (iter, 1);
				if (gameId > 0)
					obManager.ObserveGame (gameId);
			}

			private void UpdateInfoLabel ()
			{
				infoLabel.Markup =
					String.Format ("<b>{0}: {1}</b>",
						       Catalog.
						       GetString
						       ("Number of games"),
						       ngames);
			}

			public void OnObservableGameEvent (object o,
							   GameDetails
							   details)
			{
				if (details.PrivateGame)
				  {
					  return;
				  }
				ngames++;
				string filter_string =
					String.Format ("{0} {1}",
						       details.white,
						       details.black);
				filter_string = filter_string.ToLower ();

				UpdateInfoLabel ();
				TreeIter iter;
				bool needs_expansion;
				GetIter (details, out iter,
					 out needs_expansion);

				TreeIter child = store.PrependNode (iter);
				int i = 0;
				store.SetValue (child, i++, filter_string);
				store.SetValue (child, i++, details.gameId);
				store.SetValue (child, i++,
						details.ToPango ());
				store.SetValue (child, i++,
						details.
						TimeDetailsAsMarkup ());
				store.SetValue (child, i++,
						details.CategoryStr);

				if (needs_expansion)
					gamesView.ExpandRow (store.
							     GetPath (iter),
							     false);
			}

			private void GetIter (GameDetails details,
					      out TreeIter iter,
					      out bool needs_expansion)
			{
				int x = details.Examined ? 2 : details.
					Rated ? 0 : 1;
				int y;
				switch (details.GameCategory)
				  {
				  case GameCategory.Blitz:
					  y = 0;
					  needs_expansion = true;
					  break;
				  case GameCategory.Standard:
					  y = 1;
					  needs_expansion = true;
					  break;
				  case GameCategory.Lightning:
					  y = 2;
					  needs_expansion = false;
					  break;
				  default:
					  y = 3;
					  needs_expansion = false;
					  break;
				  }
				iter = iters[x, y];
			}

			public void Clear ()
			{
				ngames = 0;
				store.Clear ();
				UpdateInfoLabel ();
				filterEntry.Text = "";
				AddParentIters ();
			}
		}
	}
}
