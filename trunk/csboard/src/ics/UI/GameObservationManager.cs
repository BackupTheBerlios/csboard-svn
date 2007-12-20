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
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public delegate void ObservableGameEventHandler (object o,
								 GameDetails
								 gd);

		public class
			GameObservationManager:IAsyncCommandResponseListener
		{
			ICSClient client;
			Hashtable gameInfos;
			int commandId;

			ICSGameObserverWidget win;

			public event ObservableGameEventHandler
				ObservableGameEvent;

			public void ObserveGame (int gameid)
			{
				client.CommandSender.SendCommand ("observe " +
								  gameid);
				if (!label_bold)
					label_bold = win == null;
				UpdateTitle ();
			}

			ICSDetailsWidget appwidget;
			string label_text;
			bool label_bold = false;
			Label gamesPageLabel;

			public GameObservationManager (ICSClient client,
						       ICSDetailsWidget
						       appwidget)
			{
				gameInfos = new Hashtable ();
				this.client = client;
				client.MoveMadeEvent += OnMoveMade;
				client.ResultNotificationEvent +=
					OnResultNotification;
				client.GameInfoEvent += OnGameInfo;
				commandId = -1;
				this.appwidget = appwidget;
				label_text = Catalog.GetString ("Games");
				appwidget.Book.SwitchPage += OnSwitchPage;
			}

			public void GetGames ()
			{
				if (commandId != -1)
					return;
				commandId =
					client.CommandSender.
					SendCommand ("games /blsu", this);
			}

			public void CommandResponseLine (int id, byte[]buffer,
							 int start, int end)
			{
				try
				{
					GameDetails details =
						GameDetails.
						FromBuffer (buffer,
							    start,
							    end);
					if (ObservableGameEvent != null)
					  {
						  ObservableGameEvent (this,
								       details);
					  }
				}
				catch (Exception)
				{
					//Console.WriteLine(e);
				}
			}

			public void CommandCodeReceived (int id,
							 CommandCode code)
			{
			}

			public void CommandCompleted (int id)
			{
				commandId = -1;
			}

			private void OnMoveMade (object o,
						 MoveMadeEventArgs args)
			{
				MoveDetails details = args.Details;

				if (details.relation != Relation.IamObserving
				    && details.relation !=
				    Relation.IamObservingGameBeingObserved
				    && details.relation !=
				    Relation.IamPlayingAndMyMove
				    && details.relation !=
				    Relation.IamPlayingAndMyOppsMove)
					return;

				if (win != null)
				  {
					  win.Update (details);
					  return;
				  }
				win = new ICSGameObserverWidget (client);
				win.GamePageRemovedEvent += OnGamePageRemoved;
				win.GamePageAddedEvent += OnGamePageAdded;
				gamesPageLabel = new Label ();
				appwidget.Book.AppendPage (win,
							   gamesPageLabel);
				win.ShowAll ();
				win.Update (details);

				if (gameInfos.
				    ContainsKey (details.gameNumber))
				  {
					  GameInfo info =
						  (GameInfo)
						  gameInfos[details.
							    gameNumber];
					  gameInfos.Remove (details.
							    gameNumber);
					  win.Update (info);
				  }

				/*
				   win.Resize (App.Session.ICSGamesWinWidth,
				   App.Session.ICSGamesWinHeight);
				 */
				win.SplitPane.Position =
					App.Session.
					ICSGamesWinSplitPanePosition;

				appwidget.MakeVisible ();
			}

			private void OnGamePageRemoved (object o,
							EventArgs args)
			{
				if (win.NGames == 0)
					RemoveGamesUI ();
				else
					UpdateTitle ();
			}

			private void UpdateTitle ()
			{
				if (win == null)
					return;
				string title = label_text;
				int ngames = win.NGames;
				if (ngames > 0)
					title += String.Format (" ({0})",
								ngames);
				if (label_bold)
					title = String.Format ("<b>{0}</b>",
							       title);
				gamesPageLabel.Markup = title;
			}

			private void OnGamePageAdded (object o,
						      GamePageAddedEventArgs
						      args)
			{
				label_bold =
					!appwidget.Book.CurrentPageWidget.
					Equals (win);
				UpdateTitle ();
				if (!args.MyGame)
					return;
				appwidget.MakeVisible ();
				appwidget.Book.CurrentPage =
					appwidget.Book.PageNum (win);
			}

			private void OnGameInfo (object o, GameInfo info)
			{
				if (win == null || !win.Update (info))
					gameInfos[info.gameId] = info;
			}

			private void RemoveGamesUI ()
			{
				client.CommandSender.SendCommand ("unobserve");	// unobserve all!
				/*
				   int width, height;
				   win.GetSize (out width, out height);
				   App.Session.ICSGamesWinWidth = width;
				   App.Session.ICSGamesWinHeight = height;
				 */
				App.Session.ICSGamesWinSplitPanePosition =
					win.SplitPane.Position;
				int index = appwidget.Book.PageNum (win);
				appwidget.Book.RemovePage (index);
				win = null;
			}

			private void OnSwitchPage (object o,
						   SwitchPageArgs args)
			{
				if (win == null)
					return;

				if (appwidget.Book.PageNum (win) ==
				    args.PageNum)
				  {
					  label_bold = false;
					  UpdateTitle ();
				  }
			}

			private void OnResultNotification (object o,
							   ResultNotification
							   notification)
			{
				try
				{
					win.Update (notification);
				} catch
				{
				}
			}
		}
	}
}
