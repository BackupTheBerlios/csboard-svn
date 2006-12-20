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

using System.Text;
using System;

namespace Chess {
	namespace Parser {
		public class PGNChessMove
		{
			public string move;
			public string comment;
			NAG[] nagComments;

			public void AddNag(byte value) {
				int nreq = nagComments == null ? 1 : nagComments.Length + 1;
				NAG[] nags = new NAG[nreq];
				int i = 0;
				if(nagComments != null)
					foreach(NAG nag in nagComments)
						nags[i++] = nag;

				nags[i] = new NAG(value);
				nagComments = nags;
			}

			public override string ToString ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append (String.Format ("{0}{1}", move, comment == null ? "" : " " + comment));
				
				return buffer.ToString ();
			}
		}
	}
}
