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
using Gnome.Vfs;
using Gnome;
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
			private Gtk.Label whiteLabel, blackLabel;

			private Board boardWidget;
			GameSession gameSession;
			ChessGameWidget gameWidget;
			GamesListWidget gamesListWidget;

			string loadUrl;
			string pgnBuffer;
			bool loadingInProgress;

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
			public GameViewer (string file)
			{
				Glade.XML gXML =
					new Glade.
					XML ("resource/csboard.glade",
					     "gameViewerWindow", null);
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
				if (file != null)
					  LoadGames (file);
				  gameViewerWindow.Show ();
				  gameSession = new GameSession ();
			}

			public void LoadGames (string uri)
			{
				if (loadingInProgress)
					return;
				loadUrl = uri;
				pgnBuffer = null;
				loadingInProgress = true;
				statusBar.Push (1, "Loading: " + uri);
				GLib.Idle.Add (new GLib.
					       IdleHandler
					       (LoadGamesIdleHandler));
			}

			private void LoadGamesFromBuffer (string buffer)
			{
				if (loadingInProgress)
					return;
				pgnBuffer = buffer;
				loadingInProgress = true;
				statusBar.Push (1, "Loading from buffer...");
				GLib.Idle.Add (new GLib.
					       IdleHandler
					       (LoadGamesIdleHandler));
			}

			private bool LoadGamesIdleHandler ()
			{
				if (pgnBuffer != null)
				  {
					  games = PGNParser.
						  loadGamesFromBuffer
						  (pgnBuffer);
					  statusBar.Pop (1);
					  statusBar.Push (1,
							  "Read successfully. Parsing it...");
				  }
				else
				  {
					  if (loadUrl == null
					      || loadUrl.Length == 0)
					    {
						    loadingInProgress = false;
						    statusBar.Pop (1);
						    return false;
					    }
					  VfsStream stream = new VfsStream (loadUrl, FileMode.Open);	// url
					  //    ArrayList games = PGNParser.loadGamesFromFile(file);
					  statusBar.Pop (1);
					  statusBar.Push (1,
							  "Read successfully. Parsing it...");
					  games = PGNParser.
						  loadGamesFromStream
						  (stream);
					  stream.Close ();
				  }
				gamesListWidget.SetGames (games);
				SelectGame (0);
				statusBar.Pop (1);
				statusBar.Push (1, "Showing " + loadUrl);
				loadingInProgress = false;
				return false;
			}

			public void on_save_as_activate (System.Object b,
							 EventArgs e)
			{
				if (games == null || games.Count == 0)
					return;
				string file = AskForFile ("Save the game as",
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

			public void on_open_file_activate (System.Object b,
							   EventArgs e)
			{
				string file = AskForFile ("Choose the file to open", true);	// true for open
				if (file == null)
					return;

				LoadGames (file);
			}

			public void on_open_url_activate (System.Object b,
							  EventArgs e)
			{
				string url = AskForUrl ();
				if (url == null)
					return;

				LoadGames (url);
			}

			public void on_load_pgn_activate (System.Object b,
							  EventArgs e)
			{
				string buffer = AskForPGNBuffer ();
				if (buffer == null)
					return;

				LoadGamesFromBuffer (buffer);
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

			private void on_export_ps_activate (object obj,
							    EventArgs args)
			{
				string file =
					AskForFile
					("Export as a PostScript document to file",
					 false);
				if (file == null)
					return;
				PrintWrapper printer = new PrintWrapper ();
				ProgressDialog prog =
					new ProgressDialog (gameViewerWindow);
				ExportHandler exp =
					new ExportHandler (prog, games,
							   printer, file);
				prog.Run ();
				prog.Hide ();
				prog.Dispose ();
			}

			private void on_print_activate (object obj,
							EventArgs args)
			{
				PrintWrapper printer = new PrintWrapper ();
				PrintDialog dialog =
					new PrintDialog (printer.PrintJob,
							 "Print PGN File", 0);
				int response = dialog.Run ();

				if (response == (int) PrintButtons.Cancel)
				  {
					  dialog.Hide ();
					  dialog.Dispose ();
					  return;
				  }
				ProgressDialog prog =
					new ProgressDialog (dialog);
				prog.ShowAll ();
				new PrintHandler (prog, games, printer,
						  response);
				prog.Run ();	// The PrintHandler will bail us out!
				prog.Hide ();
				prog.Dispose ();

				dialog.Hide ();
				dialog.Dispose ();
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
						"<b>" + game.GetTagValue("White", "White") +
						"</b>";
				blackLabel.Markup =
						"<b>" + game.GetTagValue("Black", "Black") +
						"</b>";
			}

			string AskForFile (string title, bool open)
			{
				string file = null;
				Gtk.FileChooserDialog fc =
					new Gtk.FileChooserDialog (title,
								   gameViewerWindow,
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

			string AskForUrl ()
			{
				string url = null;
				UrlDialog dlg =
					new UrlDialog (gameViewerWindow);
				if (dlg.Run () == (int) ResponseType.Accept)
				  {
					  url = dlg.Url;
				  }
				dlg.Destroy ();
				return url;
			}

			string AskForPGNBuffer ()
			{
				string buffer = null;
				PGNBufferDialog dlg =
					new
					PGNBufferDialog (gameViewerWindow);
				if (dlg.Run () == (int) ResponseType.Accept)
				  {
					  buffer = dlg.Buffer;
				  }
				dlg.Destroy ();
				return buffer;
			}
		}

		public class UrlDialog:Dialog
		{
			Entry urlEntry;

			public UrlDialog (Gtk.Window par):base ("Open URL",
								par,
								DialogFlags.
								Modal,
								"Cancel",
								ResponseType.
								Cancel,
								"Open",
								ResponseType.
								Accept)
			{
				urlEntry = new Entry ();
				urlEntry.WidthChars = 80;
				urlEntry.Show ();
				VBox.PackStart (urlEntry, true, true, 4);
			}

			public string Url
			{
				get
				{
					return urlEntry.Text;
				}
			}
		}

		public class PGNBufferDialog:Dialog
		{
			TextView textView;

			public PGNBufferDialog (Gtk.
						Window par):base ("Enter PGN",
								  par,
								  DialogFlags.
								  Modal,
								  "Cancel",
								  ResponseType.
								  Cancel,
								  "Open",
								  ResponseType.
								  Accept)
			{
				textView = new TextView ();
				textView.WrapMode = WrapMode.WordChar;
				textView.Editable = true;
				textView.Show ();

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Automatic;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Child = textView;
				  win.Show ();
				  VBox.PackStart (win, true, true, 4);
			}

			public string Buffer
			{
				get
				{
					return textView.Buffer.Text;
				}
			}
		}

		class ProgressDialog:Dialog
		{
			public ProgressBar bar;
			public ProgressDialog (Gtk.
					       Window
					       parent):base ("Printing...",
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
		}

		abstract class PGNExportHandler
		{
			protected ProgressDialog dlg;
			protected ArrayList games;
			protected PrintWrapper printer;
			protected int totalgames;
			protected double ngames;	// so that a we can generate a fraction

			public PGNExportHandler (ProgressDialog d,
						 ArrayList games,
						 PrintWrapper printer)
			{
				dlg = d;
				this.games = games;
				this.printer = printer;
				totalgames = games.Count;
				ngames = 0;
				GLib.Idle.
					Add (new
					     IdleHandler
					     (PGNExportIdleHandler));
			}

			protected void OnGamePrinted (System.Object o,
						      EventArgs args)
			{
				ngames++;
				dlg.UpdateProgress (ngames / totalgames);
			}

			private bool PGNExportIdleHandler ()
			{
				PGNPrinter pr =
					new PGNPrinter (games, printer);
				pr.GamePrinted += OnGamePrinted;
				pr.Print ();
				dlg.bar.Text = "Now printing...";
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
				HandlePrinted ();
				dlg.bar.Text = "Done.";
				dlg.Respond (ResponseType.None);
				return false;
			}

			protected abstract void HandlePrinted ();
		}

		class PrintHandler:PGNExportHandler
		{
			int response;
			public PrintHandler (ProgressDialog d,
					     ArrayList games,
					     PrintWrapper printer,
					     int response):base (d, games,
								 printer)
			{
				this.response = response;
			}

			protected override void HandlePrinted ()
			{
				switch (response)
				  {
				  case (int) PrintButtons.Print:
					  printer.PrintJob.Print ();
					  break;
				  case (int) PrintButtons.Preview:
					  new PrintJobPreview (printer.
							       PrintJob,
							       "Print Preview").
						  Show ();
					  break;
				  }
			}
		}


		class ExportHandler:PGNExportHandler
		{
			string file;

			public ExportHandler (ProgressDialog d,
					      ArrayList games,
					      PrintWrapper printer,
					      string file):base (d, games,
								 printer)
			{
				this.file = file;
			}

			protected override void HandlePrinted ()
			{
				printer.Export (file);
			}
		}
	}
}
