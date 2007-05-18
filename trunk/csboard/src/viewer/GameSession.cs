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
using Chess.Game;
using Chess.Parser;

namespace CsBoard
{
	namespace Viewer
	{

		public class GameSession
		{
			ChessGame game;
			public ChessGame Game
			{
				get
				{
					return game;
				}
			}

			public ChessGamePlayer player;
			int idx;
			int total_moves;	// including whites and blacks

			PGNChessMove move;
			bool hasNext;

			public int CurrentMoveIdx
			{
				get
				{
					return idx;
				}
			}

			public void Set (ChessGame g)
			{
				idx = -1;
				move = null;
				total_moves = 0;

				player = g.HasTag ("FEN") ? ChessGamePlayer.
					CreateFromFEN (g.
						       GetTagValue ("FEN",
								    null)) :
					ChessGamePlayer.CreatePlayer ();

				game = g;

				int n = game.Moves.Count;
				if (n > 0)
				  {
					  total_moves = n;
				  }

				if (total_moves == 0)
					hasNext = false;
				else
					hasNext = true;
			}

			public bool PlayNMoves (int n)
			{
				Reset ();	// reset session
				for (int i = 0; i < n; i++)
				  {
					  Next ();
					  if (!player.Move (CurrentMove))
						  return false;
				  }
				return true;
			}

			public bool PlayTillTheEnd ()
			{
				while (HasNext ())
				  {
					  Next ();
					  if (!player.Move (CurrentMove))
						  return false;
				  }
				return true;
			}

			public void Reset ()
			{
				Set (game);
			}

			public bool HasNext ()
			{
				return hasNext;
			}

			public void Next ()
			{
				idx++;
				move = (PGNChessMove) game.Moves[idx];
				if (idx == total_moves - 1)	// we have reached the last move. no more moves
					hasNext = false;
			}

			public void Prev ()
			{
				if (idx == 0)
					return;
				idx--;
			}

			public int CurrentMoveNumber
			{
				get
				{
					if (idx < 0)
						return -1;
					return (idx / 2) + 1;
				}
			}

			public bool IsWhitesTurn
			{
				get
				{
					if (idx < 0)
						return true;

					return idx % 2 == 0;
				}
			}

			public string CurrentMove
			{
				get
				{
					if (idx < 0)
						return null;

					return move.Move;
				}
			}

			public PGNChessMove CurrentPGNMove
			{
				get
				{
					return move;
				}
			}

			public string CurrentComment
			{
				get
				{
					if (idx < 0)
						return game.Comment;

					return move.comment;
				}

				set
				{
					if (idx < 0)
					  {
						  game.Comment = value;
						  return;
					  }
					move.comment = value;
					game.Moves[idx] = move;
				}
			}
		}
	}
}
