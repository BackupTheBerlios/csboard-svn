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

namespace Chess
{
	namespace Game
	{
		public class Pawn:ChessPiece
		{
			public Pawn (ColorType color, int rank, int file,
				     ChessSide myside,
				     ChessSide oppside):base (PieceType.PAWN,
							      color, rank,
							      file, myside,
							      oppside)
			{
			}

			public override int getPoints ()
			{
				return 1;
			}

			public override string getNotationPrefix ()
			{
				return "";
			}

			protected override IList getNotationCandidates ()
			{
				return null;
			}

			public override string getNotation (ChessSide side,
							    ChessPiece[,]
							    positions, int sr,
							    int sf, int dr,
							    int df,
							    PromotionType
							    promotion_type)
			{
				string str;
				if (sf == df)
				  {
					  str = "" + (char) ('a' + df) + (dr +
									  1);
				  }
				else
				  {
					  str = "" + (char) ('a' + sf);
					  if (positions[dr, df] != null)
						  str += 'x';
					  str += "" + (char) ('a' + df) +
						  (dr + 1);
				  }
				if (dr == 7 || dr == 0)
				  {
					  /* No need to verify for specific colors
					   * only whites can reach 7 and blacks can reach 0
					   */
					  str += ChessPiece.
						  getPromotionString
						  (promotion_type);
				  }
				return str;
			}

			public bool isValidMove (int i, int j,
						 ChessPiece[,] positions)
			{
				return isValidMove (i, j, positions, 0);
			}

			public override bool isValidMove (int i, int j,
							  ChessPiece[,]
							  positions,
							  int flags)
			{
				if((flags & ChessBoardConstants.MOVE_ENPASSANT) == 0) {
					if (!base.isValidMove (i, j, positions, flags
							       | ChessBoardConstants.
							       MOVE_EXCHANGE))
						return false;
				}
				else {
					if (!base.isValidMove (i, j, positions, flags
							       | ChessBoardConstants.
							       MOVE_EXCHANGE | ChessBoardConstants.MOVE_DONT_CHECK_KINGS_EXPOSURE))
						return false;
					// Now check king exposure
					  ChessPiece dest_orig;

					  dest_orig = positions[rank, j];
					  positions[rank, file] = null;
					  positions[i, j] = this;

					  bool king_under_attack = isMyKingUnderAttack(i, j, positions, dest_orig, flags);

					  positions[i, j] = null;
					  positions[rank, file] = this;	// restore
					  positions[rank, j] = dest_orig;

					  if(king_under_attack)
						  return false;
				}

				int diff = i - rank;

				if ((flags &
				     (ChessBoardConstants.
				      MOVE_EXCHANGE | ChessBoardConstants.
				      MOVE_ENPASSANT)) != 0)
				  {
					  // exchange
					  if ((j - file) != 1
					      && (file - j) != 1)
					    {
						    // debug(flags, "\t\t\tNot a valid exchange");
						    return false;
					    }
					  if (diff != -1 && diff != 1)
						  return false;
				  }
				else
				  {
					  // advance
					  if (j != file)
					    {
						    // debug(flags, this + " invalid advance to square: " + new CBSquare(i, j));
						    return false;
					    }
					  else if (positions[i, j] != null)
					    {
						    debug (flags,
							   this +
							   " cannot advance "
							   + positions[i, j]);
						    return false;
					    }
				  }

				if ((color == ColorType.WHITE && diff == 1) ||
				    (color == ColorType.BLACK && diff == -1))
					return true;

				if (color == ColorType.WHITE)
				  {
					  if (diff == 2 && rank == 1
					      && positions[2, file] == null)
						  return true;
				  }
				else
				  {
					  if (diff == -2 && rank == 6
					      && positions[5, file] == null)
						  return true;
				  }

				// debug(flags, "\t\t\tWrong advance i=" + i + ", rank=" + rank + ", diff=" + diff);
				return false;
			}
		}
	}
}
