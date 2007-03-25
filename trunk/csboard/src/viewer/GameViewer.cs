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

		public delegate void GameLoadedEventHandler (object o,
							     EventArgs args);
		public interface IEcoDb
		{
			string GetOpeningName (string econame);
		}

		public interface IGameDb
		{
			void SaveGameDetails (ChessGame game,
					      out ChessGame updated,
					      bool overrite);
			bool GetGameDetails (ChessGame game,
					     out ChessGame details);
		}

		public class GameViewer
		{
			[Glade.Widget] private Gtk.Window gameViewerWindow;
			[Glade.Widget] private Gtk.Toolbar toolbar;
			[Glade.Widget] private Gtk.VBox chessBoardBox;
			[Glade.Widget] private Gtk.VBox chessGameDetailsBox;
			[Glade.Widget] private Gtk.HPaned gamesSplitPane;
			[Glade.Widget] private Gtk.VBox gamesListBox;
			[Glade.Widget] private Gtk.Statusbar statusBar;
			[Glade.Widget] private Gtk.MenuItem fileMenuItem;
			[Glade.Widget] private Gtk.
				SeparatorMenuItem fileOpenSeparator;
			[Glade.Widget] private Gtk.
				SeparatorMenuItem saveAsSeparator;
			[Glade.Widget] private Gtk.MenuItem printMenuItem;
			[Glade.Widget] private Gtk.MenuItem exportAsMenuItem;
			[Glade.Widget] private Gtk.MenuBar gameViewerMenuBar;
			[Glade.Widget] private Gtk.
				CheckMenuItem highlightMoveMenuItem;
			[Glade.Widget] private Gtk.MenuItem viewMenuItem;
			[Glade.Widget] private Gtk.Notebook pgnDetailsBook;
			private Gtk.Label whiteLabel, blackLabel;
			[Glade.Widget] private Gtk.Label nagCommentLabel;
			[Glade.Widget] private Gtk.Label moveNumberLabel;

			private CairoViewerBoard boardWidget;
			GameSession gameSession;

			public Toolbar Toolbar
			{
				get
				{
					return toolbar;
				}
			}

			public ChessGameWidget ChessGameWidget
			{
				get
				{
					return gameWidget;
				}
			}
			ChessGameWidget gameWidget;

			public GamesListWidget GamesListWidget
			{
				get
				{
					return gamesListWidget;
				}
			}

			GamesListWidget gamesListWidget;

			const int ALL_GAMES_PAGE = 1;
			const int GAME_DETAILS_PAGE = 0;

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

			static IEcoDb ecoDb;
			public static IEcoDb EcoDb
			{
				set
				{
					ecoDb = value;
				}
				get
				{
					return ecoDb;
				}
			}

			static IGameDb gameDb;
			public static IGameDb GameDb
			{
				set
				{
					gameDb = value;
				}
				get
				{
					return gameDb;
				}
			}

			public static void GetOpeningName (string eco,
							   out string eco_str)
			{
				if (ecoDb == null)
				  {
					  eco_str = eco;
					  return;
				  }
				string name = ecoDb.GetOpeningName (eco);
				if (name == null)
				  {
					  eco_str = eco;
					  return;
				  }

				eco_str =
					String.Format ("{0} ({1})", name,
						       eco);
			}

			public bool RegisterGameLoader (IGameLoader
							gameLoader,
							Gtk.MenuItem item)
			{
				gameLoaders.Add (gameLoader);
				return AddToMenu (fileMenuItem, item,
						  fileOpenSeparator);
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
				if (!AddToMenu (exportAsMenuItem, item, null))
					return false;
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

			public bool AddToViewMenu (Gtk.MenuItem item)
			{
				return AddToMenu (viewMenuItem, item, null);
			}

			public bool RemoveFromViewMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) viewMenuItem.Submenu;
				menu.Remove (item);
				return true;
			}

			public bool AddToFileMenu (Gtk.MenuItem item)
			{
				return AddToMenu (fileMenuItem, item,
						  saveAsSeparator);
			}

			public bool RemoveFromFileMenu (Gtk.MenuItem item)
			{
				Menu menu = (Menu) fileMenuItem.Submenu;
				menu.Remove (item);
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

			private bool AddToMenu (Gtk.MenuItem parentMenu,
						Gtk.MenuItem itemToBeAdded,
						Gtk.MenuItem beforeThis)
			{
				Gtk.Menu menu = (Gtk.Menu) parentMenu.Submenu;
				if (menu == null)
				  {
					  menu = new Menu ();
					  menu.Show ();
					  parentMenu.Submenu = menu;
				  }
				if (beforeThis == null)
				  {
					  menu.Append (itemToBeAdded);
					  return true;
				  }

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

			public void LoadGames (TextReader reader)
			{
				GameLoader loader =
					new GameLoader (this, reader);
				if (loader.Games == null)
					return;
				LoadGames (loader.Games);
			}

			public void LoadGames (ArrayList games)
			{
				this.games = games;
				gamesListWidget.SetGames (games);
				if (games.Count > 0)
				  {
					  CurrentGame = (ChessGame) games[0];
				  }
			}

			public event GameLoadedEventHandler GameLoadedEvent;

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

			private ChessGame currentGame;

			private void SelectGame (ChessGame game)
			{
				gameSession.Set (game);

				boardWidget.Reset ();
				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
				whiteLabel.Markup =
					GetMarkupForTitle (game.
							   GetTagValue
							   ("White",
							    "White"));
				blackLabel.Markup =
					GetMarkupForTitle (game.
							   GetTagValue
							   ("Black",
							    "Black"));
				moveNumberLabel.Text = "";
				nagCommentLabel.Text = "";
				pgnDetailsBook.Page = GAME_DETAILS_PAGE;

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
				gamesListWidget.UpdateGame (curgame, game);
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

			public VBox ChessGameDetailsBox
			{
				get
				{
					return chessGameDetailsBox;
				}
			}

			public static void CreateInstance ()
			{
				if (viewer != null)
				  {
					  viewer.gameViewerWindow.Show ();
					  return;
				  }
				viewer = new GameViewer ();
				CsBoard.Plugin.PluginManager.Instance.
					StartPlugins ();
			}

			private GameViewer ()
			{
				Glade.XML gXML =
					Glade.XML.
					FromAssembly ("csviewer.glade",
						      "gameViewerWindow",
						      null);
				gXML.Autoconnect (this);

				// FIXME: Use libglade to create toolbar                  

				App.Session.
					SetupViewerGeometry
					(gameViewerWindow);
				initialDirForFileChooser =
					App.Session.CurrentFolder;

				gameLoaders = new ArrayList ();
				exporters = new ArrayList ();

				boardWidget =
					new CairoViewerBoard (ChessGamePlayer.
							      GetDefaultPosition
							      ());
				//boardWidget.WidthRequest = 400;
				//boardWidget.HeightRequest = 400;
				whiteLabel =
					new Gtk.
					Label (GetMarkupForTitle
					       (Catalog.GetString ("White")));
				blackLabel =
					new Gtk.
					Label (GetMarkupForTitle
					       (Catalog.GetString ("Black")));
				whiteLabel.UseMarkup = true;
				blackLabel.UseMarkup = true;
				whiteLabel.Show ();
				blackLabel.Show ();
				blackLabel.Yalign = 1;	// bottom
				whiteLabel.Yalign = 0;	// top
				chessBoardBox.PackStart (blackLabel, false,
							 false, 2);
				chessBoardBox.PackStart (boardWidget, true,
							 true, 2);
				chessBoardBox.PackStart (whiteLabel, false,
							 false, 2);
				boardWidget.Show ();

				gameWidget = new ChessGameWidget (this);
				gameWidget.ShowNthMove += OnShowNthMoveEvent;
				chessGameDetailsBox.PackStart (gameWidget,
							       true, true, 4);

				gamesListWidget = new GamesListWidget ();

				gamesListWidget.Tree.RowActivated +=
					OnRowActivated;
				boardWidget.highLightMove =
					App.Session.HighLightMove;
				highlightMoveMenuItem.Active =
					App.Session.HighLightMove;
				gamesListBox.PackStart (gamesListWidget, true,
							true, 0);

				int pos = App.Session.ViewerSplitPanePosition;
				int height = App.Session.ViewerHeight;
				if (pos > height)
					pos = height / 2;
				gamesSplitPane.Position = pos;
				gameViewerWindow.Show ();
				gameSession = new GameSession ();
			}

			private static string GetMarkupForTitle (string str)
			{
				return String.
					Format
					("<big><big><big><b>{0}</b></big></big></big>",
					 str);
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
				foreach (ChessGame game in games)
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

				gameWidget.SetMoveIndex (gameSession.
							 CurrentMoveIdx);
				boardWidget.lastMove =
					gameSession.CurrentMove;
				moveNumberLabel.Text = "";
				nagCommentLabel.Text = "";

				boardWidget.SetPosition (gameSession.player.
							 GetPosition ());
			}

			public void on_window_delete_event (System.Object b,
							    DeleteEventArgs e)
			{
				on_quit_activate (b, e);
				// dont delete the window
				e.RetVal = true;
			}

			public void on_quit_activate (System.Object b,
						      EventArgs e)
			{
				App.Session.
					SaveViewerGeometry (gameViewerWindow);
				App.Session.CurrentFolder =
					initialDirForFileChooser;
				App.Session.ViewerSplitPanePosition =
					gamesSplitPane.Position;
				//CsBoard.Plugin.PluginManager.Instance.ClosePlugins ();
				gameViewerWindow.Hide ();
				App.Close ();
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

				UpdateMoveDetails (false);
			}

			public void on_player_clicked (System.Object o,
						       EventArgs e)
			{
				App.StartPlayer (null);
			}

			public void on_icsplayer_clicked (System.Object o,
							  EventArgs e)
			{
				App.StartICSPlayer ();
			}

			protected void on_about_activated (object o,
							   EventArgs args)
			{
				ChessWindow.
					ShowAboutDialog (gameViewerWindow);
			}

			public void on_prev_clicked (System.Object o,
						     EventArgs e)
			{
				int currentMoveIdx =
					gameSession.CurrentMoveIdx;
				if (currentMoveIdx < 0)
					return;
				PlayNMoves (currentMoveIdx);	// since we are passing the index, no need for -1
			}

			public void OnEditCommentActivated (object o,
							    EventArgs e)
			{
				BufferDialog dlg =
					new BufferDialog (gameViewerWindow,
							  Catalog.
							  GetString
							  ("Edit current comment"));
				string currentComment =
					gameSession.CurrentComment;
				if (currentComment != null)
					dlg.Buffer = currentComment;
				if (dlg.Run () == (int) ResponseType.Ok)
				  {
					  string comment = dlg.Buffer.Trim ();
					  gameSession.CurrentComment =
						  comment.Length ==
						  0 ? null : comment;
					  gameWidget.Refresh ();
				  }
				dlg.Hide ();
				dlg.Dispose ();
			}

			private void PlayNMoves (int nmoves)
			{
				if (!gameSession.PlayNMoves (nmoves))
				  {
					  Console.WriteLine
						  (Catalog.
						   GetString
						   ("Failed to play to go back"));
					  // dont return now. let the position be set so that we can see
					  // where it stopped
				  }

				UpdateMoveDetails (false);
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
				UpdateMoveDetails (true);
			}

			private void OnShowNthMoveEvent (object o,
							 MoveEventArgs args)
			{
				int idx = args.nthMove;
				PlayNMoves (idx + 1);
			}

			private void UpdateMoveDetails (bool next)
			{
				int currentMoveIdx =
					gameSession.CurrentMoveIdx;
				gameWidget.SetMoveIndex (currentMoveIdx);
				if (currentMoveIdx >= 0)
				  {
					  string str =
						  gameSession.CurrentPGNMove.
						  Nags ==
						  null ? "" : gameSession.
						  CurrentPGNMove.Nags[0].
						  Markup ();
					  nagCommentLabel.Markup = str;
					  boardWidget.lastMove =
						  gameSession.CurrentMove;
					  int r1, f1, r2, f2;
					  r1 = gameSession.player.
						  LastMoveInfo.src_rank;
					  f1 = gameSession.player.
						  LastMoveInfo.src_file;
					  r2 = gameSession.player.
						  LastMoveInfo.dest_rank;
					  f2 = gameSession.player.
						  LastMoveInfo.dest_file;
					  boardWidget.Move (r1, f1, r2, f2,
							    ' ');
					  string move_markup =
						  String.
						  Format ("<b>{0}{1} {2}</b>",
							  gameSession.
							  CurrentMoveNumber,
							  gameSession.
							  IsWhitesTurn ? "." :
							  "...",
							  gameSession.
							  CurrentMove);
					  moveNumberLabel.Markup =
						  move_markup;
				  }
				else
				  {
					  moveNumberLabel.Text = "";
					  nagCommentLabel.Text = "";
					  boardWidget.Move (0, 0, 0, 0, ' ');
				  }
				// Reload the position
				// For next, the move is enough. but for spl positions like
				// castling and enpassant, the position has to be reloaded
				// for prev and other moves, the position has to be reloaded
				if (!next
				    || gameSession.player.LastMoveInfo.
				    special_move)
					boardWidget.SetPosition (gameSession.
								 player.
								 GetPosition
								 ());
				boardWidget.QueueDraw ();
			}

			void OnRowActivated (object obj,
					     RowActivatedArgs args)
			{
				TreeIter iter;
				gamesListWidget.Tree.Model.GetIter (out iter,
								    args.
								    Path);
				ChessGame details =
					(ChessGame) gamesListWidget.Tree.
					Model.GetValue (iter, 0);
				CurrentGame = details;
			}

			public void OnHighlightMoveMenuItemActivated (object
								      o,
								      EventArgs
								      args)
			{
				boardWidget.highLightMove =
					highlightMoveMenuItem.Active;
				App.Session.HighLightMove =
					highlightMoveMenuItem.Active;
				boardWidget.QueueDraw ();
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
								   open ?
								   FileChooserAction.
								   Open :
								   FileChooserAction.
								   Save,
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

			ProgressDialog dlg;
			PGNGameLoader gameloader;

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
				gameloader = new PGNGameLoader ();
				gameloader.GameLoaded += OnGameLoaded;
				viewer.StatusBar.Pop (1);
				viewer.StatusBar.Push (1,
						       Catalog.
						       GetString
						       ("Parsing the file..."));
				GLib.Idle.Add (new GLib.IdleHandler (delegate
								     {
								     parser.
								     Parse
								     (gameloader);
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
				ChessGame game = new ChessGame (args.Game);
				if (GameViewer.GameDb == null)
					games.Add (game);
				else
				  {
					  ChessGame dbgame;
					  if (!GameViewer.GameDb.
					      GetGameDetails (game,
							      out dbgame))
					    {	// not found in the db
						    dbgame = game;
					    }
					  games.Add (dbgame);
				  }

				dlg.ProgressBar.Text =
					Catalog.GetString ("Loaded ") +
					games.Count +
					Catalog.GetString (" games");
				dlg.Pulse ();
			}
		}
	}
}
