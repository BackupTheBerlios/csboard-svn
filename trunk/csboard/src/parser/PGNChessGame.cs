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
	public class PGNTag {
		string tagname;
		string tagvalue;

		public string Name {
			get { return tagname; }
		}
		public string Value {
			get { return tagvalue; }
			set { tagvalue = value; }
		}

		public PGNTag(string n, string v) {
			tagname = n;
			tagvalue = v;
		}

		public override bool Equals(object o) {
			PGNTag t = (PGNTag) o;
			return Name.Equals(t.Name);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}
		public class PGNChessGame
		{
			ArrayList tagList;

			public ArrayList TagList { get { return tagList; } }

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

			public string White { get { return GetTagValue("White", "[White]"); } }
			public string Black { get { return GetTagValue("Black", "[Black]"); } }
			public string Result { get { return GetTagValue("Result", "?"); } }
			public string Site { get { return GetTagValue("Site", ""); } }
			public string Event { get { return GetTagValue("Event", ""); } }
			public string Date { get { return GetTagValue("Date", ""); } }

			public PGNChessGame (string c, ArrayList t, IList m)
			{
				comment = c;
				tagList = t;
				moves = m;
			}

			public bool HasTag(string name) {
				return tagList.Contains(new PGNTag(name, null));
			}

			// altvalue will be returned if the tag doesnt exist
			public string GetTagValue(string name, string altvalue) {
				PGNTag tag = new PGNTag(name, null);
				if(!tagList.Contains(tag))
					return altvalue;

				int idx = tagList.IndexOf(tag);
				tag = (PGNTag) tagList[idx];
				return tag.Value;
			}

			public override string ToString ()
			{
				StringBuilder buffer = new StringBuilder ();

				buffer.Append ("Moves:\n------\n");
				foreach (object o in moves)
				{
					buffer.Append (o + "\n");
				}

				return buffer.ToString ();
			}

			public void WritePGN (TextWriter writer)
			{
				foreach (PGNTag tag in tagList)
				{
					writer.WriteLine (String.
							  Format
							  ("[{0} \"{1}\"]",
							   tag.Name, tag.Value));
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
				if (HasTag ("Result"))
				  {
					  writer.Write (GetTagValue("Result", null));
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
