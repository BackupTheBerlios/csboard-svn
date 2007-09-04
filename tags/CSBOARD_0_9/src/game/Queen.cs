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
		public class Queen:ChessPiece
		{
			public Queen (ColorType color, int rank, int file,
				      ChessSide myside,
				      ChessSide oppside):base (PieceType.
							       QUEEN, color,
							       rank, file,
							       myside,
							       oppside)
			{
			}

			public override int getPoints ()
			{
				return 9;
			}

			public override string getNotationPrefix ()
			{
				return "Q";
			}

			protected override IList getNotationCandidates ()
			{
				return myside.Queens;
			}

			public override bool isValidMove (int i, int j,
							  ChessPiece[,]
							  positions,
							  int flags)
			{
				if (!base.
				    isValidMove (i, j, positions, flags))
				  {
					  return false;
				  }

				int r_diff = i - rank;
				int f_diff = j - file;

				if (r_diff != 0 && f_diff != 0
				    && !((r_diff + f_diff) == 0
					 || (r_diff - f_diff) == 0))
				  {
					  return false;
				  }

				// movement is ok

				int tmp_r, tmp_f;
				int r_inc, f_inc;
				int dist = -1;

				if (r_diff < 0)
				  {	// right to left
					  r_inc = -1;
					  dist = -r_diff;
				  }
				else if (r_diff > 0)
				  {
					  dist = r_diff;
					  r_inc = 1;
				  }
				else
				  {
					  r_inc = 0;
					  tmp_r = i;
				  }

				if (f_diff < 0)
				  {	// top to bottom
					  f_inc = -1;
					  dist = -f_diff;
				  }
				else if (f_diff > 0)
				  {
					  dist = f_diff;
					  f_inc = 1;
				  }
				else
				  {
					  tmp_f = j;
					  f_inc = 0;
				  }

				tmp_r = rank;
				tmp_f = file;

				for (int k = 1; k < dist; k++)
				  {
					  tmp_r += r_inc;
					  tmp_f += f_inc;
					  if (positions[tmp_r, tmp_f] != null)
					    {
						    return false;
					    }
				  }

				return true;
			}
		}
	}
}
