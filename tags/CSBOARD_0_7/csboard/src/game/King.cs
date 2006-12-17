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
		public class King:ChessPiece
		{
			bool can_castle = true;

			public King (ColorType color, int rank, int file,
				     ChessSide myside,
				     ChessSide oppside):base (PieceType.KING,
							      color, rank,
							      file, myside,
							      oppside)
			{
			}

			public override int getPoints ()
			{
				return 1000;
			}
			public override string getNotationPrefix ()
			{
				return "K";
			}

			protected override IList getNotationCandidates ()
			{
				ArrayList list = new ArrayList (1);
				list.Add (this);
				return list;
			}

			public override string getNotation (ChessSide side,
							    ChessPiece[,]
							    positions, int sr,
							    int sf, int dr,
							    int df,
							    PromotionType
							    promotion)
			{
				return "K" + (char) ('a' + df) + (dr + 1);
			}

			public override bool isValidMove (int i, int j,
							  ChessPiece[,]
							  positions,
							  int flags)
			{
				if (!base.
				    isValidMove (i, j, positions, flags))
					return false;
				int r_diff = i - rank;
				int f_diff = j - file;

				if (r_diff < 0)
					r_diff = -r_diff;
				if (f_diff < 0)
					f_diff = -f_diff;

				if (r_diff < 2 && f_diff < 2)
					return true;

				//                if( castling ) {
				//                        if( file == ChessBoardConstants.e && j == ChessBoardConstants.g )
				//                                return true;
				//                        if( file == ChessBoardConstants.e && j == ChessBoardConstants.c )
				//                                return true;
				//                }

				return false;
			}

			public override void setPosition (int i, int j)
			{
				if (can_castle)
				  {
					  can_castle = false;
					  //          System.out.println("Cant castle");
				  }
				base.setPosition (i, j);
			}

			public bool CanCastle
			{
				get
				{
					return can_castle;
				}
				set
				{
					can_castle = value;
				}
			}
		}
	}
}
