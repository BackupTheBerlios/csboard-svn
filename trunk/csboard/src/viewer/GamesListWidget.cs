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
using Gtk;
using Chess.Parser;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public class GamesListWidget:VBox
		{
			TreeView tree;
			Entry searchEntry;
			ArrayList games;
			ListStore gamesStore;
			TreeModelFilter filter;

			public TreeView Tree
			{
				get
				{
					return tree;
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
				  searchEntry = new Entry ();
				  hbox.PackStart (searchEntry, true, true, 4);
				  tree = new TreeView ();
				  PackStart (hbox, false, true, 0);

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Automatic;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Add (tree);
				  PackStart (win, true, true, 4);

				  SetupTree ();
				  ShowAll ();
				  searchEntry.Activated += OnSearch;
			}

			public void SetGames (ArrayList g)
			{
				games = g;
				Update ();
			}

			private void Update ()
			{
				gamesStore.Clear ();
				foreach (PGNChessGame game in games)
					gamesStore.AppendValues (game);
			}

			CellRendererText info_renderer, idx_renderer;
			private void SetupTree ()
			{
				gamesStore = new ListStore (typeof (object));
				tree.Model = gamesStore;
				tree.HeadersVisible = false;
				filter = new TreeModelFilter (gamesStore,
							      null);
				filter.VisibleFunc = SearchFilterFunc;

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
				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);
				string markup =
					String.Format (Catalog.
						       GetString
						       ("<b>{0} vs {1}</b>\n")
						       +
						       Catalog.
						       GetString
						       ("<small><i>Result</i>: <b>{2}</b> ({3} moves)</small>"),
						       game.White,
						       game.Black,
						       game.Result,
						       (game.Moves.Count + 1) / 2);	// adding +1 will round it properly
				string eventvalue =
					game.GetTagValue ("Event", null);
				if (eventvalue != null) {
					markup +=
						String.
						Format
						(Catalog.
						 GetString
						 ("\n<small><i>Event</i>: {0}, <i>Date</i>: {1}</small>"),
						 eventvalue,
						 game.GetTagValue ("Date",
								   "?"));
				}
				renderer.Markup = markup;
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
				if (search.Length == 0) {
					tree.Model = gamesStore;
					return;
				}
				tree.Model = filter;
				filter.Refilter ();
			}
		}
	}
}
