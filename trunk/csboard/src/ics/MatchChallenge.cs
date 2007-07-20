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
using System.Text;
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class MatchChallenge
		{
			bool rated;
			public bool Rated { get { return rated; } }
			string color;
			public string Color { get { return color; } }
			int time;
			public int Time { get { return time; } }
			int increment;
			public int Increment { get { return increment; } }
			string opp;
			public string Opponent { get { return opp; } }
			int opprating;
			public int OpponentsRating { get { return opprating; } }
			string category;
			public string Category { get { return category; } }

			public static MatchChallenge FromBuffer(byte[] buffer, int start, int end) {
			  MatchChallenge mc = new MatchChallenge();
			  string me;
			  string opprating;

			  mc.opp = ParserUtils.GetNextToken(buffer, ref start, end);
			  Console.WriteLine("OPP: " + mc.opp);
			  opprating = ParserUtils.GetNextToken(buffer, ref start, end);
			  try {
			    mc.opprating = Int32.Parse(opprating.Substring(1, opprating.Length - 2));
			  } catch {
			    mc.opprating = 0;
			  }

			  me = ParserUtils.GetNextToken(buffer, ref start, end);
			  Console.WriteLine("ME: " + me);
			  if(me[0] == '[') {
			    mc.color = me.Substring(1, me.Length - 2);
			    me = ParserUtils.GetNextToken(buffer, ref start, end);
			  }
			  Console.WriteLine("MY RATING: " + ParserUtils.GetNextToken(buffer, ref start, end));

			  mc.rated = ParserUtils.GetNextToken(buffer, ref start, end).Equals("rated");
			  Console.WriteLine(mc.rated);
			  mc.category = ParserUtils.GetNextToken(buffer, ref start, end);
			  mc.time = Int32.Parse(ParserUtils.GetNextToken(buffer, ref start, end));
			  mc.increment = Int32.Parse(ParserUtils.GetNextToken(buffer, '.', ref start, end));

			  return mc;
			}

			public override string ToString() {
			  StringBuilder buf = new StringBuilder();
			  buf.Append(String.Format("Challenge for a {0} game from {1} ({2})\n",
						   category, opp, opprating));
			  buf.Append(String.Format("\tTime: {0}, Increment: {1}", time, increment));
			  
			  return buf.ToString();
			}
		}
	}
}
