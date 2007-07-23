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

	public delegate IControl ControlCreatorFunc (string command);

	public class ChessWindowUI
	{

		[Glade.Widget] protected Gtk.Window csboardWindow;
		[Glade.Widget] protected Gtk.Container frame, appBox;
		[Glade.Widget] protected Gtk.Statusbar statusbar;
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

		protected PlayerBoard boardWidget;
		private ProgressBar progressbar;
		protected IControl control;

		bool whitesTurn;
		int nmoves = 0;

		protected ChessGameWidget chessGameWidget;

		[Glade.Widget] protected Notebook menusBook;
		[Glade.Widget] protected Toolbar appsBar;
		[Glade.Widget] protected Notebook appsBook;
		[Glade.Widget] protected ToolButton playerToolButton;
		[Glade.Widget] protected MenuBar menubar;

		public event QuitEventHandler QuitEvent;
		static ArrayList controls = new ArrayList ();

		public ChessWindowUI (string filename):this (null, filename)
		{
		}

		public void RegisterControl (EngineInfo info)
		{
			controls.Add (info);
		}

		public static void GetIDFromEngine (string engine,
						    out string id,
						    out string args)
		{
			int index;
			if ((index = engine.IndexOf (' ')) > 0)
			  {
				  id = engine.Substring (0, index);
				  args = engine.Substring (index + 1);
			  }
			else
			  {
				  id = engine;
				  args = "";
			  }
		}
		private void CreateControl (string engine)
		{
			if (engine == null)
				engine = App.Session.Engine;

			string id, args;
			GetIDFromEngine (engine, out id, out args);
			string msg = null;
			try
			{
				foreach (EngineInfo info in controls)
				{
					if (info.ID.Equals (id))
					  {
						  control =
							  info.
							  CreateInstance ();
						  break;
					  }
				}

				if (control == null)
					msg = Catalog.
						GetString
						("<b>Unknown engine</b>\n\nPlease check gconf keys of csboard");
			}
			catch
			{
				msg = String.Format (Catalog.
						     GetString
						     ("<b>Unable to load engine '{0}'</b>"),
						     engine);

			}
			if (control == null)
			  {
				  MessageDialog md =
					  new MessageDialog (csboardWindow,
							     DialogFlags.
							     DestroyWithParent,
							     MessageType.
							     Error,
							     ButtonsType.
							     Close,
							     msg);

				  md.Run ();
				  md.Hide ();
				  md.Dispose ();
				  control = new NullControl ();
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

		public ChessWindowUI (string engine, string filename)
		{
			RegisterControl (GnuChess.Info);
			RegisterControl (Phalanx.Info);
			RegisterControl (Crafty.Info);
			RegisterControl (NullControl.Info);
			CreateControl (engine);
			Glade.XML gXML =
				Glade.XML.FromAssembly ("csboard.glade",
							"csboardWindow",
							null);
			gXML.Autoconnect (this);

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
			Quit ();
		}

		public void Quit ()
		{
			if (QuitEvent != null)
				QuitEvent (this, EventArgs.Empty);
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
			ShowHelpContents ();
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
			string engine = EngineChooser.ChooseEngine (controls,
								    App.
								    Session.
								    Engine);
			if (engine != null)
				App.Session.Engine = engine;
		}
	}

	class EngineChooser:Dialog
	{
		IList engines, radiobuttons;
		  EngineChooser (IList engines,
				 string engine):base (Catalog.
						      GetString
						      ("Choose an engine"),
						      null, DialogFlags.Modal,
						      Stock.Cancel,
						      ResponseType.Cancel,
						      Stock.Ok,
						      ResponseType.Ok)
		{
			radiobuttons = new ArrayList ();
			this.engines = engines;
			RadioButton firstButton = null;
			string id, args;
			  ChessWindowUI.GetIDFromEngine (engine, out id,
							 out args);

			  foreach (EngineInfo info in engines)
			{
				RadioButton button;
				if (firstButton == null)
				  {
					  button = new RadioButton ("");
					  firstButton = button;
				  }
				else
					  button =
						new RadioButton (firstButton,
								 "");
				Label label = new Label ();
				label.UseMarkup = true;
				label.Markup = "<b>" + info.Name + "</b>";
				button.Image = label;

				if (id.Equals (info.ID))
					button.Active = true;

				radiobuttons.Add (button);
				if (!info.Exists ())
					button.Sensitive = false;
				VBox.PackStart (button, false, true, 4);
				VBox.ShowAll ();
			}
		}

		public static string ChooseEngine (IList engines,
						   string curengine)
		{
			string engine;
			EngineChooser chooser =
				new EngineChooser (engines, curengine);
			chooser.Show ();
			if (chooser.Run () != (int) ResponseType.Ok)
				engine = null;
			else
				engine = chooser.ChosenEngine ();
			chooser.Hide ();
			chooser.Dispose ();
			return engine;
		}

		private string ChosenEngine ()
		{
			int i = 0;
			foreach (RadioButton button in radiobuttons)
			{
				if (button.Active)
				  {
					  return (engines[i] as EngineInfo).
						  Command;
				  }
				i++;
			}

			return null;
		}
	}
}
