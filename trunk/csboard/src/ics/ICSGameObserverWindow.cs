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
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ICSGameObserverWindow:Window
		{
			int gameId;
			public int GameId
			{
				get
				{
					return gameId;
				}
			}

			ChessGameWidget gameWidget;
			CairoViewerBoard board;

			public ICSGameObserverWindow (MoveDetails
						      details):base ("")
			{
				gameId = details.gameNumber;

				board = new CairoViewerBoard (details.pos);
				gameWidget = new ChessGameWidget (board);
				gameWidget.Show ();
				board.Show ();

				Add (gameWidget);

				Title = String.Format ("{0} vs {1}",
						       details.white,
						       details.black);
				gameWidget.WhiteAtBottom =
					!details.blackAtBottom;
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

				gameWidget.White = details.white;
				gameWidget.Black = details.black;

				Update (details);
			}

			public void Update (MoveDetails details)
			{
				SetMoveInfo (board, details.verbose_notation);
				board.SetPosition (details.pos);
				board.QueueDraw ();

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
			}

			public void Update (ResultNotification notification)
			{
				gameWidget.whiteClock.Stop ();
				gameWidget.blackClock.Stop ();

				MessageDialog md = new MessageDialog (this,
								      DialogFlags.
								      DestroyWithParent,
								      MessageType.
								      Info,
								      ButtonsType.
								      Close,
								      String.
								      Format
								      ("<b>{0}: {1}</b>\n{2}",
								       Catalog.
								       GetString
								       ("Result"),
								       notification.
								       result,
								       notification.
								       reason));

				md.Run ();
				md.Hide ();
				md.Dispose ();
			}

			private static void SetMoveInfo (CairoViewerBoard
							 board,
							 string details)
			{
				if (details.Equals ("none"))
					return;
				int idx = details.IndexOf ('/');
				idx++;
				char src_file = details[idx++];
				char src_rank = details[idx++];
				idx++;
				char dst_file = details[idx++];
				char dst_rank = details[idx++];

				board.SetMoveInfo (src_rank - '1',
						   src_file - 'a',
						   dst_rank - '1',
						   dst_file - 'a');
			}
		}
	}
}
