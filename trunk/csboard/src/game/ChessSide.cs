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
		public class ChessSide
		{
			ColorType color;

			IList rooks;
			public IList Rooks
			{
				get
				{
					return rooks;
				}
			}
			IList knights;
			public IList Knights
			{
				get
				{
					return knights;
				}
			}
			IList bishops;
			public IList Bishops
			{
				get
				{
					return bishops;
				}
			}
			King king;
			public King King
			{
				get
				{
					return king;
				}
			}
			IList queens;
			public IList Queens
			{
				get
				{
					return queens;
				}
			}
			IList pawns;
			public IList Pawns
			{
				get
				{
					return pawns;
				}
			}

			public ChessSide (ColorType color)
			{
				rooks = new ArrayList (2);
				knights = new ArrayList (2);
				bishops = new ArrayList (2);
				pawns = new ArrayList (2);

				queens = new ArrayList (1);

				this.color = color;
			}

			public void removePiece (ChessPiece piece)
			{
				pawns.Remove (piece);
				rooks.Remove (piece);
				knights.Remove (piece);
				bishops.Remove (piece);
				queens.Remove (piece);
			}

			public bool addPiece (ChessPiece piece)
			{
				if (color != piece.Color)
					return false;

				if (piece.Type == PieceType.KING)
				  {
					  king = (King) piece;
					  return true;
				  }
				IList list = getListOfType (piece.Type);
				if (list == null)
					return false;
				if (!list.Contains (piece))
					list.Add (piece);
				return true;
			}
			private IList getListOfType (PieceType type)
			{
				switch (type)
				  {
				  case PieceType.PAWN:
					  return pawns;
				  case PieceType.KNIGHT:
					  return knights;
				  case PieceType.BISHOP:
					  return bishops;
				  case PieceType.ROOK:
					  return rooks;
				  case PieceType.QUEEN:
					  return queens;
				  }

				return null;
			}

			public IList getPiecesOfType (PieceType type)
			{
				ArrayList list = new ArrayList ();
				if (type == PieceType.KING)
					list.Add (king);
				else
					list.AddRange (getListOfType (type));
				return list;
			}

			public IList allPieces ()
			{
				ArrayList pieces = new ArrayList ();
				pieces.Add (king);
				pieces.AddRange (pawns);
				pieces.AddRange (knights);
				pieces.AddRange (bishops);
				pieces.AddRange (rooks);
				pieces.AddRange (queens);
				return pieces;
			}

			private static void FillDefaultPieces (ColorType
							       color,
							       ChessSide side,
							       ChessSide opp)
			{
				/* Add pawns */
				int rank;
				rank = color ==
					ColorType.WHITE ? ChessBoardConstants.
					RANK_2 : ChessBoardConstants.RANK_7;
				for (int file = ChessBoardConstants.FILE_A;
				     file <= ChessBoardConstants.FILE_H;
				     file++)
				  {
					  side.addPiece (new
							 Pawn (color, rank,
							       file, side,
							       opp));
				  }

				rank = color ==
					ColorType.WHITE ? ChessBoardConstants.
					RANK_1 : ChessBoardConstants.RANK_8;
				side.addPiece (new
					       Rook (color, rank,
						     ChessBoardConstants.
						     FILE_A, side, opp));
				side.addPiece (new
					       Knight (color, rank,
						       ChessBoardConstants.
						       FILE_B, side, opp));
				side.addPiece (new
					       Bishop (color, rank,
						       ChessBoardConstants.
						       FILE_C, side, opp));
				side.addPiece (new
					       Queen (color, rank,
						      ChessBoardConstants.
						      FILE_D, side, opp));
				side.addPiece (new
					       King (color, rank,
						     ChessBoardConstants.
						     FILE_E, side, opp));
				side.addPiece (new
					       Bishop (color, rank,
						       ChessBoardConstants.
						       FILE_F, side, opp));
				side.addPiece (new
					       Knight (color, rank,
						       ChessBoardConstants.
						       FILE_G, side, opp));
				side.addPiece (new
					       Rook (color, rank,
						     ChessBoardConstants.
						     FILE_H, side, opp));
			}

			public static void GetDefaultSides (out ChessSide
							    white,
							    out ChessSide
							    black)
			{
				white = new ChessSide (ColorType.WHITE);
				black = new ChessSide (ColorType.BLACK);

				FillDefaultPieces (ColorType.WHITE, white,
						   black);
				FillDefaultPieces (ColorType.BLACK, black,
						   white);
			}

			public override string ToString ()
			{
				StringBuilder buf = new StringBuilder ();
				IList pieces = allPieces ();
				foreach (ChessPiece piece in pieces)
				{
					buf.Append ("\t" + piece + "\n");
				}
				return buf.ToString ();
			}
		}
	}
}
