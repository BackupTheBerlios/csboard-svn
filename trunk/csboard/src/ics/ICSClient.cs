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
			REAUTH,
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

		public delegate void GameInfoEventHandler (object o,
							   GameInfo info);
		public delegate void ConnectionErrorEventHandler (object o,
								  string
								  reason);

		public enum LineType
		{
			Normal,
			Talk,	// server talk
			Info,	// <starts with>
			Prompt,	// ends with %
			Block,
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

		public enum BlockCode
		{
			BlockStart = 21,
			BlockSeparator = 22,
			BlockEnd = 23,
			BlockPoseStart = 24,
			BlockPoseEnd = 25
		}

		public enum CommandCode
		{
			BLK_NULL = 0,
			BLK_GAME_MOVE = 1,
			BLK_ABORT = 10,
			BLK_ACCEPT = 11,
			BLK_ADDLIST = 12,
			BLK_ADJOURN = 13,
			BLK_ALLOBSERVERS = 14,
			BLK_ASSESS = 15,
			BLK_BACKWARD = 16,
			BLK_BELL = 17,
			BLK_BEST = 18,
			BLK_BNAME = 19,
			BLK_BOARDS = 20,
			BLK_BSETUP = 21,
			BLK_BUGWHO = 22,
			BLK_CBEST = 23,
			BLK_CLEARMESSAGES = 24,
			BLK_CLRSQUARE = 25,
			BLK_CONVERT_BCF = 26,
			BLK_CONVERT_ELO = 27,
			BLK_CONVERT_USCF = 28,
			BLK_COPYGAME = 29,
			BLK_CRANK = 30,
			BLK_CSHOUT = 31,
			BLK_DATE = 32,
			BLK_DECLINE = 33,
			BLK_DRAW = 34,
			BLK_ECO = 35,
			BLK_EXAMINE = 36,
			BLK_FINGER = 37,
			BLK_FLAG = 38,
			BLK_FLIP = 39,
			BLK_FMESSAGE = 40,
			BLK_FOLLOW = 41,
			BLK_FORWARD = 42,
			BLK_GAMES = 43,
			BLK_GETGI = 44,
			BLK_GETPI = 45,
			BLK_GINFO = 46,
			BLK_GOBOARD = 47,
			BLK_HANDLES = 48,
			BLK_HBEST = 49,
			BLK_HELP = 50,
			BLK_HISTORY = 51,
			BLK_HRANK = 52,
			BLK_INCHANNEL = 53,
			BLK_INDEX = 54,
			BLK_INFO = 55,
			BLK_ISET = 56,
			BLK_IT = 57,
			BLK_IVARIABLES = 58,
			BLK_JKILL = 59,
			BLK_JOURNAL = 60,
			BLK_JSAVE = 61,
			BLK_KIBITZ = 62,
			BLK_LIMITS = 63,
			BLK_LINE = 64,
			BLK_LLOGONS = 65,
			BLK_LOGONS = 66,
			BLK_MAILHELP = 67,
			BLK_MAILMESS = 68,
			BLK_MAILMOVES = 69,
			BLK_MAILOLDMOVES = 70,
			BLK_MAILSOURCE = 71,
			BLK_MAILSTORED = 72,
			BLK_MATCH = 73,
			BLK_MESSAGES = 74,
			BLK_MEXAMINE = 75,
			BLK_MORETIME = 76,
			BLK_MOVES = 77,
			BLK_NEWS = 78,
			BLK_NEXT = 79,
			BLK_OBSERVE = 80,
			BLK_OLDMOVES = 81,
			BLK_OLDSTORED = 82,
			BLK_OPEN = 83,
			BLK_PARTNER = 84,
			BLK_PASSWORD = 85,
			BLK_PAUSE = 86,
			BLK_PENDING = 87,
			BLK_PFOLLOW = 88,
			BLK_POBSERVE = 89,
			BLK_PREFRESH = 90,
			BLK_PRIMARY = 91,
			BLK_PROMOTE = 92,
			BLK_PSTAT = 93,
			BLK_PTELL = 94,
			BLK_PTIME = 95,
			BLK_QTELL = 96,
			BLK_QUIT = 97,
			BLK_RANK = 98,
			BLK_RCOPYGAME = 99,
			BLK_RFOLLOW = 100,
			BLK_REFRESH = 101,
			BLK_REMATCH = 102,
			BLK_RESIGN = 103,
			BLK_RESUME = 104,
			BLK_REVERT = 105,
			BLK_ROBSERVE = 106,
			BLK_SAY = 107,
			BLK_SERVERS = 108,
			BLK_SET = 109,
			BLK_SHOUT = 110,
			BLK_SHOWLIST = 111,
			BLK_SIMABORT = 112,
			BLK_SIMALLABORT = 113,
			BLK_SIMADJOURN = 114,
			BLK_SIMALLADJOURN = 115,
			BLK_SIMGAMES = 116,
			BLK_SIMMATCH = 117,
			BLK_SIMNEXT = 118,
			BLK_SIMOBSERVE = 119,
			BLK_SIMOPEN = 120,
			BLK_SIMPASS = 121,
			BLK_SIMPREV = 122,
			BLK_SMOVES = 123,
			BLK_SMPOSITION = 124,
			BLK_SPOSITION = 125,
			BLK_STATISTICS = 126,
			BLK_STORED = 127,
			BLK_STYLE = 128,
			BLK_SUBLIST = 129,
			BLK_SWITCH = 130,
			BLK_TAKEBACK = 131,
			BLK_TELL = 132,
			BLK_TIME = 133,
			BLK_TOMOVE = 134,
			BLK_TOURNSET = 135,
			BLK_UNALIAS = 136,
			BLK_UNEXAMINE = 137,
			BLK_UNOBSERVE = 138,
			BLK_UNPAUSE = 139,
			BLK_UPTIME = 140,
			BLK_USCF = 141,
			BLK_USTAT = 142,
			BLK_VARIABLES = 143,
			BLK_WHENSHUT = 144,
			BLK_WHISPER = 145,
			BLK_WHO = 146,
			BLK_WITHDRAW = 147,
			BLK_WNAME = 148,
			BLK_XKIBITZ = 149,
			BLK_XTELL = 150,
			BLK_XWHISPER = 151,
			BLK_ZNOTIFY = 152,
			BLK_REPLY = 153,
			BLK_SUMMON = 154,
			BLK_SEEK = 155,
			BLK_UNSEEK = 156,
			BLK_SOUGHT = 157,
			BLK_PLAY = 158,
			BLK_ALIAS = 159,
			BLK_NEWBIES = 160,
			BLK_SR = 161,
			BLK_CA = 162,
			BLK_TM = 163,
			BLK_GETGAME = 164,
			BLK_CCNEWSE = 165,
			BLK_CCNEWSF = 166,
			BLK_CCNEWSI = 167,
			BLK_CCNEWSP = 168,
			BLK_CCNEWST = 169,
			BLK_CSNEWSE = 170,
			BLK_CSNEWSF = 171,
			BLK_CSNEWSI = 172,
			BLK_CSNEWSP = 173,
			BLK_CSNEWST = 174,
			BLK_CTNEWSE = 175,
			BLK_CTNEWSF = 176,
			BLK_CTNEWSI = 177,
			BLK_CTNEWSP = 178,
			BLK_CTNEWST = 179,
			BLK_CNEWS = 180,
			BLK_SNEWS = 181,
			BLK_TNEWS = 182,
			BLK_RMATCH = 183,
			BLK_RSTAT = 184,
			BLK_CRSTAT = 185,
			BLK_HRSTAT = 186,
			BLK_GSTAT = 187
		}

		public delegate void BlockCodeEventHandler (object o,
							    BlockCode code);
		public delegate void CommandIdentifierEventHandler (object o,
								    string
								    commandIdentifier);
		public delegate void CommandCodeEventHandler (object o,
							      CommandCode
							      commandCode);

		public class ICSClient
		{
			public string server = "www.freechess.org";
			public string port = "5000";
			string user = "";
			public string assigned_name;	// assigned for guest login
			public string passwd = "";
			bool guestLogin = false;

			public string User
			{
				set
				{
					user = value;
					if (user.Equals ("guest"))
						guestLogin = true;
					else
					  {
						  guestLogin = false;
					  }
				}
				get
				{
					return assigned_name ==
						null ? user : assigned_name;
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
			public event ConnectionErrorEventHandler
				ConnectionErrorEvent;

			public event BlockCodeEventHandler BlockCodeEvent;
			public event CommandIdentifierEventHandler
				CommandIdentifierEvent;
			public event CommandCodeEventHandler CommandCodeEvent;

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
			int m_start, m_end;
			System.Text.Decoder decoder;
			System.Text.Encoding encoding;

			Hashtable notificationMap;
			int blockCount;
			CommandSender commandSender;
			public CommandSender CommandSender
			{
				get
				{
					return commandSender;
				}
			}

			public ICSClient ()
			{
				commandSender = new CommandSender (this);
				map = new Hashtable ();
				ads = new ArrayList ();
				buffer = new byte[4096];
				m_start = m_end = 0;
				decoder =
					System.Text.Encoding.UTF8.
					GetDecoder ();
				encoding = System.Text.Encoding.ASCII;

				notificationMap = new Hashtable ();
				notificationMap["sr"] = NotificationType.SR;
				notificationMap["sc"] = NotificationType.SC;
				notificationMap["s"] = NotificationType.S;
				notificationMap["12"] =
					NotificationType.STYLE12;
				notificationMap["g1"] =
					NotificationType.GAMEINFO;
			}

			public bool Start ()
			{
				if (client == null)
				  {
					  Connect ();
					  return PostReadRequest ();
				  }

				// For reauth, no need to post a read request
				// this method will be called with the new auth details
				// so, just resume the auth process (from "login:")
				if (state == SessionState.REAUTH)
				  {
					  streamWriter.WriteLine (user);
					  streamWriter.Flush ();
					  state = SessionState.AUTH_REQUEST;
				  }
				return true;
			}

			public void Stop ()
			{
				if (client != null)
					client.Close ();
				client = null;
				stream = null;
				streamReader = null;
				streamWriter = null;
				user = "";
				assigned_name = null;
				m_start = m_end = 0;
				blockCount = 0;
				state = SessionState.NONE;
			}

			IAsyncResult pending;
			private bool PostReadRequest ()
			{
				if (m_end == buffer.Length)
				  {
					  FireConnectionErrorEvent (Catalog.
								    GetString
								    ("Internal error. Buffer full"));
					  return false;	// buffer full. but this should not happen
				  }

				try
				{
					pending =
						stream.BeginRead (buffer,
								  m_end,
								  buffer.
								  Length -
								  m_end,
								  ReadAsyncCallback,
								  null);
				}
				catch (Exception e)
				{
					FireConnectionErrorEvent (e.
								  ToString
								  ());
					return false;
				}

				return true;
			}

			// This can get triggered from async result which may not be in the
			// glib context. so add an idle handler
			private void FireConnectionErrorEvent (string reason)
			{
				state = SessionState.NONE;
				GLib.Idle.Add (delegate ()
					       {
					       if (ConnectionErrorEvent !=
						   null)
					       ConnectionErrorEvent (this,
								     reason);
					       return false;}
				);
			}

			private void ReadAsyncCallback (IAsyncResult res)
			{
				if (stream == null)
				  {
					  return;
				  }
				try
				{
					int nbytes = stream.EndRead (res);

					if (nbytes <= 0)
					  {
						  FireConnectionErrorEvent
							  (Catalog.
							   GetString
							   ("Connection closed!"));
						  return;
					  }

					m_end += nbytes;
					pending = null;
				}
				catch (Exception e)
				{
					FireConnectionErrorEvent (e.
								  ToString
								  ());
					return;
				}

				GLib.Idle.Add (ProcessBufferIdleHandler);
			}

			private bool ProcessBufferIdleHandler ()
			{
				ProcessBuffer ();
				return false;
			}

			private void ProcessBuffer ()
			{
				bool expecting_auth =
					state != SessionState.AUTHENTICATED;
				for (int i = m_start; i < m_end; i++)
				  {
					  if (buffer[i] == '\n')
					    {
						    if (i > m_start)
						      {
							      ProcessLine
								      (m_start,
								       i -
								       m_start);
						      }
						    m_start = i + 1;
					    }
					  else if (expecting_auth
						   && buffer[i] == ':')
					    {
						    ProcessLine (m_start, i - m_start + 1);	// including the delim
						    m_start = i + 1;
					    }
				  }

				if (m_start > 0)
				  {
					  for (int i = m_start, j = 0;
					       i < m_end; i++, j++)
					    {
						    buffer[j] = buffer[i];
					    }
					  m_end -= m_start;
					  m_start = 0;
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
				string str =
					encoding.GetString (buffer, start,
							    i - start);
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
					  GameInfo info =
						  GameInfo.FromBuffer (buffer,
								       start,
								       end);
					  if (GameInfoEvent != null)
						  GameInfoEvent (this, info);
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
				if (IsBlockChar (buffer[start]))
				  {
					  return LineType.Block;
				  }
				if (buffer[start] == '{')
					return LineType.ResultNotification;

				while (end > start && buffer[end - 1] == ' ')
					end--;
				if (end == start)
					return LineType.Normal;
				for (int i = start; i < end; i++)
				  {
					  if (buffer[i] == '%')
						  return LineType.Prompt;
					  if (Char.
					      IsWhiteSpace ((char) buffer[i]))
						  break;
				  }

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

			private void ProcessLine (int start, int count)
			{
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
				try
				{
					__ProcessLine (start, count);
				}
				catch (Exception e)
				{
					Console.WriteLine
						("Exception@[LINE:] [{0}]",
						 System.Text.Encoding.ASCII.
						 GetString (buffer, start,
							    count));
					Console.WriteLine (e);
				}
			}

			private static bool GotoBlockStart (byte[]buffer,
							    ref int start,
							    ref int count)
			{
				int idx = start;
				ParserUtils.GotoThisChar (buffer,
							  (char) BlockCode.
							  BlockStart, ref idx,
							  start + count);
				if (idx == start + count)
					return false;
				count -= idx - start;
				start = idx;
				return true;
			}

			private void __ProcessLine (int start, int count)
			{
				LineType type = GetLineType (buffer, start,
							     start + count);

				if ((type == LineType.Prompt
				     && GotoBlockStart (buffer, ref start,
							ref count))
				    || (type == LineType.Block
					&& buffer[start] ==
					(byte) BlockCode.BlockStart))
				  {
					  blockCount++;
				  }

				if (blockCount > 0)
				  {
					  HandleBlock (buffer, start,
						       start + count);
				  }

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

				string line =
					encoding.GetString (buffer, start,
							    count);

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
						   SessionState.AUTH_REQUEST
						   || state ==
						   SessionState.PASSWORD_SENT)
					    {
						    state = SessionState.
							    REAUTH;
						    if (AuthEvent != null)
							    AuthEvent (this,
								       false);
					    }
				  }
				else if (state == SessionState.AUTH_REQUEST
					 && (line.Equals ("password:")
					     || (guestLogin
						 && line.EndsWith (":"))))
				  {
					  if (guestLogin)
					    {
						    assigned_name =
							    GetAssignedGuestName
							    (buffer, start,
							     start + count);
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

			private void HandleBlock (byte[]buffer, int start,
						  int end)
			{
				if (buffer[start] ==
				    (byte) BlockCode.BlockStart)
				  {
					  if (BlockCodeEvent != null)
						  BlockCodeEvent (this,
								  BlockCode.
								  BlockStart);
					  start++;
					  if (blockCount == 1)
					    {
						    char commandIdentifier =
							    (char)
							    buffer[start++];
						    if (CommandIdentifierEvent
							!= null)
							    CommandIdentifierEvent
								    (this,
								     commandIdentifier.
								     ToString
								     ());
						    int i = ++start;
						    while (buffer[i] !=
							   (byte) BlockCode.
							   BlockSeparator)
							    i++;
						    string codestr =
							    encoding.
							    GetString (buffer,
								       start,
								       i -
								       start);
						    CommandCode commandCode =
							    (CommandCode)
							    Int32.
							    Parse (codestr);
						    if (CommandCodeEvent !=
							null)
							    CommandCodeEvent
								    (this,
								     commandCode);
						    start = i + 1;
					    }
				  }

				for (int i = start; i < end; i++)
				  {
					  if (!IsBlockChar (buffer[i]))
						  continue;
					  if (buffer[i] ==
					      (byte) BlockCode.BlockEnd)
						  blockCount--;
					  if (BlockCodeEvent != null)
						  BlockCodeEvent (this,
								  (BlockCode)
								  buffer[i]);
				  }
			}

			private bool IsBlockChar (byte val)
			{
				BlockCode code = (BlockCode) val;
				return code == BlockCode.BlockStart
					|| code == BlockCode.BlockEnd
					|| code == BlockCode.BlockSeparator
					|| code == BlockCode.BlockPoseStart
					|| code == BlockCode.BlockPoseEnd;
			}

			private static string
				GetAssignedGuestName (byte[]buffer, int start,
						      int end)
			{
				int i = end - 1;
				while (i > start && buffer[i] != '"')
					i--;
				if (i == start)
					return null;

				int end_offset = i;
				i--;
				while (i > start && buffer[i] != '"')
					i--;
				if (i == start)
					return null;
				i++;
				return System.Text.Encoding.ASCII.
					GetString (buffer, i, end_offset - i);
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
				streamWriter.WriteLine ("iset block 1");
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

			private void Connect ()
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
					FireConnectionErrorEvent (String.
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
