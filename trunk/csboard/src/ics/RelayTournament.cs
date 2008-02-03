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
				int id;
				public int ID
				{
					get
					{
						return id;
					}
				}
				string name;
				public string Name
				{
					get
					{
						return name;
					}
				}
				string roundinfo;
				public string RoundInfo
				{
					get
					{
						return roundinfo;
					}
				}
				string extrainfo;
				public string ExtraInfo
				{
					get
					{
						return extrainfo;
					}
				}

				public static Tournament FromLine (string
								   line)
				{
					Tournament t = new Tournament ();
					Regex reg =
						new
						Regex
						(":(?<id>\\d+)\\s+(?<name>.*)\\s+-?\\s+(?<extra>\\w.*)");
					//new Regex(":(?<id>\\d+)\\s+(?<name>.*)\\s+-\\s+(?<extra>\\w.*)");
					Match m = reg.Match (line);
					if (!m.Success)
					  {
						  return null;
					  }
					try
					{
						t.id = Int32.Parse (m.
								    Groups
								    ["id"].
								    Value);
					}
					catch
					{
						return null;
					}
					t.name = m.Groups["name"].Value.
						Trim ();
					string extra =
						m.Groups["extra"].Value;
					int index;
					if ((index =
					     extra.IndexOf ("  ")) < 0)
					  {
						  t.roundinfo = extra;
						  t.extrainfo = null;
						  return t;
					  }
					t.roundinfo =
						extra.Substring (0, index);
					t.extrainfo =
						extra.Substring (index + 2);
					return t;
				}

				public override string ToString ()
				{
					StringBuilder buf =
						new StringBuilder ();
					buf.Append (String.
						    Format ("ID: {0}\n" +
							    "Name: {1}\n" +
							    "Round: {2}\n" +
							    "Extra: {3}", id,
							    name, roundinfo,
							    extrainfo));
					return buf.ToString ();
				}
			}
		}
	}
}
