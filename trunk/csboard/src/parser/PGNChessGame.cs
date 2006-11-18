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

using System.Collections;
using System.Text;
using System;
using System.IO;

namespace Chess
{
	namespace Parser
	{
		public class PGNChessGame
		{
			private IDictionary tags;

			public IDictionary Tags
			{
				get
				{
					return tags;
				}
			}

			private IList moves;
			public IList Moves
			{
				get
				{
					return moves;
				}
			}

			private string comment;
			public string Comment
			{
				get
				{
					return comment;
				}
			}

			public PGNChessGame (string c, IDictionary t, IList m)
			{
				comment = c;
				tags = t;
				moves = m;
			}

			public override string ToString ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append ("Tags:\n----\n");
				foreach (DictionaryEntry entry in tags)
				{
					buffer.Append (entry.Key + "=" +
						       entry.Value + "\n");
				}

				buffer.Append ("Moves:\n------\n");
				foreach (object o in moves)
				{
					buffer.Append (o + "\n");
				}

				return buffer.ToString ();
			}

			public void WritePGN (TextWriter writer)
			{
				foreach (DictionaryEntry e in tags)
				{
					writer.WriteLine (String.
							  Format
							  ("[{0} \"{1}\"]",
							   e.Key, e.Value));
				}

				writer.WriteLine ();

				int i = 1;
				foreach (ChessMove move in moves)
				{
					if (move.whitemove == null)
					  {
						  // BUG. Empty move? This should not happen.
						  break;
					  }
					writer.Write (i + ". " +
						      move.whitemove + " ");
					if (move.whiteComment != null)
					  {
						  // we should escape '{' in the comment
						  writer.Write ("{" +
								move.
								whiteComment +
								"} ");
					  }

					if (move.blackmove == null)
					  {	// last move
						  break;
					  }
					writer.Write (move.blackmove + " ");
					if (move.blackComment != null)
					  {
						  writer.Write ("{" +
								move.
								blackComment +
								"} ");
					  }
					i++;
				}
				if (tags.Contains ("Result"))
				  {
					  writer.Write (tags["Result"]);
				  }
				writer.WriteLine ();
			}
		}

		public class ChessMove
		{
			public string whitemove, blackmove, whiteComment,
				blackComment;
			public int moveIdx;

			public ChessMove (int idx)
			{
				moveIdx = idx;
			}

			public override string ToString ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append (moveIdx);
				buffer.Append (". ");
				if (whitemove != null)
				  {
					  buffer.Append (String.
							 Format ("{0}{1}",
								 whitemove,
								 whiteComment
								 ==
								 null ? "" :
								 " " +
								 whiteComment));
					  if (blackmove != null)
					    {
						    buffer.Append (String.
								   Format
								   (", {0}{1}",
								    blackmove,
								    blackComment
								    ==
								    null ? ""
								    : " " +
								    blackComment));
					    }
				  }

				return buffer.ToString ();
			}
		}
	}
}
