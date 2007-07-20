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

using CsBoard.Viewer;
using Gtk;
using System;
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ICSGameObserverWindow:Window
		{
			Notebook gamesBook;
			ICSClient client;
			Hashtable currentGames;
			HPaned split;
			TreeView gamesList;
			ListStore gamesStore;

			public HPaned SplitPane
			{
				get
				{
					return split;
				}
			}

			public ICSClient Client
			{
				get
				{
					return client;
				}
			}

			public ICSGameObserverWindow (ICSClient
						      client):base (Catalog.
								    GetString
								    ("Observed games"))
			{
				split = new HPaned ();
				this.client = client;
				currentGames = new Hashtable ();
				gamesBook = new Notebook ();
				gamesBook.ShowTabs = false;

				gamesList = new TreeView ();
				gamesStore =
					new ListStore (typeof (string),
						       typeof (string));
				gamesList.Model = gamesStore;
				gamesList.AppendColumn ("Games",
							new
							CellRendererText (),
							"markup", 0);
				ScrolledWindow scroll = new ScrolledWindow ();
				scroll.HscrollbarPolicy =
					scroll.VscrollbarPolicy =
					PolicyType.Automatic;
				scroll.Add (gamesList);

				gamesList.CursorChanged +=
					OnGamesListCursorChanged;
				split.Add1 (scroll);
				split.Add2 (gamesBook);

				split.ShowAll ();
				Add (split);
				client.GameMessageEvent += OnGameMessage;
			}

			private void OnGameMessage(object o, string user, GameMessageType type) {
			  string msg;
			  string typestr;
			  switch(type) {
			    case GameMessageType.Draw:
			      msg = "<big><b>{0} offers a draw</b>.\nDo you want to agree?</big>";
			      typestr = "draw";
			      break;
			    case GameMessageType.Abort:
			      msg = "<big><b>{0} wants to abort the game</b>.\nDo you want to agree?</big>";
			      typestr = "abort";
			      break;
			    default:
			      return;
			  }
			  MessageDialog dlg = new MessageDialog(null,
								DialogFlags.Modal,
								MessageType.Question,
								ButtonsType.YesNo,
								true,
								msg, user);
			  dlg.Modal = false;
			  int ret = dlg.Run();
			  if(ret == (int) ResponseType.Yes)
			    client.CommandSender.SendCommand("accept " + user);
			  else if(ret == (int) ResponseType.No)
			    client.CommandSender.SendCommand("decline " + user);
			  dlg.Hide();
			  dlg.Dispose();
			}

			private void OnGamesListCursorChanged (object o,
							       EventArgs args)
			{
				TreePath path;
				TreeViewColumn col;
				gamesList.GetCursor (out path, out col);
				if (path == null)
					return;
				TreeIter iter;
				gamesStore.GetIter (out iter, path);
				string text =
					(string) gamesStore.GetValue (iter,
								      1);
				gamesStore.SetValue (iter, 0, text);

				int pagenum = path.Indices[0];
				gamesBook.Page = pagenum;
			}

			private void AddGamePage (MoveDetails details)
			{
				string title = String.Format ("{0} vs {1}",
							      details.white,
							      details.black);
				gamesStore.AppendValues (title,	// markup
							 title);

				ObservingGamePage info;
				if (ObservingGamePage.
				    IsMyGame (details.relation))
					info = new PlayerPage (this, details);
				else
					info = new ObservingGamePage (this,
								      details);

				currentGames[details.gameNumber] = info;

				Label label = new Label (title);
				gamesBook.AppendPage (info, label);
				gamesBook.Page = gamesBook.NPages - 1;
				AdjustCursorForCurrentPage ();
				MovesGetter.GetMovesAsync(client, details.gameNumber, info.OnGetMoves);
			}

			public void Remove (ObservingGamePage page)
			{
				// remove page
				int num = gamesBook.PageNum (page);
				// unobserve
				TreePath path =
					new TreePath (new int[]{ num });
				TreeIter iter;
				gamesStore.GetIter (out iter, path);
				gamesStore.Remove (ref iter);

				if (page.NeedsUnobserve)
					client.CommandSender.
						SendCommand ("unobserve " +
							     page.GameId);
				gamesBook.RemovePage (num);
				currentGames.Remove (page.GameId);
				AdjustCursorForCurrentPage ();
			}

			private void AdjustCursorForCurrentPage ()
			{
				int page = gamesBook.CurrentPage;
				TreePath path =
					new TreePath (new int[]{ page });
				gamesList.Selection.SelectPath (path);
			}

			public void Update (MoveDetails details)
			{
				if (!currentGames.
				    ContainsKey (details.gameNumber))
				  {
					  AddGamePage (details);
					  return;
				  }

				ObservingGamePage info = (ObservingGamePage)
					currentGames[details.gameNumber];
				info.Update (details);
				int num = gamesBook.PageNum (info);
				if (num == gamesBook.CurrentPage)
					return;

				TreePath path =
					new TreePath (new int[]{ num });
				TreeIter iter;
				gamesStore.GetIter (out iter, path);
				string text =
					(string) gamesStore.GetValue (iter,
								      1);
				string markup =
					String.Format ("<b>{0}</b>", text);
				gamesStore.SetValue (iter, 0, markup);
			}

			public bool Update (GameInfo info)
			{
				if (!currentGames.ContainsKey (info.gameId))
					return false;
				ObservingGamePage gameinfo =
					(ObservingGamePage) currentGames[info.
									 gameId];
				gameinfo.Update (info);
				return true;
			}

			public void Update (ResultNotification notification)
			{
				if (!currentGames.
				    ContainsKey (notification.gameid))
					return;
				ObservingGamePage info =
					(ObservingGamePage)
					currentGames[notification.gameid];
				info.Update (notification);
			}

			protected override bool OnDeleteEvent (Gdk.Event evnt)
			{
				foreach (DictionaryEntry de in currentGames)
				{
					ObservingGamePage page =
						(ObservingGamePage) de.Value;
					page.StopClocks ();
				}

				client.GameMessageEvent -= OnGameMessage;

				return base.OnDeleteEvent (evnt);
			}
		}
	}
}
