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

namespace CsBoard
{
	namespace Viewer
	{

		using System;
		using System.Collections;
		using Gtk;
		using Gdk;

		public class GameViewer
		{
			[Glade.Widget] private Gtk.Window gameViewerWindow;
			[Glade.Widget] private Gtk.VBox chessBoardBox;
			[Glade.Widget] private Gtk.VBox chessGameDetailsBox;
			[Glade.Widget] private Gtk.TextView gameNotesTextView;
			[Glade.Widget] private Gtk.HPaned leftSplitPane;
			[Glade.Widget] private Gtk.HPaned gamesSplitPane;
			[Glade.Widget] private Gtk.VBox gamesListBox;
			[Glade.Widget] private Gtk.Statusbar statusBar;
			[Glade.Widget] private Gtk.MenuItem fileMenuItem;
			[Glade.Widget] private Gtk.
				SeparatorMenuItem fileOpenSeparator;
			[Glade.Widget] private Gtk.
				SeparatorMenuItem lastSeparator;
			[Glade.Widget] private Gtk.MenuItem exportAsMenuItem;
			private Gtk.Label whiteLabel, blackLabel;

			private Board boardWidget;
			GameSession gameSession;
			ChessGameWidget gameWidget;
			GamesListWidget gamesListWidget;


			public Gtk.Window Window
			{
				get
				{
					return gameViewerWindow;
				}
			}

			public Gtk.Statusbar StatusBar
			{
				get
				{
					return statusBar;
				}
			}

			private bool AppendBeforeMenuItem (Gtk.
							   MenuItem
							   itemToBeAdded,
							   Gtk.
							   MenuItem
							   beforeThis)
			{
				Gtk.Menu menu =
					(Gtk.Menu) fileMenuItem.Submenu;
				// find the index
				int index = 0;
				  foreach (Gtk.MenuItem item in menu.
					   AllChildren)
				{
					if (beforeThis.Equals (item))
					  {
						  menu.Insert (itemToBeAdded,
							       index);
						  return true;
					  }
					index++;
				}
				return false;
			}

			public bool AppendToFileOpenMenu (Gtk.
							  MenuItem menuitem)
			{
				return AppendBeforeMenuItem (menuitem,
							     fileOpenSeparator);
			}

			public bool AppendBeforeLastSeparator (Gtk.
							       MenuItem
							       menuitem)
			{
				return AppendBeforeMenuItem (menuitem,
							     lastSeparator);
			}

			public bool RemoveFromFileMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) fileMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}

