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

	using System.Collections;

	public enum Level
	{
		Beginner,
		Intermediate,
		Advanced,
		Expert
	};

	public struct BookMove
	{
		public string Move;
		public int Wins;
		public int Draws;
		public int Loses;
	};

	public delegate void ControlBusyHandler ();
	public delegate void ControlWaitHandler (string move);
	public delegate void ControlPositionChangedHandler (ArrayList board);
	public delegate void ControlGameOverHandler (string reason);
	public delegate void ControlSwitchSideHandler (bool black);
	public delegate void ControlHintHandler (string move);

	public interface IControl
	{

		event ControlBusyHandler BusyEvent;
		event ControlWaitHandler WaitEvent;
		event ControlGameOverHandler GameOverEvent;
		event ControlSwitchSideHandler SwitchSideEvent;
		event ControlPositionChangedHandler PositionChangedEvent;
		event ControlHintHandler HintEvent;

		string Name
		{
			get;
		}

		void NewGame ();
		void SaveGame (string filename);
		void OpenGame (string filename);
		void Shutdown ();
		void SetLevel (Level l);

		void Undo ();
		void SwitchSide ();

		// Book opening return ArrayList of BookMove

		ArrayList Book ();
		void Hint ();

		bool MakeMove (string move);
		ArrayList GetPosition ();
		string PossibleMoves (string pos);
	}

	public abstract class EngineInfo
	{
		protected string id, name, command;
		public string Name
		{
			get
			{
				return name;
			}
		}

		public string ID
		{
			get
			{
				return id;
			}
		}

		public string Command
		{
			get
			{
				return command;
			}
		}

		public EngineInfo (string i, string n, string c)
		{
			id = i;
			name = n;
			command = c;
		}

		public abstract IControl CreateInstance ();
		public abstract bool Exists ();
	}
}
