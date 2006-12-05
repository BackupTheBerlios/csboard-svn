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

namespace CsBoard
{
	namespace Viewer
	{

		public class GamesListWidget:TreeView
		{
			ArrayList games;
			int highlightGameIndex = -1;
			ListStore gamesStore;

			public GamesListWidget ():base ()
			{
				SetupTree ();
				Show ();
			}

			public void SetGames (ArrayList g)
			{
				games = g;
				highlightGameIndex = -1;
				Update ();
			}

			private void Update ()
			{
				gamesStore.Clear ();
				foreach (PGNChessGame game in games)
					gamesStore.AppendValues (game);
			}

			private void SetupTree ()
			{
				gamesStore = new ListStore (typeof (object));
				Model = gamesStore;

				CellRendererText renderer;
				TreeViewColumn col;

				col = new TreeViewColumn ();
				renderer = new CellRendererText ();
				col.Title = "No";
				col.PackStart (renderer, true);
				col.SetCellDataFunc (renderer,
						     new
						     TreeCellDataFunc
						     (GameNoCellDataFunc));
				col.Resizable = false;
				col.Expand = false;
				AppendColumn (col);

				col = new TreeViewColumn ();
				renderer = new CellRendererText ();
				col.Title = "White";
				col.PackStart (renderer, true);
				col.SetCellDataFunc (renderer,
						     new
						     TreeCellDataFunc
						     (WhiteCellDataFunc));
				col.Resizable = false;
				col.Expand = false;
				AppendColumn (col);

				col = new TreeViewColumn ();
				renderer = new CellRendererText ();
				col.Title = "Black";
				col.PackStart (renderer, true);
				col.SetCellDataFunc (renderer,
						     new
						     TreeCellDataFunc
						     (BlackCellDataFunc));
				col.Resizable = false;
				col.Expand = false;
				AppendColumn (col);

				col = new TreeViewColumn ();
				renderer = new CellRendererText ();
				col.Title = "Moves";
				col.PackStart (renderer, true);
				col.SetCellDataFunc (renderer,
						     new
						     TreeCellDataFunc
						     (MovesCellDataFunc));
				col.Resizable = false;
				col.Expand = false;
				AppendColumn (col);

				col = new TreeViewColumn ();
				renderer = new CellRendererText ();
				col.Title = "Result";
				col.PackStart (renderer, true);
				col.SetCellDataFunc (renderer,
						     new
						     TreeCellDataFunc
						     (ResultCellDataFunc));
				col.Resizable = false;
				col.Expand = false;
				AppendColumn (col);
			}

			public void HighlightGame (int idx)
			{
				highlightGameIndex = idx;
			}

			protected void GameNoCellDataFunc (TreeViewColumn
							   column,
							   CellRenderer r,
							   TreeModel model,
							   TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				TreePath path = gamesStore.GetPath (iter);
				renderer.Text = "" + (path.Indices[0] + 1);
			}

			protected void WhiteCellDataFunc (TreeViewColumn
							  column,
							  CellRenderer r,
							  TreeModel model,
							  TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);
				if (game.Tags == null
				    || !game.Tags.Contains ("White"))
					renderer.Text = "[White]";
				else
					renderer.Text =
						(string) game.Tags["White"];
			}

			protected void BlackCellDataFunc (TreeViewColumn
							  column,
							  CellRenderer r,
							  TreeModel model,
							  TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);
				if (game.Tags == null
				    || !game.Tags.Contains ("Black"))
					renderer.Text = "[Black]";
				else
					renderer.Text =
						(string) game.Tags["Black"];
			}

			protected void MovesCellDataFunc (TreeViewColumn
							  column,
							  CellRenderer r,
							  TreeModel model,
							  TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);
				renderer.Text = "" + game.Moves.Count;
			}

			protected void ResultCellDataFunc (TreeViewColumn
							   column,
							   CellRenderer r,
							   TreeModel model,
							   TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessGame game =
					(PGNChessGame) model.GetValue (iter,
								       0);
				if (game.Tags == null
				    || !game.Tags.Contains ("Result"))
					renderer.Text = "";
				else
					renderer.Text =
						(string) game.Tags["Result"];
			}
		}
	}
}
