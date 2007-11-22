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
		public class OpeningBrowserUI:VBox, SubApp
		{
			OpeningsDb db;
			GameViewerBoard boardWidget;
			TreeView view;
			TreeStore store;

			AppMenuBar menubar;
			public MenuBar MenuBar
			{
				get
				{
					return menubar;
				}
			}

			public Widget Widget
			{
				get
				{
					return this;
				}
			}

			ToolButton toolbutton;
			public ToolButton ToolButton
			{
				get
				{
					return toolbutton;
				}
			}

			AccelGroup accel;
			public AccelGroup AccelGroup
			{
				get
				{
					return accel;
				}
			}

			string title;
			public string Title
			{
				get
				{
					return title;
				}
			}

		public string ID
		{
			get
			{
			  return "ecodb";
			}
		}

			public void SetVisibility (bool visible)
			{
			}

			public event TitleChangedEventHandler
				TitleChangedEvent;

			public OpeningBrowserUI (OpeningsDb db):base ()
			{
				menubar = new AppMenuBar ();
				title = Catalog.GetString ("Opening Browser");
				accel = new AccelGroup ();
				menubar.quitMenuItem.
					AddAccelerator ("activate", accel,
							new AccelKey (Gdk.Key.
								      q,
								      Gdk.
								      ModifierType.
								      ControlMask,
								      AccelFlags.
								      Visible));
				toolbutton = new ToolButton (Stock.Info);
				toolbutton.Label =
					Catalog.GetString ("Openings");
				toolbutton.ShowAll ();

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
						   "markup", 2);

				ScrolledWindow win = new ScrolledWindow ();
				win.SetPolicy (PolicyType.Automatic,
					       PolicyType.Automatic);
				win.Add (view);

				boardWidget = new GameViewerBoard ();
				boardWidget.showAnimations = false;
				HPaned split = new HPaned ();
				VBox box = new VBox ();
				box.PackStart (boardWidget, true, true, 2);
				split.Pack1 (box, false, true);	// resize, shrink
				split.Pack2 (win, true, true);
				split.ShowAll ();
				//split.Position = 400;
				int width, height;
				CsBoardApp.Instance.Window.GetSize (out width,
								    out
								    height);
				split.Position =
					(int) Math.Round (width * 0.5f);
				split.PositionSet = true;
				PackStart (split, true, true, 2);

				view.CursorChanged += OnCursorChanged;
				ShowAll ();
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

		public class GameViewerBoard:CairoViewerBoard
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
				Move (r1, f1, r2, f2, ' ', true);
				SetPosition (player.GetPosition ());
			}
		}
	}
}
