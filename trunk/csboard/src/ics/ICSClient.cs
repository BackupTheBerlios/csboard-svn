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
// Copyright (C) 2004 Jamin Gray

using System;
using System.Text;

namespace CsBoard
{
	namespace ICS
	{

		using System;
		using System.IO;
		using System.Collections;
		using System.Net;
		using System.Net.Sockets;
		using System.Threading;
		using Mono.Unix;

		enum SessionState
		{
			NONE,
			AUTH_REQUEST,
			PASSWORD_SENT,
			AUTH_REJECTED,
			AUTHENTICATED
		};

		public class AuthFailedException:Exception
		{
			public AuthFailedException (string str):base (str)
			{
			}
		}

		public class MoveMadeEventArgs:EventArgs
		{
			MoveDetails details;
			public MoveDetails Details
			{
				get
				{
					return details;
				}
			}

			public MoveMadeEventArgs (MoveDetails details)
			{
				this.details = details;
			}
		}

		public class LineBufferReceivedEventArgs:EventArgs
		{
			byte[] buffer;
			public byte[] Buffer
			{
				get
				{
					return buffer;
				}
			}
			int start;
			public int Start
			{
				get
				{
					return start;
				}
			}
			int end;
			public int End
			{
				get
				{
					return end;
				}
			}
			LineType type;
			public LineType LineType
			{
				get
				{
					return type;
				}
			}

			public LineBufferReceivedEventArgs (byte[]buf, int s,
							    int e,
							    LineType lt)
			{
				buffer = buf;
				start = s;
				end = e;
				type = lt;
			}
		}

		public delegate void GameAdvertisementAddEventHandler (object
								       o,
								       GameAdvertisement
								       ad);
		public delegate void
			GameAdvertisementRemoveEventHandler (object o,
							     GameAdvertisement
							     ad);
		public delegate void
			GameAdvertisementsClearedEventHandler (object o,
							       EventArgs
							       args);
		public delegate void AuthEventHandler (object o,
						       bool success);
		public delegate void MoveMadeEventHandler (object o,
							   MoveMadeEventArgs
							   args);
		public delegate void ResultNotificationEventHandler (object o,
								     ResultNotification
								     res);
		public delegate void LineReceivedHandler (object o,
							  LineReceivedEventArgs
							  args);
		public delegate void LineBufferReceivedHandler (object o,
								LineBufferReceivedEventArgs
								args);

		public delegate void GameInfoEventHandler(object o, GameInfo info);
		public delegate void ConnectionErrorEventHandler(object o, string reason);

		public enum LineType
		{
			Normal,
			Talk,	// server talk
			Info,	// <starts with>
			Prompt,	// ends with %
			ResultNotification
		}

		public class LineReceivedEventArgs:EventArgs
		{
			string line;
			public string Line
			{
				get
				{
					return line;
				}
			}

			LineType type;
			public LineType LineType
			{
				get
				{
					return type;
				}
			}

			public LineReceivedEventArgs (string line,
						      LineType type)
			{
				this.line = line;
				this.type = type;
			}
		}

		enum NotificationType
		{
			None,
			SC,
			SR,
			S,
			STYLE12,
			GAMEINFO
		}

		public class ICSClient
		{

			public string server = "www.freechess.org";
			public string port = "5000";
			string user = "";
			public string assigned_name;	// assigned for guest login
			public string passwd = "";
			bool guestLogin = false;

			public string User {
				set {
					user = value;
					if(user.Equals("guest"))
						guestLogin = true;
					else
						guestLogin = false;
				}
				get {
					return assigned_name == null ? user : assigned_name;
				}
			}

			public event GameAdvertisementAddEventHandler
				GameAdvertisementAddEvent;
			public event GameAdvertisementRemoveEventHandler
				GameAdvertisementRemoveEvent;
			public event GameAdvertisementsClearedEventHandler
				GameAdvertisementsClearedEvent;
			public event AuthEventHandler AuthEvent;
			public event LineReceivedHandler LineReceivedEvent;
			public event LineBufferReceivedHandler
				LineBufferReceivedEvent;
			public event MoveMadeEventHandler MoveMadeEvent;
			public event ResultNotificationEventHandler
				ResultNotificationEvent;
			public event GameInfoEventHandler GameInfoEvent;
			public event ConnectionErrorEventHandler ConnectionErrorEvent;

			SessionState state = SessionState.NONE;

			// This is a separate thread which runs continually while connected 
			// to an ICS server.  It reads data from the server and takes action.
			public Thread readThread;

			// Our TCP client to connect to an ICS server
			public TcpClient client;

			// Once we're connected we get a stream that we can read and write from/to
			public NetworkStream stream;
			public StreamReader streamReader;
			public StreamWriter streamWriter;


			byte[]buffer;
			int start, end;
			System.Text.Decoder decoder;

			Hashtable notificationMap;

