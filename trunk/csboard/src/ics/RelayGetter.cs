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
		public abstract class
			RelayGetter:IAsyncCommandResponseListener
		{
			bool start_parsing;
			protected ICSClient client;

			protected RelayGetter (ICSClient c)
			{
				start_parsing = false;
				client = c;
				client.LineReceivedEvent += OnLineReceived;
			}

			protected void SendCommand (string command)
			{
				client.CommandSender.SendCommand (command,
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

			private void OnLineReceived (object o,
						     LineReceivedEventArgs
						     args)
			{
				if (!start_parsing)
				  {
					  return;
				  }
				if (args.LineType != LineType.Normal
				    && args.LineType != LineType.Talk)
				  {
					  return;
				  }
				string line = args.Line;
				if (line.Length == 0 || line[0] != ':')
				  {
					  client.LineReceivedEvent -=
						  OnLineReceived;
					  HandleCompletion ();
					  return;
				  }
				if (line.Length == 1)
					return;
				ProcessLine (line);
			}

			protected abstract void ProcessLine (string line);
			protected abstract void HandleCompletion ();
		}
	}
}
