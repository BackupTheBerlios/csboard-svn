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

using Gtk;
using System;
using Mono.Unix;
using System.Collections;
using Chess.Game;
using CsBoard.ICS.Relay;

namespace CsBoard
{
	namespace ICS
	{
		public class RelayGameEventArgs:EventArgs
		{
			Game game;
			public bool IsEndOfList
			{
				get
				{
					return game == null;
				}
			}
			public Game Game
			{
				get
				{
					return game;
				}
			}
			public RelayGameEventArgs (Game g)
			{
				game = g;
			}
		}
		public delegate void RelayGameEventHandler (object o,
							    RelayGameEventArgs
							    args);
		public class GamesGetter:RelayGetter
		{
			int tournamentid;
			public event RelayGameEventHandler RelayGameEvent;

			public GamesGetter (ICSClient c, int tid):base (c)
			{
				tournamentid = tid;
			}

			public void Start ()
			{
				SendCommand ("xtell relay listgames " +
					     tournamentid);
			}

			protected override void ProcessLine (string line)
			{
				Game g = Game.FromLine (line);
				if (g == null)
					return;
				if (RelayGameEvent != null)
					RelayGameEvent (this,
							new
							RelayGameEventArgs
							(g));
			}

			protected override void HandleCompletion ()
			{
				if (RelayGameEvent != null)
					RelayGameEvent (this,
							new
							RelayGameEventArgs
							(null));
			}
		}
	}
}
