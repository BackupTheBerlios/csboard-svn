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
			public class Tournament
			{
				string name;
				public string Name
				{
					get
					{
						return name;
					}
				}
				public string RoundInfo
				{
					get
					{
						return "";
					}
				}

				public static Tournament FromLine (string
								   line)
				{
					Tournament t = new Tournament ();
					Regex reg =
						new
						Regex
						(":There are (?<id>\\d+) games in the (?<name>.*)");
					//(":(?<id>\\d+)\\s+(?<name>.*)\\s+-?\\s+(?<extra>\\w.*)");
					Match m = reg.Match (line);
					if (!m.Success)
					  {
						  return null;
					  }
					t.name = m.Groups["name"].Value.
						Trim ();
					  return t;
				}

				public override string ToString ()
				{
					StringBuilder buf =
						new StringBuilder ();
					buf.Append (String.
						    Format ("Name: {0}",
							    name));
					return buf.ToString ();
				}
			}
		}
	}
}
