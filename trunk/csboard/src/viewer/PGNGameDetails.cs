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

using Chess.Parser;
using Chess.Game;

namespace CsBoard
{
	namespace Viewer
	{

		public enum GameRating
		{
			Unknown = 0,
			Ignore = -1,
			Average = 1,
			Good = 2,
			Excellent = 3,
			MustHave = 4
		}

		public class PGNGameDetails
		{
			public PGNChessGame Game
			{
			 get
			 {
			  return game;
			  }
			  }
			  PGNChessGame game;
			  int nmoves;
			  string white; string black; string result;
			  public GameRating Rating
			  {
			  get
			  {
			  return (GameRating) rating;
			  }
			  set
			  {
			  rating = (int) value;
			  }
			  }
			  int rating = (int)GameRating.Unknown; string hash;
			  public string Hash
			  {
			  get
			  {
			  if (hash == null)
			  hash = GenerateHash (game); return hash;}
			  }
			  public PGNGameDetails (PGNChessGame game)
			  {
			  this.game = game;
			  nmoves =
			  game.Moves.Count;
			  white =
			  game.
			  GetTagValue ("White",
				       "");
			  black =
			  game.
			  GetTagValue ("Black",
				       "");
			  result = game.GetTagValue ("Result", "*");}

			  private static string GenerateHash (PGNChessGame
							      game)
			  {
			  StringBuilder buffer =
			  new StringBuilder ();
			  int nmoves = game.Moves.Count;
			  buffer.Append (String.
					 Format
					 ("{0}:",
					  nmoves));
			  ChessGamePlayer player;
			  player =
			  game.
			  HasTag ("FEN") ?
			  ChessGamePlayer.
			  CreateFromFEN (game.
					 GetTagValue
					 ("FEN",
					  null))
			  : ChessGamePlayer.
			  CreatePlayer ();
			  player =
			  Chess.Game.
			  ChessGamePlayer.
			  CreatePlayer ();
			  foreach (PGNChessMove move in game.Moves)
			  {
			  player.Move (move.Move);}

			  buffer.Append (player.
					 GetPositionAsFEN
					 ());
			  buffer.Append (((nmoves + 1) / 2));
			  return buffer.ToString ();}
			  }
			  }
			  }
