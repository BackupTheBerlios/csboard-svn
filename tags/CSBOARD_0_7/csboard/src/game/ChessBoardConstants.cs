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

namespace Chess
{
	namespace Game
	{
		public enum ColorType
		{
			WHITE,
			BLACK
		};

		public enum PieceType
		{
			NONE,
			KING,
			QUEEN,
			ROOK,
			BISHOP,
			KNIGHT,
			PAWN
		};

		public enum PromotionType
		{
			NONE = -1,
			QUEEN = PieceType.QUEEN,
			ROOK = PieceType.ROOK,
			BISHOP = PieceType.BISHOP,
			KNIGHT = PieceType.KNIGHT
		};

		public class ChessBoardConstants
		{
			public const int MOVE_EXCHANGE = 0x00000001;
			public const int MOVE_ENPASSANT = 0x00000002;
			public const int MOVE_DONT_CHECK_KINGS_EXPOSURE =
				0x00000004;
			public const int MOVE_DEBUG = 0x10000000;

			public const int FILE_A = 0;
			public const int FILE_B = 1;
			public const int FILE_C = 2;
			public const int FILE_D = 3;
			public const int FILE_E = 4;
			public const int FILE_F = 5;
			public const int FILE_G = 6;
			public const int FILE_H = 7;

			public const int RANK_1 = 0;
			public const int RANK_2 = 1;
			public const int RANK_3 = 2;
			public const int RANK_4 = 3;
			public const int RANK_5 = 4;
			public const int RANK_6 = 5;
			public const int RANK_7 = 6;
			public const int RANK_8 = 7;
		};


		public enum CastleType
		{
			LONG,
			SHORT
		}
	}
}
