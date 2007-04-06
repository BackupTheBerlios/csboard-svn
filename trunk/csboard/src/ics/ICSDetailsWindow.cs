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

using Gtk;
using System;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ICSDetailsWindow
		{
			ICSClient client;

			[Glade.Widget] Window icsWindow;
			[Glade.Widget] Frame frame;
			[Glade.Widget] Gtk.MenuItem connectMenuItem,
				disconnectMenuItem;

			Notebook book;
			public Notebook Book
			{
				get
				{
					return book;
				}
			}
			public Window Window
			{
				get
				{
					return icsWindow;
				}
			}

			public ICSDetailsWindow (ICSClient client,
						 string title)
			{
				this.client = client;
				Glade.XML xml =
					Glade.XML.
					FromAssembly ("csboard.glade",
						      "icsWindow", null);
				xml.Autoconnect (this);
				book = new Notebook ();
				book.Show ();

				frame.Add (book);
				icsWindow.Title = title;

				icsWindow.DeleteEvent +=
					delegate (object o,
						  DeleteEventArgs args)
				{
					int width, height;
					  icsWindow.GetSize (out width,
							     out height);
					  App.Session.ICSWinWidth = width;
					  App.Session.ICSWinHeight = height;
					  App.Close ();
				};

				int width = App.Session.ICSWinWidth;
				int height = App.Session.ICSWinHeight;
				icsWindow.Resize (width, height);

				client.AuthEvent += OnAuth;
				client.ConnectionErrorEvent +=
					OnConnectionError;

				GLib.Idle.Add (delegate ()
					       {
					       Authenticate ();
					       return false;
					       }
				);

				book.Sensitive = false;
				disconnectMenuItem.Sensitive = false;
				icsWindow.Show ();
			}

			private void OnAuth (object o, bool successful)
			{
				if (successful)
				  {
					  disconnectMenuItem.Sensitive = true;
					  icsWindow.Title =
						  String.Format (Catalog.
								 GetString
								 ("ICS: {0}@{1}:{2}"),
								 client.User,
								 client.
								 server,
								 client.port);
					  book.Sensitive = true;
					  return;
				  }

				// on auth failure, reauthenticate
				Authenticate ();
			}

			private void Authenticate ()
			{
				connectMenuItem.Sensitive = false;
				book.Sensitive = false;
				ICSConfigDialog dlg =
					new ICSConfigDialog (client);
				if (dlg.Run () == ResponseType.Ok)
				  {
					  client.Start ();
				  }
				else
					connectMenuItem.Sensitive = true;

			}

			private void OnConnectionError (object o,
							string reason)
			{
				client.Stop ();
				// show error
				MessageDialog md =
					new MessageDialog (icsWindow,
							   DialogFlags.
							   DestroyWithParent,
							   MessageType.Error,
							   ButtonsType.Close,
							   String.
							   Format
							   ("<b>{0}</b>",
							    reason));

				md.Run ();
				md.Hide ();
				md.Dispose ();
			}

			protected void on_quit_activate (object o,
							 EventArgs args)
			{
				int width, height;
				icsWindow.GetSize (out width, out height);
				App.Session.ICSWinWidth = width;
				App.Session.ICSWinHeight = height;
				icsWindow.Hide ();
				icsWindow.Dispose ();
				App.Close ();
			}

			protected void on_about_activate (object o,
							  EventArgs args)
			{
				ChessWindow.ShowAboutDialog (icsWindow);
			}

			protected void on_edit_engines_activate (object o,
								 EventArgs
								 args)
			{
				ChessWindow.ShowEngineChooser ();
			}

			protected void on_connect_activate (object o,
							    EventArgs args)
			{
				Authenticate ();
			}

			protected void on_disconnect_activate (object o,
							       EventArgs args)
			{
				disconnectMenuItem.Sensitive = false;
				book.Sensitive = false;
				connectMenuItem.Sensitive = true;
				client.Stop ();
			}

			public void on_viewer_clicked (System.Object b,
						       EventArgs e)
			{
				App.StartViewer (null);
			}

			public void on_player_clicked (System.Object b,
						       EventArgs e)
			{
				try
				{
					App.StartPlayer (null);
				}
				catch
				{
					MessageDialog md =
						new MessageDialog (null,
								   DialogFlags.
								   DestroyWithParent,
								   MessageType.
								   Error,
								   ButtonsType.
								   Close,
								   Catalog.
								   GetString
								   ("Unknown engine"));
					md.Run ();
					md.Hide ();
					md.Dispose ();
				}
			}
		}
	}
}