			public bool AddToExportMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) exportAsMenuItem.Submenu;
				if (menu == null)
				  {
					  menu = new Menu ();
					  menu.Show ();
					  exportAsMenuItem.Submenu = menu;
				  }
				menu.Append (item);
				return true;
			}

			public bool RemoveFromExportMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) exportAsMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}

			public PGNChessGame Game
			{
				get
				{
					return gameSession.game;
				}

				set
				{
					gameSession.Set (value);
				}
			}

			ArrayList games;
			public ArrayList Games
			{
				get
				{
					return games;
				}
			}

			static GameViewer viewer;

			public static GameViewer Instance
			{
				get
				{
					return viewer;
				}
			}

			public static void CreateInstance ()
			{
				viewer = new GameViewer ();
			}

			private GameViewer ()
			{
				Glade.XML gXML =
					Glade.XML.
					FromAssembly ("csboard.glade",
						      "gameViewerWindow",
						      null);
				gXML.Autoconnect (this);

				// FIXME: Use libglade to create toolbar                  

				App.session.SetupGeometry (gameViewerWindow);

				boardWidget =
					new Board (ChessGamePlayer.
						   GetDefaultPosition ());
				boardWidget.WidthRequest = 450;
				boardWidget.HeightRequest = 400;
				whiteLabel = new Gtk.Label ("<b>White</b>");
				blackLabel = new Gtk.Label ("<b>Black</b>");
				whiteLabel.UseMarkup = true;
				blackLabel.UseMarkup = true;
				whiteLabel.Show ();
				blackLabel.Show ();
				chessBoardBox.PackStart (blackLabel, false,
							 true, 2);
				chessBoardBox.PackStart (boardWidget, true,
							 true, 2);
				chessBoardBox.PackStart (whiteLabel, false,
							 true, 2);
				boardWidget.Show ();

				boardWidget.showCoords =
					App.session.ShowCoords;
				boardWidget.highLightMove =
					App.session.HighLightMove;
				boardWidget.showAnimations =
					App.session.showAnimations;

				gameWidget = new ChessGameWidget ();
				chessGameDetailsBox.PackStart (gameWidget,
							       true, true, 4);

				gamesListWidget = new GamesListWidget ();

				gamesListWidget.RowActivated +=
					OnRowActivated;
				ScrolledWindow win = new ScrolledWindow ();
				win.HscrollbarPolicy = PolicyType.Automatic;
				win.VscrollbarPolicy = PolicyType.Automatic;
				win.Child = gamesListWidget;
				win.Show ();
				Label label = new Label ("<b>Games</b>");
				label.UseMarkup = true;
				label.Show ();
				gamesListBox.PackStart (label, false, false,
							2);
				gamesListBox.PackStart (win, true, true, 0);

				leftSplitPane.Position = 300;
				gamesSplitPane.Position = 400;
				Gnome.Vfs.Vfs.Initialize ();
				gameViewerWindow.Show ();
				gameSession = new GameSession ();
			}

			public void SetGames (ArrayList games)
			{
				this.games = games;
				gamesListWidget.SetGames (games);
				SelectGame (0);
			}

			public void on_save_as_activate (System.Object b,
							 EventArgs e)
			{
				if (games == null || games.Count == 0)
					return;
				string file =
					AskForFile (gameViewerWindow,
						    "Save the game as",
						    false);
				if (file == null)
					return;
				TextWriter writer = new StreamWriter (file);
				foreach (PGNChessGame game in games)
				{
					game.WritePGN (writer);
					writer.WriteLine ();
				}
				writer.Close ();
			}

			private void Reset ()
			{
				gameSession.Reset ();	// reset session

				gameNotesTextView.Buffer.Text =
					gameSession.CurrentComment ==
					null ? "" : gameSession.
					CurrentComment;
				gameWidget.HighlightMove (gameSession.
							  CurrentMoveIndex,
							  gameSession.
							  IsWhitesTurn);
				boardWidget.lastMove =
					gameSession.CurrentMove;

				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void on_window_delete_event (System.Object b,
							    DeleteEventArgs e)
			{
				on_quit_activate (b, e);
			}

			public void on_quit_activate (System.Object b,
						      EventArgs e)
			{
				App.session.SaveGeometry (gameViewerWindow);
				Gtk.Application.Quit ();
			}

			public void on_first_clicked (System.Object o,
						      EventArgs e)
			{
				Reset ();
			}

			public void on_plugins_activate (System.Object b,
							 EventArgs e)
			{
				Dialog dlg =
					new
					PluginManagerDialog (gameViewerWindow,
							     CsBoard.Plugin.
							     PluginManager.
							     Instance);
				dlg.Run ();
				dlg.Hide ();
				dlg.Dispose ();
			}

			public void on_last_clicked (System.Object o,
						     EventArgs e)
			{
				gameSession.PlayTillTheEnd ();

				gameNotesTextView.Buffer.Text =
					gameSession.CurrentComment ==
					null ? "" : gameSession.
					CurrentComment;
				gameWidget.HighlightMove (gameSession.
							  CurrentMoveIndex,
							  gameSession.
							  IsWhitesTurn);
				boardWidget.lastMove =
					gameSession.CurrentMove;

				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void on_previous_clicked (System.Object o,
							 EventArgs e)
			{
				int currentMoveIdx =
					gameSession.CurrentMoveIdx;
				gameSession.PlayNMoves (currentMoveIdx);

				gameNotesTextView.Buffer.Text =
					gameSession.CurrentComment ==
					null ? "" : gameSession.
					CurrentComment;

				gameNotesTextView.Buffer.Text =
					gameSession.CurrentComment ==
					null ? "" : gameSession.
					CurrentComment;
				gameWidget.HighlightMove (gameSession.
							  CurrentMoveIndex,
							  gameSession.
							  IsWhitesTurn);
				boardWidget.lastMove =
					gameSession.CurrentMove;

				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void on_next_clicked (System.Object o,
						     EventArgs e)
			{
				if (!gameSession.HasNext ())
				  {
					  return;
				  }
				gameSession.Next ();
				gameSession.player.Move (gameSession.
							 CurrentMove);
				gameNotesTextView.Buffer.Text =
					gameSession.CurrentComment ==
					null ? "" : gameSession.
					CurrentComment;
				gameWidget.HighlightMove (gameSession.
							  CurrentMoveIndex,
							  gameSession.
							  IsWhitesTurn);

				boardWidget.lastMove =
					gameSession.CurrentMove;
				boardWidget.Move (gameSession.player.
						  LastMoveInfo.src_rank,
						  gameSession.player.
						  LastMoveInfo.src_file,
						  gameSession.player.
						  LastMoveInfo.dest_rank,
						  gameSession.player.
						  LastMoveInfo.dest_file,
						  ' ');
				if (gameSession.player.LastMoveInfo.
				    special_move)
				  {
					  // Reload the position
					  boardWidget.
						  SetPosition (gameSession.
							       player.
							       GetPosition
							       ());
					  /*
					     // move the rook also
					     ChessPiece king = gameSession.player.LastMoveInfo.movedPiece;
					     int file1, file2; // the src and dest files of the rook

					     if(king.File == ChessBoardConstants.FILE_G) { // short castle
					     file1 = ChessBoardConstants.FILE_H;
					     file2 = ChessBoardConstants.FILE_F;
					     }
					     else {
					     file1 = ChessBoardConstants.FILE_A;
					     file2 = ChessBoardConstants.FILE_D;
					     }
					     boardWidget.Move(king.Rank, file1, king.Rank, file2, ' ');
					   */
				  }
				boardWidget.QueueDraw ();
			}

			void OnRowActivated (object obj,
					     RowActivatedArgs args)
			{
				int idx = args.Path.Indices[0];
				SelectGame (idx);
			}

			private void SelectGame (int idx)
			{
				PGNChessGame game = (PGNChessGame) games[idx];
				gamesListWidget.HighlightGame (idx);
				gameSession.Set (game);
				gameWidget.SetGame (game);
				boardWidget.SetPosition (ChessGamePlayer.
							 GetDefaultPosition
							 ());
				gameNotesTextView.Buffer.Text = "";
				whiteLabel.Markup =
					"<b>" + game.GetTagValue ("White",
								  "White") +
					"</b>";
				blackLabel.Markup =
					"<b>" + game.GetTagValue ("Black",
								  "Black") +
					"</b>";
			}

			public static string AskForFile (Gtk.
							 Window parentWindow,
							 string title,
							 bool open)
			{
				string file = null;
				Gtk.FileChooserDialog fc =
					new Gtk.FileChooserDialog (title,
								   parentWindow,
								   FileChooserAction.
								   Open,
								   "Cancel",
								   ResponseType.
								   Cancel,
								   open ?
								   "Open" :
								   "Save",
								   ResponseType.
								   Accept);

				if (fc.Run () == (int) ResponseType.Accept)
				  {
					  file = fc.Filename;
				  }
				//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
				fc.Destroy ();
				return file;
			}
		}
	}
}
