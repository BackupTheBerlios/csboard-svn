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
using Gtk;
using Chess.Parser;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public class ChessGameWidget:VBox
		{
			PGNChessGame game;
			int highlightMoveIndex;
			bool highlightWhite;
			ChessGameInfoWidget gameInfoWidget;
			TreeView gameView;
			ListStore moveStore;
			TreeViewColumn whitecol, blackcol;

			public ChessGameWidget ():base ()
			{
				highlightMoveIndex = -1;

				gameInfoWidget = new ChessGameInfoWidget ();
				gameView = new TreeView ();
				moveStore =
					new ListStore (typeof (object),
						       typeof (object));
				gameView.Model = moveStore;
				SetupMovesTree ();
				gameInfoWidget.Show ();
				gameView.Show ();
				Show ();

				PackStart (gameInfoWidget, false, false, 0);
				ScrolledWindow win = new ScrolledWindow ();
				  win.Child = gameView;
				  win.HscrollbarPolicy = PolicyType.Never;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Show ();
				  PackStart (win, true, true, 0);
			}

			public void SetGame (PGNChessGame g)
			{
				highlightMoveIndex = -1;
				game = g;
				gameInfoWidget.SetGame (game);
				UpdateGameDetails ();
			}

			public void HighlightMove (int moveIdx, bool white)
			{
				highlightWhite = white;
				highlightMoveIndex = moveIdx;
				if (moveIdx < 0)
				{
					return;
				}
				TreeIter iter;
				if (!moveStore.
				    IterNthChild (out iter, moveIdx))
					return;
				TreePath path = moveStore.GetPath (iter);
				moveStore.EmitRowChanged (path, iter);
				gameView.SetCursor (path,
						    white ? whitecol :
						    blackcol, false);
			}

			private void UpdateGameDetails ()
			{
				moveStore.Clear ();
				PGNChessMove white = null;
				foreach (PGNChessMove move in game.Moves) {
					if (white == null) {
						white = move;
						continue;
					}
					moveStore.AppendValues (white, move);
					white = null;
				}
				if (white != null)	// no black move
					moveStore.AppendValues (white, null);
			}

			private void SetupMovesTree ()
			{
				TreeViewColumn col = new TreeViewColumn ();
				CellRendererText moveno_renderer =
					new CellRendererText ();
				CellRendererText whitemove_renderer =
					new CellRendererText ();
				CellRendererText blackmove_renderer =
					new CellRendererText ();
				moveno_renderer.Xalign = 1;
				col.PackStart (moveno_renderer, false);
				col.SetCellDataFunc (moveno_renderer,
						     new
						     TreeCellDataFunc
						     (MoveNumCellDataFunc));
				col.Title = Catalog.GetString ("No");
				gameView.AppendColumn (col);

				col = new TreeViewColumn ();
				col.PackStart (whitemove_renderer, true);
				col.SetCellDataFunc (whitemove_renderer,
						     new
						     TreeCellDataFunc
						     (WhiteMoveCellDataFunc));
				col.Title = Catalog.GetString ("White");
				col.Resizable = true;
				col.Expand = true;
				gameView.AppendColumn (col);
				whitecol = col;
				whitecol.Spacing = 5;

				col = new TreeViewColumn ();
				col.PackStart (blackmove_renderer, false);
				col.SetCellDataFunc (blackmove_renderer,
						     new
						     TreeCellDataFunc
						     (BlackMoveCellDataFunc));
				col.Expand = true;
				col.Title = Catalog.GetString ("Black");
				gameView.AppendColumn (col);
				blackcol = col;

				gameView.HeadersVisible = true;
			}

			protected void MoveNumCellDataFunc (TreeViewColumn
							    column,
							    CellRenderer r,
							    TreeModel model,
							    TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				renderer.Text =
					"" +
					(model.GetPath (iter).Indices[0] + 1);
			}

			protected void WhiteMoveCellDataFunc (TreeViewColumn
							      column,
							      CellRenderer r,
							      TreeModel model,
							      TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessMove move =
					(PGNChessMove) model.GetValue (iter,
								       0);
				int idx = model.GetPath (iter).Indices[0];
				if (highlightWhite
				    && (idx == highlightMoveIndex))
					renderer.Underline =
						Pango.Underline.Single;
				else
					renderer.Underline =
						Pango.Underline.None;

				renderer.Text =
					move.move == null ? "" : move.move;
			}

			protected void BlackMoveCellDataFunc (TreeViewColumn
							      column,
							      CellRenderer r,
							      TreeModel model,
							      TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				PGNChessMove move =
					(PGNChessMove) model.GetValue (iter,
								       1);
				if (move == null) {
					renderer.Text = "";
					return;
				}
				int idx = model.GetPath (iter).Indices[0];
				if (!highlightWhite
				    && (idx == highlightMoveIndex))
					renderer.Underline =
						Pango.Underline.Single;
				else
					renderer.Underline =
						Pango.Underline.None;

				renderer.Text = move.move ==
					null ? "" : move.move;
			}
		}


	}
}
