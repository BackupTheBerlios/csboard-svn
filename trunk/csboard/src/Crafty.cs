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
	using System.Threading;
        using System.Text.RegularExpressions;

	public class Crafty:IControl {

                private System.Diagnostics.Process proc;
                private System.IO.StreamReader output;
                private System.IO.StreamWriter input;
                private bool side = false;
		private int depth = 1;
		private bool WaitForOutput = false;

                public event ControlBusyHandler BusyEvent;
                public event ControlWaitHandler WaitEvent;
                public event ControlPositionChangedHandler PositionChangedEvent;
                public event ControlGameOverHandler GameOverEvent;
                public event ControlSwitchSideHandler SwitchSideEvent;
		public event ControlHintHandler HintEvent;


                public Crafty (string command) {

                        // Start child                    
                        proc = new System.Diagnostics.Process ();

                        proc.StartInfo.FileName = command.Substring (0, command.IndexOf (' '));
			proc.StartInfo.Arguments = command.Substring (command.IndexOf (' '));
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardInput = true;
                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.Start ();

                        input = proc.StandardInput;
                        output = proc.StandardOutput;

			// Start a new thread to monitor available output
			
						Thread t = new Thread (new ThreadStart (MonitorThread));
						t.IsBackground = true;
						t.Start ();
			Put ("xboard");
                        Get ();

                }

                public void Shutdown () {
                        Put ("exit");
                }

                public ArrayList GetPosition () {

                        Put ("display");
                        ArrayList output = Get ();
			
			ArrayList result = new ArrayList ();
			
			result.Add ("");
			result.Add ("");
			
			for (int i = 2; i < 17; i += 2) {

			  string str = (string) output [i];
			  string out_str = "";
			  
			  for (int j = 9; j < 38; j += 4) {
			     if (str[j-1] == '<') {
				     out_str = out_str + Char.ToLower(str[j]) + ' ';
			     } else {
				     out_str = out_str + str[j] + ' ';
			     }
			  }

			  result.Add (out_str);
			}
			    
			return result;
                }


                public void NewGame () {

                        Put ("new");
                        Put ("depth " + depth.ToString ());
                        Put ("white");
                        Get ();

                        SetSide (false);

                        if (PositionChangedEvent != null) {
                                PositionChangedEvent (GetPosition ());
                        }

                }


                public bool MakeMove (string move) {
			Put ("force");
                        Put (move);
                        ArrayList result = Get ();

                        if (result.Count > 0 && ((string)result[0]).StartsWith ("Illegal move")) {
				
				if (PositionChangedEvent != null) {
				    PositionChangedEvent(GetPosition());
				}
                                return false;
			} else {

                                if (PositionChangedEvent != null) {
                                        PositionChangedEvent (GetPosition ());
                                }

                                if (BusyEvent != null)
                                        BusyEvent ();
					
				Put ("go");

                                GLib.Timeout.Add (100,
                                                  new GLib.
                                                  TimeoutHandler
                                                  (MoveTimeout));
				WaitForOutput = true;
                        }

                        return true;
                }

                bool MoveTimeout () {

			if (WaitForOutput) {
			     return true;
			}

                        ArrayList result = Get ();

                        if (PositionChangedEvent != null) {
                                PositionChangedEvent (GetPosition ());
                        }

                        if (WaitEvent != null && result.Count > 0) {
                                WaitEvent ('m' + (string) result[0]);
                        }
                        else {
                                WaitEvent (null);
                        }

                        checkForEnd (result);

                        return false;
                }

                public void SaveGame (string filename) {
                }

                public void OpenGame (string filename) {

                }

                public void SetLevel (Level l) {

                        switch (l) {

                        case Level.Beginner:
                                depth = 1;
                                break;
                        case Level.Intermediate:
                                depth = 2;
                                break;
                        case Level.Advanced:
                                depth = 5;
                                break;
                        case Level.Expert:
                                depth = 7;
                                break;
                        }

                        Put ("depth " + depth.ToString ());
                        Get ();
                }

                public void Undo () {
                }

                public void SwitchSide () {

                        SetSide (!side);
                        Put ("go");
 
                        if (BusyEvent != null)
                                BusyEvent ();

                        GLib.Timeout.Add (100,
                                          new GLib.
                                          TimeoutHandler
                                          (MoveTimeout));

			WaitForOutput = true;
                }

		public void Hint () {	     
		}
		
                public ArrayList Book () {
                        ArrayList result = new ArrayList ();
			
                        return result;
                }

		public string PossibleMoves (string pos) {
			return "";
		}

		//         private methods 


                private void SetSide (bool s) {

                        if (side != s) {
                                side = s;

                                if (SwitchSideEvent != null) {
                                        SwitchSideEvent (side);
                                }
                        }
                }


                private ArrayList Get () {
		
                        string line;
                        ArrayList result = new ArrayList ();
			
			if (WaitForOutput)
				return result;

                        // This is used  

                        input.WriteLine ("give_me_it");

                        while (true) {
                                line = output.ReadLine ();

				
                                if (line.LastIndexOf ("give_me_it") < 0) {
                                        result.Add (line);
					if (Config.Debug) {
    	                                    Console.WriteLine (line);
					}
                                }
                                else {
                                        break;
                                }
                        };

                        return result;
                }

                private void Put (string text) {
		        
		        if (!WaitForOutput) {
                        input.WriteLine (text);
			
				if (Config.Debug) {
    	        	            Console.WriteLine (text);
		    		}
			}
                }

                private bool checkForEnd (ArrayList result) {
		        Regex reg =
                        new Regex (@"(-0|/2\-1/2|\-1)\s+\{(?<result>(.*))\}");

                        foreach (string str in result) {

                                Match match = reg.Match (str);

                                if (match.Length > 0) {

                                        if (GameOverEvent != null)
                                                GameOverEvent
                                                        (match.
                                                         Groups
                                                         ["result"].
                                                         ToString ());
							 
				    return true;

                                }
                        }
			
			return false;

                }
		
		private void MonitorThread () {
		    
			while (true) {
 				  if (WaitForOutput == true) {
				     output.Read ();
				     WaitForOutput = false;
				  } else {
				     Thread.Sleep (100);
				  }
		        }
		}

        }
}
