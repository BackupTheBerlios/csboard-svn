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
		public class ICSDetailsWindow:Window
		{
			ICSClient client;

			Notebook book;

			public Notebook Book
			{
				get
				{
					return book;
				}
			}

			public ICSDetailsWindow (ICSClient client,
						 string title):base (title)
			{
				book = new Notebook ();
				this.client = client;
				Add (book);
				book.ShowTabs = true;

				DeleteEvent +=
					delegate (object o,
						  DeleteEventArgs args)
				{
					args.RetVal = true;
				};

				  ShowAll ();
			}
		}
	}
}
