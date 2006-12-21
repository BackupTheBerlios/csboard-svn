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
// Copyright (C) 2004 Nickolay V. Shmyrev

namespace CsBoard
{

	using System.IO;
	using System;

	public class Session
	{

		public string Filename;

		private GConf.Client gconfClient;

		public Session ()
		{

			gconfClient = new GConf.Client ();
			gconfClient.AddNotify ("/apps/csboard",
					       new GConf.
					       NotifyEventHandler
					       (SessionChanged));


			string dir = "";
			string gnomedir = "";

			  gnomedir =
				Path.Combine (Environment.
					      GetEnvironmentVariable ("HOME"),
					      ".gnome2");

			if (!Directory.Exists (gnomedir))
			{
				Directory.CreateDirectory (gnomedir);
			}

			dir = Path.Combine (gnomedir, "csboard");

			if (!Directory.Exists (dir))
			{
				Directory.CreateDirectory (dir);
			}

			Filename = Path.Combine (dir, "session.pgn");
		}

		public Level level
		{
			get
			{
				return (Level) gconfClient.
					Get ("/apps/csboard/session/level");
			}
			set
			{
				gconfClient.
					Set ("/apps/csboard/session/level",
					     (int) value);
			}
		}


		public void SetupGeometry (Gtk.Window w)
		{
			int width =
				(int) gconfClient.
				Get ("/apps/csboard/session/width");
			int height =
				(int) gconfClient.
				Get ("/apps/csboard/session/height");

			w.Resize (width, height);
		}

		public void SaveGeometry (Gtk.Window w)
		{
			int width, height;
			w.GetSize (out width, out height);
			gconfClient.Set ("/apps/csboard/session/width",
					 width);
			gconfClient.Set ("/apps/csboard/session/height",
					 height);
		}

		public string Engine
		{
			get
			{
				return (string) gconfClient.
					Get ("/apps/csboard/engine");
			}
		}
		public bool HighLightMove
		{

			get
			{
				return (bool) gconfClient.
					Get
					("/apps/csboard/session/last_move");
			}
			set
			{
				gconfClient.
					Set
					("/apps/csboard/session/last_move",
					 value);
			}
		}

		public bool PossibleMoves
		{

			get
			{
				return (bool) gconfClient.
					Get
					("/apps/csboard/session/possible_moves");
			}
			set
			{
				gconfClient.
					Set
					("/apps/csboard/session/possible_moves",
					 value);
			}
		}

		public bool ShowCoords
		{
			get
			{
				return (bool) gconfClient.
					Get
					("/apps/csboard/session/show_coords");
			}
			set
			{
				gconfClient.
					Set
					("/apps/csboard/session/show_coords",
					 value);
			}
		}

		public bool showAnimations
		{
			get
			{
				return (bool) gconfClient.
					Get ("/apps/csboard/session/animate");
			}
			set
			{
				gconfClient.
					Set ("/apps/csboard/session/animate",
					     value);
			}
		}

		public string CurrentFolder
		{
			get
			{
				return (string) gconfClient.
					Get
					("/apps/csboard/session/current_folder");
			}
			set
			{
				gconfClient.
					Set
					("/apps/csboard/session/current_folder",
					 value);
			}
		}

		public int ViewerSplitPanePosition
		{
			get
			{
				return (int) gconfClient.
					Get
					("/apps/csboard/session/viewer_split_pane_position");
			}
			set
			{
				gconfClient.
					Set
					("/apps/csboard/session/viewer_split_pane_position",
					 value);
			}
		}

		private void SessionChanged (object obj,
					     GConf.NotifyEventArgs args)
		{
			return;
		}

	}
}
