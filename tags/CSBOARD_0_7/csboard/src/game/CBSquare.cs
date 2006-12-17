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
		public class CBSquare
		{
			public int rank;

			public int file;

			public CBSquare (int r, int f)
			{
				rank = r;
				file = f;
			}

			public bool equals (Object obj)
			{
				if (obj instanceof CBSquare)
				  {
					  CBSquare cbs = (CBSquare) obj;
					    return cbs.rank == rank
						  && cbs.file == file;
				  }

				return super.equals (obj);
			}

			public String toString ()
			{
				return "(" + ((char) ('a' + file)) + (rank +
								      1) +
					")";
			}

			public String toChessNotation ()
			{
				return "" + ((char) ('a' + file)) + (rank +
								     1);
			}

			public static CBSquare fromChessNotation (String
								  square)
			{
				if (square == null
				    || (square =
					square.trim ()).length () != 2)
					return null;

				square = square.toLowerCase ();
				char ch = square.charAt (0);
				if (ch < 'a' || ch > 'h')
					return null;
				int file = ch - 'a';
				ch = square.charAt (1);
				if (ch < '1' || ch > '8')
					return null;
				int rank = ch - '1';

				return new CBSquare (rank, file);
			}

			public int hashCode ()
			{
				return (int) ((rank << 4) | file);
			}
		}
	}
}
