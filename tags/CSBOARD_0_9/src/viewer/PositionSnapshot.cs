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
using System;
using CsBoard;
using Pango;

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
			private int board_width;
			private int board_height;

			private int full_width, full_height;
			private int fontwidth, fontheight;

			int board_line_thickness = 1;
			int border_line_thickness = 2;

			private Pango.Layout[] layoutx, layouty;
			private int padding = 2;

			public bool side = false;

			// Figure Renderer
			private static Figure figure;

			// Position
			private Position position;

			  Gdk.GC gc;

			  Gdk.Color border_color, blacksq_color,
				whitesq_color, background_color,
				foreground_color, arrow_color;


			private Gdk.Pixmap map;
			public Gdk.Pixmap Pixmap
			{
				get
				{
					return map;
				}
			}

			bool showCoords = false;

			public PositionSnapshot (ArrayList pos, int width,
						 int height)
			{
				Gtk.Window win =
					new Gtk.Window (Gtk.WindowType.
							Toplevel);
				win.Realize ();
				map = new Gdk.Pixmap (win.GdkWindow, width,
						      height);
				gc = new Gdk.GC (map);

				FontDescription fontdesc =
					GetFontDesc (width, height);
				  GetCoordLayoutDetails (win.PangoContext,
							 fontdesc);

				  border_color = new Gdk.Color (0, 0, 0);
//                              blacksq_color = new Gdk.Color (200, 200, 200);
//                              whitesq_color = new Gdk.Color (240, 240, 240);
				  blacksq_color =
					new Gdk.Color (250, 120, 32);
				  whitesq_color =
					new Gdk.Color (255, 250, 170);
				  background_color =
					new Gdk.Color (255, 255, 255);
				  foreground_color = new Gdk.Color (0, 0, 0);
//                                arrow_color = new Gdk.Color (159, 148, 249);
				  arrow_color = new Gdk.Color (117, 6, 6);

//                      blacksq_color = new Gdk.Color(210, 60, 0);
//                      whitesq_color = new Gdk.Color(236, 193, 130);
				// outer box, coord, inner box
				  ComputeSizes (width, height);

				if (figure == null)
					figure = new Figure ();
				  position = new Position (pos);

				  figure.SetSize (size);

				  DrawBackground ();
				  DrawPosition ();
			}

			private FontDescription GetFontDesc (int width,
							     int height)
			{
				int d = Math.Min (width, height);
				int size =
					d - (border_padding * 2) -
					(7 * space);
				  size = size / 8;

				  return Pango.FontDescription.
					FromString ("monospace " +
						    (size / 6).ToString ());
			}

			private void ComputeSizes (int width, int height)
			{
				full_width = width;
				full_height = height;

				int x_between_border_and_board =
					border_line_thickness + padding +
					fontwidth + padding +
					board_line_thickness;
				int y_between_border_and_board =
					border_line_thickness + padding +
					fontheight + padding +
					board_line_thickness;
				int tmpwidth =
					width -
					2 * x_between_border_and_board;
				int tmpheight =
					height -
					2 * y_between_border_and_board;
				int d = Math.Min (tmpwidth, tmpheight);
				size = d - (border_padding * 2) - (7 * space);
				size = size / 8;

				board_width = board_height =
					size * 8 + 7 * space;

				start_x = (full_width - board_width) / 2;
				start_y = (full_height - board_height) / 2;
			}

			private void GetCoordLayoutDetails (Pango.
							    Context context,
							    FontDescription
							    font)
			{
				if (!showCoords)
				  {
					  fontwidth = 0;
					  fontheight = 0;
					  return;
				  }
				layoutx = new Pango.Layout[8];
				layouty = new Pango.Layout[8];
				char chx = 'a';
				char chy = '1';
				fontwidth = 0;
				fontheight = 0;
				for (int i = 0; i < layoutx.Length;
				     i++, chx++, chy++)
				  {
					  Pango.Layout layout;
					  layoutx[i] = layout =
						  new Pango.Layout (context);
					  layout.FontDescription = font;
					  layout.SetText (chx.ToString ());
					  int w, h;
					  layout.GetSize (out w, out h);
					  h = (int) Math.Round (h /
								Pango.Scale.
								PangoScale);
					  if (h > fontheight)
						  fontheight = h;

					  layouty[i] = layout =
						  new Pango.Layout (context);
					  layout.FontDescription = font;
					  layout.SetText (chy.ToString ());
					  layout.GetSize (out w, out h);
					  w = (int) Math.Round (w /
								Pango.Scale.
								PangoScale);
					  if (w > fontwidth)
						  fontwidth = w;
				  }
			}

			private void DrawCoords ()
			{
				if (!showCoords)
					return;
				DrawRanks ();
				DrawFiles ();
			}

			private void DrawFiles ()
			{
				int x, y1, y2;
				y1 = start_y - board_line_thickness -
					padding - fontheight;
				y2 = start_y + board_height +
					board_line_thickness + padding;
				gc.RgbFgColor = foreground_color;
				for (int i = 0; i < layoutx.Length; i++)
				  {
					  int cellx = i * size;
					  if (i > 0)
						  cellx += (i - 1) * space;

					  x = start_x + cellx;
					  x += ((size - fontwidth) / 2);
					  int w, h;
					  layoutx[i].GetSize (out w, out h);
					  h = (int) Math.Round (h /
								Pango.Scale.
								PangoScale);
					  map.DrawLayout (gc, x,
							  y1 + fontheight - h,
							  layoutx[i]);
					  map.DrawLayout (gc, x,
							  y2 + fontheight - h,
							  layoutx[i]);
				  }
			}

			private void DrawRanks ()
			{
				int y, x1, x2;
				x1 = start_x - board_line_thickness -
					padding - fontwidth;
				x2 = start_x + board_width +
					board_line_thickness + padding;
				gc.RgbFgColor = foreground_color;
				for (int i = 0; i < layouty.Length; i++)
				  {
					  int celly = i * size;
					  if (i > 0)
						  celly += (i - 1) * space;

					  y = start_y + celly;
					  y += ((size - fontheight) / 2);
					  map.DrawLayout (gc, x1, y,
							  layouty[i]);
					  map.DrawLayout (gc, x2, y,
							  layouty[i]);
				  }
			}

			// draw
			private static void DrawRect (Gdk.GC gc,
						      Gdk.Pixmap map, int x,
						      int y, int width,
						      int height,
						      int line_thickness)
			{
				if (line_thickness == 1)
				  {
					  map.DrawRectangle (gc, false, x, y,
							     width, height);
					  return;
				  }
				// horizontal lines
				map.DrawRectangle (gc, true,
						   x,
						   y, width, line_thickness);
				map.DrawRectangle (gc, true,
						   x,
						   y + height -
						   line_thickness, width,
						   line_thickness);

				// vertical lines
				map.DrawRectangle (gc, true,
						   x,
						   y, line_thickness, height);
				map.DrawRectangle (gc, true,
						   x + width - line_thickness,
						   y, line_thickness, height);
			}

			private void DrawBackground ()
			{
				gc.RgbFgColor = new Gdk.Color (255, 255, 255);
				map.DrawRectangle (gc, true, 0, 0, full_width,
						   full_height);
				// Defining the color of the Checks
				int i, j, xcount, ycount;

				int x_between_border_and_board =
					board_line_thickness + padding +
					fontwidth + padding +
					border_line_thickness;
				int y_between_border_and_board =
					board_line_thickness + padding +
					fontheight + padding +
					border_line_thickness;
				int x = start_x - x_between_border_and_board;
				int y = start_y - y_between_border_and_board;
				int w = board_width + 2 * (x_between_border_and_board);	// 1 for the board border
				int h = board_height +
					2 * (y_between_border_and_board);

				gc.RgbFgColor = background_color;
				map.DrawRectangle (gc, true, x, y, w, h);

				gc.RgbFgColor = border_color;
				DrawRect (gc, map, x, y, w, h,
					  border_line_thickness);
				DrawRect (gc, map,
					  start_x - board_line_thickness,
					  start_y - board_line_thickness,
					  board_width +
					  2 * board_line_thickness,
					  board_height +
					  2 * board_line_thickness,
					  board_line_thickness);

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
								    blacksq_color;
						    else
							    gc.RgbFgColor =
								    whitesq_color;
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

				DrawCoords ();

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

							      Gdk.Pixbuf
								      pixbuf =
								      figure.
								      GetPixbuf
								      (fig);
							      gc.RgbFgColor =
								      blacksq_color;
							      map.DrawPixbuf
								      (gc,
								       pixbuf,
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

			public void DrawMove (int rank1, int file1, int rank2,
					      int file2)
			{
				int x1, y1, x2, y2;
				GetCoords (rank1, file1, out x1, out y1);
				GetCoords (rank2, file2, out x2, out y2);

				gc.RgbFgColor = arrow_color;
				ViewerBoard.DrawArrow (map, gc,
						       x1 + (size / 2),
						       y1 + (size / 2),
						       x2 + (size / 2),
						       y2 + (size / 2),
						       size, false);
			}

			private void GetCoords (int rank, int file, out int x,
						out int y)
			{
				x = start_x + file * (size + space);
				y = start_y + (7 - rank) * (size + space);
			}
		}
	}
}
