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

namespace CsBoard
{
	namespace Viewer
	{

		public class PositionSnapshot
		{
			// Geometry
			private int start_x;
			private int start_y;
			private int size;
			private int border_padding;
			private const int space = 1;
			private int width;
			private int height;

			public bool side = false;

			// Figure Renderer
			private Figure figure;

			// Position
			private Position position;

			  Gdk.GC gc;

			  Gdk.Color border_color, blacksq_color,
				whitesq_color;


			private Gdk.Pixmap map;
			public Gdk.Pixmap Pixmap
			{
				get
				{
					return map;
				}
			}

			public PositionSnapshot (ArrayList pos, int width,
						 int height)
			{
				this.width = width;
				this.height = height;
				border_padding = 4;
				border_color = new Gdk.Color (0, 0, 0);
				blacksq_color = new Gdk.Color (200, 200, 200);
				whitesq_color = new Gdk.Color (240, 240, 240);

//                      blacksq_color = new Gdk.Color(210, 60, 0);
//                      whitesq_color = new Gdk.Color(236, 193, 130);

				map = new Gdk.Pixmap (null, width, height,
						      24);
				gc = new Gdk.GC (map);

				figure = new Figure ();
				position = new Position (pos);

				int d = Math.Min (width, height);
				  size = d - (border_padding * 2) -
					(7 * space);
				  size = size / 8;
				  figure.SetSize (size);

				  start_x =
					(width -
					 ((size * 8) + 7 * space)) / 2;
				  start_y = start_x;

				  DrawBackground ();
				  DrawPosition ();
			}

			private void DrawBackground ()
			{

				// Defining the color of the Checks
				int i, j, xcount, ycount;

				  gc.RgbFgColor = border_color;
				  map.DrawRectangle (gc, false,
						     1,
						     1,
						     width - 2, height - 2);

				  map.DrawRectangle (gc, false,
						     start_x - 1,
						     start_y - 1,
						     width - 2 * start_x + 2,
						     height - 2 * start_y +
						     2);

				// Start redrawing the Checkerboard                     
				  xcount = 0;
				  i = start_x;
				while (xcount < 8)
				  {
					  j = start_y;
					  ycount = xcount % 2;	//start with even/odd depending on row
					  while (ycount < 8 + xcount % 2)
					    {
						    if (ycount % 2 != 0)
							    gc.RgbFgColor =
								    whitesq_color;
						    else
							    gc.RgbFgColor =
								    blacksq_color;
						    map.DrawRectangle (gc,
								       true,
								       i, j,
								       size,
								       size);

						    j += size + space;
						    ycount++;
					    }
					  i += size + space;
					  xcount++;
				  }

				return;
			}

			private void DrawPosition ()
			{
				for (int i = 0; i < 8; i++)
				  {
					  for (int j = 0; j < 8; j += 1)
					    {

						    FigureType fig =
							    position.
							    GetFigureAt (i,
									 j);

						    if (fig !=
							FigureType.None)
						      {
							      int x, y;

							      if (!side)
								{
									//White
									x = start_x + i * space + i * size;
									y = start_y + j * space + j * size;
								}
							      else
								{
									//Black
									x = start_x + (7 - i) * (space + size);
									y = start_y + (7 - j) * (space + size);
								}

							      gc.RgbFgColor =
								      blacksq_color;
							      map.DrawPixbuf
								      (gc,
								       figure.
								       GetPixbuf
								       (fig),
								       0, 0,
								       x, y,
								       size,
								       size,
								       Gdk.
								       RgbDither.
								       None,
								       0, 0);
						      }
					    }
				  }


				return;
			}

		}
	}
}
