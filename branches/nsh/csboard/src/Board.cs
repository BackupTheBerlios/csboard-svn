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

        // Signal about move done
        public delegate void BoardMoveHandler (string move);
	public delegate void StartMoveHintHandler (string position);


        public class Board:Gtk.DrawingArea {

                enum MoveStage {
                        Clear,
                        Start,
                        Drag,
			Animate,
                        Done, 
			SetPosition
                };

                class MoveInfo {

                        public MoveStage stage;

                        public Point start;
                        public Point end;
                        public Point drag;
                        public Point cursor;
                        
                        public bool cursorVisible = false;

                        public MoveInfo () {
                                start = new Point ();
                                end = new Point ();
                                drag = new Point ();
                                cursor = new Point ();
                                cursor.Center ();
            		}	

			public uint animate_timeout_id = 0;
			public ArrayList animate_list;
		};

                // Geometry
                private int start_x;
                private int start_y;
                private int size;
                private const int space = 2;
		

                // Figure Renderer
                private Figure figure;
                private MoveInfo info;

                // Position
                private Position position;

                //Event about move 
                public event BoardMoveHandler MoveEvent;
                public event StartMoveHintHandler StartMoveHintEvent;
                
                public bool side = false;

		public string lastMove = null;
		public string moveHint = null;

		public bool highLightMove = false;
		public bool showCoords = false;
		public bool showMoveHint = false;
		public bool showAnimations = false;
		
		private Pango.Layout layout;
		
                static GLib.GType gtype = GLib.GType.Invalid;

                public Board (ArrayList pos):base (GType) {
                        figure = new Figure ();
                        position = new Position (pos);
                        info = new MoveInfo ();
			layout = new Pango.Layout (PangoContext);

                        Events = EventMask.ButtonPressMask 
                                | EventMask.ButtonReleaseMask 
                                | EventMask.PointerMotionHintMask 
                                | EventMask.ButtonMotionMask
                                | Gdk.EventMask.KeyPressMask;
                        CanFocus = true;
                }
		
		public static new GLib.GType GType {
            
	                get {
                                if (gtype == GLib.GType.Invalid)
                                        gtype = RegisterGType
                                                (typeof (Board));
                                return gtype;
	                    }
		}
		///////////////////////////////////////////////////////////
		//
		//  Drawing methods
		//
		////////////////////////////////////////////////////////////
                        protected override bool OnConfigureEvent (Gdk.
                                                                  EventConfigure evnt) {

                        int width = Allocation.Width;
                        int height = Allocation.Height;

                        int d = Math.Min (width, height) / 2;
                        size = (10 * (d - 3 * space)) / 42 - 1;
			figure.SetSize (size);

                        start_x = width / 2 - 4 * size - 3 * space;
                        start_y = height / 2 - 4 * size - 3 * space;
						
			layout.FontDescription = Pango.FontDescription.FromString ("Sans " + (size / 6).ToString());

                        return false;
                }

                protected override bool OnExposeEvent (Gdk.EventExpose evnt) {

                        DrawBackground (evnt.Window);
			
			if (showMoveHint && (info.stage == MoveStage.Drag || info.stage == MoveStage.Start)) {
				DrawMoveHint (evnt.Window);
			}
			
			if (showCoords) {
			       DrawCoords (evnt.Window);
			}
			
			if (info.stage != MoveStage.SetPosition) {
	                        DrawPosition (evnt.Window);
			} else {
				DrawAnimate (evnt.Window);
			}

			if (highLightMove && info.stage != MoveStage.SetPosition) {
				DrawLastMove (evnt.Window);
			}

                        DrawMove (evnt.Window);
			
                        if (info.stage == MoveStage.Drag) {
                                DrawDrag (evnt.Window);
                        }
			

                        return false;
                }

		private void DrawMoveHint (Gdk.Window window) {

			string letters = "abcdefgh";
			string numbers = "87654321";

			if (moveHint == null) return;

			for(int k = 0; k < moveHint.Length; k += 3) {
			    int i = -1, j = -1;

			   i = letters.IndexOf (moveHint[k]);
			   j = numbers.IndexOf (moveHint[k + 1]);
			   
			   if (i<0 && j<0)
				continue;
			    
			   if (side) {
			       i = 7 - i;
			       j = 7 - j;
   	 		   }
   
                           int x = start_x + i * (space + size);
                           int y = start_y + j * (space + size);
			    
                           window.DrawRectangle (Style.BackgroundGC (StateType.Prelight), true, x, y, size, size);
   			}

		}
		
		private void DrawCoords (Gdk.Window window) {

                        Gdk.GC gc;
			 
                        gc = Style.ForegroundGC (StateType.Normal);
                        window.DrawRectangle (gc, false,
                                              start_x - space - size / 5,
                                              start_y - space - size / 5,
                                              (size + space) * 8 + 2 * size / 5,
                                              (size + space) * 8 + 2 * size / 5);
			
			string letters = "abcdefgh";
						
			for (int i = 0; i<8; i++) {		      
				layout.SetText ((letters[i]).ToString());
				window.DrawLayout (Style.TextGC (StateType.Normal), start_x + size / 2 + (size + space) * i, start_y - size / 5 - 2 * space, layout);
				window.DrawLayout (Style.TextGC (StateType.Normal), start_x + size / 2 + (size + space) * i, start_y + (size + space) * 8 - 2 * space, layout);
			}
			
			string numbers = "12345678";
			
			if (!side)
				numbers = "87654321";

			for (int i = 0; i<8; i++) {		      
				layout.SetText ((numbers[i]).ToString());
				window.DrawLayout (Style.TextGC (StateType.Normal), start_x - size / 5, start_y + size / 2 + (size + space) * i, layout);
				window.DrawLayout (Style.TextGC (StateType.Normal), start_x+ (size + space) * 8, start_y + size / 2 + (size + space) * i , layout);
			}
			
		}
		
		private void DrawLastMove (Gdk.Window window) {
		
			if (lastMove == null) return;
			
			int i = -1, j = -1;
			string letters = "abcdefgh";
			string numbers = "87654321";
			
			int k = lastMove.Length  - 2;
			while (k >= 0) {
			   i = letters.IndexOf (lastMove[k]);
			   j = numbers.IndexOf (lastMove[k + 1]);
			   if (i >= 0 && j >= 0) break;
			   k --;
			}
			
			if (i == -1 || j == -1) return;

                        Gdk.GC gc = Style.ForegroundGC (StateType.Normal);
                        Gdk.GC tempGC = new Gdk.GC (window);
                        tempGC.Copy (gc);
                        tempGC.SetLineAttributes (2, LineStyle.OnOffDash, CapStyle.Round, 0);
			
			if (side) {
			      i = 7 - i;
			      j = 7 - j;
			}

                        int x = start_x + i * (space + size);
                        int y = start_y + j * (space + size);

                        window.DrawRectangle (tempGC, false, x, y, size, size);
		
		}

                private void DrawDrag (Gdk.Window window) {

                        Gdk.GC gc = Style.ForegroundGC (StateType.Normal);

                        window.DrawPixbuf (gc,
                                           figure.GetPixbuf (position.
                                                             takenFig), 0,
                                           0, info.drag.x - size / 2,
                                           info.drag.y - size / 2, size,
                                           size, Gdk.RgbDither.None, 0, 0);

                }

                private void DrawBackground (Gdk.Window window) {

                        // Defining the color of the Checks
                        int i, j, xcount, ycount;
                        Gdk.GC gc;

                        gc = Style.ForegroundGC (StateType.Normal);
                        window.DrawRectangle (gc, false,
                                              start_x - space,
                                              start_y - space,
                                              (size + space) * 8 + space,
                                              (size + space) * 8 + space);

                        // Start redrawing the Checkerboard                     
                        xcount = 0;
                        i = start_x;
                        while (xcount < 8) {
                                j = start_y;
                                ycount = xcount % 2;    //start with even/odd depending on row
                                while (ycount < 8 + xcount % 2) {
                                        gc = new Gdk.GC (window);
                                        if (ycount % 2 != 0) {
                                                gc = Style.
                                                        BackgroundGC
                                                        (StateType.Normal);
                                        }
                                        else {
                                                gc = Style.
                                                        BackgroundGC
                                                        (StateType.Active);
                                        }
                                        window.DrawRectangle (gc,
                                                              true,
                                                              i, j,
                                                              size, size);

                                        j += size + space;
                                        ycount++;
                                }
                                i += size + space;
                                xcount++;
                        }

                        return;

                }

                private void DrawAnimate (Gdk.Window window) {
                        Gdk.GC gc = Style.ForegroundGC (StateType.Normal);

			if (info.stage != MoveStage.SetPosition) {
				return;
			}
						
			for (int i=0; i < info.animate_list.Count; i++) 
			    {
			       int x_from, y_from, x_to, y_to, x, y;
			       
			       AnimationTaskItem item = (AnimationTaskItem) info.animate_list[i];

                                    if (!side) {
                                        //White
                                            x_from = start_x + item.from.x * (space + size);
                                            y_from = start_y + item.from.y * (space + size);

                                            x_to = start_x + item.to.x * (space + size);
                                            y_to = start_y + item.to.y * (space + size);
                                    } else {
                                        //Black
                                            x_from = start_x + (7 - item.from.x) * (space + size);
                                            y_from = start_y + (7 - item.from.y) * (space + size);

                                            x_to = start_x + (7 - item.to.x) * (space + size);
                                            y_to = start_y + (7 - item.to.y) * (space + size);
                                    }
				    
				    x = x_from + (int)((x_to - x_from) * item.progress);
				    y = y_from + (int)((y_to - y_from) * item.progress);

                                    window.DrawPixbuf (gc, figure.GetPixbuf(item.fig),
                                                         0, 0,
                                                         x, y,
                                                         size,
                                                         size,
                                                         Gdk.RgbDither.None, 0, 0);
			       
			    }
			return;
		}

                private void DrawPosition (Gdk.Window window) {

                        Gdk.GC gc = Style.ForegroundGC (StateType.Normal);

                        for (int i = 0; i < 8; i++) {
                                for (int j = 0; j < 8; j += 1) {

                                        FigureType fig =
                                                position.GetFigureAt (i,
                                                                      j);

                                        if (fig != FigureType.None) {
                                                int x, y;

                                                if (!side) {
                                                        //White
                                                        x = start_x +
                                                                i * space +
                                                                i * size;
                                                        y = start_y +
                                                                j * space +
                                                                j * size;
                                                }
                                                else {
                                                        //Black
                                                        x = start_x + (7 -
                                                                       i) *
                                                                (space +
                                                                 size);
                                                        y = start_y + (7 -
                                                                       j) *
                                                                (space +
                                                                 size);
                                                }

                                                window.DrawPixbuf
                                                        (gc,
                                                         figure.
                                                         GetPixbuf
                                                         (fig),
                                                         0, 0,
                                                         x, y,
                                                         size,
                                                         size,
                                                         Gdk.
                                                         RgbDither.
                                                         None, 0, 0);
                                        }
                                }
                        }


                        return;
                }

                private void DrawMove (Gdk.Window window) {

                        Gdk.GC gc = Style.ForegroundGC (StateType.Normal);

                        if (info.stage == MoveStage.Start) {
                                int x = start_x +
                                        info.start.x * (space + size);
                                int y = start_y +
                                        info.start.y * (space + size);

                                window.DrawRectangle (gc, false, x, y,
                                                      size, size);
                        }
                        
                        if (info.cursorVisible) {
                                Gdk.GC tempGC = new Gdk.GC (window);
                                tempGC.Copy (gc);
                                tempGC.SetLineAttributes (1, LineStyle.OnOffDash, 0,0);

                                int x = start_x +
                                        info.cursor.x * (space + size);
                                int y = start_y +
                                        info.cursor.y * (space + size);

                                window.DrawRectangle (tempGC, false, x, y,
                                                      size, size);

                        }

                        return;
                }

                ///////////////////////////////////////////////////////
                ///
                /// User input handling
                ///
                //////////////////////////////////////////////////////

                
                protected override bool OnKeyPressEvent (Gdk.EventKey k) {

                   info.cursorVisible = true;
                   
                   switch (k.Key) {
                  
                   case Gdk.Key.Escape: info.cursorVisible = false;  break;

                   case Gdk.Key.Left: 
                                  if (info.cursor.x == 0) {
                                      info.cursor.x = 7;
                                  } else {
                                      info.cursor.x = info.cursor.x - 1;
                                  }
                                  break;
                   case Gdk.Key.Right: 
                                  if (info.cursor.x == 7) {
                                      info.cursor.x = 0;
                                  } else {
                                      info.cursor.x = info.cursor.x + 1;
                                  }
                                  break;
                   case Gdk.Key.Up: 
                                  if (info.cursor.y == 0) {
                                      info.cursor.y = 7;
                                  } else {
                                      info.cursor.y = info.cursor.y - 1;
                                  }
                                  break;
                   case Gdk.Key.Down: 
                                  if (info.cursor.y == 7) {
                                      info.cursor.y = 0;
                                  } else {
                                      info.cursor.y = info.cursor.y + 1;
                                  }
                                  break;
                   case Gdk.Key.space:
                   case Gdk.Key.Return: 
                                  if (info.stage == MoveStage.Clear) {
                                            int real_x = info.cursor.x;
				            int real_y = info.cursor.y;
									
				            if (side) {
						 real_x = 7 - real_x;
						 real_y = 7 - real_y;
                		            }
									
					    if (position.GetFigureAt(real_x, real_y) 
                                                                 != FigureType.None)  {
                                    	    
                        				            info.start.Set (info.cursor); 
	                                                            info.stage = MoveStage.Start;
								    EmitStartHint (info.cursor);
                                                                    break;
                                            }
                                  }  
                                  if (info.stage == MoveStage.Start) {

                                        info.end.Set (info.cursor);

                                        if (info.end.x ==
                                            info.start.x
                                            && info.end.y == info.start.y) {
                                                info.stage = MoveStage.Clear;
                                        } else {
                                                info.stage = MoveStage.Done;
                                                Move (false);
                                        }
                                  }
                                  break;
                                  
                   default: break;

                  }
                  
                  QueueDraw ();
                  return true;
                }

                protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt) {

                        int x = (int) evnt.X;
                        int y = (int) evnt.Y;

                        if (info.stage == MoveStage.Drag) {
                                position.Put ();
                            
			        if (GetPoint (x, y, ref info.end)) {

                                        if (info.end.x ==
                                            info.start.x
                                            && info.end.y == info.start.y) {
                                                info.stage = MoveStage.Clear;
                                        } else {
                                                info.stage = MoveStage.Done;

                                                Move (true);
						
                                        }
                                }
				
                                QueueDraw ();
                        }


                        return true;
                }

                protected override bool OnMotionNotifyEvent (Gdk.
                                                             EventMotion evnt)
                {

                        int x, y;
                        Gdk.ModifierType s;


                        evnt.Window.GetPointer (out x, out y, out s);

                        info.drag.x = x;
                        info.drag.y = y;

                        if (info.stage == MoveStage.Start) {
					        	if (side) {
					  				Point p = new Point (7 - info.start.x, 
						       						7 - info.start.y);
				  					position.Take (p);
								} else {
				        	                       	position.Take (info.start);
								}
                                info.stage = MoveStage.Drag;
                        }

                        QueueDraw ();

                        return true;

                }

                protected override bool OnButtonPressEvent (Gdk.
                                                            EventButton evnt)
                {
                        int x = (int) evnt.X;
                        int y = (int) evnt.Y;

                        if (info.stage == MoveStage.Clear) {

                                if (GetPoint (x, y, ref info.start)) {
                                            int real_x = info.start.x;
				            int real_y = info.start.y;
				            
									
				            if (side) {
						 real_x = 7 - real_x;
						 real_y = 7 - real_y;
                		            }
									
					    if (position.GetFigureAt(real_x, 											
								real_y) != FigureType.None) {
                                    	    
				            info.cursor.Set (info.start);
	                                    info.stage = MoveStage.Start;
					    EmitStartHint (info.start);
                                            QueueDraw ();
	                                    return true;
									}
                                }
                        }

                        if (info.stage == MoveStage.Start) {
                                if (GetPoint (x, y, ref info.end)) {

                                        if (info.end.x ==
                                            info.start.x
                                            && info.end.y == info.start.y) {
                                                info.stage = MoveStage.Clear;
                                                QueueDraw ();
                                        }
                                        else {
                                                info.stage = MoveStage.Done;
                                                Move (false);
                                        }
                                }
                        }

                        return true;
                }

                /////////////////////////////////////////////////////
                ///
                /// Actual moving methods
                ///
                ////////////////////////////////////////////////////

                private void Move (bool explicitly) {

                        if (info.stage != MoveStage.Done) {
                                return;
                        }


                        if (side) {
                                info.start.x = 7 - info.start.x;
                                info.start.y = 7 - info.start.y;
                                info.end.x = 7 - info.end.x;
                                info.end.y = 7 - info.end.y;
                        }


                        char newFigure = ' ';

                        position.Move (info.start, info.end, ref newFigure, explicitly);

                        string letter = "abcdefgh";
                        string move = string.Format ("{0}{1}{2}{3}",
                                                     letter[info.start.x],
                                                     8 - info.start.y,
                                                     letter[info.end.x],
                                                     8 - info.end.y);

                        if (newFigure != ' ') {
                                move = move + newFigure;
                        }

                        MoveEvent (move);
                }

                public void SetPosition (ArrayList pos) {
                        info.cursorVisible = false;
			lastMove = null;			
			
			if (showAnimations) {
			    info.stage = MoveStage.SetPosition;
			    if (info.animate_timeout_id > 0) {
				    GLib.Source.Remove (info.animate_timeout_id);
			    }
			    info.animate_timeout_id = GLib.Timeout.Add (100, new GLib.TimeoutHandler(on_animate_timeout)); 
                            info.animate_list = position.SetPositionAnimate (pos);
			} else {
	                    info.stage = MoveStage.Clear;
                            position.SetPosition (pos);
                            QueueDraw ();
			}
                }
		
                /////////////////////////////////////////////////////
                ///
                /// Utilities
                ///
                ////////////////////////////////////////////////////

		private void EmitStartHint (Point where) {
			int i, j;	
		
                        if (side) {
				i = 7 - where.x;
				j = 7 - where.y;
			} else {
				i = where.x;
				j = where.y;
			}
			
                        string letter = "abcdefgh";
                        string pos = string.Format ("{0}{1}",
                                                     letter[i],
                                                     8 - j);
		        StartMoveHintEvent (pos);
		}
		
                private bool GetPoint (int x, int y, ref Point p) {

                        int i;
                        int j;

                        if (x <= start_x || y <= start_y)
                                return false;

                        if (x >= start_x + 8 * (space + size)
                            || y >= start_y + 8 * (space + size))
                                return false;

                        i = (x - start_x) / (space + size);
                        j = (y - start_y) / (space + size);

                        p.x = i;
                        p.y = j;

                        return true;
                }
		
		private bool on_animate_timeout () {

			bool task_finished = true;
			
			for (int i = 0; i < info.animate_list.Count; i++) 
			    {
			       AnimationTaskItem item = (AnimationTaskItem) info.animate_list[i];
			       if (item.progress < 0.95) {
			    	    item.progress += 0.2;
				    task_finished = false;
			       } 
			    }
			    

			if (task_finished) {
			    info.stage = MoveStage.Clear;
			    info.animate_timeout_id = 0;
			}

			QueueDraw ();

			return !task_finished;
		}
		
        }
}
