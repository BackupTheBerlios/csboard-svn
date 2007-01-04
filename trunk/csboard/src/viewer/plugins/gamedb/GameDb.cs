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

using Chess.Parser;
using Chess.Game;

using System.Text;
using System.Collections;
using System.IO;
using System;

using com.db4o;
using com.db4o.query;

namespace CsBoard
{
	namespace Viewer
	{

		enum GameRating
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
			public PGNChessGame Game {get {return game;
						       }
						       }
						       PGNChessGame game;
						       int nmoves;
						       string white;
						       string black;
						       string result;
						       GameRating rating =
						       GameRating.Unknown;
						       public string Hash {
						       get {
						       return hash;
						       }
						       }
						       public string hash;
						       public
						       PGNGameDetails
						       (PGNChessGame game) {
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
						       result =
						       game.
						       GetTagValue ("Result",
								    "*");
						       hash =
						       GenerateHash ();}

						       private string
						       GenerateHash () {
						       StringBuilder buffer =
						       new StringBuilder ();
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
						       foreach (PGNChessMove
								move in game.
								Moves) {
						       player.Move (move.
								    Move);}

						       buffer.Append (player.
								      GetPositionAsFEN
								      ());
						       buffer.
						       Append (((nmoves +
								 1) / 2));
						       return buffer.
						       ToString ();}
						       }

						       public class GameDb {
						       ObjectContainer db;
						       const string DB_FILE =
						       "games.db";
						       static GameDb instance
						       = null;
						       public static GameDb
						       Instance {
						       get {
						       if (instance == null)
						       instance =
						       new GameDb ();
						       return instance;}
						       }

						       GameDb () {
						       string dir = "";
						       string gnomedir = "";
						       gnomedir =
						       Path.
						       Combine (Environment.
								GetEnvironmentVariable
								("HOME"),
								".gnome2");
						       if (!Directory.
							   Exists (gnomedir))
						       {
						       Directory.
						       CreateDirectory
						       (gnomedir);}

						       dir =
						       Path.Combine (gnomedir,
								     "csboard");
						       if (!Directory.
							   Exists (dir)) {
						       Directory.
						       CreateDirectory (dir);}

						       string dbfile =
						       Path.Combine (dir,
								     DB_FILE);
						       Db4o.Configure ().
						       MessageLevel (0);
						       Db4o.Configure ().
						       ObjectClass (typeof
								    (PGNGameDetails)).
						       ObjectField ("hash").
						       Indexed (true);
						       Db4o.Configure ().
						       ObjectClass (typeof
								    (PGNGameDetails)).
						       ObjectField ("rating").
						       Indexed (true);
						       db =
						       Db4o.
						       OpenFile (dbfile);}

						       // TODO: Check duplicates
						       public void
						       AddGame (PGNChessGame
								game) {
						       PGNGameDetails info =
						       new
						       PGNGameDetails (game);
						       if (GameExists (info))
						       {
						       return;}
						       db.Set (info);}

						       private bool
						       GameExists
						       (PGNGameDetails info) {
						       com.db4o.query.
						       Query query =
						       db.Query ();
						       query.
						       Constrain (typeof
								  (PGNGameDetails));
						       query.Descend ("hash").
						       Constrain (info.Hash).
						       Equal ();
						       ObjectSet res =
						       query.Execute ();
						       if (!res.HasNext ())
						       return false;
						       PGNChessGame game1 =
						       info.Game;
						       while (res.
							      HasNext ()) {
						       PGNChessGame game2 =
						       ((PGNGameDetails) res.
							Next ()).Game;
						       if (game1.Moves.
							   Count !=
							   game2.Moves.Count)
						       continue;
						       int i =
						       game1.Moves.Count - 1;
						       bool matched = true;
						       while (i >= 0) {
						       PGNChessMove move1 =
						       (PGNChessMove) game1.
						       Moves[i];
						       PGNChessMove move2 =
						       (PGNChessMove) game2.
						       Moves[i];
						       if (!move1.Move.
							   Equals (move2.
								   Move)) {
						       matched = false; break;}
						       i--;}

						       if (matched)
						       return true;}

						       // nothing matched (but the games had the same hash!)
						       return false;}

						       public void
						       AddGames (IList games)
						       {
						       foreach (PGNChessGame
								game in games)
						       {
						       AddGame (game);}
						       }

						       public void
						       DeleteAll () {
						       ObjectSet res =
						       db.
						       Get (typeof
							    (PGNGameDetails));
						       while (res.
							      HasNext ()) {
						       db.Delete (res.
								  Next ());}
						       }

						       public void
						       LoadGames (IList list)
						       {
						       ObjectSet res =
						       db.
						       Get (typeof
							    (PGNGameDetails));
						       while (res.
							      HasNext ()) {
						       PGNGameDetails details
						       =
						       (PGNGameDetails) res.
						       Next ();
						       list.Add (details.
								 Game);}
						       }

						       ~GameDb () {
						       db.Close ();}
						       }
						       }
						       }
