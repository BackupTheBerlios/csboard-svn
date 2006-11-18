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
using System.Text;

namespace Chess
{
	namespace Game
	{
		public abstract class ChessPiece
		{
			protected ChessSide myside, oppside;
			protected int rank;
			protected int file;
			protected ColorType color;
			protected PieceType type;

			protected ChessPiece (PieceType type, ColorType color,
					      int rank, int file,
					      ChessSide myside,
					      ChessSide oppside)
			{
				this.type = type;
				this.myside = myside;
				this.oppside = oppside;
				this.color = color;
				this.rank = rank;
				this.file = file;
			}

			public bool movePiece (int i, int j)
			{
				setPosition (i, j);
				return true;
			}

			public virtual void setPosition (int rank, int file)
			{
				this.rank = rank;
				this.file = file;
			}

			public int Rank
			{
				get
				{
					return rank;
				}
			}

			public int File
			{
				get
				{
					return file;
				}
			}

			public PieceType Type
			{
				get
				{
					return type;
				}
			}

			public override string ToString ()
			{
				string file_str = "abcdefgh";
				return (type ==
					PieceType.
					PAWN ? "p" : getNotationPrefix ()) +
					"@" + file_str[(int) file] + (rank +
								      1);
			}

			public void removeFromSide ()
			{
				myside.removePiece (this);
			}

			public bool addToSide ()
			{
				return myside.addPiece (this);
			}

			public abstract int getPoints ();

			public ColorType Color
			{
				get
				{
					return color;
				}
			}

			public virtual bool isValidMove (int i, int j,
							 ChessPiece[,]
							 positions, int flags)
			{
				if (rank == i && file == j)
				  {
					  return false;
				  }

				if (positions[i, j] != null
				    && positions[i, j].Color == color)
				  {
					  debug (flags,
						 "\t\tAnother piece of the same color present at the destination ("
						 + i + ", " + j + ")");
					  return false;
				  }

				if ((flags & ChessBoardConstants.
				     MOVE_DONT_CHECK_KINGS_EXPOSURE) == 0)
				  {
					  // This will serve both the cases of King under check, or
					  // king under pin
					  IList attackers = new ArrayList ();
					  int myking_rank, myking_file;

					  myking_rank =
						  this !=
						  myside.King ? myside.King.
						  Rank : i;
					  myking_file =
						  this !=
						  myside.King ? myside.King.
						  File : j;

					  // keep myself at the destination and see if my king will be under attack
					  ChessPiece dest_orig =
						  positions[i, j];
					  positions[rank, file] = null;
					  positions[i, j] = this;
					  getAttackers (myside, oppside, myking_rank, myking_file, positions, dest_orig,	/* this will be ignored */
							attackers);
					  positions[rank, file] = this;	// restore
					  positions[i, j] = dest_orig;

					  if (attackers.Count > 0)
					    {
						    return false;
					    }
				  }

				return true;
			}

			/* This returns the attackers of (myking_rank,myking_file) square in the position
			 * described by positions. 'ignore' will be ignored.
			 */
			public static void getAttackers (ChessSide myside,
							 ChessSide oppside,
							 int myking_rank,
							 int myking_file,
							 ChessPiece[,]
							 positions,
							 ChessPiece ignore,
							 IList attackers)
			{
				ArrayList enemy_pieces = new ArrayList ();
				enemy_pieces.AddRange (oppside.Queens);
				enemy_pieces.AddRange (oppside.Rooks);
				enemy_pieces.AddRange (oppside.Bishops);
				enemy_pieces.AddRange (oppside.Knights);
				enemy_pieces.AddRange (oppside.Pawns);

				foreach (ChessPiece cp in enemy_pieces)
				{
					if (ignore != null
					    && cp.Equals (ignore))
						continue;
					if (cp.
					    isValidMove (myking_rank,
							 myking_file,
							 positions,
							 ChessBoardConstants.
							 MOVE_DONT_CHECK_KINGS_EXPOSURE
							 |
							 ChessBoardConstants.
							 MOVE_EXCHANGE))
					  {
						  attackers.Add (cp);
					  }
				}
			}

			public virtual string getNotation (ChessSide side,
							   ChessPiece[,]
							   positions, int sr,
							   int sf, int dr,
							   int df,
							   PromotionType
							   promotion_type)
			{
				int count = 0;
				IList cands = getNotationCandidates ();

				if (cands != null && cands.Count > 1)
				  {
					  foreach (ChessPiece cand in cands)
					  {
						  if (cand.
						      isValidMove (dr, df,
								   positions,
								   ChessBoardConstants.
								   MOVE_EXCHANGE))
							  count++;
					  }
				  }

				string str = "";
				string dest_square =
					"" + (char) ('a' + df) + (dr + 1);
				if (positions[dr, df] != null)
					dest_square = 'x' + dest_square;
				if (count <= 1)
				  {
					  str = getNotationPrefix () +
						  dest_square;
				  }
				else if (count == 2)
				  {
					  str = getNotationPrefix ();
					  if (((ChessPiece) cands[0]).File == ((ChessPiece) cands[1]).File)	// Both on the same file
						  str += (sr + 1);
					  else
						  str += (char) ('a' + sf);
					  str += dest_square;
				  }
				else
				  {
					  // more than two candidates
					  str = getNotationPrefix ();
					  str = str + (char) ('a' + sf) +
						  (sr + 1) + dest_square;
				  }

				switch (promotion_type)
				  {
				  case PromotionType.QUEEN:
					  str += "=Q";
					  break;
				  case PromotionType.ROOK:
					  str += "=R";
					  break;
				  case PromotionType.BISHOP:
					  str += "=B";
					  break;
				  case PromotionType.KNIGHT:
					  str += "=N";
					  break;
				  }

				return str;
			}

			protected static string
				getPromotionString (PromotionType type)
			{
				switch (type)
				  {
				  case PromotionType.QUEEN:
					  return "=Q";
				  case PromotionType.ROOK:
					  return "=R";
				  case PromotionType.BISHOP:
					  return "=B";
				  case PromotionType.KNIGHT:
					  return "=N";
				  }

				return "";
			}

			public bool isAttackingKing (ChessPiece[,] positions)
			{
				bool res =
					isValidMove (oppside.King.Rank,
						     oppside.King.File,
						     positions,
						     ChessBoardConstants.
						     MOVE_EXCHANGE);
				return res;
			}

			public King EnemyKing
			{
				get
				{
					return oppside.King;
				}
			}

			public abstract string getNotationPrefix ();

			protected abstract IList getNotationCandidates ();

			protected void debug (int flags, string str)
			{
				if ((flags & ChessBoardConstants.
				     MOVE_DEBUG) != 0)
					Console.WriteLine (str);
			}

			public override bool Equals (object o)
			{
				ChessPiece cp = (ChessPiece) o;
				if (cp.type == type && cp.rank == rank
				    && cp.file == file && cp.color == color)
					return true;
				return false;
			}

			public override int GetHashCode ()
			{
				return ((int) type << 24) | ((int) color <<
							     16) | (rank << 8)
					| file;
			}
		}
	}
}
