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
			Hashtable gameInfos;
			int commandId;

			ICSGameObserverWindow win;

			public event ObservableGameEventHandler
				ObservableGameEvent;

			public void ObserveGame (int gameid)
			{
				client.CommandSender.SendCommand ("observe " + gameid);
			}

			public GameObservationManager (ICSClient client)
			{
				gameInfos = new Hashtable();
				this.client = client;
				client.MoveMadeEvent += OnMoveMade;
				client.ResultNotificationEvent +=
					OnResultNotification;
				client.GameInfoEvent += OnGameInfo;
				commandId = -1;
			}

			public void GetGames ()
			{
				if(commandId != -1)
					return;
				client.CommandSender.CommandResponseLineEvent += OnCommandResponseLineEvent;
				client.CommandSender.CommandCompletedEvent += OnCommandCompletedEvent;
				commandId = client.CommandSender.SendCommand ("games");
			}

			private void OnCommandCompletedEvent(object o, int commandId) {
				if(this.commandId < 0)
					return;
				if(this.commandId == commandId) {
					client.CommandSender.CommandResponseLineEvent -= OnCommandResponseLineEvent;
					client.CommandSender.CommandCompletedEvent -= OnCommandCompletedEvent;
					this.commandId = -1;
				}
			}

			private void OnCommandResponseLineEvent(object o, CommandResponseLineEventArgs args) {
				try
				{
					ProcessGameDetails (args);
				}
				catch (Exception e)
				{
					//Console.WriteLine(e);
				}
			}

			private void
				ProcessGameDetails
				(CommandResponseLineEventArgs args)
			{
				GameDetails details =
					GameDetails.FromBuffer (args.buffer,
								args.start,
								args.end);
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

				if(win != null) {
					win.Update(details);
					return;
				}
				win = new ICSGameObserverWindow(client);
				win.Update(details);

				if(gameInfos.ContainsKey(details.gameNumber)) {
					GameInfo info = (GameInfo) gameInfos[details.gameNumber];
					gameInfos.Remove(details.gameNumber);
					win.Update(info);
				}

				win.DeleteEvent += OnDelete;
				win.Resize (600, 400);
				win.Show ();
			}

			private void OnGameInfo(object o, GameInfo info) {
				if (win == null || !win.Update(info))
					gameInfos[info.gameId] = info;
			}

			private void OnDelete (object o, EventArgs args)
			{
				client.CommandSender.SendCommand("unobserve"); // unobserve all!
				win = null;
			}

			private void OnResultNotification (object o,
							   ResultNotification
							   notification)
			{
				win.Update (notification);
			}
		}
	}
}