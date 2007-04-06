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
using Chess.Game;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		/* This widget will show the chess game notation as well as the board
		 */
		public class ChessGameBoard:VBox
		{
			CairoViewerBoard boardWidget;

			Label whiteLabel, blackLabel;
			GameSession gameSession;

			public GameSession Session
			{
				get
				{
					return gameSession;
				}
			}

			public CairoViewerBoard Board
			{
				get
				{
					return boardWidget;
				}
			}

			public ChessGameBoard ():base ()
			{
				boardWidget =
					new CairoViewerBoard (ChessGamePlayer.
							      GetDefaultPosition
							      ());
				gameSession = new GameSession ();
				whiteLabel =
					new Label (GetMarkupForTitle
						   (Catalog.
						    GetString ("White")));
				blackLabel =
					new
					Label (GetMarkupForTitle
					       (Catalog.GetString ("Black")));
				whiteLabel.UseMarkup = true;
				blackLabel.UseMarkup = true;
				blackLabel.Yalign = 1;	// bottom
				whiteLabel.Yalign = 0;	// top
				PackStart (blackLabel, false, false, 2);
				PackStart (boardWidget, true, true, 2);
				PackStart (whiteLabel, false, false, 2);

				ShowAll ();
			}

			public void Reset ()
			{
				boardWidget.Reset ();
				gameSession.Reset ();	// reset session
				boardWidget.lastMove =
					gameSession.CurrentMove;
				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void SetGame (ChessGame game)
			{
				gameSession.Set (game);

				boardWidget.Reset ();
				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
				whiteLabel.Markup =
					GetMarkupForTitle (game.
							   GetTagValue
							   ("White",
							    "White"));
				blackLabel.Markup =
					GetMarkupForTitle (game.
							   GetTagValue
							   ("Black",
							    "Black"));

			}

			public void UpdateMoveFromSession (bool isNext)
			{
				int currentMoveIdx =
					gameSession.CurrentMoveIdx;
				if (currentMoveIdx >= 0)
				  {
					  boardWidget.lastMove =
						  gameSession.CurrentMove;
					  int r1, f1, r2, f2;
					  r1 = gameSession.player.
						  LastMoveInfo.src_rank;
					  f1 = gameSession.player.
						  LastMoveInfo.src_file;
					  r2 = gameSession.player.
						  LastMoveInfo.dest_rank;
					  f2 = gameSession.player.
						  LastMoveInfo.dest_file;
					  boardWidget.Move (r1, f1, r2, f2,
							    ' ');
				  }
				else
				  {
					  boardWidget.Move (0, 0, 0, 0, ' ');
				  }

				// Reload the position
				// For next, the move is enough. but for spl positions like
				// castling and enpassant, the position has to be reloaded
				// for prev and other moves, the position has to be reloaded
				if (!isNext
				    || gameSession.player.LastMoveInfo.
				    special_move)
					boardWidget.SetPosition (gameSession.
								 player.
								 GetPosition
								 ());
				boardWidget.QueueDraw ();
			}

			private static string GetMarkupForTitle (string str)
			{
				return String.
					Format
					("<big><big><big><b>{0}</b></big></big></big>",
					 str);
			}
		}

	}
}
