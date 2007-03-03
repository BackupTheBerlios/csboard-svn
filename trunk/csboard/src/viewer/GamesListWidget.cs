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

		public class GamesList
		{
			public TreeView tree;
			protected ListStore gamesStore;

			public GamesList (TreeView t)
			{
				tree = t;
				SetupTree ();
			}

			public virtual void Update (IList games)
			{
				gamesStore.Clear ();
				foreach (PGNChessGame game in games)
					gamesStore.AppendValues (game);
				tree.Model = gamesStore;
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

			CellRendererText info_renderer, idx_renderer;
			private void SetupTree ()
			{
				gamesStore = new ListStore (typeof (object));
				tree.Model = gamesStore;
				tree.HeadersVisible = false;

				TreeViewColumn col;

				col = new TreeViewColumn ();
				idx_renderer = new CellRendererText ();
				idx_renderer.Yalign = 0;
				info_renderer = new CellRendererText ();
				col.Title = Catalog.GetString ("Games");
				col.PackStart (idx_renderer, false);
				col.SetCellDataFunc (idx_renderer,
						     new
						     TreeCellDataFunc
						     (IdxCellDataFunc));
				col.PackStart (info_renderer, true);
				col.SetCellDataFunc (info_renderer,
						     new
						     TreeCellDataFunc
						     (InfoCellDataFunc));
				col.Resizable = false;
				col.Expand = true;
				tree.AppendColumn (col);
			}

			protected void IdxCellDataFunc (TreeViewColumn col,
							CellRenderer r,
							TreeModel model,
							TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				TreePath path = model.GetPath (iter);
				renderer.Markup =
					"<b>" + (path.Indices[0] + 1) +
					".</b>";
			}

			protected void InfoCellDataFunc (TreeViewColumn col,
							 CellRenderer r,
							 TreeModel model,
							 TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				ChessGame game =
					(ChessGame) model.GetValue (iter,
								    0);
				renderer.Markup = game.ToPango ();
			}
		}

		public class SearchableGamesList:GamesList
		{
			Entry searchEntry;
			TreeModelFilter filter;

			public SearchableGamesList (TreeView t,
						    Entry s):base (t)
			{
				searchEntry = s;
				filter = new TreeModelFilter (gamesStore,
							      null);
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
					  tree.Model = gamesStore;
					  return;
				  }
				tree.Model = filter;
				filter.Refilter ();
			}

			public override void Update (IList games)
			{
				searchEntry.Text = "";
				base.Update (games);
			}
		}

		public class GamesListWidget:VBox
		{
			SearchableGamesList gamesList;

			public TreeView Tree
			{
				get
				{
					return gamesList.tree;
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
				Entry searchEntry = new Entry ();
				hbox.PackStart (searchEntry, true, true, 4);
				TreeView tree = new TreeView ();
				gamesList =
					new SearchableGamesList (tree,
								 searchEntry);

				PackStart (hbox, false, true, 0);

				ScrolledWindow win = new ScrolledWindow ();
				win.HscrollbarPolicy = PolicyType.Automatic;
				win.VscrollbarPolicy = PolicyType.Automatic;
				win.Add (tree);
				PackStart (win, true, true, 4);

				ShowAll ();
			}

			public void UpdateGame (PGNChessGame game,
						PGNChessGame replace)
			{
				gamesList.UpdateGame (game, replace);
			}

			public void SetGames (ArrayList g)
			{
				gamesList.Update (g);
			}
		}
	}
}
