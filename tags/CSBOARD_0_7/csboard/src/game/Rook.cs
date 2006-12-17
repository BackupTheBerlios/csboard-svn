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
		public class Rook:ChessPiece
		{
			public Rook (ColorType color, int rank, int file,
				     ChessSide myside,
				     ChessSide oppside):base (PieceType.ROOK,
							      color, rank,
							      file, myside,
							      oppside)
			{
			}

			public override int getPoints ()
			{
				return 5;
			}

			public override string getNotationPrefix ()
			{
				return "R";
			}

			protected override IList getNotationCandidates ()
			{
				return myside.Rooks;
			}

			public override bool isValidMove (int i, int j,
							  ChessPiece[,]
							  positions,
							  int flags)
			{
				if (!base.
				    isValidMove (i, j, positions, flags))
					return false;

				if (rank == i)
				  {
					  int pos, from, to;
					  if (j > file)
					    {
						    from = file;
						    to = j;
					    }
					  else
					    {
						    from = j;
						    to = file;
					    }

					  if (to - from == 1)
						  return true;

					  for (pos = from + 1; pos < to;
					       pos++)
					    {
						    if (positions[i, pos] !=
							null)
						      {
							      return false;
						      }
					    }

					  return true;
				  }
				else if (file == j)
				  {
					  int pos, from, to;
					  if (i > rank)
					    {
						    from = rank;
						    to = i;
					    }
					  else
					    {
						    from = i;
						    to = rank;
					    }

					  if (to - from == 1)
						  return true;
					  for (pos = from + 1; pos < to;
					       pos++)
					    {
						    if (positions[pos, j] !=
							null)
						      {
							      return false;
						      }
					    }

					  return true;
				  }

				return false;
			}
		}
	}
}
