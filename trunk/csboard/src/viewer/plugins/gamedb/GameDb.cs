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
using com.db4o.ext;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameDb : IGameDb
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
					ObjectField ("id").Indexed (true);
				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("rating").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("white").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("black").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					ObjectField ("tags").Indexed (true);

				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					CascadeOnDelete (true);
				Db4o.Configure ().
					ObjectClass (typeof (PGNGameDetails)).
					CascadeOnUpdate (true);

				Db4o.Configure ().
					ObjectClass (typeof (GameCollection)).
					CascadeOnDelete (true);
				Db4o.Configure ().
					ObjectClass (typeof (GameCollection)).
					CascadeOnUpdate (true);

				db = Db4o.OpenFile (dbfile);
			}

			public void AddGame (ChessGame game)
			{
				AddGame (game, GameRating.Unknown);
			}

			public void AddGame (ChessGame game,
					     GameRating rating)
			{
				PGNGameDetails info;
				if(game is PGNGameDetails)
					info = (PGNGameDetails) game;
				else
					info = new PGNGameDetails (game);
				info.Rating = rating;

				AddGame (info);
			}

			public void AddGame (PGNGameDetails info)
			{
				PGNGameDetails existing;
				if (!FindGame (info, out existing))
				  {
					  info.ID = Config.Instance.NextID ();
					  db.Set (info);
					  Config.Instance.Save ();
					  return;
				  }

				// Game found
				if (existing.Rating == info.Rating)
					return;
				existing.Rating = info.Rating;
				db.Set (existing);
			}

			public Config LoadConfig ()
			{
				ObjectSet res = db.Get (typeof (Config));
				if (res.HasNext ())
					return (Config) res.Next ();
				return null;
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
				ChessGame game1 = info;
				while (res.HasNext ())
				  {
					  PGNGameDetails info2 =
						  (PGNGameDetails) res.
						  Next ();
					  ChessGame game2 = info2;
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

			public void AddCollection (GameCollection col)
			{
				db.Set (col);
				db.Commit ();
			}

			public void DeleteCollection (GameCollection col)
			{
				db.Delete (col);
				db.Commit ();
			}

			public void GetGameCollections (IList list,
							string filter)
			{
				ObjectSet res;
				if (filter == null)
				  {
					  res = db.
						  Get (typeof
						       (GameCollection));
				  }
				else
				  {
					  com.db4o.query.Query query =
						  db.Query ();
					  query.Constrain (typeof
							   (GameCollection));
					  query.Descend ("title").
						  Constrain (filter).Like ();
					  res = query.Execute ();
				  }

				while (res.HasNext ())
				  {
					  list.Add (res.Next ());
				  }
			}

			public void AddGames (IList games)
			{
				foreach (ChessGame game in games)
				{
					AddGame (game);
				}
				db.Commit ();
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

			public void SaveGameDetails (ChessGame game, bool overrite) {
				if(!(game is PGNGameDetails))
					game = new PGNGameDetails(game);
				AddGame(game); // this will check for duplicates
			}

			public bool GetGameDetails (ChessGame game,
					     out ChessGame details) {
				PGNGameDetails gamedetails;
				if(!(game is PGNGameDetails))
					gamedetails = new PGNGameDetails(game);
				else
					gamedetails = (PGNGameDetails) game;

				PGNGameDetails dbgame;
				bool ret = FindGame(gamedetails, out dbgame);
				details = dbgame;
				return ret;
			}

			~GameDb ()
			{
				db.Close ();
			}
		}

		public class GameCollection
		{
			string title;
			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			string description;
			public string Description
			{
				get
				{
					return description;
				}

				set
				{
					description = value;
				}
			}

			IList ids;
			public IList Games
			{
				get
				{
					return ids;
				}
			}

			public GameCollection (string title,
					       string description,
					       ArrayList list)
			{
				this.title = title;
				this.description = description;
				ids = list;
			}

			public void AddGame (PGNGameDetails details)
			{
				if (!ids.Contains (details.ID))
					ids.Add (details.ID);
			}

			public void RemoveGame (PGNGameDetails details)
			{
				ids.Remove (details.ID);
			}

			public void LoadGames (ArrayList list)
			{
				foreach (int id in ids)
				{
					Query query =
						GameDb.Instance.DB.Query ();
					query.Constrain (typeof
							 (PGNGameDetails));
					Query idQuery = query.Descend ("id");
					idQuery.Constrain (id).Equal ();
					ObjectSet set = query.Execute ();
					if (set.HasNext ())
						list.Add (set.Next ());
				}
			}

			public override string ToString ()
			{
				StringBuilder buf = new StringBuilder ();
				buf.Append (String.
					    Format ("Title: {0}\n\t{1} games",
						    title, ids.Count));
				return buf.ToString ();
			}
		}

		public class Config
		{
			int nextid;
			  Config ()
			{
			}

			public int NextID ()
			{
				return ++nextid;
			}

			static Config instance;
			public static Config Instance
			{
				get
				{
					if (instance == null)
						instance = LoadInstance ();
					return instance;
				}
			}

			private static Config LoadInstance ()
			{
				Config config = GameDb.Instance.LoadConfig ();
				if (config == null)
					config = CreateNew ();
				return config;
			}

			private static Config CreateNew ()
			{
				return new Config ();
			}

			public void Save ()
			{
				GameDb.Instance.DB.Set (this);
			}
		}
	}
}
