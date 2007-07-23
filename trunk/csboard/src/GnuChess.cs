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
	using System.Threading;
	using System.Text.RegularExpressions;

	class GnuChessEngineInfo:EngineInfo
	{
		public GnuChessEngineInfo ():base ("gnuchess", "GnuChess",
						   "gnuchess -x -e")
		{
		}

		public override IControl CreateInstance ()
		{
			return new GnuChess (command);
		}

		public override bool Exists ()
		{
			return true;
		}
	}
	public class GnuChess:IControl
	{

		private System.Diagnostics.Process proc;
		private System.IO.StreamReader output;
		private System.IO.StreamWriter input;
		private bool side = false;
		private int depth = 1;
		private bool WaitForOutput = false;

		public event ControlBusyHandler BusyEvent;
		public event ControlWaitHandler WaitEvent;
		public event ControlPositionChangedHandler
			PositionChangedEvent;
		public event ControlGameOverHandler GameOverEvent;
		public event ControlSwitchSideHandler SwitchSideEvent;
		public event ControlHintHandler HintEvent;

		public string Name
		{
			get
			{
				return "GnuChess";
			}
		}


		static EngineInfo instance;
		public static EngineInfo Info
		{
			get
			{
				if (instance == null)
					instance = new GnuChessEngineInfo ();
				return instance;
			}
		}

		public GnuChess (string command)
		{

			// Check for version

			command = "gnuchess -x -e";
			proc = new System.Diagnostics.Process ();

			proc.StartInfo.FileName = "gnuchess";
			proc.StartInfo.Arguments = "--version";
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start ();
			string str = proc.StandardOutput.ReadLine ();

			if (!
			    (str.StartsWith ("GNU Chess 5.07")
			     || str.StartsWith ("GNU Chess 2.17")))
			  {
				  throw new ApplicationException
					  ("This program only tested with gnuchess 5.07\n"
					   +
					   "Your gnuchess have returned version:\n"
					   + str);
			  }
			// Start child                    
			proc = new System.Diagnostics.Process ();

			proc.StartInfo.FileName =
				command.Substring (0, command.IndexOf (' '));
			proc.StartInfo.Arguments =
				command.Substring (command.IndexOf (' '));
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.Start ();

			input = proc.StandardInput;
			output = proc.StandardOutput;

			// Start a new thread to monitor available output

			Thread t =
				new Thread (new ThreadStart (MonitorThread));
			t.IsBackground = true;
			t.Start ();

			Get ();
		}

		public void Shutdown ()
		{
			Put ("exit");
			try
			{
				proc.Kill ();
				proc.WaitForExit ();
			} catch
			{
			}
		}

		public ArrayList GetPosition ()
		{
			input.WriteLine ("show board");
			return Get ();
		}


		public void NewGame ()
		{

			Put ("new");
			Put ("depth " + depth.ToString ());
			Put ("white");
			Get ();

			SetSide (false);

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

		}


		public bool MakeMove (string move)
		{

			Put ("manual");
			Put (move);
			string result = output.ReadLine ();

			if (result.StartsWith ("Illegal move"))
			  {

				  if (PositionChangedEvent != null)
				    {
					    PositionChangedEvent (GetPosition
								  ());
				    }
				  return false;

			  }
			else
			  {

				  if (PositionChangedEvent != null)
				    {
					    PositionChangedEvent (GetPosition
								  ());
				    }

				  Put ("go");
				  if (BusyEvent != null)
					  BusyEvent ();

				  GLib.Timeout.Add (100,
						    new GLib.
						    TimeoutHandler
						    (MoveTimeout));
				  WaitForOutput = true;
			  }

			return true;
		}

		bool MoveTimeout ()
		{

			if (WaitForOutput)
			  {
				  return true;
			  }

			ArrayList result = Get ();

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

			if (WaitEvent != null && result.Count > 1)
			  {
				  WaitEvent ((string) result[1]);
			  }
			else
			  {
				  WaitEvent (null);
			  }

			checkForEnd (result);

			return false;
		}

		public void SaveGame (string filename)
		{

			if (System.IO.File.Exists (filename))
			  {
				  System.IO.File.Delete (filename);
			  }

			Put ("pgnsave " + filename);
			Get ();
		}

		public void OpenGame (string filename)
		{

			Put ("new");
			Put ("pgnload " + filename);

			ArrayList result = Get ();

			if (((string) result[0]).
			    LastIndexOf ("Cannot open file") >= 0)
			  {
				  return;
			  }

			if (((string) result[1]).LastIndexOf ("white") >= 0)
			  {
				  SetSide (false);
			  }
			else
			  {
				  SetSide (true);
			  }

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }
		}

		public void SetLevel (Level l)
		{

			switch (l)
			  {

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

		public void Undo ()
		{

			Put ("undo");
			Put ("undo");
			Get ();

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

		}

		public void SwitchSide ()
		{

			SetSide (!side);
			Put ("go");

			if (BusyEvent != null)
				BusyEvent ();

			GLib.Timeout.Add (100,
					  new GLib.
					  TimeoutHandler (MoveTimeout));

			WaitForOutput = true;
		}

		public void Hint ()
		{


			Put ("go");

			if (BusyEvent != null)
				BusyEvent ();

			GLib.Timeout.Add (100,
					  new GLib.
					  TimeoutHandler (HintTimeout));

			WaitForOutput = true;

		}

		bool HintTimeout ()
		{

			if (WaitForOutput)
			  {
				  return true;
			  }

			Put ("undo");

			ArrayList result = Get ();

			if (WaitEvent != null)
			  {
				  WaitEvent (null);

			  }

			if (HintEvent != null && result.Count > 1)
			  {

				  string str = (string) result[1];
				  string move;

				  int i = str.LastIndexOf ("My move is: ");
				  if (i >= 0)
				    {
					    move = str.Substring (i + 12);
					    HintEvent (move);
				    }
			  }


			return false;
		}

		public ArrayList Book ()
		{
			Regex reg = new Regex (@"(?<move>(\S+))\(" +
					       @"(?<percent>(\d+))/" +
					       @"(?<wins>(\d+))/" +
					       @"(?<loses>(\d+))/" +
					       @"(?<draws>(\d+))\)");

			ArrayList result = new ArrayList ();

			Put ("bk");
			ArrayList answer = Get ();



			if (answer.Count < 2 || ((string) answer[1]).
			    LastIndexOf ("there is no move") > 0)
			  {
				  return result;
			  }


			for (int i = 2; i < answer.Count; i++)
			  {
				  string str = (string) answer[i];

				  MatchCollection matches = reg.Matches (str);

				  foreach (Match m in matches)
				  {
					  BookMove move;

					  move.Move =
						  m.Groups["move"].
						  ToString ();
					  move.Wins =
						  Convert.ToInt32 (m.
								   Groups
								   ["wins"].
								   ToString
								   ());
					  move.Loses =
						  Convert.ToInt32 (m.
								   Groups
								   ["loses"].
								   ToString
								   ());
					  move.Draws =
						  Convert.ToInt32 (m.
								   Groups
								   ["draws"].
								   ToString
								   ());

					  result.Add (move);
				  }
			  }

			return result;
		}

		public string PossibleMoves (string pos)
		{
			string letters = "abcdefgh";
			string correct_moves = pos + " ";

			Put ("manual");

			for (int i = 0; i < 8; i++)

				for (int j = 1; j < 9; j++)
				  {
					  string move =
						  string.Format ("{0}{1}{2}",
								 pos,
								 letters[i],
								 j);
					  Put (move);
					  string result = output.ReadLine ();

					  if (!result.
					      StartsWith ("Illegal move"))
					    {
						    Put ("undo");
						    correct_moves =
							    correct_moves +
							    string.
							    Format ("{0}{1} ",
								    letters
								    [i], j);
					    }
				  }

			return correct_moves;
		}

//         private methods 


		private void SetSide (bool s)
		{

			if (side != s)
			  {
				  side = s;

				  if (SwitchSideEvent != null)
				    {
					    SwitchSideEvent (side);
				    }
			  }
		}


		private ArrayList Get ()
		{

			string line;
			ArrayList result = new ArrayList ();

			if (WaitForOutput)
				return result;

			// This is used because gnuchess doesn't properly flush output 

			input.WriteLine ("give me it");

			while (true)
			  {
				  line = output.ReadLine ();

				  if (String.
				      Compare (line,
					       "Illegal move: give me it") !=
				      0)
				    {
					    result.Add (line);

					    if (Config.Debug)
					      {
						      Console.WriteLine
							      (line);
					      }
				    }
				  else
				    {
					    break;
				    }
			  };

			return result;
		}

		private void Put (string text)
		{
			if (!WaitForOutput)
			  {
				  input.WriteLine (text);

				  if (Config.Debug)
				    {
					    Console.WriteLine (text);
				    }
			  }
		}

		private void checkForEnd (ArrayList result)
		{

			Regex reg =
				new
				Regex
				(@"(-0|/2\-1/2|\-1)\s+\{(?<result>(.*))\}");

			foreach (string str in result)
			{

				Match match = reg.Match (str);

				if (match.Length > 0)
				  {

					  if (GameOverEvent != null)
						  GameOverEvent
							  (match.
							   Groups
							   ["result"].
							   ToString ());

				  }
			}

		}

		private void MonitorThread ()
		{

			while (true)
			  {
				  if (WaitForOutput == true)
				    {
					    output.Read ();
					    WaitForOutput = false;
				    }
				  else
				    {
					    Thread.Sleep (100);
				    }
			  }
		}

	}
}
