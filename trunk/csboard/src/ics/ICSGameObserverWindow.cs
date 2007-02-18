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
		public class ObservingGamePage : VBox, IAsyncCommandResponseListener {
			int gameId;
			bool needsUnobserve = true;
			public bool NeedsUnobserve {
				get {
					return needsUnobserve;
				}
			}

			public int GameId
			{
				get
				{
					return gameId;
				}
			}

			public Widget Widget {
				get {
					return gameWidget;
				}
			}

			ChessGameWidget gameWidget;
			CairoBoard board;
			string white, black;
			ICSGameObserverWindow win;
			Label resultLabel;

			MoveDetails lastMove;

			public void CommandResponseLine(int id, byte[] buffer, int start, int end) {
				board.SetPosition (lastMove.pos);
				SetMoveInfo (board, lastMove);
				board.QueueDraw ();
			}

			public void CommandCodeReceived(int id, CommandCode code) {
			}

			public void CommandCompleted(int id) {
			}

			public ObservingGamePage(ICSGameObserverWindow win, MoveDetails details) : base() {
				this.win = win;
				gameId = details.gameNumber;

				bool isMyGame = details.relation == Relation.IamPlayingAndMyMove ||
					details.relation == Relation.IamPlayingAndMyOppsMove;

				if(isMyGame) {
					board = new CairoPlayerBoard(details.pos);
					board.MoveEvent += OnMoveEvent;
				}
				else
					board = new CairoViewerBoard (details.pos);
				gameWidget = new ChessGameWidget (board);

				gameWidget.WhiteAtBottom =
					!details.blackAtBottom;
				board.side = details.blackAtBottom;
				gameWidget.whiteClock.Configure (details.
								 initial_time
								 * 60,
								 (uint)
								 details.
								 increment);
				gameWidget.blackClock.Configure (details.
								 initial_time
								 * 60,
								 (uint)
								 details.
								 increment);

				white = details.white;
				black = details.black;
				gameWidget.White = white;
				gameWidget.Black = black;

				gameWidget.Show ();
				board.Show ();

				HBox box = new HBox();
				Button closeButton = new Button("");
				resultLabel = new Label();
				closeButton.Image = new Image(Stock.Close, IconSize.Menu);
				box.PackStart(resultLabel, true, true, 2);
				box.PackStart(closeButton, false, false, 2);

				PackStart(box, false, true, 2);
				PackStart(gameWidget);

				closeButton.Clicked += OnCloseButtonClicked;

				Update(details);
				ShowAll();
			}

			private void OnMoveEvent(string move) {
				win.Client.CommandSender.SendCommand(move, this);
			}

			public void StopClocks() {
				gameWidget.whiteClock.Stop();
				gameWidget.blackClock.Stop();
			}

			private void OnCloseButtonClicked(object o, EventArgs args) {
				// stop clocks
				StopClocks();
				win.Remove(this);
			}

			public void Update(MoveDetails details) {
				lastMove = details;
				SetMoveInfo (board, details);
				board.SetPosition (details.pos);
				board.QueueDraw ();

				int factor =
					details.inMilliseconds ? 1 : 1000;
				  gameWidget.whiteClock.RemainingTime =
					details.whites_remaining_time *
					factor;
				  gameWidget.blackClock.RemainingTime =
					details.blacks_remaining_time *
					factor;
				if (details.whiteToMove)
				  {
					  gameWidget.whiteClock.Start ();
					  gameWidget.blackClock.Stop ();
				  }
				else
				  {
					  gameWidget.whiteClock.Stop ();
					  gameWidget.blackClock.Start ();
				  }
			}

			public void Update(GameInfo info) {
				if(info.whitesRating > 0)
					gameWidget.White = String.Format("{0} ({1})", white, info.whitesRating);
				if(info.blacksRating > 0)
					gameWidget.Black = String.Format("{0} ({1})", black, info.blacksRating);
			}

			public void Update(ResultNotification notification) {
				resultLabel.Markup = String.Format("<b>{0}: {1}</b>", notification.result.Trim(), notification.reason);
				needsUnobserve = false;
				gameWidget.whiteClock.Stop ();
				gameWidget.blackClock.Stop ();
			}

			private static void SetMoveInfo (CairoBoard
							 board,
							 MoveDetails details)
			{
				string notation = details.verbose_notation;
				if (notation.Equals ("none"))
					return;
				char src_rank, src_file, dst_rank, dst_file;
				if(notation.ToLower().Equals("o-o")) {
					src_file = 'e';
					// Note: whiteToMove indicates that black made the move!
					src_rank = dst_rank = details.whiteToMove ? '8' : '1';
					dst_file = 'g';
				}
				else if(notation.ToLower().Equals("o-o-o")) {
					src_file = 'e';
					// Note: whiteToMove indicates that black made the move!
					src_rank = dst_rank = details.whiteToMove ? '8' : '1';
					dst_file = 'c';
				}
				else {
					int idx = notation.IndexOf ('/');
					idx++;
					src_file = notation[idx++];
					src_rank = notation[idx++];
					idx++; // skip extra char
					dst_file = notation[idx++];
					dst_rank = notation[idx++];
				}

				board.SetMoveInfo (src_rank - '1',
						   src_file - 'a',
						   dst_rank - '1',
						   dst_file - 'a');
			}
		}

		public class ICSGameObserverWindow:Window
		{
			Notebook gamesBook;
			ICSClient client;
			Hashtable currentGames;
			HPaned split;
			TreeView gamesList;
			ListStore gamesStore;

			public ICSClient Client {
				get {
					return client;
				}
			}

			public ICSGameObserverWindow (ICSClient client):base (Catalog.GetString("Observed games"))
			{
				split = new HPaned();
				this.client = client;
				currentGames = new Hashtable();
				gamesBook = new Notebook();
				gamesBook.ShowTabs = false;

				gamesList = new TreeView();
				gamesStore = new ListStore(typeof(string), typeof(string));
				gamesList.Model = gamesStore;
				gamesList.AppendColumn("Games", new CellRendererText(), "markup", 0);
				ScrolledWindow scroll = new ScrolledWindow();
				scroll.HscrollbarPolicy = scroll.VscrollbarPolicy = PolicyType.Automatic;
				scroll.Add(gamesList);

				gamesList.CursorChanged += OnGamesListCursorChanged;
				split.Add1(scroll);
				split.Add2(gamesBook);

				split.Position = 100;
				split.ShowAll();
				Add(split);
			}

			private void OnGamesListCursorChanged(object o, EventArgs args) {
				TreePath path;
				TreeViewColumn col;
				gamesList.GetCursor(out path, out col);
				if(path == null)
					return;
				TreeIter iter;
				gamesStore.GetIter(out iter, path);
				string text = (string) gamesStore.GetValue(iter, 1);
				gamesStore.SetValue(iter, 0, text);

				int pagenum = path.Indices[0];
				gamesBook.Page = pagenum;
			}

			private void AddGamePage(MoveDetails details) {
				string title = String.Format("{0} vs {1}", details.white, details.black);
				gamesStore.AppendValues(title, // markup
							title);

				ObservingGamePage info = new ObservingGamePage(this, details);
				currentGames[details.gameNumber] = info;

				Label label = new Label(title);
				gamesBook.AppendPage(info, label);
				gamesBook.Page = gamesBook.NPages - 1;
				AdjustCursorForCurrentPage();
			}

			public void Remove(ObservingGamePage page) {
				// remove page
				int num = gamesBook.PageNum(page);
				// unobserve
				TreePath path = new TreePath(new int[] {num});
				TreeIter iter;
				gamesStore.GetIter(out iter, path);
				gamesStore.Remove(ref iter);

				if(page.NeedsUnobserve)
					client.CommandSender.SendCommand("unobserve " + page.GameId);
				gamesBook.RemovePage(num);
				currentGames.Remove(page.GameId);
				AdjustCursorForCurrentPage();
			}

			private void AdjustCursorForCurrentPage() {
				int page = gamesBook.CurrentPage;
				TreePath path = new TreePath(new int[] {page});
				gamesList.Selection.SelectPath(path);
			}

			public void Update (MoveDetails details)
			{
				if (!currentGames.
				    ContainsKey (details.gameNumber)) {
					AddGamePage(details);
					return;
				}

				ObservingGamePage info = (ObservingGamePage)
					currentGames[details.
						     gameNumber];
				info.Update(details);
				int num = gamesBook.PageNum(info);
				if(num == gamesBook.CurrentPage)
					return;

				TreePath path = new TreePath(new int[] {num});
				TreeIter iter;
				gamesStore.GetIter(out iter, path);
				string text = (string) gamesStore.GetValue(iter, 1);
				string markup = String.Format("<b>{0}</b>", text);
				gamesStore.SetValue(iter, 0, markup);
			}

			public bool Update(GameInfo info) {
				if(!currentGames.ContainsKey(info.gameId))
					return false;
				ObservingGamePage gameinfo = (ObservingGamePage) currentGames[info.gameId];
				gameinfo.Update(info);
				return true;
			}

			public void Update (ResultNotification notification)
			{
				if(!currentGames.ContainsKey(notification.gameid))
					return;
				ObservingGamePage info = (ObservingGamePage) currentGames[notification.gameid];
				info.Update(notification);
			}

			protected override bool OnDeleteEvent(Gdk.Event evnt) {
				foreach(DictionaryEntry de in currentGames) {
					ObservingGamePage page = (ObservingGamePage) de.Value;
					page.StopClocks();
				}
				return base.OnDeleteEvent(evnt);
			}
		}
	}
}
