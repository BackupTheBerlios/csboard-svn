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
using System.Collections;
using System.Text;
using System.IO;

namespace CsBoard
{
	namespace ICS
	{

		/*
		 * <12> rnbqkb-r pppppppp -----n-- -------- ----P--- -------- PPPPKPPP RNBQ-BNR \
		 * B -1 0 0 1 1 0 7 Newton Einstein 1 2 12 39 39 119 122 2 K/e1-e2 (0:06) Ke2 0
		 *
		 * 8 fields to represent the board. The first line is white's 8th rank (black's 1st rank)
		 * regardless of whose move it is.
		 *
		 * W/B               - whose turn is it to move?
		 * -1|[0-7]          - -1 if the previous move was not a pawn double push. otherwise, the file in which
		 *                     it was made
		 * 0|1               - Can white still castle short? 0=no, 1=yes
		 * 0|1               - Can white still castle long?
		 * 0|1               - Can black still castle short?
		 * 0|1               - Can black still castle long?
		 * n                 - The number of moves since the last irreversible move (if it is >= 100, the game
		 *                     can be declared a draw due to the 50 move rule
		 * gameno            - The game number
		 * white             - white's name
		 * black             - black's name
		 * [0,1,-1,2,-2,-3]  - my relation to this position
		 *                     -3 = isolated position
		 *                     -2 = I am observing game being observed
		 *                      2 = I am the examiner of this game
		 *                     -1 = I am playing, it is my opponents move
		 *                      1 = I am playing and it is my move
		 *                      0 = I am observing a game being played
		 * initial time
		 * increment
		 * White material strength
		 * Black material strength
		 * White's remaining time
		 * Black's remaining time
		 * moveno           - the number of the move to be made
		 * notation         - verbose coordinate notation for the previous move. "none" if there was none
		 * prev_move_time   - time taken by the previous move
		 * pretty_notation  - pretty notation of the previous move
		 * flip field       - flip field for board orientation. 1 = black at bottom, 0 = white at bottom
		 */

		public enum Relation
		{
			IamObserving = 0,
			IamPlayingAndMyMove = 1,
			IamPlayingAndMyOppsMove = -1,
			IamExamining = 2,
			IamObservingGameBeingObserved = -2,
			IsolatedPosition = -3
		}

		public class MoveDetails
		{
			public ArrayList pos;

			public bool WhiteMoved
			{
				get
				{
					return !whiteToMove;
				}
			}

			public bool whiteToMove;
			public int doublePushFile;
			public bool whiteCanCastleShort;
			public bool whiteCanCastleLong;
			public bool blackCanCastleShort;
			public bool blackCanCastleLong;
			public int movesSinceLastIrreversibleMove;
			public int gameNumber;
			public string white;
			public string black;
			public Relation relation;
			public int initial_time;
			public int increment;
			public int whites_material_strength;
			public int blacks_material_strength;
			public int whites_remaining_time;
			public int blacks_remaining_time;
			public int movenumber;
			public string verbose_notation;
			public int previous_move_time;
			public string pretty_notation;
			public bool blackAtBottom;
			public bool inMilliseconds;

			public MoveDetails (ArrayList pos)
			{
				this.pos = pos;
			}

			// <12> r-r---k- ---nqpp- --p-pnp- b-Pp---- ---P-P-- --N-P-P- --QB--BP RR----K- B -1 0 0 0 0 4 134 GMPopov GMAkopian 0 120 0 34 34 3060 352 19 R/f1-b1 (14:29) Rfb1 0 0 0
			public static MoveDetails FromBuffer (byte[]buffer,
							      int start,
							      int end)
			{
				MoveDetails details = new MoveDetails (new ArrayList());

				// <12> rnbqkb-r pppppppp -----n-- -------- ---P---- -------- PPP-PPPP RNBQKBNR
				for (int i = 0; i < 8; i++)
				  {
					  ParserUtils.SkipWhitespace (buffer,
								      ref
								      start,
								      end);
					  StringBuilder buf =
						  new StringBuilder ();
					  for (int j = 0; j < 8; j++)
					    {
						    char ch =
							    (char)
							    buffer[start++];
						    if (ch == '-')
							    ch = '.';
						    buf.Append (ch);
						    if(j != 7)
							    buf.Append (' ');
					    }
					  details.pos.Add (buf.ToString ());
				  }

				// W -1 1 1 1 1 1 65 GuestSDSP uvsravikiran
				string token;

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.whiteToMove = token[0] == 'W';

				details.doublePushFile =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.whiteCanCastleShort = token[0] == '1';
				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.whiteCanCastleLong = token[0] == '1';

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.blackCanCastleShort = token[0] == '1';
				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.blackCanCastleLong = token[0] == '1';

				details.movesSinceLastIrreversibleMove =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				details.gameNumber =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				details.white =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.black =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);

				// -1 3 0 39 39 180000 180000 2 N/g8-f6 (0:00.000) Nf6 1 1 0
				details.relation =
					(Relation) Int32.Parse (ParserUtils.
								GetNextToken
								(buffer,
								 ref start,
								 end));

				details.initial_time =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));
				details.increment =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				details.whites_material_strength =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));
				details.blacks_material_strength =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				details.whites_remaining_time =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));
				details.blacks_remaining_time =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));

				details.movenumber =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));
				details.verbose_notation =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				token = token.Substring (1, token.Length - 2);	// strip off the braces

				details.previous_move_time =
					ParserUtils.ParseMoveTime (token,
								   out
								   details.
								   inMilliseconds);

				details.pretty_notation =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);

				token = ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				details.blackAtBottom = token.Length > 0
					&& token[0] == '1';


				details.pos.Insert (0,
						    String.
						    Format
						    ("{0} {1}{2}{3}{4}",
						     details.
						     whiteToMove ? "white" :
						     "black",
						     details.
						     whiteCanCastleShort ? 'K' : ' ',
						     details.
						     whiteCanCastleLong ? 'Q' : ' ',
						     details.
						     blackCanCastleShort ? 'k' : ' ',
						     details.
						     blackCanCastleLong ? 'q' : ' '));
				details.pos.Insert (0, "");

				return details;
			}

			public override string ToString ()
			{
				StringBuilder buf = new StringBuilder ();
				foreach (string str in pos)
				{
					buf.Append (str);
					buf.Append ("\n");
				}

				buf.Append (String.
					    Format ("{0} | {1}\n",
						    verbose_notation,
						    pretty_notation));
				return buf.ToString ();
			}

			public void PrintTimeInfo ()
			{
				Console.WriteLine ("Initial time: {0}",
						   initial_time);
				Console.WriteLine ("Increment: {0}",
						   increment);
				Console.WriteLine
					("Whites remaining time: {0}",
					 whites_remaining_time);
				Console.WriteLine
					("Blacks remaining time: {0}",
					 blacks_remaining_time);
				Console.WriteLine ("Previous move time: {0}",
						   previous_move_time);
				Console.WriteLine ("In milliseconds: {0}",
						   inMilliseconds);
			}
		}
	}
}
