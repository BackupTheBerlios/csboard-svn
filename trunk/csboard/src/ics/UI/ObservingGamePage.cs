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

using CsBoard.Viewer;
using Gtk;
using System;
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ObservingGamePage:VBox
		{
			protected int gameId;
			protected bool needsUnobserve = true;
			public bool NeedsUnobserve
			{
				get
				{
					return needsUnobserve;
				}
			}

			public int GameId
			{
				get
				{
					return gameId;
				}
			}

			public Widget Widget
			{
				get
				{
					return gameWidget;
				}
			}

			protected ChessGameWidget gameWidget;
			protected CairoBoard board;
			protected string white, black;
			protected ICSGameObserverWidget win;
			protected Label resultLabel;
			protected ChessMovesWidget movesWidget;

			protected MoveDetails lastMove;
			protected Button firstButton, prevButton, nextButton,
				lastButton;

			protected VBox movesBox;
			public static bool IsMyGame (Relation relation)
			{
				return relation ==
					Relation.IamPlayingAndMyMove
					|| relation ==
					Relation.IamPlayingAndMyOppsMove;
			}

			public ObservingGamePage (ICSGameObserverWidget
						  widget,
						  MoveDetails details):base ()
			{
				this.win = widget;
				gameId = details.gameNumber;

				InitGameWidget (details);

				movesWidget = new ChessMovesWidget ();
				movesWidget.CursorChanged += OnCursorChanged;

				gameWidget.WhiteAtBottom =
					!details.blackAtBottom;
				board.side = details.blackAtBottom;
				gameWidget.whiteClock.Configure (details.
								 initial_time
								 * 60,
								 (uint)
								 details.
								 increment);
				gameWidget.blackClock.Configure (details.
								 initial_time
								 * 60,
								 (uint)
								 details.
								 increment);

				white = details.white;
				black = details.black;
				gameWidget.White = white;
				gameWidget.Black = black;

				gameWidget.Show ();
				board.Show ();
				movesWidget.Show ();

				HBox box = new HBox ();
				Button closeButton;
				if (Config.WindowsBuild)
					closeButton =
						new Button (Stock.Close);
				else
				  {
					  closeButton = new Button ("");
					  closeButton.Image =
						  new Image (Stock.Close,
							     IconSize.Menu);
				  }
				resultLabel = new Label ();
				resultLabel.Xalign = 0;
				box.PackStart (resultLabel, true, true, 2);
				box.PackStart (closeButton, false, false, 2);

				PackStart (box, false, true, 2);

				box = new HBox ();
				ScrolledWindow scroll = new ScrolledWindow ();
				scroll.HscrollbarPolicy = PolicyType.Never;
				scroll.VscrollbarPolicy =
					PolicyType.Automatic;
				scroll.Add (movesWidget);

				movesBox = new VBox ();
				movesBox.PackStart (scroll, true, true, 2);
				AddGameNavigationButtons (movesBox);

				box.PackStart (gameWidget, true, true, 2);
				box.PackStart (movesBox, false, true, 2);
				PackStart (box, true, true, 2);

				closeButton.Clicked += OnCloseButtonClicked;

				Update (details);
				ShowAll ();
			}

			private void AddGameNavigationButtons (VBox box)
			{
				firstButton = new Button ();
				firstButton.Clicked += OnClicked;
				firstButton.Image =
					new Image (Stock.GotoFirst,
						   IconSize.Button);
				prevButton = new Button ();
				prevButton.Clicked += OnClicked;
				prevButton.Image =
					new Image (Stock.GoBack,
						   IconSize.Button);
				nextButton = new Button ();
				nextButton.Clicked += OnClicked;
				nextButton.Image =
					new Image (Stock.GoForward,
						   IconSize.Button);
				lastButton = new Button ();
				lastButton.Clicked += OnClicked;
				lastButton.Image =
					new Image (Stock.GotoLast,
						   IconSize.Button);

				HBox hbox = new HBox ();
				hbox.PackStart (firstButton, false, false, 2);
				hbox.PackStart (prevButton, false, false, 2);
				hbox.PackStart (nextButton, false, false, 2);
				hbox.PackStart (lastButton, false, false, 2);

				Alignment align =
					new Alignment (0.5f, 1, 1, 0);
				align.Add (hbox);
				box.PackStart (align, false, true, 2);
			}

			private void OnClicked (object obj, EventArgs args)
			{
				MoveDetails details;
				if (obj.Equals (prevButton))
					details = movesWidget.PrevMove ();
				else if (obj.Equals (nextButton))
					details = movesWidget.NextMove ();
				else if (obj.Equals (firstButton))
					details = movesWidget.FirstMove ();
				else if (obj.Equals (lastButton))
					details = movesWidget.LastMove ();
				else
					details = null;

				if (details == null)
					return;

				SetMoveInfo (board, details);
				board.SetPosition (details.pos);
				board.QueueDraw ();
			}

			public void OnGetMoves (ArrayList moves, int id)
			{
				movesWidget.Prepend (moves);
			}

			protected virtual void InitGameWidget (MoveDetails
							       details)
			{
				board = new CairoViewerBoard (details.pos);
				board.showAnimations =
					App.Session.showAnimations;
				gameWidget = new ChessGameWidget (board);
			}

			public void StopClocks ()
			{
				gameWidget.whiteClock.Stop ();
				gameWidget.blackClock.Stop ();
			}

			private void OnCloseButtonClicked (object o,
							   EventArgs args)
			{
				// stop clocks
				StopClocks ();
				win.Remove (this);
			}

			private void OnCursorChanged (object o,
						      EventArgs args)
			{
				MoveDetails details =
					movesWidget.
					GetMoveDetailsForCursor ();
				if (details == null)
					return;

				SetMoveInfo (board, details);
				board.SetPosition (details.pos);
				board.QueueDraw ();
			}

			public virtual void Update (MoveDetails details)
			{
				lastMove = details;
				UpdateTitleLabelForMove (board, details);
				if (movesWidget.AutoAppend)
				  {
					  SetMoveInfo (board, details);
					  board.SetPosition (details.pos);
					  board.QueueDraw ();
				  }

				int factor =
					details.inMilliseconds ? 1 : 1000;
				gameWidget.whiteClock.RemainingTime =
					details.whites_remaining_time *
					factor;
				gameWidget.blackClock.RemainingTime =
					details.blacks_remaining_time *
					factor;
				if (details.whiteToMove)
				  {
					  gameWidget.whiteClock.Start ();
					  gameWidget.blackClock.Stop ();
				  }
				else
				  {
					  gameWidget.whiteClock.Stop ();
					  gameWidget.blackClock.Start ();
				  }
				movesWidget.Add (details);
			}

			public void Update (GameInfo info)
			{
				if (info.whitesRating > 0)
					gameWidget.White =
						String.Format ("{0} ({1})",
							       white,
							       info.
							       whitesRating);
				if (info.blacksRating > 0)
					gameWidget.Black =
						String.Format ("{0} ({1})",
							       black,
							       info.
							       blacksRating);
			}

			private static string GetColorForResult (string res)
			{
				if (res.Equals ("1/2-1/2"))
					return "#2A2081";
				if (res.Equals ("*"))
					return "#027202";
				return "#800000";
			}

			public void Update (ResultNotification notification)
			{
				string color =
					GetColorForResult (notification.
							   result.Trim ());
				resultLabel.Markup =
					String.
					Format
					("<span color=\"{0}\"><big><b>{1}: {2}</b></big></span>",
					 color, notification.result.Trim (),
					 notification.reason);
				needsUnobserve = false;
				gameWidget.whiteClock.Stop ();
				gameWidget.blackClock.Stop ();
				board.Sensitive = false;
			}

			protected void UpdateTitleLabelForMove (CairoBoard
								board,
								MoveDetails
								details)
			{
				string notation = details.verbose_notation;
				if (notation.Equals ("none"))
					return;
				if (details.WhiteMoved)
					resultLabel.Markup =
						String.
						Format ("<b>{0}. {1}</b>",
							details.movenumber,
							details.
							pretty_notation);
				else
					resultLabel.Markup =
						String.
						Format ("<b>{0}... {1}</b>",
							details.movenumber -
							1,
							details.
							pretty_notation);
			}

			protected void SetMoveInfo (CairoBoard
						    board,
						    MoveDetails details)
			{
				string notation = details.verbose_notation;
				if (notation.Equals ("none"))
					return;
				char src_rank, src_file, dst_rank, dst_file;
				if (notation.ToLower ().Equals ("o-o"))
				  {
					  src_file = 'e';
					  // Note: whiteToMove indicates that black made the move!
					  src_rank = dst_rank =
						  details.
						  whiteToMove ? '8' : '1';
					  dst_file = 'g';
				  }
				else if (notation.ToLower ().Equals ("o-o-o"))
				  {
					  src_file = 'e';
					  // Note: whiteToMove indicates that black made the move!
					  src_rank = dst_rank =
						  details.
						  whiteToMove ? '8' : '1';
					  dst_file = 'c';
				  }
				else
				  {
					  int idx = notation.IndexOf ('/');
					  idx++;
					  src_file = notation[idx++];
					  src_rank = notation[idx++];
					  idx++;	// skip extra char
					  dst_file = notation[idx++];
					  dst_rank = notation[idx++];
				  }

				board.SetMoveInfo (src_rank - '1',
						   src_file - 'a',
						   dst_rank - '1',
						   dst_file - 'a');
			}
		}
	}
}
