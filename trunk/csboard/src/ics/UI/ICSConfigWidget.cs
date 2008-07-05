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
		public delegate void ResponseEventHandler (object o,
							   ResponseType type);

		public delegate void ConfigResponseEventHandler (object o,
								 ResponseType
								 r);
		class ICSConfigWidget:Alignment
		{
			ICSClient client;
			Entry serverNameEntry,
				portEntry, usernameEntry, passwordEntry;
			CheckButton guestLoginCheckButton;
			public event ConfigResponseEventHandler
				ConfigResponseEvent;

			public ICSConfigWidget (ICSClient client):base (0.5f,
									0.5f,
									0, 0)
			{
				this.client = client;

				DecorateUI ();
				guestLoginCheckButton.Active = true;
				SetGuestLogin (guestLoginCheckButton.Active);
				guestLoginCheckButton.Toggled +=
					delegate (object o, EventArgs args)
				{
					SetGuestLogin (guestLoginCheckButton.
						       Active);
				};
				  serverNameEntry.Text = client.server;
				  portEntry.Text = client.port;
				  usernameEntry.Text = client.User;

				  ShowAll ();
			}

			private void AttachToTable (Table table,
						    Widget widget, uint x,
						    uint y, uint width)
			{
				//                      table.Attach(widget, x, x + width, y, y + 1, AttachOptions.Fill, AttachOptions.Fill, 2, 2);
				table.Attach (widget, x, x + width, y, y + 1);
			}

			private void DecorateUI ()
			{
				HButtonBox actionArea = new HButtonBox ();

				actionArea.LayoutStyle = ButtonBoxStyle.End;
				Button button = new Button (Stock.Ok);
				button.Clicked += OnActivated;
				actionArea.PackStart (button,
						      false, false, 4);

				serverNameEntry = new Entry ();
				portEntry = new Entry ();
				usernameEntry = new Entry ();
				passwordEntry = new Entry ();
				passwordEntry.Visibility = false;
				passwordEntry.Activated += OnActivated;

				guestLoginCheckButton = new CheckButton ();

				Table table = new Table (5, 2, false);
				uint row = 0;
				// left, right, top, bottom
				Label label =
					new Label (Catalog.
						   GetString ("Server"));
				label.Xalign = 0;
				uint width = 1;
				AttachToTable (table, label, 0, row, width);
				AttachToTable (table, serverNameEntry, 1, row,
					       width);
				row++;

				label = new Label (Catalog.
						   GetString ("Port"));
				label.Xalign = 0;
				AttachToTable (table, label, 0, row, 1);
				AttachToTable (table, portEntry, 1, row, 1);
				row++;

				label = new Label (Catalog.
						   GetString ("Guest Login"));
				label.Xalign = 0;
				AttachToTable (table, label, 0, row, 1);
				AttachToTable (table, guestLoginCheckButton,
					       1, row, 1);
				row++;

				label = new Label (Catalog.
						   GetString ("Username"));
				label.Xalign = 0;
				AttachToTable (table, label, 0, row, 1);
				AttachToTable (table, usernameEntry, 1, row,
					       1);
				row++;

				label = new Label (Catalog.
						   GetString ("Password"));
				label.Xalign = 0;
				AttachToTable (table, label, 0, row, 1);
				AttachToTable (table, passwordEntry, 1, row,
					       1);
				row++;

				label = new Label (Catalog.
						   GetString
						   ("<big><big><b>Login</b></big></big>"));
				label.Xalign = 0;
				label.Xpad = 2;
				label.UseMarkup = true;
				VBox vbox = new VBox ();
				vbox.PackStart (label, false, true, 4);
				vbox.PackStart (table, false, false, 4);
				vbox.PackStart (actionArea, false, true, 4);
				Child = vbox;
			}

			void OnActivated (object o, EventArgs args)
			{
				if (ConfigResponseEvent != null)
					ConfigResponseEvent (this,
							     ResponseType.Ok);
				client.server = serverNameEntry.Text.Trim ();
				client.port = portEntry.Text.Trim ();
				client.User = usernameEntry.Text.Trim ();
				client.passwd = passwordEntry.Text;	// Dont Trim!
			}

			private void SetGuestLogin (bool status)
			{
				if (status)
				  {
					  client.User = "guest";
					  usernameEntry.Sensitive = false;
					  passwordEntry.Sensitive = false;
				  }
				else
				  {
					  usernameEntry.Sensitive = true;
					  passwordEntry.Sensitive = true;
				  }
			}
		}
	}
}
