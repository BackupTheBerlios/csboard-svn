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
						      Catalog.GetString("PGN File Loader"),
						      Catalog.GetString("Loads games from a PGN file"))
			{
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				menuItem = new MenuItem (Catalog.GetString("Open File"));
				menuItem.Activated += on_open_file_activate;
				menuItem.Show ();
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
							  (Catalog.GetString("Exception: \n") +
							   e);
					  }
				  }
				return false;
			}

			public void on_open_file_activate (System.Object b,
							   EventArgs e)
			{
				file = viewer.AskForFile (viewer.Window, Catalog.GetString("Choose the file to open"), true);	// true for open
				if (file == null)
					return;

				LoadGames (file);
			}

			private void LoadGames (string file)
			{
				if (loadingInProgress)
					return;
				loadingInProgress = true;

				this.file = file;
				viewer.StatusBar.Push (1, Catalog.GetString("Loading: ") + file);
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
				viewer.StatusBar.Push (1, Catalog.GetString("File: ") + file);
				loadingInProgress = false;
				return false;
			}
		}
	}
}
