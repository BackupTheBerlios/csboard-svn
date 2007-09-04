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

using Gtk;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;

using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class PGNFileLoader:CsPlugin, IGameLoader
		{
			GameViewer viewer;
			MenuItem menuItem;
			string file;
			bool loadingInProgress;

			public PGNFileLoader ():base ("file-loader",
						      Catalog.
						      GetString
						      ("PGN File Loader"),
						      Catalog.
						      GetString
						      ("Loads games from a PGN file"))
			{
				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Open File"));
				  item.Image =
					new Image (Stock.Open, IconSize.Menu);
				  menuItem = item;
				  menuItem.Activated += on_open_file_activate;
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
								       Key.o,
								       Gdk.
								       ModifierType.
								       ControlMask,
								       AccelFlags.
								       Visible));
				//viewer.Window.AddAccelGroup (accel);
				viewer.RegisterGameLoader (this, menuItem);
				return true;
			}

			public override bool Shutdown ()
			{
				//viewer.Window.RemoveAccelGroup (accel);
				viewer.UnregisterGameLoader (this, menuItem);
				return true;
			}

			public bool Load (string file)
			{
				if (File.Exists (file))
				  {
					  try
					  {
						  LoadGames (file);
						  return true;
					  }
					  catch (Exception e)
					  {
						  Console.WriteLine
							  (Catalog.
							   GetString
							   ("Exception: \n") +
							   e);
					  }
				  }
				return false;
			}

			public void on_open_file_activate (System.Object b,
							   EventArgs e)
			{
				FileFilter pgn_filter = new FileFilter ();
				pgn_filter.Name =
					Catalog.GetString ("PGN Files");
				pgn_filter.AddCustom (FileFilterFlags.
						      Filename,
						      new
						      FileFilterFunc
						      (PGNFileFilterFunc));
				FileFilter all_filter = new FileFilter ();
				all_filter.Name =
					Catalog.GetString ("All Files");
				all_filter.AddCustom (FileFilterFlags.
						      Filename,
						      new
						      FileFilterFunc
						      (AllFileFilterFunc));
				FileFilter[]filters = new FileFilter[]
				{
				pgn_filter, all_filter};
				file = viewer.AskForFile (null, Catalog.GetString ("Choose the file to open"), true, filters);	// true for open
				if (file == null)
					return;

				LoadGames (file);
			}

			private bool PGNFileFilterFunc (FileFilterInfo info)
			{
				return info.Filename.ToLower ().
					EndsWith (".pgn");
			}

			private bool AllFileFilterFunc (FileFilterInfo info)
			{
				return true;
			}

			private void LoadGames (string file)
			{
				if (loadingInProgress)
					return;
				loadingInProgress = true;

				this.file = file;
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString ("Loading: ")
						       + file);
				LoadGamesIdleHandler ();
			}

			private bool LoadGamesIdleHandler ()
			{
				if (file == null)
				  {
					  loadingInProgress = false;

					  return false;
				  }

				TextReader reader =
					new StreamReader (new
							  FileStream (file,
								      FileMode.
								      Open,
								      FileAccess.
								      Read));
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Parsing the file..."));

				viewer.GameLoader.Load (reader);
				reader.Close ();

				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString ("File: ") +
						       file);
				loadingInProgress = false;
				return false;
			}
		}
	}
}
