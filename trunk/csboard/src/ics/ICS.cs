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
			GameAdvertisementGraph graph;

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

				graph = new GameAdvertisementGraph (client);
				icsWin.Book.AppendPage (graph,
							new Label (Catalog.
								   GetString
								   ("Seek Graph")));
				ads = new GameAdvertisements (client);
				icsWin.Book.AppendPage (ads,
							new Label (Catalog.
								   GetString
								   ("Game Seeks")));

				icsWin.Book.AppendPage (observableGames,
							new Label (Catalog.
								   GetString
								   ("Watch Games")));

				shell = new ICSShell (client);
				icsWin.Book.AppendPage (shell,
							new Label (Catalog.
								   GetString
								   ("Shell")));

				client.ChallengeEvent += OnChallengeEvent;
			}

			private void OnChallengeEvent(object o, MatchChallenge mc) {
			  Console.WriteLine(mc);
			  ShowChallengeDialog(mc);
			}

			public void Shutdown ()
			{
			}

			private void ShowChallengeDialog(MatchChallenge mc) {
			  StringBuilder buf = new StringBuilder();
			  string rating;

			  if(mc.OpponentsRating != 0)
			    rating = mc.OpponentsRating.ToString();
			  else
			    rating = "----";
			  buf.Append(String.Format("<big><b>{0} ({1}) wants to play a {2} game</b></big>\n",
						   mc.Opponent, rating, mc.Category));
			  buf.Append(String.Format(
						   "<b><u>Time:</u> {0} </b><i>mins</i>, <b><u>Increment:</u></b> {1}\n",
						   mc.Time,
						   mc.Increment
						   ));
			  if(mc.Color != null)
			    buf.Append(String.Format("\n<b><u>Color:</u></b> {0}\n", mc.Color));

			  buf.Append("\n\n<b>Do you want to play?</b>");

			  MessageDialog dlg = new MessageDialog(null,
								DialogFlags.Modal,
								MessageType.Question,
								ButtonsType.YesNo,
								true,
								buf.ToString());
			  dlg.Modal = false;
			  dlg.GrabFocus();
			  int ret = dlg.Run();
			  if(ret == (int) ResponseType.Yes)
			    client.CommandSender.SendCommand("accept");
			  else if(ret == (int) ResponseType.No)
			    client.CommandSender.SendCommand("decline");
			  dlg.Hide();
			  dlg.Dispose();
			}
		}
	}
}
