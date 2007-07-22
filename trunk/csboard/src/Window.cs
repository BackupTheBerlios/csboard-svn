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
// Copyright (C) 2004 Nickolay V. Shmyrev

namespace CsBoard
{

	using System;
	using System.Collections;
	using Gtk;
	using Gdk;
	using Mono.Unix;

	public interface MainApp
	{
		void AddApp (SubApp app);
		void ShowApp (int i);
		void ShowApp (SubApp app);
	}

	public interface SubApp
	{
		MenuBar MenuBar
		{
			get;
		}

		Widget Widget
		{
			get;
		}

		ToolButton ToolButton
		{
			get;
		}

		AccelGroup AccelGroup
		{
			get;
		}

		string Title
		{
			get;
		}

		void SetVisibility (bool visible);
	}

	public class ChessWindow:MainApp, SubApp
	{

		[Glade.Widget] private Gtk.Window csboardWindow;
		[Glade.Widget] private Gtk.Container frame, appBox;
		[Glade.Widget] private Gtk.Statusbar statusbar;
		[Glade.Widget] private Gtk.Frame status_frame;

		[Glade.Widget] private Gtk.CheckMenuItem beginner;
		[Glade.Widget] private Gtk.CheckMenuItem intermediate;
		[Glade.Widget] private Gtk.CheckMenuItem advanced;
		[Glade.Widget] private Gtk.CheckMenuItem expert;

		[Glade.Widget] private Gtk.CheckMenuItem last_move;
		[Glade.Widget] private Gtk.CheckMenuItem show_coords;
		[Glade.Widget] private Gtk.CheckMenuItem possible_moves;
		[Glade.Widget] private Gtk.CheckMenuItem animate;

		// Menus to make them unsensitive
		[Glade.Widget] private Gtk.Container game_menu;
		[Glade.Widget] private Gtk.Container action_menu;

		private bool menusSensitive;

		private uint gameStatusbarId;
		private uint moveStatusbarId;

		private PlayerBoard boardWidget;
		private ProgressBar progressbar;
		private IControl control;

		bool whitesTurn;
		int nmoves = 0;

		ChessGameWidget chessGameWidget;

		[Glade.Widget] Notebook menusBook;
		[Glade.Widget] Toolbar appsBar;
		[Glade.Widget] Notebook appsBook;
		[Glade.Widget] ToolButton playerToolButton;
		[Glade.Widget] MenuBar menubar;

		AccelGroup accel;
		public AccelGroup AccelGroup
		{
			get
			{
				return accel;
			}
		}

