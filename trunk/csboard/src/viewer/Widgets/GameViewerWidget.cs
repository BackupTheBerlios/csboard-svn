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

using Chess.Parser;
using Chess.Game;
using System.IO;
using Gtk;
using GLib;
using Mono.Unix;

using System;
using System.Collections;

namespace CsBoard
{
	namespace Viewer
	{
		public delegate void GameLoadedEventHandler (object o,
							     EventArgs args);

		public delegate void GamesLoadedEventHandler (object o,
							      EventArgs args);

		public class GameViewerWidget:Notebook
		{
			private ChessGame currentGame;
			const int GAME_DETAILS_PAGE = 1;
			ArrayList games;

			public ArrayList Games
			{
				get
				{
					return games;
				}
			}

			public ChessGame CurrentGame
			{
				get
				{
					return currentGame;
				}
				set
				{
					currentGame = value;
					SelectGame (currentGame);
				}
			}

			SearchableGamesListWidget gamesListWidget;
			ChessGameWidget chessGameWidget;

			public ChessGameWidget ChessGameWidget
			{
				get
				{
					return chessGameWidget;
				}
			}


			public event GamesLoadedEventHandler GamesLoadedEvent;

			public event GameLoadedEventHandler GameLoadedEvent;

			public SearchableGamesListWidget GamesListWidget
			{
				get
				{
					return gamesListWidget;
				}
			}

			public GameViewerWidget ()
			{
				gamesListWidget =
					new SearchableGamesListWidget ();
				chessGameWidget = new ChessGameWidget ();

				chessGameWidget.BoardWidget.Board.
					highLightMove =
					App.Session.HighLightMove;


				int pos = App.Session.ViewerSplitPanePosition;
				int height = App.Session.ViewerHeight;
				if (pos > height)
					pos = height / 2;
				chessGameWidget.SplitPane.Position = pos;

				gamesListWidget.View.GameSelectionEvent +=
					OnGameSelectionEvent;

				AppendPage (gamesListWidget,
					    new Label (Catalog.
						       GetString ("Games")));
				AppendPage (chessGameWidget,
					    new Label (Catalog.
						       GetString
						       ("Current Game")));

				ShowAll ();
			}

			void OnGameSelectionEvent (ChessGame game)
			{
				CurrentGame = game;
				Page = GAME_DETAILS_PAGE;
			}

			private void SelectGame (ChessGame game)
			{
				chessGameWidget.SetGame (game);
				//Page = GAME_DETAILS_PAGE;

				if (GameLoadedEvent != null)
					GameLoadedEvent (this,
							 EventArgs.Empty);
			}

			/* This replaces the current game with the new game!
			 * This needs to replace the object in the list and also
			 * from the tree views (including the filter)
			 * The game is assumed to be an exact copy of the existing
			 * game but a subclass of it.
			 */

			public void UpdateCurrentGame (ChessGame game)
			{
				UpdateGame (currentGame, game);
			}

			public void UpdateGame (ChessGame curgame,
						ChessGame game)
			{
				int idx = games.IndexOf (curgame);
				games.RemoveAt (idx);
				games.Insert (idx, game);
				// TODO: fire an event
				// Replace it in the stores
				gamesListWidget.View.UpdateGame (curgame,
								 game);
			}


			public void LoadGames (ArrayList games)
			{
				this.games = games;
				if (GamesLoadedEvent != null)
				  {
					  GamesLoadedEvent (this,
							    EventArgs.Empty);
				  }

				gamesListWidget.SetGames (games);
				if (games.Count > 0)
				  {
					  CurrentGame = games[0] as ChessGame;
				  }
			}
		}
	}
}
