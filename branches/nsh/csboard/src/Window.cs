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

namespace CsBoard {

        using System;
        using System.Collections;
        using Gtk;
        using Gdk;

        public class ChessWindow {

                [Glade.Widget] private Gtk.Window csboardWindow;
                [Glade.Widget] private Gtk.Container frame;
                [Glade.Widget] private Gtk.Statusbar statusbar;
                [Glade.Widget] private Gtk.Frame     status_frame;

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

                private Board boardWidget;
		private ProgressBar progressbar;
                private IControl control;

                public ChessWindow (string filename) {

                        try {
				string engine = App.session.Engine;
				
				if (engine.LastIndexOf ("crafty ") >= 0) {
                                    control = new Crafty (engine);
				} else 
				if (engine.LastIndexOf ("phalanx ") >= 0) {
                                    control = new Phalanx (engine);
				} else 
				if (engine.LastIndexOf ("gnuchess ") >= 0) {
				    control = new GnuChess (engine);
				} else 
				if (engine.LastIndexOf ("ICS ") >= 0) {
				    control = new ICS (engine);
				} else {
				  MessageDialog md =
                                        new MessageDialog (csboardWindow,
                                                           DialogFlags.
                                                           DestroyWithParent,
                                                           MessageType.Error,
                                                           ButtonsType.Close,
                                                           Catalog.GetString(
							   "Unknown engine. Please check gconf keys of csboard"));

                                  md.Run ();
                                  md.Hide ();
                                  md.Dispose ();

				  control = new GnuChess ("gnuchess -x -e");

				}
				
                        } catch (ApplicationException e) {

                                MessageDialog md =
                                        new MessageDialog (csboardWindow,
                                                           DialogFlags.
                                                           DestroyWithParent,
                                                           MessageType.Error,
                                                           ButtonsType.Close,
                                                           e.Message);

                                md.Run ();
                                md.Hide ();
                                md.Dispose ();  
				
				throw e;
			}		

                        Glade.XML gXML =
                                new Glade.
                                XML ("resource/csboard.glade",
                                     "csboardWindow", null);
                        gXML.Autoconnect (this);
                        
                        gameStatusbarId = statusbar.GetContextId ("game");
                        gameStatusbarId = statusbar.GetContextId ("move");

                        // FIXME: Use libglade to create toolbar			

			App.session.SetupGeometry (csboardWindow);			
			csboardWindow.Show ();
			
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
                                new
                                ControlHintHandler (on_control_hint);


			    
			// Setup board widget
			progressbar = new ProgressBar ();
			status_frame.Add (progressbar);
			
                        boardWidget = new Board (control.GetPosition ());
                        frame.Add (boardWidget);
                        boardWidget.Show ();
			    
			SetupLevel ();

			boardWidget.showCoords = App.session.ShowCoords;
			boardWidget.highLightMove = App.session.HighLightMove;
			boardWidget.showAnimations = App.session.showAnimations;

			show_coords.Active = App.session.ShowCoords;
			last_move.Active = App.session.HighLightMove;	
			possible_moves.Active = App.session.PossibleMoves;	
			animate.Active = App.session.showAnimations;


                        boardWidget.MoveEvent +=
                                new BoardMoveHandler (on_board_move);

                        boardWidget.StartMoveHintEvent +=
                                new StartMoveHintHandler (on_board_start_move);

			if (filename == null)
                            control.OpenGame (App.session.Filename);			
			else 
			    control.OpenGame (filename);

                }

                private void on_quit_activate (System.Object b, EventArgs e) {
		        App.session.SaveGeometry (csboardWindow);
                        control.Shutdown ();
                        Application.Quit ();
                }

                private void on_new_activate (System.Object b, EventArgs e) {

                        statusbar.Pop (gameStatusbarId);

                        control.NewGame ();
                        control.SaveGame(App.session.Filename);
                }

