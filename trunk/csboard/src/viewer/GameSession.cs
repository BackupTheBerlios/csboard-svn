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
			public PGNChessGame game;
			public ChessGamePlayer player;
			int idx;
			int total_moves;	// including whites and blacks

			ChessMove move;
			bool hasNext;

			public int CurrentMoveIdx
			{
				get
				{
					return idx;
				}
			}

			public void Set (PGNChessGame g)
			{
				idx = -1;
				move = null;
				total_moves = 0;

				player = ChessGamePlayer.CreatePlayer ();

				game = g;

				int n = game.Moves.Count;
				if (n > 0)
				  {
					  total_moves = (n - 1) * 2;
					  // now see the last move
					  ChessMove lastmove =
						  (ChessMove) game.Moves[n -
									 1];
					  if (lastmove.blackmove == null)
						  total_moves += 1;
					  else
						    total_moves += 2;
				  }

				if (total_moves == 0)
					hasNext = false;
				else
					  hasNext = true;
			}

			public void PlayNMoves (int n)
			{
				Reset ();	// reset session
				for (int i = 0; i < n; i++)
				  {
					  Next ();
					  player.Move (CurrentMove);
				  }
			}

			public void PlayTillTheEnd ()
			{
				while (HasNext ())
				  {
					  Next ();
					  player.Move (CurrentMove);
				  }
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
				move = (ChessMove) game.Moves[idx / 2];
				if (idx == total_moves - 1)	// we have reached the last move. no more moves
					hasNext = false;
			}

			public void Prev ()
			{
				if (idx == 0)
					return;
				idx--;
			}

			public int CurrentMoveIndex
			{
				get
				{
					if (idx < 0)
						return -1;
					return idx / 2;
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

					int j = idx % 2;
					if (j == 0)
						return move.whitemove;

					  return move.blackmove;
				}
			}

			public string CurrentComment
			{
				get
				{
					if (idx < 0)
						return null;

					int j = idx % 2;
					if (j == 0)
						return move.whiteComment;

					  return move.blackComment;
				}
			}
		}
	}
}
