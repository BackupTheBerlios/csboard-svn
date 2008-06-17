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

namespace CsBoard
{
	namespace ICS
	{
		public abstract class Notification
		{
			public abstract string Text
			{
				get;
			}
			public abstract void Accepted ();
		}

		public class TournamentInfoNotification:Notification
		{
			string text;
			ICSDetailsWidget ics;
			public TournamentInfoNotification (ICSDetailsWidget i,
							   string t)
			{
				text = t;
				ics = i;
			}

			public override string Text
			{
				get
				{
					return text;
				}
			}

			public override void Accepted ()
			{
				ics.ShowTournamentsPage ();
			}
		}

		public class NotificationWidget:EventBox
		{
			Button acceptButton, closeButton;
			Label label;
			uint timeoutid;
			public Notification notification;

			public NotificationWidget ():base ()
			{
				HBox box = new HBox ();
				  label = new Label ();
				  label.Xalign = 0;
				  closeButton = new Button ();
				  closeButton.Image =
					new Image (Stock.Close,
						   IconSize.Button);
				  closeButton.Clicked += on_close;
				  acceptButton = new Button (Stock.Ok);
				  acceptButton.Image =
					new Image (Stock.Ok, IconSize.Button);
				  acceptButton.Clicked += on_accept;
				  ModifyBg (StateType.Normal,
					    new Gdk.Color (0xff, 0xff, 0xc0));

				  box.PackStart (label, true, true, 5);
				  box.PackStart (acceptButton, false, false,
						 5);
				  box.PackStart (closeButton, false, false,
						 5);
				  label.Show ();
				  closeButton.Show ();

				// This box will add some vertical padding
				VBox b = new VBox ();
				  b.PackStart (box, false, true, 5);

				  Add (b);
			}

			public string AcceptLabel
			{
				set
				{
					acceptButton.Label = value;
				}
			}

			public void SetNotification (Notification
						     notification,
						     uint timeout)
			{
				this.notification = notification;
				label.Text = notification.Text;
				if (timeout > 0)
					timeoutid =
						GLib.Timeout.Add (1000 *
								  timeout,
								  on_timeout);
				ShowAll ();
			}

			private bool on_timeout ()
			{
				notification = null;
				Hide ();
				Console.WriteLine ("done");
				return false;
			}

			private void on_accept (object o, EventArgs args)
			{
				if (notification == null)
					return;
				notification.Accepted ();
				Close ();
			}

			private void on_close (object o, EventArgs args)
			{
				Close ();
			}

			void Close ()
			{
				if (timeoutid != 0)
					GLib.Source.Remove (timeoutid);
				Hide ();
			}
		}
	}
}
