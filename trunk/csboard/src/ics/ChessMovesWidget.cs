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
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ChessMovesWidget:TreeView
		{
			ListStore store;
			int total_rows;
			int first_move_number;
			int last_row, last_col;
			const int WHITE_MOVE_COL = 1;
			const int BLACK_MOVE_COL = 2;
			const int WHITE_MOVE_DETAILS_COL = 3;
			const int BLACK_MOVE_DETAILS_COL = 4;
			bool blacksMoveNext;
			bool autoAppend;
			public bool AutoAppend {
				get {
					return autoAppend;
				}
			}

			public ChessMovesWidget ():base ()
			{
				first_move_number = -1;
				last_row = last_col = -1;
				blacksMoveNext = false;
				autoAppend = true;
				store = new ListStore (typeof (int),
						       typeof (string),
						       typeof (string),
						       typeof (MoveDetails),
						       typeof (MoveDetails));
				  Model = store;

				  AppendColumn (Catalog.GetString ("No"),
						new CellRendererText (),
						"text", 0);
				  AppendColumn (Catalog.GetString ("White"),
						new CellRendererText (),
						"markup", 1);
				  AppendColumn (Catalog.GetString ("Black"),
						new CellRendererText (),
						"markup", 2);
				  Selection.Mode = SelectionMode.None;

				  CursorChanged += OnCursorChanged;
			}

			private void OnCursorChanged (object o,
						      EventArgs args)
			{
				TreeIter iter;
				int col;
				int row;

				if (!GetIterAndCol
				    (out iter, out row, out col))
				  {
					  return;
				  }

				SetHighlighted (iter, row, col);
				autoAppend = IsLastMove(row, col);
			}

			private void ClearHighlighting ()
			{
				if (last_row < 0 || last_col < 0)
					return;
				int idx;
				MoveDetails details;
				TreePath path =
					new TreePath (new int[]{ last_row });
				TreeIter lastiter;
				store.GetIter (out lastiter, path);
				idx = last_col ==
					WHITE_MOVE_COL ?
					WHITE_MOVE_DETAILS_COL :
					BLACK_MOVE_DETAILS_COL;
				details =
					(MoveDetails) store.
					GetValue (lastiter, idx);
				idx = last_col;
				store.SetValue (lastiter, idx,
						GetMove (details));
				last_col = last_row = -1;
			}

			private bool IsLastMove(int row, int col) {
			  if(row != total_rows - 1)
			    return false;
			  if(blacksMoveNext && col != WHITE_MOVE_COL)
			    return false;
			  if(!blacksMoveNext && col != BLACK_MOVE_COL)
			    return false;

			  return true;
			}

			private void SetHighlighted (TreeIter iter, int row,
						     int col)
			{
				ClearHighlighting ();

				int idx =
					col ==
					WHITE_MOVE_COL ?
					WHITE_MOVE_DETAILS_COL :
					BLACK_MOVE_DETAILS_COL;
				MoveDetails details =
					(MoveDetails) store.GetValue (iter,
								      idx);
				if (details == null)
					return;
				store.SetValue (iter, col,
						GetHighlightedMove (details));
				ScrollToCell (store.GetPath (iter),
					      GetColumn (col), false, 0, 0);
				last_row = row;
				last_col = col;
			}

			public MoveDetails GetMoveDetailsForCursor ()
			{
				TreeIter iter;
				int col;
				int row;

				if (!GetIterAndCol
				    (out iter, out row, out col))
					return null;
				// col 1 for white, col 2 for black
				int idx =
					col ==
					WHITE_MOVE_COL ?
					WHITE_MOVE_DETAILS_COL :
					BLACK_MOVE_DETAILS_COL;
				return (MoveDetails) store.GetValue (iter,
								     idx);
			}

			private bool GetIterAndCol (out TreeIter iter,
						    out int row, out int col)
			{
				iter = TreeIter.Zero;
				row = col = -1;

				TreeViewColumn column;
				TreePath path;
				GetCursor (out path, out column);

				if (path == null || column == null)
					return false;

				if (column.Equals (GetColumn (0)))
					return false;

				row = path.Indices[0];
				store.GetIter (out iter, path);
				col = column.
					Equals (GetColumn (WHITE_MOVE_COL)) ?
					WHITE_MOVE_COL : BLACK_MOVE_COL;
				return true;
			}

			public void Add (MoveDetails details)
			{
				if (details.movenumber < first_move_number)
					first_move_number =
						details.movenumber;

				if (details.WhiteMoved)
				  {
					  WhiteMove (details);
					  return;
				  }

				if (total_rows == 0
				    && details.movenumber == 1)
					return;
				// just update the value. a row would already have been added
				BlackMove (details);
			}

			private string GetMove (MoveDetails details)
			{
				return details.pretty_notation;
			}

			private string GetHighlightedMove (MoveDetails
							   details)
			{
				return String.
					Format
					("<span background=\"#f0f0ff\"><b>{0}</b></span>",
					 details.pretty_notation);
			}

			public void Prepend (ArrayList moves)
			{
				TreeIter iter;
				int end = moves.Count;
				int nrows = (moves.Count + 1) / 2;
				if (store.GetIterFirst (out iter))
				  {
					  int movenumber =
						  (int) store.GetValue (iter,
									0);
					  bool black_move =
						  store.GetValue (iter,
								  WHITE_MOVE_DETAILS_COL)
						  == null;
					  end = (movenumber - 1) * 2;

					  nrows = movenumber - 1;
					  if (black_move)
					    {
						    end++;
					    }

					  // create the required number of rows
					  int j;
					  for (j = 0; j < nrows; j++)
					    {
						    store.Insert (j);
						    total_rows++;
					    }
					  if (last_row >= 0)
						  last_row += j;
				  }

				int row = 0;
				int i = 0;

				foreach (MoveDetails md in moves)
				{
					TreePath path =
						new TreePath (new int[]{ row
							      });
					if (i >= end)
						break;
					if (i % 2 == 0)
					  {
						  store.GetIter (out iter,
								 path);
						  store.SetValue (iter, 0,
								  md.
								  movenumber);
						  store.SetValue (iter,
								  WHITE_MOVE_COL,
								  GetMove
								  (md));
						  store.SetValue (iter,
								  WHITE_MOVE_DETAILS_COL,
								  md);
					  }
					else
					  {
						  store.GetIter (out iter,
								 path);
						  store.SetValue (iter,
								  BLACK_MOVE_COL,
								  GetMove
								  (md));
						  store.SetValue (iter,
								  BLACK_MOVE_DETAILS_COL,
								  md);
						  row++;
					  }
					i++;
				}

				ScrollToCell (new TreePath (new int[]
							    {
							    total_rows - 1}),
					      GetColumn (WHITE_MOVE_COL),
					      false, 0, 0);
			}

			private void WhiteMove (MoveDetails details)
			{
				TreeIter iter;
				TreePath path;
				if (FindMoveRow
				    (details.movenumber, out path, out iter))
				  {
					  store.SetValue (iter,
							  WHITE_MOVE_COL,
							  GetMove (details));
					  store.SetValue (iter,
							  WHITE_MOVE_DETAILS_COL,
							  details);
				  }
				else
				  {
					  iter = store.AppendValues (details.
								     movenumber,
								     GetMove
								     (details),
								     null,
								     details,
								     null);
					  path = new TreePath (new int[]
							       {
							       total_rows});
					  total_rows++;
				  }

				blacksMoveNext = true;
				if(!autoAppend)
					return;
				SetHighlighted (iter, path.Indices[0],
						WHITE_MOVE_COL);

				ScrollToCell (path,
					      GetColumn (WHITE_MOVE_COL),
					      false, 0, 0);
			}

			private void BlackMove (MoveDetails details)
			{
				TreeIter iter;

				if (total_rows == 0)
				  {
					  store.AppendValues (details.
							      movenumber - 1,
							      "*",
							      GetMove
							      (details), null,
							      details);
					  store.GetIterFirst (out iter);
					  total_rows++;
					  blacksMoveNext = false;
					  if(!autoAppend)
					  	return;
					  SetHighlighted (iter, 0,
							  BLACK_MOVE_COL);
					  return;
				  }

				TreePath path;

				if (!FindMoveRow
				    (details.movenumber - 1, out path,
				     out iter))
					return;

				store.SetValue (iter,
						BLACK_MOVE_COL,
						GetMove (details));
				store.SetValue (iter,
						BLACK_MOVE_DETAILS_COL,
						details);
				blacksMoveNext = false;
				if(!autoAppend)
					return;
				SetHighlighted (iter,
						path.
						Indices[0], BLACK_MOVE_COL);
			}

			private bool FindMoveRow (int movenumber,
						  out TreePath path,
						  out TreeIter iter)
			{
				iter = TreeIter.Zero;
				if (total_rows <= 0)
				  {
					  path = null;
					  return false;
				  }

				path = new TreePath (new int[]
						     {
						     total_rows - 1});
				do
				  {
					  store.GetIter (out iter, path);
					  int number =
						  (int) store.GetValue (iter,
									0);
					  if (number == movenumber)
						  return true;
					  if (number < movenumber)
						  return false;
				  }
				while (path.Prev ());

				return false;
			}

			public MoveDetails NextMove ()
			{
				int row, col;
				if (last_row < 0 && last_col < 0)
				  {
					  row = col = 0;
				  }
				else
				  {
					  row = last_row;
					  col = last_col;
					  if (++col > BLACK_MOVE_COL)
					    {
						    row++;
						    col = WHITE_MOVE_COL;
					    }
				  }

				return GetMove (row, col);
			}

			public MoveDetails PrevMove ()
			{
				int row, col;
				row = last_row;
				col = last_col;
				if (--col < WHITE_MOVE_COL)
				  {
					  row--;
					  col = BLACK_MOVE_COL;
				  }

				if (row < 0 || col < 0)
					return null;

				return GetMove (row, col);
			}

			public MoveDetails FirstMove ()
			{
				return GetMove (0, WHITE_MOVE_COL);
			}

			public MoveDetails LastMove ()
			{
				if (total_rows <= 0)
					return null;
				return GetMove (total_rows - 1,
						blacksMoveNext ?
						WHITE_MOVE_COL :
						BLACK_MOVE_COL);
			}

			private MoveDetails GetMove (int row, int col)
			{
				// now check if this is present
				int idx =
					col ==
					WHITE_MOVE_COL ?
					WHITE_MOVE_DETAILS_COL :
					BLACK_MOVE_DETAILS_COL;
				TreePath path =
					new TreePath (new int[]{ row });
				TreeIter iter;
				if (!store.GetIter (out iter, path))
					return null;

				MoveDetails details = store.GetValue (iter,
								      idx) as
					MoveDetails;
				if (details == null)
					return null;

				SetHighlighted (iter, row, col);
				autoAppend = IsLastMove(row, col);
				ScrollToCell (path,
					      GetColumn (col),
					      false, 0, 0);
				return details;
			}
		}
	}
}
