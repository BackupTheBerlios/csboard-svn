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
			[Glade.Widget] private Gtk.MenuItem printMenuItem;
			[Glade.Widget] private Gtk.MenuItem exportAsMenuItem;
			[Glade.Widget] private Gtk.MenuBar gameViewerMenuBar;
			[Glade.Widget] private Gtk.
				CheckMenuItem highlightMoveMenuItem;
			private Gtk.Label whiteLabel, blackLabel;

			private ViewerBoard boardWidget;
			GameSession gameSession;
			ChessGameWidget gameWidget;
			GamesListWidget gamesListWidget;

			string initialDirForFileChooser = null;


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

			public Gtk.MenuBar MenuBar
			{
				get
				{
					return gameViewerMenuBar;
				}
			}

			ArrayList gameLoaders;
			ArrayList exporters;

			public void RegisterGameLoader (IGameLoader
							gameLoader,
							Gtk.MenuItem item)
			{
				gameLoaders.Add (gameLoader);
				AppendToFileOpenMenu (item);
			}

			public void UnregisterGameLoader (IGameLoader
							  gameLoader,
							  Gtk.MenuItem item)
			{
				gameLoaders.Remove (gameLoader);
				RemoveFromFileMenu (item);
			}

			public bool RegisterExporter (IExporter exporter,
						      Gtk.MenuItem item)
			{
				Menu menu = (Menu) exportAsMenuItem.Submenu;
				if (menu == null)
				  {
					  menu = new Menu ();
					  menu.Show ();
					  exportAsMenuItem.Submenu = menu;
				  }
				menu.Append (item);
				exporters.Add (exporter);
				return true;
			}

			public bool UnregisterExporter (IExporter exporter,
							Gtk.MenuItem item)
			{
				Menu menu = (Menu) exportAsMenuItem.Submenu;
				menu.Remove (item);
				exporters.Remove (exporter);
				return true;
			}

			public bool RegisterPrintHandler (IPrintHandler
							  handler)
			{
				printMenuItem.Activated +=
					handler.OnPrintActivated;
				return true;
			}

			public void UnregisterPrintHandler (IPrintHandler
							    handler)
			{
				printMenuItem.Activated -=
					handler.OnPrintActivated;
			}

			private bool AppendToFileOpenMenu (Gtk.
							   MenuItem
							   itemToBeAdded)
			{
				Gtk.Menu menu =
					(Gtk.Menu) fileMenuItem.Submenu;
				// find the index
				int index = 0;
				foreach (Gtk.MenuItem item in menu.
					 AllChildren)
				{
					if (fileOpenSeparator.Equals (item))
					  {
						  menu.Insert (itemToBeAdded,
							       index);
						  return true;
					  }
					index++;
				}
				return false;
			}

			bool RemoveFromFileMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) fileMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}

			public void LoadGames (TextReader reader)
			{
				GameLoader loader =
					new GameLoader (this, reader);
				if (loader.Games == null)
					return;
				this.games = loader.Games;
				gamesListWidget.SetGames (games);
				if (games.Count > 0)
					SelectGame ((PGNChessGame) games[0]);
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
				initialDirForFileChooser =
					App.session.CurrentFolder;

				gameLoaders = new ArrayList ();
				exporters = new ArrayList ();

				boardWidget =
					new ViewerBoard (ChessGamePlayer.
							 GetDefaultPosition
							 ());
				boardWidget.WidthRequest = 450;
				boardWidget.HeightRequest = 400;
				whiteLabel =
					new Gtk.Label (Catalog.
						       GetString
						       ("<b>White</b>"));
				blackLabel =
					new Gtk.Label (Catalog.
						       GetString
						       ("<b>Black</b>"));
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

				gameWidget = new ChessGameWidget ();
				chessGameDetailsBox.PackStart (gameWidget,
							       true, true, 4);

				gamesListWidget = new GamesListWidget ();

				gamesListWidget.Tree.RowActivated +=
					OnRowActivated;
				boardWidget.ShowMove =
					App.session.HighLightMove;
				highlightMoveMenuItem.Active =
					App.session.HighLightMove;
				Label label =
					new Label (Catalog.
						   GetString
						   ("<b>Games</b>"));
				label.UseMarkup = true;
				label.Show ();
				gamesListBox.PackStart (label, false, false,
							2);
				gamesListBox.PackStart (gamesListWidget, true,
							true, 0);

				leftSplitPane.Position = 300;
				gamesSplitPane.Position = 400;
				Gnome.Vfs.Vfs.Initialize ();
				gameViewerWindow.Show ();
				gameSession = new GameSession ();
			}

			public void Load (string resource)
			{
				// just ask each IGameLoader
				foreach (IGameLoader gameLoader in
					 gameLoaders)
				{
					if (gameLoader.Load (resource))
						break;
				}
			}

			public void on_save_as_activate (System.Object b,
							 EventArgs e)
			{
				if (games == null || games.Count == 0)
					return;
				string file = AskForFile (gameViewerWindow,
							  Catalog.
							  GetString
							  ("Save the game as"),
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
				boardWidget.Reset ();
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
				App.session.CurrentFolder =
					initialDirForFileChooser;
				Gtk.Application.Quit ();
			}

			public void on_first_clicked (System.Object o,
						      EventArgs e)
			{
				Reset ();
			}

			public void on_last_clicked (System.Object o,
						     EventArgs e)
			{
				if (!gameSession.PlayTillTheEnd ())
					Console.WriteLine
						(Catalog.
						 GetString
						 ("Operation failed"));

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

				int r1, f1, r2, f2;
				r1 = gameSession.player.LastMoveInfo.src_rank;
				f1 = gameSession.player.LastMoveInfo.src_file;
				r2 = gameSession.player.LastMoveInfo.
					dest_rank;
				f2 = gameSession.player.LastMoveInfo.
					dest_file;
				boardWidget.Move (r1, f1, r2, f2, ' ');

				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void on_previous_clicked (System.Object o,
							 EventArgs e)
			{
				int currentMoveIdx =
					gameSession.CurrentMoveIdx;
				if (!gameSession.PlayNMoves (currentMoveIdx))
				  {
					  Console.WriteLine
						  (Catalog.
						   GetString
						   ("Failed to play to go back"));
					  // dont return now. let the position be set so that we can see
					  // where it stopped
				  }

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

				int r1, f1, r2, f2;
				r1 = gameSession.player.LastMoveInfo.src_rank;
				f1 = gameSession.player.LastMoveInfo.src_file;
				r2 = gameSession.player.LastMoveInfo.
					dest_rank;
				f2 = gameSession.player.LastMoveInfo.
					dest_file;
				boardWidget.Move (r1, f1, r2, f2, ' ');

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
				if (!gameSession.player.Move (gameSession.
							      CurrentMove))
				  {
					  Console.WriteLine
						  (Catalog.
						   GetString
						   ("Failed to play the move: ")
						   + gameSession.CurrentMove);
					  return;
				  }
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
				int r1, f1, r2, f2;
				r1 = gameSession.player.LastMoveInfo.src_rank;
				f1 = gameSession.player.LastMoveInfo.src_file;
				r2 = gameSession.player.LastMoveInfo.
					dest_rank;
				f2 = gameSession.player.LastMoveInfo.
					dest_file;
				boardWidget.Move (r1, f1, r2, f2, ' ');
				if (gameSession.player.LastMoveInfo.
				    special_move)
				  {
					  // Reload the position
					  boardWidget.
						  SetPosition (gameSession.
							       player.
							       GetPosition
							       ());
				  }
				boardWidget.QueueDraw ();
			}

			void OnRowActivated (object obj,
					     RowActivatedArgs args)
			{
				TreeIter iter;
				gamesListWidget.Tree.Model.GetIter (out iter,
								    args.
								    Path);
				PGNChessGame game =
					(PGNChessGame) gamesListWidget.Tree.
					Model.GetValue (iter, 0);
				SelectGame (game);
			}

			void OnHighlightMoveMenuItemActivated (object o,
							       EventArgs args)
			{
				App.session.HighLightMove =
					highlightMoveMenuItem.Active;
				boardWidget.ShowMove =
					highlightMoveMenuItem.Active;
				boardWidget.QueueDraw ();
			}

			private void SelectGame (PGNChessGame game)
			{
				gameSession.Set (game);
				gameWidget.SetGame (game);
				boardWidget.Reset ();
				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
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

			public string AskForFile (Gtk.
						  Window parentWindow,
						  string title, bool open)
			{
				return AskForFile (parentWindow, title, open,
						   null);
			}

			public string AskForFile (Gtk.
						  Window parentWindow,
						  string title, bool open,
						  FileFilter[]filters)
			{
				string file = null;
				Gtk.FileChooserDialog fc =
					new Gtk.FileChooserDialog (title,
								   parentWindow,
								   FileChooserAction.
								   Open,
								   Catalog.
								   GetString
								   ("Cancel"),
								   ResponseType.
								   Cancel,
								   open ?
								   Catalog.
								   GetString
								   ("Open") :
								   Catalog.
								   GetString
								   ("Save"),
								   ResponseType.
								   Accept);
				if (!open)
					fc.DoOverwriteConfirmation = true;
				if (filters != null)
				  {
					  foreach (FileFilter filter in
						   filters) fc.
						  AddFilter (filter);
				  }
				if (initialDirForFileChooser != null)
					fc.SetCurrentFolder
						(initialDirForFileChooser);
				if (fc.Run () == (int) ResponseType.Accept)
				  {
					  file = fc.Filename;
				  }
				initialDirForFileChooser = fc.CurrentFolder;
				//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
				fc.Destroy ();
				return file;
			}
		}

		public interface IGameLoader
		{
			bool Load (string url);
		}

		public interface IExporter
		{
			bool Export (IList games);
		}

		public interface IPrintHandler
		{
			EventHandler OnPrintActivated
			{
				get;
			}
		}

		public class ProgressDialog:Dialog
		{
			public ProgressBar bar;
			public ProgressBar ProgressBar
			{
				get
				{
					return bar;
				}
			}

			public ProgressDialog (Gtk.
					       Window
					       parent,
					       string title):base (title,
								   parent,
								   DialogFlags.
								   Modal)
			{
				bar = new ProgressBar ();
				bar.Orientation =
					ProgressBarOrientation.LeftToRight;
				bar.Show ();
				VBox.PackStart (bar, true, true, 4);
				Modal = true;
			}

			public void UpdateProgress (double fraction)
			{
				bar.Fraction = fraction;
				bar.Text =
					(int) Math.Round (fraction * 100) +
					" %";
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
			}

			public void Pulse ()
			{
				bar.Pulse ();
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
			}
		}

		class GameLoader
		{
			ArrayList games;

			public ArrayList Games
			{
				get
				{
					return games.Count > 0 ? games : null;
				}
			}

			GameViewer viewer;
			ProgressDialog dlg;

			public GameLoader (GameViewer viewer,
					   TextReader reader)
			{
				games = new ArrayList ();
				dlg = new ProgressDialog (viewer.Window,
							  Catalog.
							  GetString
							  ("Loading..."));
				dlg.ProgressBar.PulseStep = 0.01;
				PGNParser parser = new PGNParser (reader);
				parser.GameLoaded += OnGameLoaded;
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Parsing the file..."));
				GLib.Idle.Add (new GLib.IdleHandler (delegate
								     {
								     parser.
								     Parse ();
								     dlg.
								     Respond
								     (ResponseType.
								      None);
								     return
								     false;}
					       ));
				dlg.Run ();
				dlg.Hide ();
				dlg.Dispose ();
			}

			private void OnGameLoaded (System.Object o,
						   GameLoadedEventArgs args)
			{
				games.Add (args.Game);
				dlg.ProgressBar.Text =
					Catalog.GetString ("Loaded ") +
					games.Count +
					Catalog.GetString (" games");
				dlg.Pulse ();
			}
		}
	}
}
