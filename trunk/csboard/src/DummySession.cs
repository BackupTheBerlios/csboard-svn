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

		//private GConf.Client gconfClient;

		public Session ()
		{
/*
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
*/
		}

		public Level level
		{
			get
			{
				return (Level) 0;
			}
			set
			{
			}
		}


		public void SetupGeometry (Gtk.Window w)
		{
			int width = (int) 800;
			int height = 650;

			w.Resize (width, height);
		}

		public void SaveGeometry (Gtk.Window w)
		{
		}

		public string Engine
		{
			get
			{
				return "null";
			}
			set
			{
			}
		}
		public bool HighLightMove
		{

			get
			{
				return true;
			}
			set
			{
			}
		}

		public bool PossibleMoves
		{

			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool ShowCoords
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		public bool showAnimations
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public string CurrentFolder
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public int ViewerWidth
		{
			get
			{
				return 500;
			}
		}

		public int ViewerHeight
		{
			get
			{
				return 500;
			}
		}

		public void SetupViewerGeometry (Gtk.Window w)
		{
			w.Resize (800, 650);
		}

		public void SaveViewerGeometry (Gtk.Window w)
		{
		}

		public int ViewerSplitPanePosition
		{
			get
			{
				return 400;
			}
			set
			{
			}
		}

		public int ICSGamesWinSplitPanePosition
		{
			get
			{
				int pos = 100;
				if (pos > ICSGamesWinWidth / 2)
					  pos = ICSGamesWinWidth / 2;
				  return pos;
			}
			set
			{
			}
		}

		public int ICSWinWidth
		{
			get
			{
				return 800;
			}
			set
			{
			}
		}

		public int ICSWinHeight
		{
			get
			{
				return 650;
			}

			set
			{
			}
		}

		public int ICSGamesWinWidth
		{
			get
			{
				return 800;
			}
			set
			{
			}
		}

		public int ICSGamesWinHeight
		{
			get
			{
				return 650;
			}

			set
			{
			}
		}
		public string LastAppName
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		public bool ICSShowTabs
		{
			get
			{
				return true;
			}
			set
			{
			}
		}
	}
}
