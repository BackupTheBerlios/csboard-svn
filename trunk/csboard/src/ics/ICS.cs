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
//  Copyright (C) 2004 Jamin Gray

namespace CsBoard
{

	using System;
	using System.IO;
	using System.Collections;
	using Mono.Unix;

	using Gtk;
	using Glade;

	using System.Text;
	using System.Text.RegularExpressions;

	public class ICS:IControl
	{

		public event ControlBusyHandler BusyEvent
		{
			add
			{
			}
			remove
			{
			}
		}
		public event ControlWaitHandler WaitEvent
		{
			add
			{
			}
			remove
			{
			}
		}
		public event ControlPositionChangedHandler
			PositionChangedEvent
		{
			add
			{
			}
			remove
			{
			}
		}
		public event ControlGameOverHandler GameOverEvent
		{
			add
			{
			}
			remove
			{
			}
		}
		public event ControlSwitchSideHandler SwitchSideEvent
		{
			add
			{
			}
			remove
			{
			}
		}
		public event ControlHintHandler HintEvent
		{
			add
			{
			}
			remove
			{
			}
		}

		private ICSClient client;

		ICSDetailsWindow adWin;

		public ICS (string command)
		{
			client = new ICSClient ();

			try
			{
				string[]args = Regex.Split (command, " +");

				for (int i = 0; i < args.Length; i++)
				  {
					  if (args[i].Equals ("--server"))
					    {
						    client.server =
							    args[i + 1];
					    }
					  if (args[i].Equals ("--port"))
					    {
						    client.port = args[i + 1];
					    }
					  if (args[i].Equals ("--user"))
					    {
						    client.user = args[i + 1];
					    }
					  if (args[i].Equals ("--passwd"))
					    {
						    client.passwd =
							    args[i + 1];
					    }
				  }
			}
			catch
			{
				throw new ApplicationException (Catalog.
								GetString
								("Can't parse command line"));
			}

			ICSConfigDialog dlg = new ICSConfigDialog (client);
			if (dlg.Run () != ResponseType.Ok)
			  {
				  throw new ApplicationException (Catalog.
								  GetString
								  ("No details to connect"));
			  }

			adWin = new ICSDetailsWindow (client,
						      String.Format (Catalog.
								     GetString
								     ("ICS: {0}@{1}:{2}"),
								     client.
								     user,
								     client.
								     server,
								     client.
								     port));
			adWin.Show ();

			client.Connect ();
			client.Start ();
		}

		public void Shutdown ()
		{
		}

		public ArrayList GetPosition ()
		{
			ArrayList result = new ArrayList ();

			result.Add ("");
			result.Add ("");

			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");
			result.Add (". . . . . . . . .");

			return result;
		}


		public void NewGame ()
		{
			SeekDialog sd = new SeekDialog (client);
			sd.SeekNewGame ();
		}


		public bool MakeMove (string move)
		{
			return false;
		}


		public void SaveGame (string filename)
		{
			return;
		}

		public void OpenGame (string filename)
		{
			return;
		}

		public void SetLevel (Level l)
		{
		}

		public void Undo ()
		{

		}

		public void SwitchSide ()
		{
		}

		public void Hint ()
		{
		}

		public ArrayList Book ()
		{
			ArrayList result = new ArrayList ();

			return result;
		}

		public string PossibleMoves (string pos)
		{
			return "";
		}

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
