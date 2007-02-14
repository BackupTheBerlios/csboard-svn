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
