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
using System.Text;

namespace CsBoard
{
	public delegate void ClockTimeoutHandler (object o, EventArgs args);
	public class ChessClock:Label
	{
		int limit;
		public int Limit
		{
			get
			{
				return limit;
			}
		}
		uint increment;
		public uint Increment
		{
			get
			{
				return increment;
			}
		}

		uint elapsed_time;
		public uint ElapsedTime
		{
			get
			{
				return elapsed_time;
			}
		}

		bool started;
		DateTime startTime;
		DateTime tickerStart;
		uint timeoutHandle;
		uint tickerHandle;

		bool countdown;

		public event ClockTimeoutHandler ClockTimeoutEvent;

		long remaining_msecs;
		public long RemainingTime
		{
			get
			{
				return remaining_msecs;
			}

			set
			{
				remaining_msecs = value;
				Update ();
			}
		}

		bool ticker = true;

		public ChessClock ():base ("")
		{
			limit = -1;
			Update ();
		}

		public void Configure (int secs, uint increment)
		{
			started = false;
			this.limit = secs;
			this.increment = increment;
			elapsed_time = 0;
			countdown = secs >= 0;
			if (countdown)
				remaining_msecs = secs * 1000;
			Update ();
		}

		public void Start ()
		{
			if (started)
				return;
			if (countdown && remaining_msecs < 0)
				return;

			startTime = DateTime.Now;
			started = true;
			if (countdown)
				timeoutHandle =
					GLib.Timeout.
					Add ((uint) (remaining_msecs + 1),
					     OnClockTimeout);
			if (ticker)
			  {
				  tickerStart = startTime;
				  tickerHandle =
					  GLib.Timeout.Add (1000,
							    OnTickTimeout);
			  }
		}

		private bool OnTickTimeout ()
		{
			Calculate ();
			Update ();
			return started;	// if started, another tick!
		}

		public void Stop ()
		{
			if (!started)	// already stopped
				return;

			if (ticker)
				GLib.Source.Remove (tickerHandle);

			if (countdown)
				GLib.Source.Remove (timeoutHandle);
			Calculate ();
			started = false;
			remaining_msecs += increment * 1000;
			Update ();
		}

		private void Calculate ()
		{
			DateTime now = DateTime.Now;
			TimeSpan span =
				now.
				Subtract (ticker ? tickerStart : startTime);
			if (countdown)
				remaining_msecs -=
					(uint) span.TotalMilliseconds;

			elapsed_time += (uint) span.TotalMilliseconds;

			if (ticker)
				tickerStart = now;
			if (remaining_msecs <= 0)
			  {
				  FireTimeoutEvent ();
				  return;
			  }
		}

		public void Update ()
		{
			StringBuilder buffer = new StringBuilder ();
			buffer.Append ("<big><big><big>");

			long secs =
				(countdown ? remaining_msecs : elapsed_time) /
				1000;

			if(secs < 0) {
				buffer.Append("-");
				secs = -secs;
			}

			long mins = secs / 60;
			long hrs = mins / 60;

			secs %= 60;
			mins %= 60;

			if (hrs > 0)
			  {
				  buffer.Append (String.
						 Format ("{0:D2}:", hrs));
			  }
			buffer.Append (String.
				       Format ("{0:D2}:{1:D2}", mins, secs));
			buffer.Append ("</big></big></big>");

			base.Markup = buffer.ToString ();
		}

		public void Reset (int lim, uint incr)
		{
			if (countdown && started)
				GLib.Source.Remove (timeoutHandle);
			Configure (lim, incr);
		}

		private bool OnClockTimeout ()
		{
			Stop ();
			return false;
		}

		private void FireTimeoutEvent ()
		{
			if (ClockTimeoutEvent != null)
				ClockTimeoutEvent (this, EventArgs.Empty);
		}
	}
}
