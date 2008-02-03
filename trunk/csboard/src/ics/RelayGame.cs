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
using Mono.Unix;
using System.Text;
using System.Text.RegularExpressions;

namespace CsBoard
{
	namespace ICS
	{
		namespace Relay
		{
			public class Game
			{
				int id;
				public int ID
				{
					get
					{
						return id;
					}
				}
				string white;
				public string White
				{
					get
					{
						return white;
					}
				}
				string black;
				public string Black
				{
					get
					{
						return black;
					}
				}
				string result;
				public string Result
				{
					get
					{
						return result;
					}
				}
				string opening;
				public string Opening
				{
					get
					{
						return opening;
					}
				}

				public static Game FromLine (string line)
				{
					Game g = new Game ();
					Regex reg =
						new Regex (":(?<id>\\d+)\\s+"
							   +
							   "(?<white>\\w+)\\s+"
							   +
							   "(?<black>\\w+)\\s+"
							   +
							   "(?<result>[012/*-]+)\\s+"
							   +
							   "(?<opening>\\w+)");
					Match m = reg.Match (line);
					if (!m.Success)
					  {
						  return null;
					  }
					try
					{
						g.id = Int32.Parse (m.
								    Groups
								    ["id"].
								    Value);
					}
					catch
					{
						return null;
					}
					g.white = m.Groups["white"].Value;
					g.black = m.Groups["black"].Value;
					g.result = m.Groups["result"].Value;
					g.opening = m.Groups["opening"].Value;
					return g;
				}

				public override string ToString ()
				{
					StringBuilder buf =
						new StringBuilder ();
					buf.Append (String.
						    Format ("ID: {0}\n" +
							    "White: {1}\n" +
							    "Black: {2}\n" +
							    "Result: {3}\n" +
							    "Opening: {4}",
							    id, white, black,
							    result, opening));
					return buf.ToString ();
				}
			}
		}
	}
}
