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
			  Gtk.MenuItem item;
			ToolButton ecodbToolButton;

			public EcoDBPlugin ():base ("ecodb",
						    Catalog.
						    GetString
						    ("ECO Database Plugin"),
						    Catalog.
						    GetString
						    ("Provides the ECO database"))
			{
				item = new MenuItem (Catalog.
						     GetString
						     ("_Opening Browser"));
				item.Activated +=
					on_view_opening_browser_activate;
				item.Show ();
			}

			private void on_view_opening_browser_activate (object
								       o,
								       EventArgs
								       args)
			{
				Dialog dlg =
					new OpeningBrowser (viewer.Window,
							    db);
				  dlg.ShowAll ();
				  dlg.Run ();
				  dlg.Hide ();
				  dlg.Dispose ();
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				System.Reflection.Assembly exec =
					System.Reflection.Assembly.
					GetExecutingAssembly ();
				Stream stream =
					exec.
					GetManifestResourceStream ("eco.pgn");
				EcoDbLoader loader = new EcoDbLoader (stream);
				  db = loader.Openings;
				  GameViewer.EcoDb = this;
				  viewer.AddToViewMenu (item);

				  ecodbToolButton =
					new ToolButton (Stock.Info);
				  ecodbToolButton.Label =
					Catalog.GetString ("Openings");
				  ecodbToolButton.Clicked +=
					on_view_opening_browser_activate;
				  ecodbToolButton.Show ();
				  viewer.Toolbar.Insert (ecodbToolButton,
							 viewer.Toolbar.
							 NItems);

				  return true;
			}

			public override bool Shutdown ()
			{
				viewer.RemoveFromViewMenu (item);
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

		public class OpeningBrowser:Dialog
		{
			OpeningsDb db;
			GameViewerBoard boardWidget;
			TreeView view;
			TreeStore store;
			public OpeningBrowser (Window parent,
					       OpeningsDb db):base (Catalog.
								    GetString
								    ("Opening Browser"),
								    parent,
								    DialogFlags.
								    DestroyWithParent,
								    Catalog.
								    GetString
								    ("Close"),
								    ResponseType.
								    Close)
			{
				this.db = db;
				store = new TreeStore (typeof (string),
						       typeof (int),
						       typeof (string));
				  this.db.PopulateTree (store);
				  view = new TreeView ();
				  view.Model = store;
				  view.AppendColumn (Catalog.
						     GetString ("Moves"),
						     new CellRendererText (),
						     "text", 0);
				  view.AppendColumn (Catalog.
						     GetString ("Variations"),
						     new CellRendererText (),
						     "text", 1);
				  view.AppendColumn (Catalog.
						     GetString ("Name"),
						     new CellRendererText (),
						     "text", 2);

				ScrolledWindow win = new ScrolledWindow ();
				  win.SetPolicy (PolicyType.Automatic,
						 PolicyType.Automatic);
				  win.Add (view);

				  boardWidget = new GameViewerBoard ();
				HPaned split = new HPaned ();
				VBox box = new VBox ();
				  box.PackStart (boardWidget, false, true, 2);
				  split.Pack1 (box, false, true);	// resize, shrink
				  split.Pack2 (win, true, true);
				  split.ShowAll ();
				  split.Position = 250;
				  split.PositionSet = true;
				  VBox.PackStart (split, true, true, 2);
				  SetSizeRequest (700, 300);

				  view.CursorChanged += OnCursorChanged;
			}

			private void OnCursorChanged (object o,
						      System.EventArgs args)
			{
				TreePath path;
				TreeViewColumn col;
				view.GetCursor (out path, out col);
				ArrayList moves = new ArrayList ();

				TreePath tmppath = new TreePath ();
				foreach (int i in path.Indices)
				{
					tmppath.AppendIndex (i);
					TreeIter iter;
					store.GetIter (out iter, tmppath);
					moves.Add (store.GetValue (iter, 0));
				}

				boardWidget.PlayMoves (moves);
			}
		}

		public class GameViewerBoard:ViewerBoard
		{
			public GameViewerBoard ():base (ChessGamePlayer.
							GetDefaultPosition ())
			{
			}

			public void PlayMoves (ArrayList moves)
			{
				ChessGamePlayer player =
					ChessGamePlayer.CreatePlayer ();
				foreach (string move in moves)
				{
					player.Move (move);
				}
				int r1, f1, r2, f2;
				r1 = player.LastMoveInfo.src_rank;
				f1 = player.LastMoveInfo.src_file;
				r2 = player.LastMoveInfo.dest_rank;
				f2 = player.LastMoveInfo.dest_file;
				Move (r1, f1, r2, f2, ' ');
				SetPosition (player.GetPosition ());
			}
		}
	}
}
