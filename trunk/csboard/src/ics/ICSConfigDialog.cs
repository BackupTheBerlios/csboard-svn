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

namespace CsBoard {
	namespace ICS {
	class ICSConfigDialog
	{
		ICSClient client;
		[Glade.Widget] private Gtk.Dialog icsConfigDialog;
		[Glade.Widget] private Gtk.Entry serverNameEntry,
			portEntry, usernameEntry, passwordEntry;
		
		public ICSConfigDialog (ICSClient client)
		{
			this.client = client;
			Glade.XML xml =
				Glade.XML.
				FromAssembly ("csboard.glade",
					      "icsConfigDialog",
					      null);
			xml.Autoconnect (this);
			
			serverNameEntry.Text = client.server;
			portEntry.Text = client.port;
			usernameEntry.Text = client.user;
		}
		
		public ResponseType Run ()
		{
			ResponseType ret =
				(ResponseType) icsConfigDialog.Run ();
			if (ret != ResponseType.Ok)
				return ret;
			client.server = serverNameEntry.Text.Trim ();
			client.port = portEntry.Text.Trim ();
			client.user = usernameEntry.Text.Trim ();
			client.passwd = passwordEntry.Text;	// Dont Trim!
			icsConfigDialog.Hide ();
			
			return ret;
		}
		
		~ICSConfigDialog ()
		{
			icsConfigDialog.Dispose ();
		}
	}
	}
}
