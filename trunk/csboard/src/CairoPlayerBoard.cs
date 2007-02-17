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

/*
	public delegate void StartMoveHintHandler (string position);
*/

        public class CairoPlayerBoard : CairoBoard {
                public event StartMoveHintHandler StartMoveHintEvent;

                public CairoPlayerBoard (ArrayList pos) : base (pos) {
                        Events = EventMask.ButtonPressMask 
                                | EventMask.ButtonReleaseMask 
                                | EventMask.PointerMotionHintMask 
                                | EventMask.ButtonMotionMask
                                | Gdk.EventMask.KeyPressMask;
                        CanFocus = true;
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
        }
}
