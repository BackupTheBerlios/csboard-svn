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

namespace CsBoard {

	using System;
	using System.IO;
	using System.Net;
	using System.Net.Sockets;
	using System.Threading;

	public class ICSClient {

		public string server = "www.freechess.org";
		public string port = "6667";
		public string user = "guest";
		public string passwd = "";


		// This is a separate thread which runs continually while connected 
		// to an ICS server.  It reads data from the server and takes action.
		public Thread readThread;

		// Our TCP client to connect to an ICS server
		public TcpClient client;

		// Once we're connected we get a stream that we can read and write from/to
		public NetworkStream stream;
		public StreamReader streamReader;
		public StreamWriter streamWriter;


		public ICSClient () {
		}

		public void Write (string message) {
		}

		public void Connect () {
			try {
	 		    client = new TcpClient (server, int.Parse(port));
			    stream = client.GetStream ();
			    streamWriter = new StreamWriter (stream);
			} catch (SocketException e) {
				throw new ApplicationException (String.Format(Catalog.GetString("Can't connect to {0} port {1}"), server, port));
			}

		}

	}
}
