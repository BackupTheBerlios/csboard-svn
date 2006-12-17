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
		public class Knight:ChessPiece
		{
			public Knight (ColorType color, int rank, int file,
				       ChessSide myside,
				       ChessSide oppside):base (PieceType.
								KNIGHT, color,
								rank, file,
								myside,
								oppside)
			{
			}

			public override int getPoints ()
			{
				return 3;
			}

			public override bool isValidMove (int i, int j,
							  ChessPiece[,]
							  positions,
							  int flags)
			{
				if (!base.
				    isValidMove (i, j, positions, flags))
					return false;

				int r_diff = j - file;
				int f_diff = i - rank;

				int temp = r_diff * f_diff;
				//                System.out.println( "rank diff = " + r_diff + ", file diff = " +
				// f_diff );
				if (temp == 2 || temp == -2)
				  {
					  return true;
				  }

				return false;
			}

			public override string getNotationPrefix ()
			{
				return "N";
			}

			protected override IList getNotationCandidates ()
			{
				return myside.Knights;
			}
		}
	}
}
