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

namespace Chess
{
	namespace Game
	{
		public class ChessUtils
		{
			public static PieceType getPiece (string str)
			{
				char ch = str[0];
				if (!Char.IsUpper (ch))
				  {
					  if (ch >= 'a' && ch <= 'h')
						  return PieceType.PAWN;
					  else
						  return PieceType.NONE;
				  }

				switch (ch)
				  {
				  case 'K':
					  return PieceType.KING;
					  case 'Q':return PieceType.QUEEN;
					  case 'R':return PieceType.ROOK;
					  case 'B':return PieceType.BISHOP;
					  case 'N':return PieceType.KNIGHT;
				  }

				return PieceType.NONE;
			}

			public static bool getSquare (string move,
						      out int rank,
						      out int file)
			{
				int length = move.Length;
				char ch1 = move[length - 2];
				char ch2 = move[length - 1];

				rank = ch2 - '1';
				file = ch1 - 'a';

				if (rank < 8 && rank >= 0 && file < 8
				    && file >= 0)
				  {
					  return true;
				  }

				return false;
			}

			public static ColorType getSquareColor (int i, int j)
			{
				int temp = (i + j) & 0x1;
				if (temp == 0)
					return ColorType.BLACK;

				return ColorType.WHITE;
			}

			public static bool isPawnPromotion (ChessPiece piece,
							    int file)
			{
				if (piece.Type != PieceType.PAWN)
					return false;

				if (piece.Color == ColorType.WHITE
				    && file == 7)
					return true;

				if (piece.Color == ColorType.BLACK
				    && file == 0)
					return true;

				return false;
			}
		}
	}
}
