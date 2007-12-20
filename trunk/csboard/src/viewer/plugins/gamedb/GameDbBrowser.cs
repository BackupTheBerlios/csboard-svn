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
using System.Collections;
using System;
using Mono.Unix;

using com.db4o;
using com.db4o.query;

namespace CsBoard
{
	namespace Viewer
	{
		public class GameDbBrowser:GameDbBrowserUI, SubApp
		{
			AppMenuBar menubar;
			public MenuBar MenuBar
			{
				get
				{
					return menubar;
				}
			}

			ToolButton toolbutton;
			public ToolButton ToolButton
			{
				get
				{
					return toolbutton;
				}
			}

			public Widget Widget
			{
				get
				{
					return this;
				}
			}

			string title;
			public string Title
			{
				get
				{
					return title;
				}
			}

			public string ID
			{
				get
				{
					return "gamedb";
				}
			}

			public event TitleChangedEventHandler
				TitleChangedEvent;

			public void SetVisibility (bool val)
			{
			}

			AccelGroup accel;
			public AccelGroup AccelGroup
			{
				get
				{
					return accel;
				}
			}

			public GameDbBrowser ():base ()
			{
				accel = new AccelGroup ();
				title = Catalog.GetString ("Game Database");
				menubar = new AppMenuBar ();
				menubar.ShowAll ();
				Image img =
					new Image (Gdk.Pixbuf.
						   LoadFromResource
						   ("dbicon.png"));
				toolbutton =
					new ToolButton (img, Catalog.
							GetString
							("Database"));
				toolbutton.ShowAll ();
				menubar.quitMenuItem.
					AddAccelerator ("activate", accel,
							new AccelKey (Gdk.Key.
								      q,
								      Gdk.
								      ModifierType.
								      ControlMask,
								      AccelFlags.
								      Visible));
			}
		}
	}
}
