using System;
using System.Collections;
using Gtk;
using System.IO;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;

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
							"PGN Buffer Loader",
							"Loads games from a PGN buffer")
			{
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				menuItem = new MenuItem ("Open buffer");
				menuItem.Activated += on_load_pgn_activate;
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
				return false;
			}

			private void LoadGamesFromBuffer (string buffer)
			{
				if (loadingInProgress)
					return;
				pgnBuffer = buffer;
				loadingInProgress = true;
				viewer.StatusBar.Push (1,
						       "Loading from buffer...");
				GLib.Idle.Add (new GLib.
					       IdleHandler
					       (LoadGamesIdleHandler));
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
				PGNBufferDialog dlg =
					new PGNBufferDialog (viewer.Window);
				if (dlg.Run () == (int) ResponseType.Accept)
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

				viewer.LoadGames (new
						  StringReader (pgnBuffer));

				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       "Showing games from buffer.");
				loadingInProgress = false;
				return false;
			}

			public class PGNBufferDialog:Dialog
			{
				TextView textView;

				public PGNBufferDialog (Gtk.
							Window
							par):base
					("Enter PGN", par, DialogFlags.Modal,
					 "Cancel", ResponseType.Cancel,
					 "Open", ResponseType.Accept)
				{
					textView = new TextView ();
					textView.WrapMode = WrapMode.WordChar;
					textView.Editable = true;
					textView.Show ();

					ScrolledWindow win =
						new ScrolledWindow ();
					  win.HscrollbarPolicy =
						PolicyType.Automatic;
					  win.VscrollbarPolicy =
						PolicyType.Automatic;
					  win.Child = textView;
					  win.Show ();
					  VBox.PackStart (win, true, true, 4);
				}

				public string Buffer
				{
					get
					{
						return textView.Buffer.Text;
					}
				}
			}
		}
	}
}
