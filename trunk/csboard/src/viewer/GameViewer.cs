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

		public class GameViewer:GameViewerUI, SubApp
		{
			public event TitleChangedEventHandler
				TitleChangedEvent;

			string initialDirForFileChooser = null;

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

			public static void CreateInstance ()
			{
				if (viewer != null)
				  {
					  return;
				  }
				viewer = new GameViewer ();
				CsBoardApp.Instance.AddApp (viewer);
				CsBoard.Plugin.PluginManager.Instance.
					StartPlugins ();
			}

			ToolButton toolbutton;
			public ToolButton ToolButton
			{
				get
				{
					return toolbutton;
				}
			}

			public Widget Widget
			{
				get
				{
					return this;
				}
			}

			AccelGroup accel;
			public AccelGroup AccelGroup
			{
				get
				{
					return accel;
				}
			}

			string title;
			public string Title
			{
				get
				{
					return title;
				}
			}

			bool app_visible = false;
			private GameViewer ()
			{
				title = "Game Viewer";
				accel = new AccelGroup ();
				initialDirForFileChooser =
					App.Session.CurrentFolder;

				gameLoaders = new ArrayList ();
				exporters = new ArrayList ();

				menubar.highlightMoveMenuItem.Active =
					App.Session.HighLightMove;

				menubar.saveAsMenuItem.Activated +=
					on_save_as_activate;
				menubar.switchSideMenuItem.Activated +=
					on_switch_side_activate;
				menubar.moveCommentMenuItem.Activated +=
					OnEditCommentActivated;
				menubar.highlightMoveMenuItem.Activated +=
					OnHighlightMoveMenuItemActivated;
				menubar.switchSideMenuItem.
					AddAccelerator ("activate", accel,
							new AccelKey (Gdk.Key.
								      t,
								      Gdk.
								      ModifierType.
								      ControlMask,
								      AccelFlags.
								      Visible));

				menubar.quitMenuItem.
					AddAccelerator ("activate", accel,
							new AccelKey (Gdk.Key.
								      q,
								      Gdk.
								      ModifierType.
								      ControlMask,
								      AccelFlags.
								      Visible));

				Gtk.Image img = new Gtk.Image ();
				img.Stock = Stock.JustifyFill;
				toolbutton =
					new ToolButton (img,
							Catalog.
							GetString ("Viewer"));
				ShowAll ();
				toolbutton.ShowAll ();

				gameViewerWidget.ChessGameWidget.SplitPane.
					Position =
					App.Session.ViewerSplitPanePosition;

				CsBoardApp.Instance.QuitEvent += OnQuitEvent;
			}

			public void SetVisibility (bool visible)
			{
				app_visible = visible;
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
				return AddToMenu (menubar.fileMenuItem, item,
						  menubar.fileOpenSeparator);
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
				if (!AddToMenu
				    (menubar.exportAsMenuItem, item, null))
					return false;
				exporters.Add (exporter);
				return true;
			}

			public bool UnregisterExporter (IExporter exporter,
							Gtk.MenuItem item)
			{
				Menu menu =
					(Menu) menubar.exportAsMenuItem.
					Submenu;
				menu.Remove (item);
				exporters.Remove (exporter);
				return true;
			}

			public bool RegisterPrintHandler (IPrintHandler
							  handler)
			{
				menubar.printMenuItem.Activated +=
					handler.OnPrintActivated;
				return true;
			}

			public void UnregisterPrintHandler (IPrintHandler
							    handler)
			{
				menubar.printMenuItem.Activated -=
					handler.OnPrintActivated;
			}

			public void LoadGames (TextReader reader)
			{
				if (!app_visible)
					CsBoardApp.Instance.ShowApp (this);
				GameLoader loader =
					new GameLoader (this, reader);
				SetGames (loader.Games);
			}

			public void LoadGames (ArrayList games)
			{
				if (!app_visible)
					CsBoardApp.Instance.ShowApp (this);
				SetGames (games);
			}

			public GameViewerWidget GameViewerWidget
			{
				get
				{
					return gameViewerWidget;
				}
			}

			static GameViewer viewer;

			public static GameViewer Instance
			{
				get
				{
					if (viewer == null)
						CreateInstance ();
					return viewer;
				}
			}

			public VBox ChessGameDetailsBox
			{
				get
				{
					return gameViewerWidget.
						ChessGameWidget.
						ChessGameDetailsBox;
				}
			}

			public void Load (string resource)
			{
				if (!app_visible)
					CsBoardApp.Instance.ShowApp (this);

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
				ArrayList games = gameViewerWidget.Games;
				if (games == null || games.Count == 0)
					return;
				string file = AskForFile (null,
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

			private void OnQuitEvent (System.Object b,
						  EventArgs e)
			{
				App.Session.CurrentFolder =
					initialDirForFileChooser;
				App.Session.ViewerSplitPanePosition =
					gameViewerWidget.ChessGameWidget.
					SplitPane.Position;
			}

			public void on_switch_side_activate (System.Object b,
							     EventArgs e)
			{
				gameViewerWidget.ChessGameWidget.BoardWidget.
					SwitchSides ();
			}

			public void OnEditCommentActivated (object o,
							    EventArgs e)
			{
				if (gameViewerWidget.ChessGameWidget.
				    BoardWidget.Session.Game == null)
					return;
				BufferDialog dlg = new BufferDialog (null,
								     Catalog.
								     GetString
								     ("Edit current comment"));
				string currentComment =
					gameViewerWidget.ChessGameWidget.
					BoardWidget.Session.CurrentComment;
				if (currentComment != null)
					dlg.Buffer = currentComment;
				if (dlg.Run () == (int) ResponseType.Ok)
				  {
					  string comment = dlg.Buffer.Trim ();
					  gameViewerWidget.ChessGameWidget.
						  BoardWidget.Session.
						  CurrentComment =
						  comment.Length ==
						  0 ? null : comment;
					  gameViewerWidget.ChessGameWidget.
						  NotationView.Refresh ();
				  }
				dlg.Hide ();
				dlg.Dispose ();
			}

			public void OnHighlightMoveMenuItemActivated (object
								      o,
								      EventArgs
								      args)
			{
				gameViewerWidget.ChessGameWidget.BoardWidget.
					Board.highLightMove =
					menubar.highlightMoveMenuItem.Active;
				App.Session.HighLightMove =
					menubar.highlightMoveMenuItem.Active;
				gameViewerWidget.ChessGameWidget.BoardWidget.
					Board.QueueDraw ();
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
				dlg = new ProgressDialog (null,
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
