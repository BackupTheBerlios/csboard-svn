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

using Chess.Parser;
using System;
using System.Text;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class ChessGame:Chess.Parser.PGNChessGame
		{
			public ChessGame (PGNChessGame game):base (game)
			{
			}

			public virtual string ToPango ()
			{
				StringBuilder buffer = new StringBuilder ();
				  buffer.Append (String.
						 Format
						 ("<b>{0} {1} {2}</b>\n",
						  MarkupEncode (this.White),
						  Catalog.GetString ("vs"),
						  MarkupEncode (this.Black)));
				  buffer.Append (String.Format ("<small><i>{0}</i>: <b>{1}</b> ({2} {3})</small>", Catalog.GetString ("Result"), this.Result, (this.Moves.Count + 1) / 2, Catalog.GetString ("moves")));	// adding +1 will round it properly
				string eventvalue =
					this.GetTagValue ("Event", null);
				if (eventvalue != null)
				  {
					  buffer.Append (String.
							 Format
							 ("\n<small><i>{0}</i>: {1}, <i>{2}</i>: {3}</small>",
							  Catalog.
							  GetString ("Event"),
							  MarkupEncode
							  (eventvalue),
							  Catalog.
							  GetString ("Date"),
							  this.
							  GetTagValue ("Date",
								       "?")));
				  }
				return buffer.ToString ();
			}

			static string MarkupEncode (string str)
			{
				string chars = "&<>";
				string[]strs =
				{
				"&amp;", "&lt;", "&gt;"};
				bool somethingFound = false;
				StringBuilder buffer = new StringBuilder ();
				for (int i = 0; i < str.Length; i++)
				  {
					  char ch = str[i];
					  int idx;
					  if ((idx = chars.IndexOf (ch)) < 0)
					    {
						    buffer.Append (ch);
						    continue;
					    }
					  somethingFound = true;
					  string replace_str = strs[idx];
					  buffer.Append (replace_str);
				  }
				if (!somethingFound)
					return str;
				return buffer.ToString ();
			}
		}
	}
}
