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
using System.Text;
using System.Collections;
using Gtk;
using Chess.Parser;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public delegate void GameSelectionEventHandler (ChessGame
								game);

		public class GamesListWidget:VBox
		{
			protected ListStore gamesStore;
			protected IconView view;
			public event GameSelectionEventHandler
				GameSelectionEvent;

			public TreeModel Model
			{
				get
				{
					return gamesStore;
				}
			}

			public IconView View
			{
				get
				{
					return view;
				}
			}

			ScrolledWindow win;
			public ScrolledWindow ScrolledWindow
			{
				get
				{
					return win;
				}
			}

			public GamesListWidget ():base ()
			{
				HBox hbox = new HBox ();
				hbox.PackStart (new
						Label (Catalog.
						       GetString
						       ("Filter")), false,
						false, 4);

				view = CreateIconView ();
				win = new ScrolledWindow ();
				win.HscrollbarPolicy = PolicyType.Automatic;
				win.VscrollbarPolicy = PolicyType.Automatic;
				win.Add (view);
				PackStart (win, true, true, 4);

				ShowAll ();
			}

			public void SetModel (TreeModel model)
			{
				view.Model =
					model == null ? gamesStore : model;
			}

			private IconView CreateIconView ()
			{
				gamesStore =
					new ListStore (typeof (object),
						       typeof (string));
				IconView view = new IconView (gamesStore);
				view.MarkupColumn = 1;
				view.ItemActivated += OnItemActivated;

				view.Clear ();
				CellRendererText renderer =
					new CellRendererText ();
				renderer.Xalign = 0;
				view.PackStart (renderer, false);
				view.SetAttributes (renderer, "markup", 1);
				return view;
			}

			void OnItemActivated (object sender,
					      ItemActivatedArgs a)
			{
				TreeIter iter;
				view.Model.GetIter (out iter, a.Path);
				ChessGame details =
					(ChessGame) view.Model.GetValue (iter,
									 0);
				FireGameSelectionEvent (details);
			}

			protected void FireGameSelectionEvent (ChessGame
							       details)
			{
				if (GameSelectionEvent != null)
					GameSelectionEvent (details);
			}

			public void SetGames (IList games)
			{
				gamesStore.Clear ();
				foreach (ChessGame game in games)
					gamesStore.AppendValues (game,
								 game.
								 ToPango ());
				view.Model = gamesStore;
			}

			public void UpdateGame (PGNChessGame game,
						PGNChessGame replace)
			{
				UpdateGameInModel (game, replace, gamesStore);
			}

			private static bool UpdateGameInModel (PGNChessGame
							       game,
							       PGNChessGame
							       replace,
							       TreeModel
							       model)
			{
				TreeIter iter;
				bool ret;
				for (ret = model.GetIterFirst (out iter); ret;
				     ret = model.IterNext (ref iter))
				  {
					  PGNChessGame g =
						  (PGNChessGame) model.
						  GetValue (iter, 0);
					  if (g.Equals (game))
					    {
						    model.SetValue (iter, 0,
								    replace);
						    return true;
					    }
				  }

				return false;
			}
		}

		public class GameViewerGamesListWidget:GamesListWidget
		{
			GameViewerUI viewer;
			public GameViewerGamesListWidget (GameViewerUI
							  viewer):base ()
			{
				this.viewer = viewer;
				view.ButtonPressEvent += OnButtonPress;
			}

			private void OnButtonPress (object o,
						    ButtonPressEventArgs args)
			{
				if (args.Event.Button != 3)
					return;
				if (viewer.Games == null
				    || viewer.Games.Count == 0)
					return;

				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_View Game"));
				item.Image =
					new Image (Stock.Open, IconSize.Menu);
				TreePath path;
				path = view.GetPathAtPos (System.Convert.
							  ToInt16 (args.Event.
								   X),
							  System.Convert.
							  ToInt16 (args.Event.
								   Y));
				Menu menu = new Menu ();

				if (path != null)
				  {
					  view.SelectPath (path);
					  item.Activated +=
						  OnViewPopupItemActivated;
					  menu.Append (item);
				  }

				if (viewer.PrintHandler != null)
				  {
					  ImageMenuItem printItem =
						  new ImageMenuItem (Catalog.
								     GetString
								     ("_Print Games"));
					  printItem.Image =
						  new Image (Stock.Print,
							     IconSize.Menu);
					  printItem.Activated +=
						  viewer.PrintHandler.
						  OnPrintActivated;
					  menu.Append (printItem);
				  }

				menu.ShowAll ();
				menu.Popup ();
			}

			void OnViewPopupItemActivated (object o,
						       EventArgs args)
			{
				TreePath[]paths = view.SelectedItems;
				if (paths == null || paths.Length != 1)
					return;
				TreeIter iter;
				view.Model.GetIter (out iter, paths[0]);

				ChessGame details =
					(ChessGame) view.Model.GetValue (iter,
									 0);
				FireGameSelectionEvent (details);
			}
		}

		public class SearchableGamesListWidget:VBox
		{
			Entry searchEntry;
			TreeModelFilter filter;
			GameViewerGamesListWidget gamesListWidget;
			public GamesListWidget View
			{
				get
				{
					return gamesListWidget;
				}
			}

			public SearchableGamesListWidget (GameViewerUI viewer)
			{
				HBox box = new HBox ();
				box.PackStart (new
					       Label (Catalog.
						      GetString ("Filter")),
					       false, false, 4);
				searchEntry = new Entry ();
				box.PackStart (searchEntry, true, true, 2);

				PackStart (box, false, true, 2);

				gamesListWidget =
					new
					GameViewerGamesListWidget (viewer);

				PackStart (gamesListWidget, true, true, 2);
				ShowAll ();

				filter = new TreeModelFilter (gamesListWidget.
							      Model, null);
				filter.VisibleFunc = SearchFilterFunc;
				searchEntry.Changed += OnSearch;

			}

			protected bool SearchFilterFunc (TreeModel model,
							 TreeIter iter)
			{
				string search = searchEntry.Text.Trim ();
				if (search.Length == 0)
					return true;
				search = search.ToLower ();

				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);

				string str;
				if ((str =
				     game.GetTagValue ("White", null)) != null
				    && str.ToLower ().IndexOf (search) >= 0)
					return true;
				if ((str =
				     game.GetTagValue ("Black", null)) != null
				    && str.ToLower ().IndexOf (search) >= 0)
					return true;
				if ((str =
				     game.GetTagValue ("Event", null)) != null
				    && str.ToLower ().IndexOf (search) >= 0)
					return true;
				if ((str =
				     game.GetTagValue ("Result",
						       null)) != null
				    && str.ToLower ().Equals (search))
					return true;
				return false;
			}

			protected void OnSearch (object o, EventArgs args)
			{
				string search = searchEntry.Text.Trim ();
				if (search.Length == 0)
				  {
					  gamesListWidget.SetModel (null);
					  return;
				  }
				gamesListWidget.SetModel (filter);
				filter.Refilter ();
			}

			public void SetGames (IList games)
			{
				searchEntry.Text = "";
				gamesListWidget.SetGames (games);
			}
		}
	}
}
