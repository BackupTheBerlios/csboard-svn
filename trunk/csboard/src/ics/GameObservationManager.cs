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
using System.Collections;

namespace CsBoard
{
	namespace ICS
	{
		public delegate void ObservableGameEventHandler (object o,
								 GameDetails
								 gd);

		public class GameObservationManager
		{
			ICSClient client;
			Hashtable currentGames;
			bool expecting_results;
			bool first_result;

			public event ObservableGameEventHandler
				ObservableGameEvent;

			public void ObserveGame (int gameid)
			{
				client.WriteLine ("observe " + gameid);
			}

			public GameObservationManager (ICSClient client)
			{
				expecting_results = false;
				currentGames = new Hashtable ();
				this.client = client;
				client.MoveMadeEvent += OnMoveMade;
				client.ResultNotificationEvent +=
					OnResultNotification;
			}

			public void GetGames ()
			{
				if (expecting_results)
					return;
				client.WriteLine ("games");
				client.LineBufferReceivedEvent +=
					OnLineBufferReceived;
				expecting_results = true;
				first_result = false;
			}

			private void OnLineBufferReceived (object o,
							   LineBufferReceivedEventArgs
							   args)
			{
				if (!expecting_results)
					return;
				try
				{
					ProcessGameDetails (args);
					first_result = true;
				}
				catch (Exception e)
				{
					if (first_result)
						RequestDone ();
				}
			}

			private void RequestDone ()
			{
				client.LineBufferReceivedEvent -=
					OnLineBufferReceived;
				expecting_results = false;
				first_result = false;
			}

			private void
				ProcessGameDetails
				(LineBufferReceivedEventArgs args)
			{
				if (first_result
				    && args.LineType == LineType.Prompt)
				  {
					  RequestDone ();
					  return;
				  }

				if (args.LineType != LineType.Normal)
					return;

				GameDetails details =
					GameDetails.FromBuffer (args.Buffer,
								args.Start,
								args.End);
				if (ObservableGameEvent != null)
				  {
					  ObservableGameEvent (this, details);
				  }
			}

			private void OnMoveMade (object o,
						 MoveMadeEventArgs args)
			{
				MoveDetails details = args.Details;

				if (details.relation != Relation.IamObserving
				    && details.relation !=
				    Relation.IamObservingGameBeingObserved)
					return;
				ICSGameObserverWindow win;
				if (currentGames.
				    ContainsKey (details.gameNumber))
					win = (ICSGameObserverWindow)
						currentGames[details.
							     gameNumber];
				else
				  {
					  win = new
						  ICSGameObserverWindow
						  (details);
					  currentGames[details.gameNumber] =
						  win;
					  win.DeleteEvent += OnDelete;
					  win.Resize (500, 400);
					  win.Show ();
				  }
				win.Update (details);
			}

			private void OnDelete (object o, EventArgs args)
			{
				ICSGameObserverWindow win =
					o as ICSGameObserverWindow;
				if (win == null)
					return;
				client.WriteLine (String.
						  Format ("unobserve {0}",
							  win.GameId));
			}

			private void OnResultNotification (object o,
							   ResultNotification
							   notification)
			{
				if (!currentGames.
				    ContainsKey (notification.gameid))
					return;
				ICSGameObserverWindow win =
					(ICSGameObserverWindow)
					currentGames[notification.gameid];
				win.Update (notification);
			}
		}
	}
}
