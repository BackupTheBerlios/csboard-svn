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
			AccelGroup accel;

			public PGNFileLoader ():base ("file-loader",
						      Catalog.
						      GetString
						      ("PGN File Loader"),
						      Catalog.
						      GetString
						      ("Loads games from a PGN file"))
			{
				accel = new AccelGroup ();
				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Open File"));
				  item.Image =
					new Image (Stock.Open, IconSize.Menu);
				  menuItem = item;
				  menuItem.Activated += on_open_file_activate;
				  menuItem.Show ();
				  menuItem.AddAccelerator ("activate", accel,
							   new AccelKey (Gdk.
									 Key.
									 o,
									 Gdk.
									 ModifierType.
									 ControlMask,
									 AccelFlags.
									 Visible));
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;
				viewer.Window.AddAccelGroup (accel);
				viewer.RegisterGameLoader (this, menuItem);
				return true;
			}

			public override bool Shutdown ()
			{
				viewer.Window.RemoveAccelGroup (accel);
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
						      (PGNFileFilterFunc));
				FileFilter[]filters = new FileFilter[]
				{
				pgn_filter, all_filter};
				file = viewer.AskForFile (viewer.Window, Catalog.GetString ("Choose the file to open"), true, filters);	// true for open
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
				GLib.Idle.Add (new GLib.
					       IdleHandler
					       (LoadGamesIdleHandler));
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
				viewer.LoadGames (reader);
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
