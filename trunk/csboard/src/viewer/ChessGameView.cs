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

		public class MoveEventArgs:EventArgs
		{
			public int nthMove;
			public MoveEventArgs (int n)
			{
				nthMove = n;
			}
		}

		public delegate void MoveEvent (object o, EventArgs args);
		public delegate void NthMoveEvent (object o,
						   MoveEventArgs args);

		public class ChessGameWidget:VBox
		{
			PGNChessGame game;
			int highlightMoveIndex;
			bool highlightWhite;

			public GameBrowserButtonsWidget browserButtons;

			public event MoveEvent FirstMove, PreviousMove,
				NextMove, LastMove;
			public event NthMoveEvent NthMove;

			HTML html;
			public ChessGameWidget ():base ()
			{
				highlightMoveIndex = -1;

				html = new HTML ();
				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Never;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.AddWithViewport (html);

				  PackStart (win, true, true, 0);

				  browserButtons =
					new GameBrowserButtonsWidget ();
				  browserButtons.firstButton.Clicked +=
					OnFirstClicked;
				  browserButtons.prevButton.Clicked +=
					OnPreviousClicked;
				  browserButtons.nextButton.Clicked +=
					OnNextClicked;
				  browserButtons.lastButton.Clicked +=
					OnLastClicked;
				Alignment alignment =
					new Alignment (0.5f, 1, 0, 0);
				  alignment.Add (browserButtons);
				  alignment.Show ();
				  PackStart (alignment, false, false, 2);
				  ShowAll ();
				  html.WidthRequest = 200;
			}

			private void OnFirstClicked (object o, EventArgs args)
			{
				if (FirstMove != null)
					FirstMove (o, args);
			}

			private void OnPreviousClicked (object o,
							EventArgs args)
			{
				if (PreviousMove != null)
					PreviousMove (o, args);
			}

			private void OnNextClicked (object o, EventArgs args)
			{
				if (NextMove != null)
					NextMove (o, args);
			}

			private void OnLastClicked (object o, EventArgs args)
			{
				if (LastMove != null)
					LastMove (o, args);
			}

			public void SetGame (PGNChessGame g)
			{
				highlightMoveIndex = -1;
				game = g;
				UpdateGameDetails ();
			}

			public void HighlightMove (int moveIdx, bool white)
			{
				highlightWhite = white;
				highlightMoveIndex = moveIdx;
				if (moveIdx < 0) {
					return;
				}
			}

			private void UpdateGameDetails ()
			{
				int i = 0;
				int moveno = 1;
				HTMLStream stream = html.Begin ();
				stream.Write ("<HTML><HEAD></HEAD><BODY>");
				PrintTitle (stream);
				if (game == null) {
					stream.Write ("</BODY></HTML>");
					html.End (stream,
						  HTMLStreamStatus.Ok);
					return;
				}
				foreach (PGNChessMove move in game.Moves) {
					if (i % 2 == 0)
						stream.Write (String.
							      Format
							      ("<b>{0}. </b>",
							       moveno++));
					stream.Write (String.
						      Format ("<b>{0} </b>",
							      move.move));
					if (move.comment != null) {
						stream.Write ("<BR>");
						stream.Write (move.comment);	// TODO: format the markup
						stream.Write ("<BR>");
					}
					i++;
				}

				stream.Write ("</BODY></HTML>");
				html.End (stream, HTMLStreamStatus.Ok);
			}

			private void PrintTitle (HTMLStream stream)
			{
				if (game == null)
					return;
				stream.Write (String.
					      Format ("<H3>{0} vs {1}</H3>",
						      game.White,
						      game.Black));
				stream.Write
					("<TABLE BORDER=0 CELLSPACING=4>");
				string format =
					"<TR><TD><B>{0}</B></TD><TD>{1}</TD></TR>";
				string eco;
				GameViewer.GetOpeningName (game.
							   GetTagValue ("ECO",
									""),
							   out eco);

				stream.Write (String.Format (format,
							     Catalog.
							     GetString
							     ("Result"),
							     game.Result));
				stream.Write (String.
					      Format (format,
						      Catalog.
						      GetString ("Date"),
						      game.Date));
				stream.Write (String.
					      Format (format,
						      Catalog.
						      GetString ("Event"),
						      game.Event));
				stream.Write (String.
					      Format (format,
						      Catalog.
						      GetString ("Site"),
						      game.Site));
				stream.Write (String.
					      Format (format,
						      Catalog.
						      GetString ("Opening"),
						      eco));
				stream.Write ("</TABLE><BR>");
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

		public class GameBrowserButtonsWidget:HBox
		{
			public Button firstButton, prevButton, nextButton,
				lastButton;
			public GameBrowserButtonsWidget ():base ()
			{
				firstButton = new Button ();
				firstButton.Image =
					new Image (Stock.GotoFirst,
						   IconSize.Button);
				prevButton = new Button ();
				prevButton.Image =
					new Image (Stock.GoBack,
						   IconSize.Button);
				nextButton = new Button ();
				nextButton.Image =
					new Image (Stock.GoForward,
						   IconSize.Button);
				lastButton = new Button ();
				lastButton.Image =
					new Image (Stock.GotoLast,
						   IconSize.Button);

				PackStart (firstButton, false, false, 1);
				PackStart (prevButton, false, false, 1);
				PackStart (nextButton, false, false, 1);
				PackStart (lastButton, false, false, 1);
			}
		}

	}
}
