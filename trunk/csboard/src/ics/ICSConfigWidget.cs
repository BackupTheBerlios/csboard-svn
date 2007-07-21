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
		class DialogWidget:VBox
		{
			VBox box;
			HButtonBox actionArea;
			int[] response_ids;
			  Button[] buttons;

			bool awaitingResponse = false;
			int response;

			public VBox VBox
			{
				get
				{
					return box;
				}
			}

			public event ResponseEventHandler ResponseEvent;

			public DialogWidget (params object[]data):base ()
			{
				box = new VBox ();
				actionArea = new HButtonBox ();
				actionArea.LayoutStyle = ButtonBoxStyle.End;
				buttons = new Button[data.Length / 2];
				response_ids = new int[data.Length / 2];

				for (int i = 0; i < data.Length; i += 2)
				  {
					  Button button =
						  new Button (data[i] as
							      string);
					    button.Clicked += OnClicked;
					    actionArea.PackStart (button,
								  false,
								  false, 4);
					    buttons[i / 2] = button;
					    response_ids[i / 2] =
						  (int) data[i + 1];
				  }

				PackStart (box, true, true, 4);
				  PackStart (actionArea, false, true, 4);
				  ShowAll ();
			}

			private void OnClicked (object o, EventArgs args)
			{
				int i = 0;
				foreach (Button b in buttons)
				{
					if (b.Equals (o))
					  {
						  awaitingResponse = false;
						  response = response_ids[i];
						  if (ResponseEvent != null)
							  ResponseEvent (this,
									 (ResponseType)
									 response_ids
									 [i]);
						  break;
					  }
					i++;
				}
			}

			public void Respond (int resp)
			{
				response = resp;
				awaitingResponse = false;
			}

			public virtual int Run ()
			{
				awaitingResponse = true;
				response = -1;
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();

				while (awaitingResponse)
				  {
					  // process the events here. basically waiting till the buttons
					  // are clicked
					  bool ret =
						  Gtk.Application.
						  RunIteration (true);
					  if (ret)
					    {
						    Application.Quit ();
						    break;
					    }
				  }
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();

				return response;
			}
		}

		class ICSConfigWidget:Alignment
		{
			ICSClient client;
			Entry serverNameEntry,
				portEntry, usernameEntry, passwordEntry;
			CheckButton guestLoginCheckButton;
			DialogWidget widget;

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

				Child = widget;
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
				widget = new DialogWidget (Stock.Ok,
							   ResponseType.Ok);

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
				widget.VBox.PackStart (label, false, true, 4);
				widget.VBox.PackStart (table, false, false,
						       4);
			}

			void OnActivated (object o, EventArgs args)
			{
				widget.Respond ((int) ResponseType.Ok);
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

			public int Run ()
			{
				int ret = widget.Run ();
				if (ret == (int) ResponseType.Ok)
				  {
					  client.server =
						  serverNameEntry.Text.
						  Trim ();
					  client.port =
						  portEntry.Text.Trim ();
					  client.User =
						  usernameEntry.Text.Trim ();
					  client.passwd = passwordEntry.Text;	// Dont Trim!
				  }

				return ret;
			}
		}
	}
}
