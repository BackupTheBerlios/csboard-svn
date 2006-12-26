using System;
using System.Collections;
using System.IO;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class EcoDBPlugin:CsPlugin, IEcoDb
		{
			OpeningsDb db;

			public EcoDBPlugin ():base ("url-loader",
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
				Opening opening;
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
						("skipping this opening. econame = [{0}], name = [{1}]",
						 opening.ecoName,
						 opening.name);
			}
		}

		class OpeningsDb
		{
			Hashtable openings;
			public OpeningsDb ()
			{
				openings = new Hashtable ();
			}

			public void AddOpening (Opening opening)
			{
				ArrayList list;
				if (openings.ContainsKey (opening.ecoName))
					list = (ArrayList) openings[opening.
								    ecoName];
				else
					list = new ArrayList ();
				list.Add (opening);
				openings[opening.ecoName] = list;
			}

			public string GetName (string eco)
			{
				if (!openings.ContainsKey (eco)) {
					Console.WriteLine ("[{0}] not found",
							   eco);
					return null;
				}
				ArrayList list = (ArrayList) openings[eco];
				if (list.Count == 0)
					return null;
				return ((Opening) list[0]).name;
			}
		}

		struct Opening
		{
			public string ecoName;
			public string name;
			public string variation;
			public IList moves;
		}
	}
}
