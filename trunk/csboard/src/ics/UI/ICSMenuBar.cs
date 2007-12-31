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
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		class ICSMenuBar:AppMenuBar
		{
			public ImageMenuItem connectMenuItem,
				disconnectMenuItem;
			public CheckMenuItem showTabsMenuItem;
			public ICSMenuBar ():base ()
			{
				Menu menu = fileMenuItem.Submenu as Menu;
				  connectMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Connect"));
				  connectMenuItem.Image =
					new Image (Stock.Connect,
						   IconSize.Menu);
				  disconnectMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Disconnect"));
				  disconnectMenuItem.Image =
					new Image (Stock.Disconnect,
						   IconSize.Menu);
				int i = 0;
				  menu.Insert (connectMenuItem, i++);
				  menu.Insert (disconnectMenuItem, i++);

				  showTabsMenuItem =
					new CheckMenuItem (Catalog.
							   GetString
							   ("Show _Tabs"));
				  try
				{
					showTabsMenuItem.Active =
						App.Session.ICSShowTabs;
				}
				catch
				{
					showTabsMenuItem.Active = true;
				}

				Menu vmenu = viewMenuItem.Submenu as Menu;
				  vmenu.Append (showTabsMenuItem);
				  vmenu.Append (new SeparatorMenuItem ());

				  ShowAll ();
			}
		}

		class PageMenuItem:MenuItem
		{
			Notebook book;
			Widget page;
			public PageMenuItem (string label, Widget p,
					     Notebook b):base (label)
			{
				book = b;
				page = p;
				Activated += on_activated;
				ShowAll ();
			}

			private void on_activated (object o, EventArgs args)
			{
				book.CurrentPage = book.PageNum (page);
			}
		}
	}
}
