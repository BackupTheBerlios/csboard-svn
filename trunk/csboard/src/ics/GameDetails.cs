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

namespace CsBoard
{
	namespace ICS
	{

		public enum GameCategory
		{
			Blitz = 'b',
			Bughouse = 'B',
			Lightning = 'l',
			NonStandard = 'n',
			Standard = 's',
			SuicideChess = 'S',
			Untimed = 'u',
			Wild = 'w',
			CrazyHouse = 'z'
		}

		public class GameDetails
		{
			bool exam;
			public string white;
			public string black;
			int whiteRating;
			int blackRating;

			bool privateGame;
			public bool PrivateGame
			{
				get
				{
					return privateGame;
				}
			}

			public string CategoryStr
			{
				get
				{
					switch (gameCategory)
					  {
					  case GameCategory.Blitz:
						  return "Blitz";
						  case GameCategory.
							  Bughouse:return
							  "Bughouse";
						  case GameCategory.
							  Lightning:return
							  "Lightning";
						  case GameCategory.
							  NonStandard:return
							  "NonStandard";
						  case GameCategory.
							  Standard:return
							  "Standard";
						  case GameCategory.
							  SuicideChess:return
							  "SuicideChess";
						  case GameCategory.
							  Untimed:return
							  "Untimed";
						  case GameCategory.
							  Wild:return "Wild";
						  case GameCategory.
							  CrazyHouse:return
							  "CrazyHouse";
					  }
					return String.Format("Unknown category ({0})", (char) gameCategory);
				}
			}

			GameCategory gameCategory;
			public GameCategory GameCategory {
				get {
					return gameCategory;
				}
			}

			bool rated;
			public bool Rated
			{
				get
				{
					return rated;
				}
			}

			int initial_time;
			int increment;
			int whites_remaining_time;
			int blacks_remaining_time;

			int whites_material_strength;
			int blacks_material_strength;

			bool whiteToMove;
			int moveNumber;
			public int gameId;
			bool inMilliseconds;

			string whites_remaining_time_str;
			string blacks_remaining_time_str;


			// 34 (Exam. 2144 litmus      1898 callaghan ) [ sr 15   5] B: 27
			// 19 ++++ GuestFFDY   ++++ GuestGLHK  [ bu  5  10]   1:24 -  5:56 (32-36) W: 14
			public static GameDetails FromBuffer (byte[]buffer,
							      int start,
							      int end)
			{
				string token;

				GameDetails details = new GameDetails ();
				details.gameId =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				if (token[0] == '(')
				  {
					  details.exam = true;
					  token = ParserUtils.
						  GetNextToken (buffer,
								ref start,
								end);
				  }
				else
					details.exam = false;

				if (token[0] == '+' || token[0] == '-')
					details.whiteRating = -1;
				else
					details.whiteRating =
						Int32.Parse (token);
				details.white =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				if (token[0] == '+' || token[0] == '-')
					details.blackRating = -1;
				else
					details.blackRating =
						Int32.Parse (token);

				if (!details.exam)
					details.black =
						ParserUtils.
						GetNextToken (buffer,
							      ref start, end);
				else
				  {
					  details.black =
						  ParserUtils.
						  GetNextToken (buffer, ')',
								ref start,
								end);
					  start++;
				  }

				token = ParserUtils.GetNextToken (buffer, '[', ref start, end);	// go to [
				start++;

/*
				details.gameType =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
*/
				details.privateGame = buffer[start++] == 'p';
				details.gameCategory = (GameCategory) buffer[start++];
				details.rated = buffer[start++] == 'r';
				
				details.initial_time =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				token = ParserUtils.GetNextToken (buffer, ']',
								  ref start,
								  end);
				start++;
				token = token.Trim ();
				details.increment = Int32.Parse (token);

				if (!details.exam)
				  {
					  token = ParserUtils.
						  GetNextToken (buffer,
								ref start,
								end);
					  details.whites_remaining_time =
						  ParserUtils.
						  ParseMoveTime (token,
								 out details.
								 inMilliseconds);
					  details.whites_remaining_time_str =
						  token;

					  ParserUtils.GotoThisChar(buffer, '-', ref start, end);
					  start++;
					  token = ParserUtils.
						  GetNextToken (buffer,
								ref start,
								end);
					  details.blacks_remaining_time =
						  ParserUtils.
						  ParseMoveTime (token,
								 out details.
								 inMilliseconds);
					  details.blacks_remaining_time_str =
						  token;

					  ParserUtils.ReadWord (buffer, '(',
								ref start,
								end,
								out token);
					  start++;
					  ParserUtils.ReadWord (buffer, '-',
								ref start,
								end,
								out token);
					  start++;
					  token = token.Trim ();
					  details.whites_material_strength =
						  Int32.Parse (token);

					  ParserUtils.ReadWord (buffer, ')',
								ref start,
								end,
								out token);
					  start++;
					  token = token.Trim ();
					  details.blacks_material_strength =
						  Int32.Parse (token);
				  }

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.whiteToMove = token[0] == 'W';
				details.moveNumber =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				return details;
			}

			public override string ToString ()
			{
				StringBuilder buf = new StringBuilder ();
				buf.Append (String.
					    Format ("{0} vs {1}", white,
						    black));

				return buf.ToString ();
			}

			string WhiteStr
			{
				get
				{
					if (whiteRating > 0)
						return String.
							Format ("{0} ({1})",
								white,
								whiteRating);
					return white;
				}
			}

			string BlackStr
			{
				get
				{
					if (blackRating > 0)
						return String.
							Format ("{0} ({1})",
								black,
								blackRating);
					return black;
				}
			}

			public string ToPango ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append (String.
					       Format
					       ("<big><b>{0}</b></big> vs <big><b>{1}</b></big>\n",
						WhiteStr, BlackStr));
				buffer.Append (String.
					       Format
					       ("Time left for {0}: {1}\n",
						white,
						whites_remaining_time_str));
				buffer.Append (String.
					       Format
					       ("Time left for {0}: {1}\n",
						black,
						blacks_remaining_time_str));
				buffer.Append (String.
					       Format
					       ("<i>Move number: </i> {0}, {1} to move",
						moveNumber,
						whiteToMove ? white : black));

				return buffer.ToString ();
			}

			public string TimeDetailsAsMarkup ()
			{
				return String.Format ("<b>{0}: {1}</b>",
						      initial_time,
						      increment);
			}
		}
	}
}
