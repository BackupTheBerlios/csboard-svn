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
	namespace ICS
	{

		using System;
		using System.IO;
		using System.Collections;
		using Mono.Unix;

		using Gtk;
		using Glade;

		using System.Text;
		using System.Text.RegularExpressions;

		using CsBoard;

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
			GameObservationManager obManager;
			ObservableGamesWidget observableGames;
			GameAdvertisements ads;
			ICSShell shell;


			public ICS (string command)
			{
				client = new ICSClient ();

				try
				{
					string[]args =
						Regex.Split (command, " +");

					for (int i = 0; i < args.Length; i++)
					  {
						  if (args[i].
						      Equals ("--server"))
						    {
							    client.server =
								    args[i +
									 1];
						    }
						  if (args[i].
						      Equals ("--port"))
						    {
							    client.port =
								    args[i +
									 1];
						    }
						  if (args[i].
						      Equals ("--user"))
						    {
							    client.User =
								    args[i +
									 1];
						    }
						  if (args[i].
						      Equals ("--passwd"))
						    {
							    client.passwd =
								    args[i +
									 1];
						    }
					  }
				}
				catch
				{
					throw new
						ApplicationException (Catalog.
								      GetString
								      ("Can't parse command line"));
				}

				//Test();

				ICSConfigDialog dlg =
					new ICSConfigDialog (client);
				if (dlg.Run () != ResponseType.Ok)
				  {
					  throw new
						  ApplicationException
						  (Catalog.
						   GetString
						   ("No details to connect"));
				  }

				adWin = new ICSDetailsWindow (client,
							      String.
							      Format (Catalog.
								      GetString
								      ("ICS: {0}@{1}:{2}"),
								      client.
								      User,
								      client.
								      server,
								      client.
								      port));

				obManager =
					new GameObservationManager (client);

				observableGames =
					new ObservableGamesWidget (obManager);

				ads = new GameAdvertisements (client);
				adWin.Book.AppendPage (ads,
						 new Label (Catalog.
							    GetString
							    ("Game Advertisements")));

				adWin.Book.AppendPage (observableGames,
						       new Label (Catalog.
								  GetString
								  ("Observe Games")));

				shell = new ICSShell (client);
				adWin.Book.AppendPage (shell,
						 new Label (Catalog.
							    GetString
							    ("Shell")));


				client.AuthEvent += OnAuth;
				client.ConnectionErrorEvent += OnConnectionError;

				GLib.Idle.Add(delegate() {
					client.Connect ();
					client.Start ();
					return false;
				});

				adWin.Resize(500, 500);
				adWin.Book.Sensitive = false;
				adWin.Show ();
			}

			private void OnAuth(object o, bool successful) {
				if(successful) {
					adWin.Title = String.Format (Catalog.GetString("ICS: {0}@{1}:{2}"),client.User,client.server,client.port);
					adWin.Book.Sensitive = true;
					return;
				}

				// TODO: show error/relogin
			}

			private void OnConnectionError(object o, string reason) {
				client.Stop();
				Console.WriteLine(reason);
				// show error
			}

			static void Popup (string str)
			{
				byte[]buffer =
					System.Text.Encoding.ASCII.
					GetBytes (str);
				MoveDetails details =
					MoveDetails.FromBuffer (buffer, 4,
								buffer.
								Length);
				details.PrintTimeInfo ();
				ICSGameObserverWindow win = new ICSGameObserverWindow (null);
				win.Update(details);
				win.Show ();
			}

			static void Test ()
			{
				string str =
					"<12> r-r---k- ---nqpp- --p-pnp- b-Pp---- ---P-P-- --N-P-P- --QB--BP RR----K- B -1 0 0 0 0 4 134 GMPopov GMAkopian 0 120 0 34 34 3060 522 19 R/f1-b1 (14:29) Rfb1 0 1 0";
				str = "<12> r-r---k- ---nqpp- --p-pnp- b-Pp---- ---P-P-- --N-P-P- --QB--BP RR----K- B -1 0 0 0 0 4 134 GMPopov GMAkopian 0 120 0 34 34 3060 352 19 R/f1-b1 (14:29) Rfb1 0 0 0\n\n";
				Popup (str);
				str = "<12> rnbqkb-r pppppppp -----n-- -------- ---P---- -------- PPP-PPPP RNBQKBNR W -1 1 1 1 1 1 65 GuestSDSP uvsravikiran -1 3 0 39 39 180000 180000 2 N/g8-f6 (0:00.000) Nf6 1 1 0";
				Popup (str);

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
}
