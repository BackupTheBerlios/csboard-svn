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
	using System.IO;

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
		private bool side = false;
		private int depth = 1;

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

		ProcIO pio;

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

			pio = new ProcIO (proc);
			pio.GetAsync (ProcIO.FLUSH_STR);
		}

		public void Shutdown ()
		{
			pio.GetAsync ("exit");
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
			ArrayList list = null;
			list = pio.GetSync ("show board",
					    ProcIO.FLUSH_STR);
			return list;
		}


		public void NewGame ()
		{

			pio.GetSync ("new", "depth " + depth, "white",
				     ProcIO.FLUSH_STR);

			SetSide (false);

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

		}


		public bool MakeMove (string move)
		{

			ArrayList list = null;
			list = pio.GetSync ("manual", move);

			if ((list[0] as string).StartsWith ("Illegal move"))
			  {

				  if (PositionChangedEvent != null)
				    {
					    PositionChangedEvent (GetPosition
								  ());
				    }
				  return false;
			  }

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

			if (BusyEvent != null)
				BusyEvent ();
			/*
			   GLib.Timeout.Add (100,
			   new GLib.
			   TimeoutHandler
			   (MoveTimeout));
			   WaitForOutput = true;
			 */

			pio.GetAsync (OnMoveDataReceived, "go");
			return true;
		}

		void OnMoveDataReceived (ArrayList result)
		{
			ArrayList pos = GetPosition ();
			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (pos);
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
		}

		public void SaveGame (string filename)
		{
			if (System.IO.File.Exists (filename))
			  {
				  System.IO.File.Delete (filename);
			  }

			pio.GetAsync ("pgnsave " + filename,
				      ProcIO.FLUSH_STR);
		}

		public void OpenGame (string filename)
		{

			ArrayList result =
				pio.GetSync ("new", "pgnload " + filename,
					     ProcIO.FLUSH_STR);

			if ((result[0] as string).
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

			pio.GetAsync ("depth " + depth.ToString (),
				      ProcIO.FLUSH_STR);
		}

		public void Undo ()
		{

			pio.GetAsync ("undo");

			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }

		}

		public void SwitchSide ()
		{

			SetSide (!side);
			pio.GetAsync (OnMoveDataReceived, "go");

			if (BusyEvent != null)
				BusyEvent ();

			/*
			   GLib.Timeout.Add (100,
			   new GLib.
			   TimeoutHandler (MoveTimeout));

			   WaitForOutput = true;
			 */
		}

		public void Hint ()
		{


			pio.GetAsync (OnHintCallback, "go");

			if (BusyEvent != null)
				BusyEvent ();

			/*
			   GLib.Timeout.Add (100,
			   new GLib.
			   TimeoutHandler (HintTimeout));

			   WaitForOutput = true;
			 */

		}

		void OnHintCallback (ArrayList result)
		{
			pio.GetAsync ("undo", ProcIO.FLUSH_STR);

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

			//pio.GetSync(ProcIO.FLUSH_STR);
		}

		/*
		   bool HintTimeout ()
		   {

		   if (WaitForOutput)
		   {
		   return true;
		   }

		   pio.Put ("undo");

		   ArrayList result = pio.GetSync();

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
		 */

		public ArrayList Book ()
		{
			Regex reg = new Regex (@"(?<move>(\S+))\(" +
					       @"(?<percent>(\d+))/" +
					       @"(?<wins>(\d+))/" +
					       @"(?<loses>(\d+))/" +
					       @"(?<draws>(\d+))\)");

			ArrayList result = new ArrayList ();

			ArrayList answer =
				pio.GetSync ("bk", ProcIO.FLUSH_STR);



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
			/*
			   string letters = "abcdefgh";
			   string correct_moves = pos + " ";

			   pio.Put ("manual");

			   for (int i = 0; i < 8; i++)

			   for (int j = 1; j < 9; j++)
			   {
			   string move =
			   string.Format ("{0}{1}{2}",
			   pos,
			   letters[i],
			   j);
			   string result =
			   pio.GetSync (move, ProcIO.FLUSH_STR)[0] as string;

			   if (!result.
			   StartsWith ("Illegal move"))
			   {
			   pio.Put ("undo");
			   correct_moves =
			   correct_moves +
			   string.
			   Format ("{0}{1} ",
			   letters
			   [i], j);
			   }
			   }

			   return correct_moves;
			 */
			return "";
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
		/*
		   bool MoveTimeout ()
		   {

		   if (WaitForOutput)
		   {
		   return true;
		   }

		   ArrayList result = pio.GetSync();

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
		 */

	}
}
