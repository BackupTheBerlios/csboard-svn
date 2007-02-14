using Gtk;
using System;
using System.Text;

namespace CsBoard {
	public delegate void ClockTimeoutHandler(object o, EventArgs args);
	public class ChessClock : Label {
		int limit;
		public int Limit {
			get {
				return limit;
			}
		}
		uint increment;
		public uint Increment {
			get {
				return increment;
			}
		}

		uint elapsed_time;
		public uint ElapsedTime {
			get {
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
		public long RemainingTime {
			get {
				return remaining_msecs;
			}

			set {
				remaining_msecs = value;
			}
		}

		bool ticker = true;

		public ChessClock() : base("") {
			limit = -1;
			Update();
		}

		public void Configure(int secs, uint increment) {
			started = false;
			this.limit = secs;
			this.increment = increment;
			elapsed_time = 0;
			countdown = secs >= 0;
			if(countdown)
				remaining_msecs = secs * 1000;
			Update();
		}

		public void Start() {
			if(started)
				return;
			if(countdown && remaining_msecs < 0)
				return;

			startTime = DateTime.Now;
			started = true;
			if(countdown)
				timeoutHandle = GLib.Timeout.Add((uint)(remaining_msecs + 1), OnClockTimeout);
			if(ticker) {
				tickerStart = startTime;
				tickerHandle = GLib.Timeout.Add(1000, OnTickTimeout);
			}
		}

		private bool OnTickTimeout() {
			Calculate();
			Update();
			return started; // if started, another tick!
		}

		public void Stop() {
			if(!started) // already stopped
				return;

			if(ticker)
				GLib.Source.Remove(tickerHandle);

			if(countdown)
				GLib.Source.Remove(timeoutHandle);
			Calculate();
			started = false;
			Update();
		}

		private void Calculate() {
			DateTime now = DateTime.Now;
			TimeSpan span = now.Subtract(ticker ? tickerStart : startTime);
			if(countdown)
				remaining_msecs -= (uint) span.TotalMilliseconds;

			elapsed_time += (uint) span.TotalMilliseconds;

			if(ticker)
				tickerStart = now;
			if(remaining_msecs <= 0) {
				FireTimeoutEvent();
				return;
			}
			remaining_msecs += increment * 1000;
		}

		public void Update() {
			long secs = (countdown ? remaining_msecs : elapsed_time) / 1000;
			long mins = secs / 60;
			long hrs = mins / 60;

			secs %= 60;
			mins %= 60;

			StringBuilder buffer = new StringBuilder();
			buffer.Append("<big><big><big>");
			if(hrs > 0) {
				buffer.Append(String.Format("{0:2}:", hrs));
			}
			buffer.Append(String.Format("{0:D2}:{1:D2}", mins, secs));
			buffer.Append("</big></big></big>");
			base.Markup = buffer.ToString();
		}

		public void Reset(int lim, uint incr) {
			if(countdown && started)
				GLib.Source.Remove(timeoutHandle);
			Configure(lim, incr);
		}

		private bool OnClockTimeout() {
			Stop();
			return false;
		}

		private void FireTimeoutEvent() {
			if(ClockTimeoutEvent != null)
				ClockTimeoutEvent(this, EventArgs.Empty);
		}
	}
}
