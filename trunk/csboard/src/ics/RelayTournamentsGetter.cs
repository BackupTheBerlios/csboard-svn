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
		public delegate void GetTournamentsDelegate (ArrayList
							     tournaments);

		public class RelayTournamentEventArgs:EventArgs
		{
			Tournament tournament;
			public bool IsEndOfList
			{
				get
				{
					return tournament == null;
				}
			}
			public Tournament Tournament
			{
				get
				{
					return tournament;
				}
			}

			public RelayTournamentEventArgs (Tournament t)
			{
				tournament = t;
			}
		}

		public class RelayTournamentGameEventArgs:EventArgs
		{
			Tournament tournament;
			public Tournament Tournament
			{
				get
				{
					return tournament;
				}
			}

			Game game;
			public Game Game
			{
				get
				{
					return game;
				}
			}

			public RelayTournamentGameEventArgs (Tournament t,
							     Game g)
			{
				tournament = t;
				game = g;
			}
		}
		public delegate void RelayTournamentEventHandler (object o,
								  RelayTournamentEventArgs
								  args);
		public delegate void RelayTournamentGameEventHandler (object
								      o,
								      RelayTournamentGameEventArgs
								      args);
		public class TournamentsGetter:RelayGetter
		{
			ArrayList tournaments;
			int curidx;
			Tournament curtournament;
			GamesGetter gamesgetter;
			public event RelayTournamentEventHandler
				RelayTournamentEvent;
			public event RelayTournamentGameEventHandler
				RelayTournamentGameEvent;

			public TournamentsGetter (ICSClient c):base (c)
			{
				tournaments = new ArrayList ();
				curidx = 0;
				curtournament = null;
			}

			public void Start ()
			{
				SendCommand ("xtell relay listtourney");
			}

			protected override void ProcessLine (string line)
			{
				Tournament t = Tournament.FromLine (line);
				if (t == null)
					return;
				if (RelayTournamentEvent != null)
					RelayTournamentEvent (this,
							      new
							      RelayTournamentEventArgs
							      (t));
				tournaments.Add (t);
			}

			protected override void HandleCompletion ()
			{
				// Now get the games of each tournament
				get_games ();
			}

			private void get_games ()
			{
				if (curidx >= tournaments.Count)
				  {	// end
					  if (RelayTournamentEvent != null)
						  RelayTournamentEvent (this,
									new
									RelayTournamentEventArgs
									(null));
					  return;
				  }
				curtournament =
					tournaments[curidx] as Tournament;
				gamesgetter =
					new GamesGetter (client,
							 curtournament.ID);
				gamesgetter.RelayGameEvent += OnRelayGame;
				gamesgetter.Start ();
			}

			private void OnRelayGame (object o,
						  RelayGameEventArgs args)
			{
				if (args.IsEndOfList)
				  {
					  gamesgetter.RelayGameEvent -=
						  OnRelayGame;
					  curidx++;
					  get_games ();
					  return;
				  }

				if (RelayTournamentGameEvent != null)
					RelayTournamentGameEvent (this,
								  new
								  RelayTournamentGameEventArgs
								  (curtournament,
								   args.
								   Game));
			}
		}
	}
}
