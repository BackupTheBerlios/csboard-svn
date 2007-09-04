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
using Gtk;
using System.IO;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class PGNBufferLoader:CsPlugin, IGameLoader
		{
			GameViewer viewer;
			string pgnBuffer;
			MenuItem menuItem;
			bool loadingInProgress;

			public PGNBufferLoader ():base ("buffer-loader",
							Catalog.
							GetString
							("PGN Buffer Loader"),
							Catalog.
							GetString
							("Loads games from a PGN buffer"))
			{
				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("Open _buffer"));
				  item.Image =
					new Image (Stock.Open, IconSize.Menu);
				  menuItem = item;
				  menuItem.Activated += on_load_pgn_activate;
				  menuItem.Show ();
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				menuItem.AddAccelerator ("activate",
							 viewer.AccelGroup,
							 new AccelKey (Gdk.
								       Key.b,
								       Gdk.
								       ModifierType.
								       ControlMask,
								       AccelFlags.
								       Visible));
				viewer.RegisterGameLoader (this, menuItem);
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.UnregisterGameLoader (this, menuItem);
				return true;
			}

			public bool Load (string file)
			{
				return false;
			}

			private void LoadGamesFromBuffer (string buffer)
			{
				if (loadingInProgress)
					return;
				pgnBuffer = buffer;
				loadingInProgress = true;
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Loading from buffer..."));
				LoadGamesIdleHandler ();
			}

			public void on_load_pgn_activate (System.Object b,
							  EventArgs e)
			{
				string buffer = AskForPGNBuffer ();
				if (buffer == null)
					return;

				LoadGamesFromBuffer (buffer);
			}

			string AskForPGNBuffer ()
			{
				string buffer = null;
				BufferDialog dlg = new BufferDialog (null,
								     Catalog.
								     GetString
								     ("Enter PGN"));
				if (dlg.Run () == (int) ResponseType.Ok)
				  {
					  buffer = dlg.Buffer;
				  }
				dlg.Destroy ();
				return buffer;
			}

			private bool LoadGamesIdleHandler ()
			{
				if (pgnBuffer == null)
				  {
					  loadingInProgress = false;
					  return false;
				  }

				StringReader reader = new
					StringReader (pgnBuffer);

				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Parsing from buffer..."));

				viewer.GameLoader.Load (reader);
				reader.Close ();

				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Showing games from buffer."));
				loadingInProgress = false;
				return false;
			}
		}
	}
}
