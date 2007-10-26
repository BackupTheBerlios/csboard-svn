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
		/* This widget will show the chess game notation as well as the board
		 */
		public class ChessGameWidget:VBox
		{
			Label nagCommentLabel;
			Label moveNumberLabel;
			public Button firstButton, prevButton, nextButton,
				lastButton;
		  PlayPauseButton playButton;
			VBox chessGameDetailsBox;
			ChessGameView gameView;

			HPaned splitPane;

			ChessGameBoard boardWidget;

			public VBox ChessGameDetailsBox
			{
				get
				{
					return chessGameDetailsBox;
				}
			}
			public ChessGameView NotationView
			{
				get
				{
					return gameView;
				}
			}

			public ChessGameBoard BoardWidget
			{
				get
				{
					return boardWidget;
				}
			}

			public HPaned SplitPane
			{
				get
				{
					return splitPane;
				}
			}

			public ChessGameWidget ():base ()
			{
				gameView = new ChessGameView ();
				gameView.ShowNthMove += OnShowNthMoveEvent;

				boardWidget = new ChessGameBoard ();

				splitPane = new HPaned ();

				splitPane.Add1 (boardWidget);
				splitPane.Add2 (GetRightPane ());

				PackStart (splitPane, true, true, 2);

				ShowAll ();
			}

			public void SetGame (ChessGame game)
			{
				boardWidget.SetGame (game);
				gameView.SetGame (game);

				moveNumberLabel.Text = "";
				nagCommentLabel.Text = "";
			}

			public void Reset ()
			{
				boardWidget.Reset ();
				gameView.SetMoveIndex (boardWidget.Session.
						       CurrentMoveIdx);
				moveNumberLabel.Text = "";
				nagCommentLabel.Text = "";
			}

			public void on_first_clicked (System.Object o,
						      EventArgs e)
			{
				Reset ();
			  playButton.Pause();
			}

			public void on_last_clicked (System.Object o,
						     EventArgs e)
			{
				if (!boardWidget.Session.PlayTillTheEnd ())
					Console.WriteLine
						(Catalog.
						 GetString
						 ("Operation failed"));

				UpdateMoveDetails (false);
			  playButton.Pause();
			}

			public void on_prev_clicked (System.Object o,
						     EventArgs e)
			{
				int currentMoveIdx =
					boardWidget.Session.CurrentMoveIdx;
				if (currentMoveIdx < 0)
					return;
			  playButton.Pause();
				PlayNMoves (currentMoveIdx);	// since we are passing the index, no need for -1
			}

			private void PlayNMoves (int nmoves)
			{
				if (!boardWidget.Session.PlayNMoves (nmoves))
				  {
					  Console.WriteLine
						  (Catalog.
						   GetString
						   ("Failed to play to go back"));
					  // dont return now. let the position be set so that we can see
					  // where it stopped
				  }

				UpdateMoveDetails (false);
			}

			public void on_next_clicked (System.Object o,
						     EventArgs e)
			{
			  playButton.Pause();
				handle_next();
			}

		  private void handle_next() {
				if (!boardWidget.Session.HasNext ())
				  {
					  return;
				  }
				boardWidget.Session.Next ();
				if (!boardWidget.Session.player.
				    Move (boardWidget.Session.CurrentMove))
				  {
					  Console.WriteLine
						  (Catalog.
						   GetString
						   ("Failed to play the move: ")
						   +
						   boardWidget.Session.
						   CurrentMove);
					  return;
				  }
				UpdateMoveDetails (true);
			}

			private void OnShowNthMoveEvent (object o,
							 MoveEventArgs args)
			{
				int idx = args.nthMove;
				PlayNMoves (idx + 1);
			}

			private void UpdateMoveDetails (bool isNext)
			{
				boardWidget.UpdateMoveFromSession (isNext);

				int currentMoveIdx;
				GameSession session = boardWidget.Session;
				currentMoveIdx = session.CurrentMoveIdx;
				gameView.SetMoveIndex (currentMoveIdx);

				if (currentMoveIdx < 0)
				  {
					  moveNumberLabel.Text = "";
					  nagCommentLabel.Text = "";
					  return;
				  }

				string str =
					session.CurrentPGNMove.
					Nags ==
					null ? "" : session.
					CurrentPGNMove.Nags[0].Markup ();
				nagCommentLabel.Markup = str;
				string move_markup =
					String.Format ("<b>{0}{1} {2}</b>",
						       session.
						       CurrentMoveNumber,
						       session.
						       IsWhitesTurn ? "." :
						       "...",
						       session.CurrentMove);
				moveNumberLabel.Markup = move_markup;
			}

			private VBox GetRightPane ()
			{
				VBox vbox = new VBox ();

				// labels
				moveNumberLabel = new Label ();
				nagCommentLabel = new Label ();
				nagCommentLabel.Xalign = 0;
				HBox hbox = new HBox ();
				hbox.PackStart (moveNumberLabel, false, false,
						2);
				hbox.PackStart (nagCommentLabel, false, false,
						2);

				vbox.PackStart (hbox, false, false, 2);

				// board
				chessGameDetailsBox = new VBox ();
				chessGameDetailsBox.PackStart (gameView,
							       true, true, 4);

				vbox.PackStart (chessGameDetailsBox, true,
						true, 2);


				// buttons
				playButton = new PlayPauseButton ();
				playButton.PlayNextEvent += on_play_next_event;

				firstButton = new Button ();
				firstButton.Clicked += on_first_clicked;
				firstButton.Image =
					new Image (Stock.GotoFirst,
						   IconSize.Button);
				prevButton = new Button ();
				prevButton.Clicked += on_prev_clicked;
				prevButton.Image =
					new Image (Stock.GoBack,
						   IconSize.Button);
				nextButton = new Button ();
				nextButton.Clicked += on_next_clicked;
				nextButton.Image =
					new Image (Stock.GoForward,
						   IconSize.Button);
				lastButton = new Button ();
				lastButton.Clicked += on_last_clicked;
				lastButton.Image =
					new Image (Stock.GotoLast,
						   IconSize.Button);

				HBox bbox = new HBox ();
				bbox.PackStart (firstButton, false, false, 1);
				bbox.PackStart (prevButton, false, false, 1);
				bbox.PackStart (playButton, false, false, 1);
				bbox.PackStart (nextButton, false, false, 1);
				bbox.PackStart (lastButton, false, false, 1);
				Alignment alignment =
					new Alignment (0.5f, 1, 0, 0);
				alignment.Add (bbox);
				alignment.Show ();

				vbox.PackStart (alignment, false, false, 2);

				return vbox;
			}

		  private void on_play_next_event(object o, EventArgs args) {
		    handle_next();
		    if (!boardWidget.Session.HasNext ())
		    {
		      playButton.Pause();
		    }
		  }
		}

		  public delegate void PlayNextEventHandler(object o, EventArgs args);
		  class PlayPauseButton : Button {
		    Image playImg, pauseImg;
		    bool playing;
				public event PlayNextEventHandler PlayNextEvent;
		    uint timeout;
		    uint timeoutid;
		    public PlayPauseButton() : base() {
		      playImg = new Image(Stock.MediaPlay, IconSize.Button);
		      pauseImg = new Image(Stock.MediaPause, IconSize.Button);
		      playing = false;
		      Image = playImg;
		      timeout = 1500;

		      Clicked += OnClicked;
		    }

private void OnClicked(object o, EventArgs args) {
  Toggle();
}

public void Toggle() {
  if(playing)
    Pause();
  else
    Play();
}

		    public void Play() {
		      if(playing)
			return;
		      playing = true;
		      Image = pauseImg;

		      if(PlayNextEvent != null)
			PlayNextEvent(this, EventArgs.Empty);

		      timeoutid = GLib.Timeout.Add(timeout, new GLib.TimeoutHandler(on_timeout));
		    }

		    public void Pause() {
		      if(!playing)
			return;
		      playing = false;
		      Image = playImg;
		      if(timeoutid > 0) {
			GLib.Source.Remove(timeoutid);
			timeoutid = 0;
		      }
		    }

		    private bool on_timeout() {
		      if(!playing)
			return false;

		      if(PlayNextEvent != null)
			PlayNextEvent(this, EventArgs.Empty);

		      return true;
		    }
		  }

	}
}
