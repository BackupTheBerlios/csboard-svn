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
			bool showMove = true;
			bool firstTime = true;

			public bool ShowMove
			{
				get
				{
					return showMove;
				}
				set
				{
					showMove = value;
				}
			}

			public CairoViewerBoard (ArrayList pos):base (pos)
			{
				highLightMove = true;
				position.AskForPromotion = false;
			}

			public void SetMoveInfo (int sr, int sf,
						 int dr, int df)
			{
				src_rank = sr;
				src_file = sf;
				dest_rank = dr;
				dest_file = df;
			}

			public override void Move (int sr, int sf, int dr,
						   int df,
						   char promotion_type)
			{
				SetMoveInfo (sr, sf, dr, df);
				base.Move (sr, sf, dr, df, promotion_type);
			}

			protected override void DrawLastMove (Cairo.
							      Context cairo)
			{
				if (!showMove)
					return;
				int x1, y1, x2, y2;
				GetCoordinates (src_rank, src_file, out x1,
						out y1);
				GetCoordinates (dest_rank, dest_file, out x2,
						out y2);
				// gc.RgbFgColor = new Gdk.Color (128, 128, 240);
				DrawArrow (cairo, x1, y1, x2, y2, size, true);
			}

			public void GetCoordinates (int rank, int file,
						    out int x, out int y)
			{
				if (side)
				  {
					  rank = 7 - rank;
					  file = 7 - file;
				  }
				//White
				x = start_x + file * (space + size) +
					size / 2;
				y = start_y + (7 - rank) * (space + size) +
					size / 2;
			}

			public void Reset ()
			{
				src_rank = src_file = dest_rank = dest_file =
					0;
			}

			/* This will draw an arrow from the source point to the destination.
			 * The caller has to resolve the centers of the source and destination squares
			 * and pass them to this.
			 * Instead of drawing the arrow to the center of the destination (which can
			 * overlap with the piece at the destination), the arrow will be drawn in
			 * such a way that only a limited portion of it will be inside the square.
			 * The horizontal lines of the arrow needs to be perpendicular to the direction
			 * of the arrow. To compute the points which are equidistant and at a distance 'alpha'
			 * from a given point (x,y), the following formula is used:
			 * (x + sin * alpha, y - cos * alpha) and (x - sin * alpha, y + cos * alpha)
			 * The sin and cos are the values for the slope of the arrow.
			 * GetPerpendicularCoords is used to get these values.
			 * Another formula is used to find a point on the arrow at a distance 'dist' from a
			 * point (x, y) in the reverse direction. This is used in drawing the edge of the arrow.
			 * The formula used is:
			 * (x - dist * cos, y - dist * sin)
			 */
			public static void DrawArrow (Cairo.Context cairo,
						      int x1,
						      int y1, int x2, int y2,
						      int size, bool filled)
			{
				double len =
					Math.Sqrt ((y2 - y1) * (y2 - y1) +
						   (x2 - x1) * (x2 - x1));
				double sin = (y2 - y1) / len;
				double cos = (x2 - x1) / len;

				int alpha = size / 4;

				double line_portion = 0.75 * size / 2;
				// the tip now touches the end of the square.
				// computing it like this takes care of the direction
				// from which the arrow is coming!
				Gdk.Point tip = new Gdk.Point ((int) Math.
							       Round (x2 -
								      line_portion
								      * cos),
							       (int) Math.
							       Round (y2 -
								      line_portion
								      * sin));
				x2 = tip.X;
				y2 = tip.Y;

				Gdk.Point[]a = new Gdk.Point[2];
				GetPerpendicularCoords (x1, y1, sin, cos,
							alpha, out a[0],
							out a[1]);

				// This is the point where the arrow will start.
				// We need to draw a rectangle from the above point to this.
				// And the a triangle to the final dest.
				double factor = 1.5;
				Gdk.Point p =
					new Gdk.Point ((int) Math.
						       Round (x2 -
							      factor * alpha *
							      cos),
						       (int) Math.Round (y2 -
									 factor
									 *
									 alpha
									 *
									 sin));


				Gdk.Point[]b = new Gdk.Point[2];
				GetPerpendicularCoords (p.X, p.Y, sin, cos,
							alpha, out b[0],
							out b[1]);
				Gdk.Point c, d;
				GetPerpendicularCoords (p.X, p.Y, sin, cos,
							3 * alpha, out c,
							out d);

				Gdk.Point[]points = new Gdk.Point[]
				{
				a[0], a[1],
						b[1], d, tip, c, b[0], a[0]};
				Cairo.Color color =
					new Cairo.Color (0.5, 0.5, 0.8, 0.5);
				cairo.Color = color;
				double orig = cairo.LineWidth;
				double fraction = 5;
				if (alpha > fraction)
					cairo.LineWidth = alpha / fraction;

				for (int i = 0; i < points.Length; i++)
				  {
					  if (i == 0)
						  cairo.MoveTo (points[i].X,
								points[i].Y);
					  if (i == points.Length - 1)
						  cairo.LineTo (points[0].X,
								points[0].Y);
					  else
						  cairo.LineTo (points[i + 1].
								X,
								points[i +
								       1].Y);
				  }
				cairo.FillPreserve ();
				color.A = 0.8;
				cairo.Color = color;
				cairo.Stroke ();
				cairo.LineWidth = orig;
			}

			private static void GetPerpendicularCoords (int x,
								    int y,
								    double
								    sin,
								    double
								    cos,
								    int width,
								    out Gdk.
								    Point p1,
								    out Gdk.
								    Point p2)
			{
				int alpha = width / 2;
				p1 = new Gdk.Point ((int) Math.
						    Round (x + (alpha * sin)),
						    (int) Math.Round (y -
								      (alpha *
								       cos)));
				p2 = new Gdk.Point ((int) Math.
						    Round (x - (alpha * sin)),
						    (int) Math.Round (y +
								      (alpha *
								       cos)));
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
