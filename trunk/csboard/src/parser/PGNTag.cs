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
	namespace Parser
	{
		public class PGNTag
		{
			string tagname;
			string tagvalue;

			public string Name
			{
				get
				{
					return tagname;
				}
			}
			public string Value
			{
				get
				{
					return tagvalue;
				}
				set
				{
					tagvalue = value;
				}
			}

			public PGNTag (string n, string v)
			{
				tagname = n;
				tagvalue = v;
			}

			public override bool Equals (object o)
			{
				PGNTag t = (PGNTag) o;
				return Name.Equals (t.Name);
			}

			public override int GetHashCode ()
			{
				return Name.GetHashCode ();
			}
		}
	}
}
