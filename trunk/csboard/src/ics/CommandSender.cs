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

using System;
using System.Collections;

namespace CsBoard
{
	namespace ICS
	{
		public class CommandResponseLineEventArgs:EventArgs
		{
			public int commandId;
			public byte[] buffer;
			public int start;
			public int end;

			public CommandResponseLineEventArgs (int commandId,
							     byte[]buffer,
							     int start,
							     int end)
			{
				this.commandId = commandId;
				this.buffer = buffer;
				this.start = start;
				this.end = end;
			}
		}

		public interface IAsyncCommandResponseListener
		{
			void CommandResponseLine (int cmd, byte[]buffer,
						  int start, int end);
			void CommandCodeReceived (int cmd, CommandCode code);
			void CommandCompleted (int cmd);
		}

		public class
			SimpleCommandResponseListener:IAsyncCommandResponseListener
		{
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
			}
		}

		public class CommandSender
		{
			ICSClient client;
			int commandId;
			int pending;

			int currentCommand;
			const int MAX_COMMANDS = 9;
			int blockCount;

			Hashtable slots;

			public CommandSender (ICSClient client)
			{
				slots = new Hashtable ();
				this.client = client;
				commandId = 1;
				currentCommand = -1;
				blockCount = 0;

				client.BlockCodeEvent += OnBlockCodeEvent;
				client.CommandIdentifierEvent +=
					OnCommandIdentifierEvent;
				client.CommandCodeEvent += OnCommandCodeEvent;

				client.LineBufferReceivedEvent +=
					OnLineBufferReceivedEvent;
			}

			private void OnCommandIdentifierEvent (object o,
							       string str)
			{
				currentCommand = Int32.Parse (str);
			}

			private void OnCommandCodeEvent (object o,
							 CommandCode code)
			{
				// TODO: do some verification. there might be some error messages
				IAsyncCommandResponseListener listener =
					slots[currentCommand] as
					IAsyncCommandResponseListener;
				if (listener != null)
					listener.
						CommandCodeReceived
						(currentCommand, code);

			}

			private void OnBlockCodeEvent (object o,
						       BlockCode code)
			{
				if (code == BlockCode.BlockStart)
				  {
					  blockCount++;
					  return;
				  }
				if (code == BlockCode.BlockEnd)
				  {
					  blockCount--;
					  if (blockCount > 0)
						  return;
					  IAsyncCommandResponseListener
						  listener =
						  slots[currentCommand] as
						  IAsyncCommandResponseListener;
					  if (listener != null)
						  listener.
							  CommandCompleted
							  (currentCommand);
					  pending--;
					  currentCommand = -1;
				  }
			}

			private void OnLineBufferReceivedEvent (object o,
								LineBufferReceivedEventArgs
								args)
			{
				if (currentCommand < 0)
					return;
				IAsyncCommandResponseListener listener =
					slots[currentCommand] as
					IAsyncCommandResponseListener;
				if (listener == null)
					return;
				listener.CommandResponseLine (currentCommand,
							      args.Buffer,
							      args.Start,
							      args.End);
			}

			private int NextCommandId ()
			{
				int ret = commandId;
				if (++commandId == 10)
					commandId = 1;
				return ret;
			}

			public int SendCommand (string str)
			{
				return SendCommand (str,
						    new
						    SimpleCommandResponseListener
						    ());
			}

			public int SendCommand (string str,
						IAsyncCommandResponseListener
						listener)
			{
				if (pending > MAX_COMMANDS)
					return -1;
				int cmd = NextCommandId ();
				pending++;
				slots[cmd] = listener;
				client.WriteLine (String.
						  Format ("{0} {1}", cmd,
							  str));
				return cmd;
			}
		}
	}
}