		public Widget Widget
		{
			get
			{
				return appBox;
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

		public ToolButton ToolButton
		{
			get
			{
				return playerToolButton;
			}
		}

		public MenuBar MenuBar
		{
			get
			{
				return menubar;
			}
		}

		private static ChessWindow instance;
		public static ChessWindow Instance
		{
			get
			{
				return instance;
			}
		}
		ArrayList subapps;

		public void SetVisibility (bool visible)
		{
		}

		public void AddApp (SubApp app)
		{
			subapps.Add (app);

			menusBook.AppendPage (app.MenuBar, new Label ());
			int i = appsBar.NItems;
			appsBar.Insert (app.ToolButton, i++);
			SeparatorToolItem separator = new SeparatorToolItem();
			separator.Show();
			appsBar.Insert (separator, i);
			app.ToolButton.Clicked += OnToolButtonClicked;
			appsBook.AppendPage (app.Widget, new Label ());
		}

		private void OnToolButtonClicked (object o, EventArgs args)
		{
			int i = 0;
			foreach (SubApp app in subapps)
			{
				if (app.ToolButton.Equals (o))
				  {
					  ShowApp (i);
					  break;
				  }
				i++;
			}
		}

		public void ShowApp (SubApp app)
		{
			int i = 0;
			foreach (SubApp subapp in subapps)
			{
				if (subapp.Equals (app))
				  {
					  ShowApp (i);
					  return;
				  }
				i++;
			}
		}

		public void ShowApp (int i)
		{
			int curappIndex = appsBook.CurrentPage;
			SubApp app = subapps[curappIndex] as SubApp;
			app.SetVisibility (false);
			if (app.AccelGroup != null)
			  {
				  csboardWindow.RemoveAccelGroup (app.
								  AccelGroup);
			  }

			menusBook.CurrentPage = i;
			appsBook.CurrentPage = i;
			app = subapps[i] as SubApp;
			if (app.AccelGroup != null)
				csboardWindow.AddAccelGroup (app.AccelGroup);
			app.SetVisibility (true);
		}

		public ChessWindow (string filename):this (null, filename)
		{
		}

		private void CreateControl (string engine)
		{
			if (engine == null)
				engine = App.Session.Engine;
			/* try { */

			if (engine.LastIndexOf ("crafty ") >= 0)
			  {
				  control = new Crafty (engine);
			  }
			else if (engine.LastIndexOf ("phalanx ") >= 0)
			  {
				  control = new Phalanx (engine);
			  }
			else if (engine.LastIndexOf ("gnuchess ") >= 0)
			  {
				  control = new GnuChess (engine);
			  }
			else
			  {
				  MessageDialog md =
					  new MessageDialog (csboardWindow,
							     DialogFlags.
							     DestroyWithParent,
							     MessageType.
							     Error,
							     ButtonsType.
							     Close,
							     Catalog.
							     GetString
							     ("<b>Unknown engine</b>\n\nPlease check gconf keys of csboard"));

				  md.Run ();
				  md.Hide ();
				  md.Dispose ();

				  control =
					  new
					  GnuChess
					  ("/usr/bin/gnuchess -x -e");

			  }

			control.WaitEvent +=
				new ControlWaitHandler (on_control_wait);
			control.BusyEvent +=
				new ControlBusyHandler (on_control_busy);
			control.PositionChangedEvent +=
				new
				ControlPositionChangedHandler
				(on_position_changed);
			control.GameOverEvent +=
				new
				ControlGameOverHandler (on_control_game_over);
			control.SwitchSideEvent +=
				new
				ControlSwitchSideHandler (on_control_side);
			control.HintEvent +=
				new ControlHintHandler (on_control_hint);

		}

		public ChessWindow (string engine, string filename)
		{
			title = Catalog.GetString ("Welcome to CS Board");
			instance = this;
			subapps = new ArrayList ();
			CreateControl (engine);
			Glade.XML gXML =
				Glade.XML.FromAssembly ("csboard.glade",
							"csboardWindow",
							null);
			gXML.Autoconnect (this);
			accel = new AccelGroup ();
			csboardWindow.AddAccelGroup (accel);
			Gtk.Image img =
				new Gtk.Image (Gdk.Pixbuf.
					       LoadFromResource
					       ("computer.png"));
			img.Show ();
			playerToolButton.IconWidget = img;


			subapps.Add (this);
			AddApp (new CsBoard.ICS.ICSDetailsWidget ());
			AddApp (CsBoard.Viewer.GameViewer.Instance);
			playerToolButton.Clicked += OnToolButtonClicked;

			gameStatusbarId = statusbar.GetContextId ("game");
			gameStatusbarId = statusbar.GetContextId ("move");

			// FIXME: Use libglade to create toolbar                        

			App.Session.SetupGeometry (csboardWindow);

			// Setup board widget
			progressbar = new ProgressBar ();
			status_frame.Add (progressbar);

			boardWidget =
				new PlayerBoard (control.GetPosition ());
			chessGameWidget = new ChessGameWidget (boardWidget);
			chessGameWidget.Black = Catalog.GetString ("Black");
			chessGameWidget.White = Catalog.GetString ("White");

			frame.Add (chessGameWidget);

			SetupLevel ();

			boardWidget.showCoords = App.Session.ShowCoords;
			boardWidget.highLightMove = App.Session.HighLightMove;
			boardWidget.showAnimations =
				App.Session.showAnimations;

			show_coords.Active = App.Session.ShowCoords;
			last_move.Active = App.Session.HighLightMove;
			possible_moves.Active = App.Session.PossibleMoves;
			animate.Active = App.Session.showAnimations;


			boardWidget.MoveEvent +=
				new BoardMoveHandler (on_board_move);

			boardWidget.StartMoveHintEvent +=
				new
				StartMoveHintHandler (on_board_start_move);

			if (filename == null)
				control.OpenGame (App.Session.Filename);
			else
				CsBoard.Viewer.GameViewer.Instance.
					Load (filename);

			chessGameWidget.Show ();
			boardWidget.Show ();
			csboardWindow.Show ();
		}

		public void on_quit_window_activate (System.Object b,
						     DeleteEventArgs e)
		{
			on_quit_activate (b, null);
			chessGameWidget.whiteClock.Stop ();
			chessGameWidget.blackClock.Stop ();
		}

		public void on_quit_activate (System.Object b, EventArgs e)
		{
			Quit();
		}

		public void Quit() {
			App.Session.SaveGeometry (csboardWindow);
			control.Shutdown ();
			csboardWindow.Hide ();
			csboardWindow.Dispose ();
			App.Close ();
		}

		public void on_viewer_clicked (System.Object b, EventArgs e)
		{
			App.StartViewer (null);
		}

		public void on_icsplayer_clicked (System.Object b,
						  EventArgs e)
		{
			App.StartICSPlayer ();
		}

		public void on_new_activate (System.Object b, EventArgs e)
		{

			statusbar.Pop (gameStatusbarId);

			control.NewGame ();
			whitesTurn = true;
			nmoves = 0;

			boardWidget.Reset ();
			chessGameWidget.whiteClock.Reset (5 * 60, 0);
			chessGameWidget.blackClock.Reset (5 * 60, 0);
			UpdateGameDetails ();

			control.SaveGame (App.Session.Filename);
		}

		private void UpdateGameDetails ()
		{
			chessGameWidget.White = GetWhitePlayerName ();
			chessGameWidget.Black = GetBlackPlayerName ();
		}

		private string GetWhitePlayerName ()
		{
			return "White";
		}

		private string GetBlackPlayerName ()
		{
			return "Black";
		}

		public void on_open_activate (System.Object b, EventArgs e)
		{

			statusbar.Pop (gameStatusbarId);

			FileChooserDialog fd =
				new FileChooserDialog (Catalog.
						       GetString
						       ("Open Game"),
						       csboardWindow,
						       FileChooserAction.
						       Open);

			fd.AddButton (Stock.Close, (int) ResponseType.Close);
			fd.AddButton (Stock.Open, (int) ResponseType.Ok);

			if (fd.Run () == (int) ResponseType.Ok)
			  {
				  control.OpenGame (fd.Filename);
				  control.SaveGame (App.Session.Filename);
			  }
			fd.Hide ();
			fd.Dispose ();

		}
		public void on_save_activate (System.Object b, EventArgs e)
		{

			FileChooserDialog fd =
				new FileChooserDialog (Catalog.
						       GetString
						       ("Save Game"),
						       csboardWindow,
						       FileChooserAction.
						       Save);

			fd.AddButton (Stock.Close, (int) ResponseType.Close);
			fd.AddButton (Stock.Save, (int) ResponseType.Ok);

			int res = fd.Run ();
			fd.Hide ();

			if (res == (int) ResponseType.Ok)
			  {

				  if (System.IO.File.Exists (fd.Filename))
				    {
					    MessageDialog md =
						    new
						    MessageDialog
						    (csboardWindow,
						     DialogFlags.
						     DestroyWithParent,
						     MessageType.Warning,
						     ButtonsType.OkCancel,
						     Catalog.
						     GetString ("File ") +
						     fd.Filename +
						     Catalog.
						     GetString
						     (" already exists\n\n") +
						     Catalog.
						     GetString
						     ("Do you wish to overwrite it?"));
					    if (md.Run () ==
						(int) ResponseType.Ok)
					      {
						      control.SaveGame (fd.
									Filename);
					      }
					    md.Hide ();
					    md.Dispose ();
				    }
				  else
				    {
					    control.SaveGame (fd.Filename);
				    }
			  }

			fd.Dispose ();

		}

		public void on_undo_activate (System.Object b, EventArgs e)
		{
			control.Undo ();
		}

		public void on_redo_activate (System.Object b, EventArgs e)
		{
			control.OpenGame (App.Session.Filename);
		}

		public void on_switch_side_activate (System.Object b,
						     EventArgs e)
		{
			control.SwitchSide ();
			return;
		}

		public void on_book_activate (System.Object b, EventArgs e)
		{

			ArrayList result = control.Book ();

			if (result.Count == 0)
			  {

				  MessageDialog md =
					  new MessageDialog (csboardWindow,
							     DialogFlags.
							     DestroyWithParent,
							     MessageType.Info,
							     ButtonsType.
							     Close,
							     Catalog.
							     GetString
							     ("There is no book move in this position"));
				  md.Run ();
				  md.Hide ();
				  md.Dispose ();
			  }
			else
			  {
				  BookDialog dialog = new BookDialog (result);

				  if (dialog.Run () ==
				      (int) ResponseType.Apply)
				    {
					    dialog.Hide ();

					    string move;
					    move = dialog.GetMove ();
					    if (move != null)
					      {
						      control.MakeMove (move);
					      }

					    dialog.Dispose ();
				    }
				  else
				    {
					    dialog.Hide ();
					    dialog.Dispose ();
				    }
			  }

		}

		public void on_hint_activate (System.Object b, EventArgs e)
		{
			control.Hint ();
		}

		public void on_control_hint (string hint)
		{


			MessageDialog md = new MessageDialog (csboardWindow,
							      DialogFlags.
							      DestroyWithParent,
							      MessageType.
							      Info,
							      ButtonsType.
							      OkCancel,
							      Catalog.
							      GetString
							      ("You can move ")
							      + hint);

			md.DefaultResponse = ResponseType.Ok;
			int response = md.Run ();

			if (response == (int) ResponseType.Ok)
			  {
				  control.MakeMove (hint);
			  }

			md.Hide ();
			md.Dispose ();
		}

		public void on_level_activate (System.Object b, EventArgs e)
		{
			Level level;

			level = Level.Expert;

			if (beginner.Active)
				possible_moves.Active = true;
			level = Level.Beginner;
			if (intermediate.Active)
				level = Level.Intermediate;
			if (advanced.Active)
				level = Level.Advanced;

			App.Session.level = level;
			control.SetLevel (level);
		}

		public void on_last_move_activate (System.Object b,
						   EventArgs e)
		{
			App.Session.HighLightMove = last_move.Active;
			boardWidget.highLightMove = last_move.Active;
			boardWidget.QueueDraw ();
		}

		public void on_possible_moves_activate (System.Object b,
							EventArgs e)
		{
			App.Session.PossibleMoves = possible_moves.Active;
			boardWidget.showMoveHint = possible_moves.Active;
			boardWidget.QueueDraw ();
		}

		public void on_animate_activate (System.Object b, EventArgs e)
		{
			boardWidget.showAnimations = animate.Active;
			App.Session.showAnimations = animate.Active;
		}

		public void on_show_coords_activate (System.Object b,
						     EventArgs e)
		{
			App.Session.ShowCoords = show_coords.Active;
			boardWidget.showCoords = show_coords.Active;
			boardWidget.QueueDraw ();
		}

		public void on_contents_activate (System.Object b,
						  EventArgs e)
		{
			ShowHelpContents();
		}

		public void on_about_activate (System.Object b, EventArgs e)
		{
			ShowAboutDialog (csboardWindow);
		}

		public static void ShowHelpContents ()
		{
			System.Diagnostics.Process proc =
				new System.Diagnostics.Process ();

			proc.StartInfo.FileName = "yelp";
			proc.StartInfo.Arguments =
				Config.prefix +
				"/share/gnome/help/csboard/C/csboard.xml";
			proc.StartInfo.UseShellExecute = true;
			try
			{
				proc.Start ();
			} catch
			{
				// do nothing
			}
		}

		public static void ShowAboutDialog (Gtk.Window win)
		{
			AboutDialog ad = new AboutDialog ();

			ad.Name = "CsBoard";
			ad.Authors = new string[]
			{
			"Nickolay V. Shmyrev <nshmyrev@yandex.ru>",
					"Ravi Kiran U V S <uvsravikiran@gmail.com>"};
			ad.TranslatorCredits =
				Catalog.GetString ("translator_credits");
			ad.Documenters = new string[]
			{
			"Nickolay V. Shmyrev <nshmyrev@yandex.ru>"};
			ad.Logo =
				new Gdk.Pixbuf (Config.prefix +
						"/share/pixmaps/csboard.png");
			ad.Website = "http://csboard.berlios.de";

			ad.TransientFor = win;

			ad.Run ();
			ad.Hide ();
			ad.Dispose ();
		}

		public void on_control_wait (string move)
		{

			statusbar.Pop (moveStatusbarId);

			if (move != null)
			  {
				  statusbar.Push (moveStatusbarId, move);
				  control.SaveGame (App.Session.Filename);
				  boardWidget.lastMove = move;
				  boardWidget.QueueDraw ();
			  }

			SetSensitive (true);
			progressbar.Stop ();

			UpdateAfterMove ();
		}

		public void on_position_changed (ArrayList data)
		{
			statusbar.Pop (moveStatusbarId);
			boardWidget.SetPosition (data);
		}

		private void UpdateAfterMove ()
		{
			nmoves++;
			whitesTurn = !whitesTurn;	// flip turn
			if (whitesTurn)
			  {
				  chessGameWidget.blackClock.Stop ();
				  chessGameWidget.whiteClock.Start ();
			  }
			else
			  {
				  chessGameWidget.whiteClock.Stop ();
				  chessGameWidget.blackClock.Start ();
			  }

			return;
		}

		public void on_board_move (string move)
		{
			if (!control.MakeMove (move))
			  {

				  MessageDialog md =
					  new MessageDialog (csboardWindow,
							     DialogFlags.
							     DestroyWithParent,
							     MessageType.
							     Warning,
							     ButtonsType.
							     Close,
							     Catalog.
							     GetString
							     ("Illegal move"));
				  md.Run ();
				  md.Hide ();
				  md.Dispose ();

			  }
			else
			  {
				  UpdateAfterMove ();
			  }
		}

		public void on_board_start_move (string pos)
		{
			if (possible_moves.Active)
			  {
				  string hint = control.PossibleMoves (pos);
				  boardWidget.moveHint = hint;
				  boardWidget.QueueDraw ();
			  }
		}

		public void on_control_game_over (string reason)
		{


			statusbar.Push (gameStatusbarId,
					Catalog.GetString ("Game Over"));
			if (whitesTurn)
				chessGameWidget.whiteClock.Stop ();
			else
				chessGameWidget.blackClock.Stop ();

			MessageDialog md = new MessageDialog (csboardWindow,
							      DialogFlags.
							      DestroyWithParent,
							      MessageType.
							      Info,
							      ButtonsType.
							      Close,
							      Catalog.
							      GetString
							      ("Game Over") +
							      "\n" + reason);
			md.Run ();
			md.Hide ();
			md.Dispose ();

			return;
		}

		public void on_control_busy ()
		{

			statusbar.Pop (moveStatusbarId);
			statusbar.Push (moveStatusbarId,
					Catalog.GetString ("Thinking"));
			SetSensitive (false);
			progressbar.Start ();

		}

		public void on_control_side (bool side)
		{
			boardWidget.side = side;
			// side == false for white at bottom
			chessGameWidget.WhiteAtBottom = !side;

			boardWidget.QueueDraw ();
		}

		private void SetWidgetSensitive (Widget w)
		{
			w.Sensitive = menusSensitive;
		}

		private void SetSensitive (bool val)
		{

			boardWidget.Sensitive = val;
			menusSensitive = val;

			game_menu.Foreach (new Gtk.
					   Callback (SetWidgetSensitive));
			action_menu.Foreach (new Gtk.
					     Callback (SetWidgetSensitive));
		}
		private void SetupLevel ()
		{

			Level level = App.Session.level;
			control.SetLevel (level);
			switch (level)
			  {
			  case Level.Beginner:
				  beginner.Active = true;
				  break;
			  case Level.Intermediate:
				  intermediate.Active = true;
				  break;
			  case Level.Advanced:
				  advanced.Active = true;
				  break;
			  default:
				  expert.Active = true;
				  break;
			  }

		}

		protected void on_edit_engine_activate (object o,
							EventArgs args)
		{
			ShowEngineChooser ();
		}

		public static void ShowEngineChooser ()
		{
			string engine =
				EngineChooser.ChooseEngine (App.Session.
							    Engine);
			try
			{
				Console.WriteLine
					("EngineChooser returned: {0}",
					 engine);
				App.StartPlayer (engine, null);
				if (engine != null)
					App.Session.Engine = engine;
			}
			catch
			{
				MessageDialog md = new MessageDialog (null,
								      DialogFlags.
								      DestroyWithParent,
								      MessageType.
								      Error,
								      ButtonsType.
								      Close,
								      Catalog.
								      GetString
								      ("Unknown engine"));
				md.Run ();
				md.Hide ();
				md.Dispose ();
			}
		}
	}

	class EngineChooser
	{
		[Glade.Widget] private Gtk.Dialog chooseEngineDialog;
		[Glade.Widget] private Gtk.RadioButton gnuchessButton,
			craftyButton, phalanxButton, icsButton;

		  EngineChooser (string engine)
		{
			Glade.XML xml =
				Glade.XML.FromAssembly ("csboard.glade",
							"chooseEngineDialog",
							null);
			xml.Autoconnect (this);

			if (engine.StartsWith ("gnuchess"))
				gnuchessButton.Active = true;
			else if (engine.StartsWith ("crafty"))
				craftyButton.Active = true;
			else if (engine.StartsWith ("phalanx"))
				phalanxButton.Active = true;
			else if (engine.StartsWith ("ICS"))
				icsButton.Active = true;
		}

		public static string ChooseEngine (string curengine)
		{
			EngineChooser chooser = new EngineChooser (curengine);
			string engine;
			if (chooser.chooseEngineDialog.Run () !=
			    (int) ResponseType.Ok)
				engine = null;
			else
				engine = chooser.ChosenEngine ();
			chooser.chooseEngineDialog.Hide ();
			chooser.chooseEngineDialog.Dispose ();
			return engine;
		}

		private string ChosenEngine ()
		{
			if (gnuchessButton.Active)
				return "gnuchess -x -e";
			if (craftyButton.Active)
				return "crafty ";
			if (phalanxButton.Active)
				return "phalanx -l-";
			if (icsButton.Active)
				return "ICS ";
			return null;
		}
	}
}
