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



/*
  Example output:
  
  - <g1> 1 p=0 t=blitz r=1 u=1,1 it=5,5 i=8,8 pt=0 rt=1586E,2100  ts=1,0
  
  This is in the format:
  - <g1> game_number p=private(1/0) t=type r=rated(1/0)
  u=white_registered(1/0),black_registered(1/0)
  it=initial_white_time,initial_black_time
  i=initial_white_inc,initial_black_inc
  pt=partner's_game_number(or 0 if none)
  rt=white_rating(+ provshow character),black_rating(+ provshow character)
  ts=white_uses_timeseal(0/1),black_uses_timeseal(0/1)
  
  Note any new fields will be appended to the end so the interface must be
  able to handle this.

taken from:
http://www.freechess.org/Help/HelpFiles/iv_gameinfo.html
*/

using System;

namespace CsBoard {
	namespace ICS {
	public class GameInfo {
		public int gameId; // number
		public bool privateGame; // 1/0
		public string gameType; // blitz
		public bool rated;  // 1/0

		// u=
		public bool whiteRegistered; // 1/0
		public bool blackRegistered; // 1/0

		// it
		public int initialWhiteTime; // number
		public int initialBlackTime; // number

		// i
		public int whiteIncrement; // number
		public int blackIncrement; // number

		public int partners_game_number; // may have meaning for simuls

		// rt=
		public int whitesRating;
		public char whitesRatingChar;

		// rt=
		public int blacksRating;
		public char blacksRatingChar;

		// ts=
		public bool whiteUsesTimeseal;
		public bool blackUsesTimeseal;

		// <g1> 1 p=0 t=blitz r=1 u=1,1 it=5,5 i=8,8 pt=0 rt=1586E,2100  ts=1,0
		public static GameInfo FromBuffer(byte[] buffer, int start, int end) {
			GameInfo info = new GameInfo();
			string name;
			string token;

			info.gameId = Int32.Parse(ParserUtils.GetNextToken(buffer, ref start, end));

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.privateGame = buffer[start] == '0';

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			ParserUtils.ReadWord(buffer, ' ', ref start, end, out info.gameType);

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.rated = buffer[0] == '1';

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.whiteRegistered = buffer[start] == '1';
			start += 2;
			info.blackRegistered = buffer[start] == '1';
			start++;

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.initialWhiteTime = Int32.Parse(ParserUtils.GetNextToken(buffer, ',', ref start, end));
			start++;
			info.initialBlackTime = Int32.Parse(ParserUtils.GetNextToken(buffer, ref start, end));

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.whiteIncrement = Int32.Parse(ParserUtils.GetNextToken(buffer, ',', ref start, end));
			start++;
			info.blackIncrement = Int32.Parse(ParserUtils.GetNextToken(buffer, ref start, end));

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			info.partners_game_number = Int32.Parse(ParserUtils.GetNextToken(buffer, ref start, end));

			string rating_str;
			int len;

			ParserUtils.GotoThisChar(buffer, '=', ref start, end);
			start++;
			rating_str = ParserUtils.GetNextToken(buffer, ',', ref start, end);
			start++;
			len = rating_str.Length;
			if(!Char.IsDigit(rating_str[len - 1])) {
				info.whitesRatingChar = rating_str[len - 1];
				info.whitesRating = Int32.Parse(rating_str.Substring(0, len - 1));
			}
			else
				info.whitesRating = Int32.Parse(rating_str);

			rating_str = ParserUtils.GetNextToken(buffer, ref start, end);
			start++;
			len = rating_str.Length;

			if(!Char.IsDigit(rating_str[len - 1])) {
				info.blacksRatingChar = rating_str[len - 1];
				info.blacksRating = Int32.Parse(rating_str.Substring(0, len - 1));
			}
			else
				info.blacksRating = Int32.Parse(rating_str);

			return info;
		}
	}
	}
}
