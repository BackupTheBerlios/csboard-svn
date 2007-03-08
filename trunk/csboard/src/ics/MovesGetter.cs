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

using Gtk;
using System;
using Mono.Unix;
using System.Collections;
using Chess.Game;

namespace CsBoard {
  namespace ICS {
  public delegate void GetMovesDelegate(ArrayList moves, int id);
  public class MovesGetter : IAsyncCommandResponseListener {
    ChessGamePlayer player;
    bool start_parsing;
    ArrayList moves;
    GetMovesDelegate callback;
    int gameid;

    private MovesGetter(int id, GetMovesDelegate cb) {
      moves = new ArrayList();
      player = ChessGamePlayer.CreatePlayer ();
      start_parsing = true;
      callback = cb;
      gameid = id;
    }

    public static void GetMovesAsync(ICSClient client, int id, GetMovesDelegate callback) {
      MovesGetter getter = new MovesGetter(id, callback);
      client.CommandSender.SendCommand("moves " + id, getter);
    }

    public virtual void CommandResponseLine (int id, byte[]buffer,
					     int start, int end)
    {
      if(start_parsing) {
	try {
	  ParseLine(id, buffer, start, end);
	}
	catch(Exception e) {
	}
	return;
      }
    }

    public virtual void CommandCodeReceived (int id,
					     CommandCode code)
    {
    }

    public virtual void CommandCompleted (int id)
    {
      callback(moves, gameid);
    }

    private void ParseLine(int id, byte[] buffer, int start, int end) {
      int movenum = Int32.Parse(ParserUtils.GetNextToken(buffer, '.', ref start, end));
      start++;
      string move = ParserUtils.GetNextToken (buffer, ref start, end);
      CreateMoveDetails(movenum, move);
      ParserUtils.GetNextToken(buffer, ref start, end); // move time

      move = ParserUtils.GetNextToken (buffer, ref start, end);
      CreateMoveDetails(movenum + 1, move);
    }

    private void CreateMoveDetails(int movenum, string move) {
      string detailed_notation;
      player.Move(move, out detailed_notation);
      MoveDetails details = new MoveDetails(player.GetPosition());
      details.pretty_notation = move;
      details.movenumber = movenum;
      details.verbose_notation = detailed_notation;
      details.whiteToMove = player.WhiteToMove;
      moves.Add(details);
    }
  }
  }
}
