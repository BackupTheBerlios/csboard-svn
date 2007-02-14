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

namespace CsBoard {
	namespace ICS {

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

	enum Relation {
		IamObserving = 0,
		IamPlayingAndMyMove = 1,
		IamPlayingAndMyOppsMove = -1,
		IamExamining = 2,
		IamObservingGameBeingObserved = -2,
		IsolatedPosition = -3
	}

	public class MoveDetails {
		ArrayList pos;

		bool whiteToMove;
		int doublePushFile;
		bool whiteCanCastleShort;
		bool whiteCanCastleLong;
		bool blackCanCastleShort;
		bool blackCanCastleLong;
		int movesSinceLastIrreversibleMove;
		int gameNumber;
		string white;
		string black;
		Relation relation;
		int initial_time;
		int increment;
		int whites_material_strength;
		int blacks_material_strength;
		int whites_remaining_time;
		int blacks_remaining_time;
		int movenumber;
		string verbose_notation;
		int previous_move_time;
		string pretty_notation;
		bool blackAtBottom;

		private MoveDetails() {
			pos = new ArrayList();
		}

		public static MoveDetails FromBuffer(byte[] buffer, int start, int end) {
			MoveDetails details = new MoveDetails();

			// <12> rnbqkb-r pppppppp -----n-- -------- ---P---- -------- PPP-PPPP RNBQKBNR
			for(int i = 0; i < 8; i++) {
				ParserUtils.SkipWhitespace(buffer, ref start, end);
				StringBuilder buf = new StringBuilder();
				for(int j = 0; j < 8; j++) {
					if(j != 0)
						buf.Append(' ');
					char ch = (char) buffer[start++];
					if(ch == '-')
						ch = '.';
					buf.Append(ch);
				}
				details.pos.Add(buf.ToString());
			}

			// W -1 1 1 1 1 1 65 GuestSDSP uvsravikiran
			string token;

			token = GetNextToken(buffer, ref start, end);
			details.whiteToMove = token[0] == 'W';

			details.doublePushFile = Int32.Parse(GetNextToken(buffer, ref start, end));

			token = GetNextToken(buffer, ref start, end);
			details.whiteCanCastleShort = token[0] == '1';
			token = GetNextToken(buffer, ref start, end);
			details.whiteCanCastleLong = token[0] == '1';

			token = GetNextToken(buffer, ref start, end);
			details.blackCanCastleShort = token[0] == '1';
			token = GetNextToken(buffer, ref start, end);
			details.blackCanCastleLong = token[0] == '1';

			details.movesSinceLastIrreversibleMove = Int32.Parse(GetNextToken(buffer, ref start, end));

			details.gameNumber = Int32.Parse(GetNextToken(buffer, ref start, end));

			details.white = GetNextToken(buffer, ref start, end);
			details.black = GetNextToken(buffer, ref start, end);

			// -1 3 0 39 39 180000 180000 2 N/g8-f6 (0:00.000) Nf6 1 1 0
			details.relation = (Relation) Int32.Parse(GetNextToken(buffer, ref start, end));

			details.initial_time = Int32.Parse(GetNextToken(buffer, ref start, end));
			details.increment = Int32.Parse(GetNextToken(buffer, ref start, end));

			details.whites_material_strength = Int32.Parse(GetNextToken(buffer, ref start, end));
			details.blacks_material_strength = Int32.Parse(GetNextToken(buffer, ref start, end));

			details.whites_remaining_time = Int32.Parse(GetNextToken(buffer, ref start, end));
			details.blacks_remaining_time = Int32.Parse(GetNextToken(buffer, ref start, end));

			details.movenumber = Int32.Parse(GetNextToken(buffer, ref start, end));
			details.verbose_notation = GetNextToken(buffer, ref start, end);

			details.previous_move_time = ParseMoveTime(GetNextToken(buffer, ref start, end));

			details.pretty_notation = GetNextToken(buffer, ref start, end);

			token = GetNextToken(buffer, ref start, end);
			details.blackAtBottom = token[0] == '1';
			
			return details;
		}

		private static int ParseMoveTime(string str) {
			str = str.Substring(1, str.Length - 2); // strip off the braces
			int idx = str.IndexOf('.');
			str = str.Substring(0, idx) + str.Substring(idx + 1); // remove the dot.

			string[] toks = str.Split(':');
			int time = 0;
			for(int i = 0; i < toks.Length; i++) {
				time *= 60;
				int val = Int32.Parse(toks[i]);
				if(i != toks.Length - 1) // the last component is already in millisecs
					val *= 1000;
				time += val;
			}

			return time;
		}

		private static string GetNextToken(byte[] buffer, ref int start, int end) {
			ParserUtils.SkipWhitespace(buffer, ref start, end);
			string token;
			ParserUtils.ReadWord(buffer, ' ', ref start, end, out token);
			return token;
		}

		public override string ToString() {
			StringBuilder buf = new StringBuilder();
			foreach(string str in pos) {
				buf.Append(str);
				buf.Append("\n");
			}

			buf.Append(String.Format("{0} | {1}\n", verbose_notation, pretty_notation));
			return buf.ToString();
		}

		private static void ParseFile(string file) {
			StreamReader reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read));
			string line;
			while((line = reader.ReadLine()) != null) {
				if(!line.StartsWith("<12>"))
					continue;
				byte[] buffer = System.Text.Encoding.ASCII.GetBytes(line);
				MoveDetails details = MoveDetails.FromBuffer(buffer, 4, buffer.Length);
				Console.WriteLine(details);
			}
		}
	}
	}
}
