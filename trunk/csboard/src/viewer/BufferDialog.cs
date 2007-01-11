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
using CsBoard;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class BufferDialog:Dialog
		{
			TextView textView;

			public BufferDialog (Gtk.
					     Window
					     par, string title):base
				(title, par,
				 DialogFlags.Modal,
				 Catalog.GetString ("Cancel"),
				 ResponseType.Cancel,
				 Catalog.GetString ("Ok"), ResponseType.Ok)
			{
				textView = new TextView ();
				textView.WrapMode = WrapMode.WordChar;
				textView.Editable = true;
				textView.Show ();

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Automatic;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Child = textView;
				  win.Show ();
				  VBox.PackStart (win, true, true, 4);
			}

			public string Buffer
			{
				get
				{
					return textView.Buffer.Text;
				}
				set
				{
					textView.Buffer.Text = value;
				}
			}

			public TextView TextView
			{
				get
				{
					return textView;
				}
			}
		}
	}
}
