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
using System.IO;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;
using Gtk;

namespace CsBoard
{
	namespace Viewer
	{
		public class EcoDBPlugin:CsPlugin, IEcoDb
		{
			OpeningsDb db;
			GameViewer viewer;

			public EcoDBPlugin ():base ("ecodb",
						    Catalog.
						    GetString
						    ("ECO Database Plugin"),
						    Catalog.
						    GetString
						    ("Provides the ECO database"))
			{
			}

			public override bool Initialize ()
			{
				System.Reflection.Assembly exec =
					System.Reflection.Assembly.
					GetExecutingAssembly ();
				Stream stream =
					exec.
					GetManifestResourceStream ("eco.pgn");
				EcoDbLoader loader = new EcoDbLoader (stream);
				  db = loader.Openings;
				  GameViewer.EcoDb = this;
				  CsBoardApp.Instance.
				    AddApp (new OpeningBrowserUI (db));

				  return true;
			}

			public override bool Shutdown ()
			{
				return true;
			}

			public string GetOpeningName (string econame)
			{
				return db.GetName (econame);
			}
		}

		class EcoDbLoader
		{
			OpeningsDb db;
			public OpeningsDb Openings
			{
				get
				{
					return db;
				}
			}

			public EcoDbLoader (Stream filestream)
			{
				TextReader reader =
					new StreamReader (filestream);
				PGNParser parser = new PGNParser (reader);
				PGNGameLoader loader = new PGNGameLoader ();
				db = new OpeningsDb ();
				loader.GameLoaded += OnGameLoaded;
				parser.Parse (loader);
				reader.Close ();
			}

			private void OnGameLoaded (object o,
						   GameLoadedEventArgs args)
			{
				PGNChessGame game = args.Game;
				Opening opening = new Opening ();
				opening.ecoName =
					game.GetTagValue ("Site", null);
				opening.name =
					game.GetTagValue ("White", null);
				opening.variation =
					game.GetTagValue ("Black", null);
				opening.moves = game.Moves;
				if (opening.ecoName != null
				    && opening.name != null)
					db.AddOpening (opening);
				else
					Console.WriteLine
						(Catalog.
						 GetString
						 ("skipping this opening. econame = [{0}], name = [{1}]"),
						 opening.ecoName,
						 opening.name);
			}
		}
	}
}
