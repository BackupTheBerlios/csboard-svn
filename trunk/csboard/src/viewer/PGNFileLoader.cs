using System;
using System.Collections;
using System.IO;

using Gtk;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;

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
						      "PGN File Loader",
						      "Loads games from a PGN file")
			{
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				menuItem = new MenuItem ("Open File");
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
				if (File.Exists (file)
				    && File.GetAttributes (file) ==
				    FileAttributes.Normal)
				  {
					  try
					  {
						  LoadGames (file);
					  }
					  catch (Exception e)
					  {
						  Console.WriteLine
							  ("Exception : \n" +
							   e);
					  }
				  }
				return false;
			}

			public void on_open_file_activate (System.Object b,
							   EventArgs e)
			{
				file = GameViewer.AskForFile (viewer.Window, "Choose the file to open", true);	// true for open
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
				viewer.StatusBar.Push (1, "Loading: " + file);
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

				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       "Read successfully. Parsing it...");
				Stream stream =
					new FileStream (file, FileMode.Open,
							FileAccess.Read);
				ArrayList games =
					PGNParser.
					loadGamesFromStream (stream);
				stream.Close ();

				viewer.SetGames (games);
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1, "File: " + file);
				loadingInProgress = false;
				return false;
			}
		}
	}
}
