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
		public class GameDb
		{
			ObjectContainer db;

			public ObjectContainer DB
			{
				get
				{
					return db;
				}
			}

			const string DB_FILE = "games.db";
			static GameDb instance = null;
			public static GameDb Instance
			{
				get
				{
					if (instance == null)
						instance = new GameDb ();
					return instance;
				}
			}

			GameDb ()
			{
				string dir = "";
				string gnomedir = "";
				  gnomedir =
					Path.
					Combine (Environment.
						 GetEnvironmentVariable
						 ("HOME"), ".gnome2");
				if (!Directory.Exists (gnomedir))
				  {
					  Directory.
						  CreateDirectory (gnomedir);
				  }

				dir = Path.Combine (gnomedir, "csboard");
				if (!Directory.Exists (dir))
				  {
					  Directory.CreateDirectory (dir);
				  }

				string dbfile = Path.Combine (dir, DB_FILE);
				// 1 - generate for specific classes only
				// Int32.MaxValue - generate for all classes
				Db4o.Configure ().GenerateUUIDs (1);
				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					GenerateUUIDs (true);
				Db4o.Configure ().MessageLevel (0);
				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("hash").Indexed (true);
				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("rating").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("white").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("black").Indexed (true);

				db = Db4o.OpenFile (dbfile);
			}

			public void AddGame (PGNChessGame game)
			{
				AddGame (game, GameRating.Unknown);
			}

			public void AddGame (PGNChessGame game,
					     GameRating rating)
			{
				PGNGameDetails info =
					new PGNGameDetails (game);
				info.Rating = rating;

				AddGame (info);
			}

			public void AddGame (PGNGameDetails info)
			{
				PGNGameDetails existing;
				if (!FindGame (info, out existing))
				  {
					  db.Set (info);
					  return;
				  }

				// Game found
				if (existing.Rating == info.Rating)
					return;
				existing.Rating = info.Rating;
				db.Set (existing);
			}

			private bool FindGame (PGNGameDetails info,
					       out PGNGameDetails match)
			{
				match = null;
				com.db4o.query.Query query = db.Query ();
				query.Constrain (typeof (PGNGameDetails));
				query.Descend ("hash").
					Constrain (info.Hash).Equal ();
				ObjectSet res = query.Execute ();
				if (!res.HasNext ())
					return false;
				PGNChessGame game1 = info.Game;
				while (res.HasNext ())
				  {
					  PGNGameDetails info2 =
						  (PGNGameDetails) res.
						  Next ();
					  PGNChessGame game2 = info2.Game;
					  if (game1.Moves.
					      Count != game2.Moves.Count)
						  continue;
					  int i = game1.Moves.Count - 1;
					  bool matched = true;
					  while (i >= 0)
					    {
						    PGNChessMove move1 =
							    (PGNChessMove)
							    game1.Moves[i];
						    PGNChessMove move2 =
							    (PGNChessMove)
							    game2.Moves[i];
						    if (!move1.Move.
							Equals (move2.Move))
						      {
							      matched = false;
							      break;
						      }
						    i--;
					    }

					  if (matched)
					    {
						    match = info2;
						    return true;
					    }
				  }

				// nothing matched (but the games had the same hash!)
				return false;
			}

			public void AddGames (IList games)
			{
				foreach (PGNChessGame game in games)
				{
					AddGame (game);
				}
			}

			public void DeleteAll ()
			{
				ObjectSet res =
					db.Get (typeof (PGNGameDetails));
				while (res.HasNext ())
				  {
					  db.Delete (res.Next ());
				  }
			}

			public void GetAllGames (IList list)
			{
				ObjectSet res =
					db.Get (typeof (PGNGameDetails));
				while (res.HasNext ())
				  {
					  list.Add (res.Next ());
				  }
			}

			public void LoadRatedGames (IList list,
						    GameRating aboveThis)
			{
				Query query = db.Query ();
				query.Constrain (typeof (PGNGameDetails));
				Query ratingQuery = query.Descend ("rating");
				ratingQuery.Constrain (aboveThis).Greater ();
				ratingQuery.OrderDescending ();

				ObjectSet res = query.Execute ();
				while (res.HasNext ())
				  {
					  PGNGameDetails details
						  =
						  (PGNGameDetails) res.
						  Next ();
					  list.Add (details);
				  }
			}

			~GameDb ()
			{
				db.Close ();
			}
		}
	}
}