                private void on_open_activate (System.Object b, EventArgs e) {

                        statusbar.Pop (gameStatusbarId);
			
                        FileChooserDialog fd = new FileChooserDialog (Catalog.GetString ("Open Game"), csboardWindow, FileChooserAction.Open);

			fd.AddButton (Stock.Close, (int) ResponseType.Close);
                        fd.AddButton (Stock.Open, (int) ResponseType.Ok);
			
                        if (fd.Run () == (int)ResponseType.Ok) {
                                control.OpenGame (fd.Filename);
                                control.SaveGame(App.session.Filename);
                                }
                        fd.Hide ();
                        fd.Dispose ();

                }
                private void on_save_activate (System.Object b, EventArgs e) {

                        FileChooserDialog fd = new FileChooserDialog (Catalog.GetString("Save Game"), csboardWindow, FileChooserAction.Save);
			
			fd.AddButton (Stock.Close, (int) ResponseType.Close);
                        fd.AddButton (Stock.Save, (int) ResponseType.Ok);

                        int res = fd.Run();
			fd.Hide ();
			
                        if (res == (int) ResponseType.Ok) {

                                        if (System.IO.File.Exists (fd.Filename)) {
                                                MessageDialog md =
                                                new MessageDialog (csboardWindow,
                                                           DialogFlags.
                                                           DestroyWithParent,
                                                           MessageType.Warning,
                                                           ButtonsType.OkCancel,
                                                           Catalog.GetString("File ") + fd.Filename + 
                                                           Catalog.GetString(" already exists\n\n") + 
                                                           Catalog.GetString("Do you wish to overwrite it?"));
                                                if (md.Run () == (int)ResponseType.Ok) {
                                                        control.SaveGame (fd.Filename);
                                                }                                                
                                                md.Hide ();
                                                md.Dispose ();                                
                                        } else {
                                                control.SaveGame (fd.Filename);
                                        }
                                }

                        fd.Dispose ();

                }

                private void on_undo_activate (System.Object b, EventArgs e) {
                        control.Undo ();
                }

                private void on_redo_activate (System.Object b, EventArgs e) {
                        control.OpenGame (App.session.Filename);
                }

                private void on_switch_side_activate (System.Object b,
                                                      EventArgs e) {
                        control.SwitchSide ();
                        return;
                }

                private void on_book_activate (System.Object b, EventArgs e) {

                        ArrayList result = control.Book ();

                        if (result.Count == 0) {

                                MessageDialog md =
                                        new MessageDialog (csboardWindow,
                                                           DialogFlags.
                                                           DestroyWithParent,
                                                           MessageType.Info,
                                                           ButtonsType.Close,
                                                           Catalog.GetString("There is no book move in this position"));
                                md.Run ();
                                md.Hide ();
                                md.Dispose ();
                        }
                        else {
                                BookDialog dialog = new BookDialog (result);

                                if (dialog.Run () == (int) ResponseType.Apply) {
                                        dialog.Hide ();

                                        string move;
                                        move = dialog.GetMove ();
                                        if (move != null) {
                                                control.MakeMove (move);
                                        }

                                        dialog.Dispose ();
                                }
                                else {
                                        dialog.Hide ();
                                        dialog.Dispose ();
                                }
                        }

                }

                private void on_hint_activate (System.Object b, EventArgs e) {
			control.Hint ();
                }

		private void on_control_hint (string hint) {		
		

                 MessageDialog md = new MessageDialog (csboardWindow,
                                                              DialogFlags.
                                                              DestroyWithParent,
                                                              MessageType.
                                                              Info,
                                                              ButtonsType.
                                                              OkCancel,
							      Catalog.GetString("You can move ") + hint);
		    
		    		md.DefaultResponse = ResponseType.Ok;							      
	                        int response = md.Run ();
		    
				    	if (response == (int)ResponseType.Ok) {
					    	   control.MakeMove (hint);
					    }		    
		    
    		                md.Hide ();
    		                md.Dispose ();
		}

                private void on_level_activate (System.Object b, EventArgs e) {
			Level level;
  		       
  		        level = Level.Expert;
			
		        if (beginner.Active) 
				      possible_moves.Active = true;
				      level = Level.Beginner;
			if (intermediate.Active) 
				      level = Level.Intermediate;
			if (advanced.Active) 
				      level = Level.Advanced;
			    
			App.session.level = level;
			control.SetLevel (level);
                }

                private void on_last_move_activate (System.Object b, EventArgs e) {
		         App.session.HighLightMove = last_move.Active;
			 boardWidget.highLightMove = last_move.Active;
			 boardWidget.QueueDraw ();
		}

                private void on_possible_moves_activate (System.Object b, EventArgs e) {
		         App.session.PossibleMoves = possible_moves.Active;
			 boardWidget.showMoveHint = possible_moves.Active;
			 boardWidget.QueueDraw ();
		}

