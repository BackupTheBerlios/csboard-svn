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
					  App.session.ICSWinWidth = width;
					  App.session.ICSWinHeight = height;
					  Application.Quit ();
				};

				int width = App.session.ICSWinWidth;
				int height = App.session.ICSWinHeight;
				icsWindow.Resize (width, height);
			}

			protected void on_quit_activate (object o,
							 EventArgs args)
			{
				Application.Quit ();
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
		}
	}
}