			public ICSClient ()
			{
				map = new Hashtable ();
				ads = new ArrayList ();
				buffer = new byte[4096];
				start = end = 0;
				decoder =
					System.Text.Encoding.UTF8.
					GetDecoder ();

				notificationMap = new Hashtable ();
				notificationMap["sr"] = NotificationType.SR;
				notificationMap["sc"] = NotificationType.SC;
				notificationMap["s"] = NotificationType.S;
				notificationMap["12"] =
					NotificationType.STYLE12;
				notificationMap["g1"] = NotificationType.GAMEINFO;
			}

			public bool Start ()
			{
				return PostReadRequest ();
			}

			public void Stop ()
			{
				if (pending != null)
					stream.EndRead (pending);
			}

			IAsyncResult pending;
			private bool PostReadRequest ()
			{
				if (end == buffer.Length) {
					FireConnectionErrorEvent("Internal error. Buffer full");
					return false;	// buffer full. but this should not happen
				}
				try
				{
					pending =
						stream.BeginRead (buffer, end,
								  buffer.
								  Length -
								  end,
								  ReadAsyncCallback,
								  null);
				}
				catch (Exception e)
				{
					FireConnectionErrorEvent(e.ToString());
					return false;
				}

				return true;
			}

			// This can get triggered from async result which may not be in the
			// glib context. so add an idle handler
			private void FireConnectionErrorEvent(string reason) {
				GLib.Idle.Add(delegate () {
					if(ConnectionErrorEvent != null)
						ConnectionErrorEvent(this, reason);
					
					return false;
				});
			}

			private void ReadAsyncCallback (IAsyncResult res)
			{
				int nbytes = stream.EndRead (res);

				end += nbytes;
				GLib.Idle.Add (ProcessBufferIdleHandler);
				pending = null;
			}

			private bool ProcessBufferIdleHandler ()
			{
				ProcessBuffer (state !=
					       SessionState.AUTHENTICATED);
				return false;
			}

			private void ProcessBuffer (bool expecting_auth)
			{
				for (int i = start; i < end; i++)
				  {
					  if (buffer[i] == '\n')
					    {
						    if (i > start)
						      {
							      ProcessLine
								      (start,
								       i -
								       start);
						      }
						    start = i + 1;
					    }
					  else if (expecting_auth
						   && buffer[i] == ':')
					    {
						    ProcessLine (start, i - start + 1);	// including the delim
						    start = i + 1;
					    }
				  }

				if (start > 0)
				  {
					  for (int i = start, j = 0; i < end;
					       i++, j++)
					    {
						    buffer[j] = buffer[i];
					    }
					  end -= start;
					  start = 0;
				  }

				PostReadRequest ();
			}

			private NotificationType GetNotificationType (ref int
								      start,
								      int end)
			{
				int i = start;
				while (buffer[i] != '>' && i < end)
					i++;
				char[] chrs = new char[i - start];
				decoder.GetChars (buffer, start, i - start,
						  chrs, 0);
				string str = new string (chrs);

				start = i;

				if (notificationMap.ContainsKey (str))
					return (NotificationType)
						notificationMap[str];
				return NotificationType.None;
			}

			private void ProcessServerNotification (int start,
								int end)
			{
				NotificationType type =
					GetNotificationType (ref start, end);
				start++;
				switch (type)
				  {
				  case NotificationType.SR:
					  ArrayList list = new ArrayList ();
					  GameAdvertisement.
						  ReadCancellations (buffer,
								     start,
								     end,
								     list);
					  foreach (int handle in list)
					  {
						  RemoveGameAdvertisement
							  (handle);
					  }
					  break;
				  case NotificationType.S:
					  GameAdvertisement ad =
						  GameAdvertisement.
						  FromBuffer (buffer, start,
							      end);
					  AddGameAdvertisement (ad);
					  break;
				  case NotificationType.SC:
					  ClearGameAdvertisements ();
					  break;
				  case NotificationType.STYLE12:
					  MoveDetails details =
						  MoveDetails.
						  FromBuffer (buffer, start,
							      end);
					  if (MoveMadeEvent != null)
						  MoveMadeEvent (this,
								 new
								 MoveMadeEventArgs
								 (details));
					  break;
				  case NotificationType.GAMEINFO:
					  GameInfo info = GameInfo.FromBuffer(buffer, start, end);
					  if(GameInfoEvent != null)
						  GameInfoEvent(this, info);
					  break;
				  }
			}

			ArrayList ads;
			Hashtable map;

			private void AddGameAdvertisement (GameAdvertisement
							   ad)
			{
				ads.Add (ad);
				map[ad.gameHandle] = ad;
				if (GameAdvertisementAddEvent != null)
					GameAdvertisementAddEvent (this, ad);
			}

			private void RemoveGameAdvertisement (int handle)
			{
				GameAdvertisement ad =
					(GameAdvertisement) map[handle];
				if (ad == null)
				  {
					  return;
				  }
				map.Remove (handle);
				ads.Remove (ad);
				if (GameAdvertisementRemoveEvent != null)
					GameAdvertisementRemoveEvent (this,
								      ad);
			}

			private void ClearGameAdvertisements ()
			{
				map.Clear ();
				if (GameAdvertisementsClearedEvent != null)
					GameAdvertisementsClearedEvent (this,
									EventArgs.
									Empty);
			}

