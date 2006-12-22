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
namespace Chess
{
	namespace Parser
	{
		public class PGNGameLoader : IParserListener {
			ArrayList curTagList;
			ArrayList curMoves;
			string initialComment;

			public event GameLoadedEvent GameLoaded;

			public PGNGameLoader() {
				curTagList = new ArrayList();
				curMoves = new ArrayList();
			}

			public void TagFound(string name, string value) {
				PGNTag tag = new PGNTag(name, value);
				if(curTagList.Contains(tag))
					return;
				curTagList.Add(tag);
			}

			public void MoveFound(string movestr) {
				PGNChessMove move = new PGNChessMove();
				move.move = movestr;
				curMoves.Add(move);
			}

			public void CommentFound(string comment) {
				if(curMoves.Count == 0) {
					initialComment = comment;
					return;
				}

				PGNChessMove move = (PGNChessMove) curMoves[curMoves.Count - 1];
				move.comment = comment;
			}

			public void NAGsFound(PGNNAG[] nags) {
				PGNChessMove move = (PGNChessMove) curMoves[curMoves.Count - 1];
				move.Nags = nags;
			}

			public void GameEndFound() {
				PGNChessGame game = new PGNChessGame (initialComment, curTagList, curMoves);
				if (GameLoaded != null) {
					GameLoaded (this,
						    new
						    GameLoadedEventArgs
						    (game));
				}
				initialComment = null;
				curTagList = new ArrayList();
				curMoves = new ArrayList();
			}
		}
	}
}
