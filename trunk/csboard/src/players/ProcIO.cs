namespace CsBoard {
	using System;
	using System.Collections;
	using System.Threading;
	using System.Text.RegularExpressions;
	using System.IO;

	  delegate void DataReceivedCallback(ArrayList list);
  delegate void DataReceivedEventHandler(object o, ArrayList list);

	class ProcIO
	{
		byte[] buf;
		char[] charbuf;
		int charbuflen;
		StreamReader output;
		StreamWriter input;
		IAsyncResult asyncRes;
	  ArrayList result;
		bool iopending;
		bool sync_get;
	  Stack dataReceivedCallbacks;
	  public const string FLUSH_STR = "give me it";
	  public const string FLUSH_STR_ILLEGAL_MOVE = "Illegal move:give me it";
	  public event DataReceivedEventHandler DataReceivedEvent;

		public ProcIO (System.Diagnostics.Process proc)
		{
			dataReceivedCallbacks = new Stack();
			input = proc.StandardInput;
			output = proc.StandardOutput;
			buf = new byte[4096];
			charbuf = new char[4096];
			result = new ArrayList();
			iopending = false;
			sync_get = false;
			output.DiscardBufferedData();
		}

		public void GetAsync (DataReceivedCallback c, params string[] commands)
		{
		  if(c != null)
		    dataReceivedCallbacks.Push(c);
		  GetFromProc (false, commands);
		}

		public void GetAsync (params string[] commands)
		{
		  GetAsync(null, commands);
		}

		public ArrayList GetSync (params string[] commands)
		{
		  return GetFromProc (true, commands);
		}

		private ArrayList GetFromProc (bool wait, params string[] commands)
		{
		  foreach(string s in commands) {
			input.WriteLine (s);
			input.Flush();

			if (Config.Debug)
				Console.WriteLine (s);
		  }
			// Wait till there are no pending I/O events
			Monitor.Enter (result);
			if(wait)
			  sync_get = true;
			while (iopending)
				Monitor.Wait (result);
			iopending = true;
			Monitor.Exit (result);

			asyncRes =
				output.BaseStream.BeginRead (buf, 0,
							     buf.Length,
							     ReadCallback,
							     null);

			if (!wait) {
				return null;
			}

			Monitor.Enter (result);
			while (iopending)
			  {
				  Monitor.Wait (result);
			  }

			ArrayList res = new ArrayList (result);
			result.Clear ();

			sync_get = false;
			Monitor.Exit (result);

			return res;
		}

		void ReadCallback (IAsyncResult res)
		{
			if (Read (res) == 0)	// stream closed!
				return;
			string line;
			Monitor.Enter (result);
			while (ReadLine (out line))
			  {
				  if (String.Compare (line,
						      FLUSH_STR_ILLEGAL_MOVE)
				      == 0)
				    {
					    continue;
				    }
				  result.Add (line);
			  }
			iopending = false;
			Monitor.Pulse (result);
			ArrayList idlearray = null;
			if (!sync_get) {
			  idlearray = new ArrayList(result);
			  result.Clear ();
			}
			Monitor.Exit (result);
			DiscardBuffer ();

			Gtk.Application.Invoke(delegate {
			    ProcessAsyncData(idlearray);
			  });
		}

	  void ProcessAsyncData(ArrayList idlearray) {
	    ArrayList arr = idlearray;
	    idlearray = null;
			if(arr != null && DataReceivedEvent != null)
			  DataReceivedEvent(this, arr);

			if(arr != null && dataReceivedCallbacks.Count > 0) {
			  DataReceivedCallback cb = dataReceivedCallbacks.Pop() as DataReceivedCallback;
			  cb(arr);
			}
		}

		int Read (IAsyncResult res)
		{
			int nread =
				output.BaseStream.EndRead (res);
			if (nread == 0)
				return nread;
			output.CurrentEncoding.GetChars (buf, 0, nread,
							 charbuf, charbuflen);
			charbuflen += nread;
			return nread;
		}

		bool ReadLine (out string line)
		{
			int i;
			for (i = 0; i < charbuflen; i++)
				if (charbuf[i] == '\n')
					break;
			if (i == charbuflen)
			  {
				  line = null;
				  return false;
			  }

			int next = i + 1;
			if (i > 1 && charbuf[i - 1] == '\r')
				i--;
			line = new string (charbuf, 0, i);

			// shift the remaining buffer left
			charbuflen -= next;
			for (i = 0; i < charbuflen; i++)
			  {
				  charbuf[i] = charbuf[next + i];
			  }
			return true;
		}

		public void DiscardBuffer ()
		{
		  if(charbuflen > 0) {
		    Console.WriteLine("Warning: Discarding \"{0}\"", new string(charbuf, 0, charbuflen));
		  }
			charbuflen = 0;
		}

	  public void Put(string s) {
	    input.WriteLine (s);
	    input.Flush();
	    Console.WriteLine("[CMD]: " + s);
	  }
	}
}
