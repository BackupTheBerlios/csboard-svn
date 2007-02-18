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

using System.Collections;
using Chess.Parser;
using Chess.Game;
using Gtk;
using Gnome;
using System;
using CsBoard;
using Pango;

namespace CsBoard
{
	namespace Viewer
	{
		public class CairoViewerBoard:CairoBoard
		{
			int src_rank, src_file, dest_rank, dest_file;
			bool firstTime = true;

			public CairoViewerBoard (ArrayList pos):base (pos)
			{
				highLightMove = true;
				position.AskForPromotion = false;
			}

		
			protected override bool OnConfigureEvent (Gdk.
								  EventConfigure
								  evnt)
			{
				if (firstTime)
				{
					base.OnConfigureEvent (evnt);
					firstTime = false;
				}
				if (Allocation.Height != Allocation.Width)
				{
					WidthRequest = HeightRequest =
						Allocation.Width;
					return false;
				}
				return base.OnConfigureEvent (evnt);
			}
		}
	}
}