			private LineType GetLineType (byte[]buffer, int start,
						      int end)
			{
				if (buffer[start] == '{')
					return LineType.ResultNotification;

				while (end > start && buffer[end - 1] == ' ')
					end--;
				if (end == start)
					return LineType.Normal;

				if (buffer[end - 1] == '%')
					return LineType.Prompt;

				if (buffer[end - 1] == ':')
					return LineType.Talk;

				if (buffer[start] == '<')
				  {
					  int i = start + 1;
					  while (buffer[i] != '>' && i < end)
						  i++;
					  return i !=
						  end ? LineType.
						  Info : LineType.Normal;
				  }

				return LineType.Normal;
			}

			private void ProcessLine(int start, int count) {
				if (buffer[start + count - 1] == '\r')
				  {
					  count--;
				  }
				if (buffer[start] == '\r')
				  {
					  start++;
					  count--;
				  }
				if (count <= 0)
					return;
				try {
					__ProcessLine(start, count);
				}
				catch(Exception e) {
					Console.WriteLine("[LINE:] [{0}]", System.Text.Encoding.ASCII.GetString(buffer, start, count));
					Console.WriteLine(e);
				}
			}

			private void __ProcessLine (int start, int count)
			{
				char[] chrs = new char[count];
				decoder.GetChars (buffer, start, count, chrs,
						  0);

				LineType type = GetLineType (buffer, start,
							     start + count);

				if (type == LineType.ResultNotification)
				  {
					  ResultNotification notification =
						  ResultNotification.
						  FromBuffer (buffer,
							      start + 1,
							      start + count +
							      1);
					  if (ResultNotificationEvent != null)
						  ResultNotificationEvent
							  (this,
							   notification);
					  return;
				  }

				if (LineBufferReceivedEvent != null)
				  {
					  LineBufferReceivedEvent (this,
								   new
								   LineBufferReceivedEventArgs
								   (buffer,
								    start,
								    start +
								    count,
								    type));
				  }

				string line = new string (chrs);

				if (LineReceivedEvent != null)
					LineReceivedEvent (this,
							   new
							   LineReceivedEventArgs
							   (line, type));

				if (buffer[start] == '<')
				  {
					  ProcessServerNotification (start +
								     1,
								     start +
								     count);
					  return;
				  }

				if (line.Equals ("login:"))
				  {
					  if (state == SessionState.NONE)
					    {
						    streamWriter.
							    WriteLine (user);
						    streamWriter.Flush ();
						    state = SessionState.
							    AUTH_REQUEST;
					    }
					  else if (state ==
						   SessionState.AUTH_REQUEST)
					    {
						    if (AuthEvent != null)
							    AuthEvent (this,
								       false);
					    }
				  }
				else if (state == SessionState.AUTH_REQUEST && (line.Equals ("password:") || (guestLogin && line.EndsWith(":"))))
				  {
					  if(guestLogin) {
						  assigned_name = GetAssignedGuestName(buffer, start, start + count);
					  }
					  streamWriter.WriteLine (passwd);
					  streamWriter.Flush ();
					  state = SessionState.PASSWORD_SENT;
				  }
				else if (state == SessionState.PASSWORD_SENT
					 && line.Trim ().EndsWith ("%"))
				  {
					  state = SessionState.AUTHENTICATED;
					  HandleAuthSuccess ();
					  if (AuthEvent != null)
						  AuthEvent (this, true);
				  }
			}

			private static string GetAssignedGuestName(byte[] buffer, int start, int end) {
				int i = end - 1;
				while(i > start && buffer[i] != '"')
					i--;
				if(i == start)
					return null;

				int end_offset = i;
				i--;
				while(i > start && buffer[i] != '"')
					i--;
				if(i == start)
					return null;
				i++;
				return System.Text.Encoding.ASCII.GetString(buffer, i, end_offset - i);
			}

			private void HandleAuthSuccess ()
			{
				string logo =
					Catalog.
					GetString
					("CsBoard (http://csboard.berlios.de)");
				streamWriter.WriteLine ("iset seekinfo 1");
				streamWriter.WriteLine ("iset seekremove 1");
				streamWriter.WriteLine ("iset gameinfo 1");
				streamWriter.WriteLine ("set seek 1");
				streamWriter.WriteLine ("set bell 0");
				streamWriter.WriteLine ("set style 12");
				streamWriter.WriteLine (String.
							Format
							("set interface {0}",
							 logo));
				streamWriter.Flush ();
			}

			public void WriteLine (string str)
			{
				streamWriter.WriteLine (str);
				streamWriter.Flush ();
			}

			public void Write (string message)
			{
				streamWriter.Write (message);
				streamWriter.Flush ();
			}

			public void Connect ()
			{
				try
				{
					client = new TcpClient (server,
								int.
								Parse (port));
					stream = client.GetStream ();
					streamWriter =
						new StreamWriter (stream);
					streamReader =
						new StreamReader (stream);
				} catch
				{
					throw new
						ApplicationException (String.
								      Format
								      (Catalog.
								       GetString
								       ("Can't connect to {0} port {1}"),
								       server,
								       port));
				}

			}

		}
	}
}
