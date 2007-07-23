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

		public string Name
		{
			get
			{
				return "ICS";
			}
		}
		private ICSClient client;

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

			client.Connect ();
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

	}
}
