using System;
using System.Collections;
using System.IO;

using Gtk;
using Gnome.Vfs;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;

namespace CsBoard
{
	namespace Viewer
	{
		public class PGNUrlLoader:CsPlugin, IGameLoader
		{
			GameViewer viewer;
			MenuItem menuItem;
			string loadUrl;
			bool loadingInProgress;

			public PGNUrlLoader ():base ("url-loader",
						     "PGN URL Loader",
						     "Loads games from a PGN file from a url")
			{
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				menuItem = new MenuItem ("Open Url");
				menuItem.Activated += on_open_url_activate;
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

			public void on_open_url_activate (System.Object b,
							  EventArgs e)
			{
				string url = AskForUrl ();
				if (url == null)
					return;

				LoadGames (url);
			}

			private void LoadGames (string uri)
			{
				if (loadingInProgress)
					return;
				loadUrl = uri;
				loadingInProgress = true;
				viewer.StatusBar.Push (1, "Loading: " + uri);
				GLib.Idle.Add (new GLib.
					       IdleHandler
					       (LoadGamesIdleHandler));
			}

			private bool LoadGamesIdleHandler ()
			{
				if (loadUrl == null)
				  {
					  loadingInProgress = false;
					  viewer.StatusBar.Pop (1);
					  return false;
				  }
				VfsStream stream = new VfsStream (loadUrl, FileMode.Open);	// url
				//    ArrayList games = PGNParser.loadGamesFromFile(file);
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       "Read successfully. Parsing it...");
				ArrayList games = PGNParser.
					loadGamesFromStream (stream);
				stream.Close ();
				viewer.SetGames (games);
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1, "File: " + loadUrl);
				loadingInProgress = false;
				return false;
			}

			string AskForUrl ()
			{
				string url = null;
				UrlDialog dlg = new UrlDialog (viewer.Window);
				if (dlg.Run () == (int) ResponseType.Accept)
				  {
					  url = dlg.Url;
				  }
				dlg.Destroy ();
				return url;
			}
			public class UrlDialog:Dialog
			{
				Entry urlEntry;

				public UrlDialog (Gtk.
						  Window
						  par):base ("Open URL", par,
							     DialogFlags.
							     Modal, "Cancel",
							     ResponseType.
							     Cancel, "Open",
							     ResponseType.
							     Accept)
				{
					urlEntry = new Entry ();
					urlEntry.WidthChars = 80;
					urlEntry.Show ();
					VBox.PackStart (urlEntry, true, true,
							4);
				}

				public string Url
				{
					get
					{
						return urlEntry.Text;
					}
				}
			}
		}
	}
}
