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

		public class ICS
		{
			private ICSClient client;

			ICSDetailsWindow icsWin;
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

				icsWin = new ICSDetailsWindow (client,
							       String.
							       Format
							       (Catalog.
								GetString
								("ICS: {0}@{1}:{2}"),
								client.User,
								client.server,
								client.port));

				obManager =
					new GameObservationManager (client);

				observableGames =
					new ObservableGamesWidget (obManager);

				ads = new GameAdvertisements (client);
				icsWin.Book.AppendPage (ads,
							new Label (Catalog.
								   GetString
								   ("Game Advertisements")));

				icsWin.Book.AppendPage (observableGames,
							new Label (Catalog.
								   GetString
								   ("Observe Games")));

				shell = new ICSShell (client);
				icsWin.Book.AppendPage (shell,
							new Label (Catalog.
								   GetString
								   ("Shell")));


			}

			public void Shutdown ()
			{
			}
		}
	}
}
