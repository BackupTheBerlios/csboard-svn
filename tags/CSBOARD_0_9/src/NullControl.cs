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

namespace CsBoard
{

	using System;
	using System.Collections;

	class NullEngineInfo:EngineInfo
	{
		public NullEngineInfo ():base ("null", "No Engine", "null ")
		{
		}

		public override IControl CreateInstance ()
		{
			return new NullControl ();
		}

		public override bool Exists ()
		{
			return true;
		}
	}

	public class NullControl:IControl
	{

		public event ControlBusyHandler BusyEvent;
		public event ControlWaitHandler WaitEvent;
		public event ControlPositionChangedHandler
			PositionChangedEvent;
		public event ControlGameOverHandler GameOverEvent;
		public event ControlSwitchSideHandler SwitchSideEvent;
		public event ControlHintHandler HintEvent;

		ArrayList position;

		public string Name
		{
			get
			{
				return "No Engine";
			}
		}

		static EngineInfo instance;
		public static EngineInfo Info
		{
			get
			{
				if (instance == null)
					instance = new NullEngineInfo ();
				return instance;
			}
		}

		public NullControl ()
		{
			position = new ArrayList ();
			position.Add ("");
			position.Add ("white  KQkq");
			string row = ". . . . . . . . ";
			for (int i = 0; i < 8; i++)
				position.Add (row);
			position.Add ("");
		}

		public void Shutdown ()
		{
		}

		public ArrayList GetPosition ()
		{
			return position;
		}


		public void NewGame ()
		{
			if (PositionChangedEvent != null)
			  {
				  PositionChangedEvent (GetPosition ());
			  }
		}


		public bool MakeMove (string move)
		{
			return false;
		}

		public void SaveGame (string filename)
		{
		}

		public void OpenGame (string filename)
		{
		}

		public void SetLevel (Level l)
		{
		}

		public void Undo ()
		{
		}

		public void SwitchSide ()
		{
		}

		public void Hint ()
		{
		}

		public ArrayList Book ()
		{
			return null;
		}

		public string PossibleMoves (string pos)
		{
			return pos + " ";
		}
	}
}
