using System;
using System.Collections;
using System.IO;

using Gtk;
using Gnome.Vfs;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;

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
						     Catalog.
						     GetString
						     ("PGN URL Loader"),
						     Catalog.
						     GetString
						     ("Loads games from a PGN file from a url"))
			{
				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("Open _URL"));
				  item.Image =
					new Image (Stock.Open, IconSize.Menu);
				  menuItem = item;
				  menuItem.Activated += on_open_url_activate;
				  menuItem.Show ();
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;
				Gnome.Vfs.Vfs.Initialize ();

				menuItem.AddAccelerator ("activate",
							 viewer.AccelGroup,
							 new AccelKey (Gdk.
								       Key.u,
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
				TextReader reader = new StreamReader (new VfsStream (loadUrl, FileMode.Open));	// url
				viewer.LoadGames (reader);
				reader.Close ();
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString ("URL: ") +
						       loadUrl);
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
						  par):base (Catalog.
							     GetString
							     ("Open URL"),
							     par,
							     DialogFlags.
							     Modal,
							     Catalog.
							     GetString
							     ("Cancel"),
							     ResponseType.
							     Cancel,
							     Catalog.
							     GetString
							     ("Open"),
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
