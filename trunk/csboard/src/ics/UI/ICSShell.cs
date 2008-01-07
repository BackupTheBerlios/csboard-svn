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

using System;
using Gtk;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ICSShell:VBox
		{
			Button sendButton;
			Entry commandEntry;
			ShellTextView textView;

			ICSClient client;

			public ICSShell (ICSClient client):base ()
			{
				this.client = client;
				textView = new ShellTextView (client);

				commandEntry = new Entry ();
				sendButton =
					new Button (Catalog.
						    GetString ("Send"));

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy =
					win.VscrollbarPolicy =
					PolicyType.Automatic;
				  win.Add (textView);

				  PackStart (win, true, true, 4);
				HBox box = new HBox ();
				  box.PackStart (commandEntry, true, true, 4);
				  box.PackStart (sendButton, false, false, 4);
				  PackStart (box, false, true, 4);

				  textView.Editable = false;

				  commandEntry.Activated += OnCommand;
				  sendButton.Clicked += OnCommand;
				  ShowAll ();
			}

			private void OnCommand (object o, EventArgs args)
			{
				string cmd = commandEntry.Text.Trim ();
				  commandEntry.Text = "";
				if (cmd.Length == 0)
					return;

				  client.CommandSender.SendCommand (cmd);
				  textView.AddCommandToBuffer (cmd);
			}
		}
	}
}
