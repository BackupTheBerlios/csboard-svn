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
		public delegate void RelayGameEventHandler (object o,
							    Game game);
		public delegate void RelayTournamentEventHandler (object o,
								  Tournament
								  t);
		public class RelayGetter:IAsyncCommandResponseListener
		{
			bool start_parsing;
			bool first_colon_seen;
			protected ICSClient client;
			public event RelayGameEventHandler RelayGameEvent;
			public event RelayTournamentEventHandler
				RelayTournamentEvent;

			public RelayGetter (ICSClient c)
			{
				start_parsing = false;
				client = c;
				client.LineReceivedEvent += OnLineReceived;
			}

			public void Start ()
			{
				client.CommandSender.
					SendCommand ("xtell relay listgames",
						     this);
			}

			public virtual void CommandResponseLine (int id,
								 byte[]buffer,
								 int start,
								 int end)
			{
			}

			public virtual void CommandCodeReceived (int id,
								 CommandCode
								 code)
			{
			}

			public virtual void CommandCompleted (int id)
			{
				start_parsing = true;
			}

			void OnLineReceived (object o,
					     LineReceivedEventArgs args)
			{
				if (!start_parsing)
				  {
					  return;
				  }
				string line = args.Line.Trim ();

				if (first_colon_seen)
				  {
					  if (line.Length > 0
					      && line[0] != ':')
						  HandleCompletion ();
					  else
						  ProcessLine (line);
					  return;
				  }

				if (!first_colon_seen && line.Length > 0
				    && line[0] == ':')
					first_colon_seen = true;
			}

			void ProcessLine (string line)
			{
				Tournament t = Tournament.FromLine (line);
				if (t != null)
				  {
					  if (RelayTournamentEvent != null)
						  RelayTournamentEvent (this,
									t);
					  return;
				  }

				Game g = Game.FromLine (line);
				if (g != null)
				  {
					  if (RelayGameEvent != null)
						  RelayGameEvent (this, g);
					  return;
				  }
			}
			void HandleCompletion ()
			{
				client.LineReceivedEvent -= OnLineReceived;
				if (RelayTournamentEvent != null)
					RelayTournamentEvent (this, null);
			}
		}
	}
}