                private void on_animate_activate (System.Object b, EventArgs e) {
 			 boardWidget.showAnimations = animate.Active;
		         App.session.showAnimations = animate.Active;
		}

                private void on_show_coords_activate (System.Object b, EventArgs e) {
		         App.session.ShowCoords = show_coords.Active;
			 boardWidget.showCoords = show_coords.Active;
			 boardWidget.QueueDraw ();
		}

                private void on_contents_activate (System.Object b, EventArgs e) {
		        System.Diagnostics.Process proc = new System.Diagnostics.Process ();

                        proc.StartInfo.FileName = "yelp";
                        proc.StartInfo.Arguments = Config.prefix + "/share/gnome/help/csboard/C/csboard.xml";
                        proc.StartInfo.UseShellExecute = true;
			try {
	                        proc.Start ();
			} catch (Exception ex) {
			  // do nothing
			}
		}
		
                private void on_about_activate (System.Object b, EventArgs e) {
                        MessageDialog md = new MessageDialog (csboardWindow,
                                                              DialogFlags.
                                                              DestroyWithParent,
                                                              MessageType.
                                                              Info,
                                                              ButtonsType.
                                                              Close,
							      "CSBoard " + 
							      Config.packageVersion +
							      "\n\n" +
                                                              Catalog.GetString("Frontend to gnuchess written in C#\n\n") +
                                                              "Nickolay Shmyrev 2004");
                        md.Run ();
                        md.Hide ();
                        md.Dispose ();
                }

                private void on_control_wait (string move) {

                        statusbar.Pop (moveStatusbarId);

                        if (move != null) {
                                statusbar.Push (moveStatusbarId, move);
	                        control.SaveGame (App.session.Filename);			
				boardWidget.lastMove = move;
				boardWidget.QueueDraw ();
			}

                        SetSensitive (true);
			progressbar.Stop();
                }

                private void on_position_changed (ArrayList data) {
                        statusbar.Pop (moveStatusbarId);
                        boardWidget.SetPosition (data);
                }

                private void on_board_move (string move) {

                        if (!control.MakeMove (move)) {

                                MessageDialog md =
                                        new MessageDialog (csboardWindow,
                                                           DialogFlags.
                                                           DestroyWithParent,
                                                           MessageType.
                                                           Warning,
                                                           ButtonsType.Close,
                                                           Catalog.GetString("Illegal move"));
                                md.Run ();
                                md.Hide ();
                                md.Dispose ();

                        }
                        return;
                }

                private void on_board_start_move (string pos) {
			if (possible_moves.Active) {
				   string hint = control.PossibleMoves (pos);
				   boardWidget.moveHint = hint;
				   boardWidget.QueueDraw ();
			}
		}

                private void on_control_game_over (string reason) {


                        statusbar.Push (gameStatusbarId, Catalog.GetString("Game Over"));

                        MessageDialog md = new MessageDialog (csboardWindow,
                                                              DialogFlags.
                                                              DestroyWithParent,
                                                              MessageType.
                                                              Info,
                                                              ButtonsType.
                                                              Close,
                                                              Catalog.GetString("Game Over") + "\n" +
                                                              reason);
                        md.Run ();
                        md.Hide ();
                        md.Dispose ();

                        return;
                }

                private void on_control_busy () {

                        statusbar.Pop (moveStatusbarId);
                        statusbar.Push (moveStatusbarId, Catalog.GetString("Thinking"));
                        SetSensitive(false);
			progressbar.Start ();

                }

                private void on_control_side (bool side) {
                        boardWidget.side = side;
                        boardWidget.QueueDraw ();
                }
		
		private void SetWidgetSensitive (Widget w) {
		    w.Sensitive = menusSensitive;
		}
		
		private void SetSensitive (bool val) {		     

		    boardWidget.Sensitive = val;
		    menusSensitive = val;
		    
		    game_menu.Foreach (new Gtk.Callback (SetWidgetSensitive));
		    action_menu.Foreach (new Gtk.Callback (SetWidgetSensitive));
		}
		private void SetupLevel () {

			Level level = App.session.level;
			control.SetLevel (level);
			switch (level) {
			   case Level.Beginner: beginner.Active = true;
						break; 
			   case Level.Intermediate: intermediate.Active = true; 
						    break; 
			   case Level.Advanced: advanced.Active = true; 
						break; 
			   default: expert.Active = true; 
				    break; 
			}
			
		}

        }
}
