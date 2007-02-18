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

using System;
using System.Collections;
using Gtk;
using Gdk;
using System.Runtime.InteropServices;
using System.Reflection;
using Cairo;

namespace CsBoard
{
	public class CairoBoard:Gtk.DrawingArea
	{

		protected enum MoveStage
		{
			Clear,
			Start,
			Drag,
			Animate,
			Done,
			SetPosition
		};

		protected class MoveInfo
		{

			public MoveStage stage;

			public Point start;
			public Point end;
			public Point drag;
			public Point cursor;

			public bool cursorVisible = false;

			public MoveInfo ()
			{
				start = new Point ();
				end = new Point ();
				drag = new Point ();
				cursor = new Point ();
				cursor.Center ();
			}

			public uint animate_timeout_id = 0;
			public ArrayList animate_list;
		};

		// Geometry
		protected int start_x;
		protected int start_y;
		protected int size;
		protected const int space = 2;


		// Figure Renderer
		private Figure figure;
		protected MoveInfo info;

		private FigureManager fm;

		// Position
		protected Position position;

		//Event about move 
		public event BoardMoveHandler MoveEvent;

		public bool side = false;

		public string lastMove = null;
		public string moveHint = null;

		public bool highLightMove = true;
		public bool showCoords = false;
		public bool showMoveHint = false;
		public bool showAnimations = false;

		private Pango.Layout layout;

		private Cairo.Color whiteSqColor, blackSqColor;
		private Cairo.Color backgroundColor, coordColor;

		public CairoBoard (ArrayList pos):base ()
		{
			figure = new Figure ();
			fm = new FigureManager ();
			position = new Position (pos);
			info = new MoveInfo ();

			whiteSqColor = new Cairo.Color (1, 1, 1, 1);
			blackSqColor = new Cairo.Color (0.9, 0.8, 0.95, 1);
			backgroundColor = new Cairo.Color (1, 0.95, 0.95, 1);
			coordColor = new Cairo.Color (0.3, 0.1, 0.1, 1);

			layout = new Pango.Layout (PangoContext);
		}

		///////////////////////////////////////////////////////////
		//
		//  Drawing methods
		//
		////////////////////////////////////////////////////////////
		protected override bool OnConfigureEvent (Gdk.
							  EventConfigure evnt)
		{
			int width = Allocation.Width;
			int height = Allocation.Height;

			int d = Math.Min (width, height) / 2;
			size = (10 * (d - 3 * space)) / 42 - 1;
			figure.SetSize (size);
			fm.SetSize (size);

			start_x = width / 2 - 4 * size - 3 * space;
			start_y = height / 2 - 4 * size - 3 * space;

			layout.FontDescription =
				Pango.FontDescription.FromString ("bold " +
								  (size /
								   6).
								  ToString
								  ());

			return false;
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			// HACK: generate one more expose event by QueueDraw
			// TODO: Avoid this hack. Find a better way
			Cairo.Context cairo =
				Gdk.CairoHelper.Create (evnt.Window);
			cairo.Rectangle (evnt.Area.X, evnt.Area.Y,
					 evnt.Area.Width, evnt.Area.Height);
			cairo.Clip ();

			DrawBackground (cairo);

			if (showMoveHint
			    && (info.stage == MoveStage.Drag
				|| info.stage == MoveStage.Start))
			  {
				  DrawMoveHint (cairo);
			  }

			if (showCoords)
			  {
				  DrawCoords (cairo);
			  }

			if (info.stage != MoveStage.SetPosition)
			  {
				  DrawPosition (cairo);
			  }
			else
			  {
				  DrawAnimate (cairo);
			  }

			if (highLightMove
			    && info.stage != MoveStage.SetPosition)
			  {
				  DrawLastMove (cairo);
			  }

			DrawMove (cairo);

			if (info.stage == MoveStage.Drag)
			  {
				  DrawDrag (cairo);
			  }

			return false;
		}

		private void DrawMoveHint (Cairo.Context cairo)
		{

			string letters = "abcdefgh";
			string numbers = "87654321";

			if (moveHint == null)
				return;

			for (int k = 0; k < moveHint.Length; k += 3)
			  {
				  int i = -1, j = -1;

				  i = letters.IndexOf (moveHint[k]);
				  j = numbers.IndexOf (moveHint[k + 1]);

				  if (i < 0 && j < 0)
					  continue;

				  if (side)
				    {
					    i = 7 - i;
					    j = 7 - j;
				    }

				  int x = start_x + i * (space + size);
				  int y = start_y + j * (space + size);

				  cairo.Color = backgroundColor;
				  cairo.Rectangle (x, y, size, size);
				  cairo.Fill ();
			  }

		}

		private void DrawCoords (Cairo.Context cairo)
		{

			cairo.Color = coordColor;
			cairo.Rectangle (start_x - space - size / 5,
					 start_y - space - size / 5,
					 (size + space) * 8 +
					 2 * size / 5,
					 (size + space) * 8 + 2 * size / 5);
			cairo.Stroke ();

			string letters = "abcdefgh";

			cairo.Color = new Cairo.Color (0, 0, 0);
			double scale = Pango.Scale.PangoScale;
			for (int i = 0; i < 8; i++)
			  {
				  layout.SetText ((letters[i]).ToString ());
				  Pango.Rectangle logical, ink;
				  layout.GetExtents (out ink, out logical);
				  int width =
					  (int) Math.Round (logical.Width /
							    scale);
				  int height =
					  (int) Math.Round (logical.Height /
							    scale);
				  cairo.MoveTo (start_x + size / 2 +
						(size + space) * i -
						(width / 4),
						start_y - size / 5 -
						2 * space + height - 4);
				  cairo.ShowText (letters[i].ToString ());
				  cairo.MoveTo (start_x + size / 2 +
						(size + space) * i -
						(width / 4),
						start_y + (size + space) * 8 -
						2 * space + height - 4);
				  cairo.ShowText (letters[i].ToString ());
			  }

			string numbers = "12345678";

			if (!side)
				numbers = "87654321";

			for (int i = 0; i < 8; i++)
			  {
				  layout.SetText ((numbers[i]).ToString ());
				  Pango.Rectangle logical, ink;
				  layout.GetExtents (out ink, out logical);
				  //int width = (int) Math.Round(logical.Width / scale);
				  int height =
					  (int) Math.Round (logical.Height /
							    scale);
				  cairo.MoveTo (start_x - size / 5,
						start_y + size / 2 + (size +
								      space) *
						i + (height / 4));
				  cairo.ShowText (numbers[i].ToString ());
				  cairo.MoveTo (start_x + (size + space) * 8,
						start_y + size / 2 + (size +
								      space) *
						i + (height / 4));
				  cairo.ShowText (numbers[i].ToString ());
			  }
		}

		protected virtual void DrawLastMove (Cairo.Context cairo)
		{
			int x1, y1, x2, y2;
			GetCoordinates (SrcRank, SrcFile, out x1,
					out y1);
			GetCoordinates (DestRank, DestFile, out x2,
					out y2);
			// gc.RgbFgColor = new Gdk.Color (128, 128, 240);
			if(info.stage == MoveStage.Clear || info.stage == MoveStage.Done)
				DrawArrow (cairo, x1, y1, x2, y2, size, true);
		}

		private void DrawDrag (Cairo.Context cairo)
		{
			DrawPiece (cairo, position.takenFig,
				   info.drag.x - size / 2,
				   info.drag.y - size / 2, size);
		}

		private void DrawBackground (Cairo.Context cairo)
		{

			// Defining the color of the Checks
			int i, j, xcount, ycount;
			cairo.Color = backgroundColor;
			cairo.Rectangle (start_x - space,
					 start_y - space,
					 (size + space) * 8 + space,
					 (size + space) * 8 + space);
			cairo.Stroke ();

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
					      {
						      cairo.Color =
							      blackSqColor;
					      }
					    else
					      {
						      cairo.Color =
							      whiteSqColor;
					      }
					    cairo.Rectangle (i, j,
							     size, size);
					    cairo.Fill ();

					    j += size + space;
					    ycount++;
				    }
				  i += size + space;
				  xcount++;
			  }

			return;

		}

		private void DrawAnimate (Cairo.Context cairo)
		{
			if (info.stage != MoveStage.SetPosition)
			  {
				  return;
			  }

			for (int i = 0; i < info.animate_list.Count; i++)
			  {
				  int x_from, y_from, x_to, y_to, x, y;

				  AnimationTaskItem item =
					  (AnimationTaskItem) info.
					  animate_list[i];

				  if (!side)
				    {
					    //White
					    x_from = start_x +
						    item.from.x * (space +
								   size);
					    y_from = start_y +
						    item.from.y * (space +
								   size);

					    x_to = start_x +
						    item.to.x * (space +
								 size);
					    y_to = start_y +
						    item.to.y * (space +
								 size);
				    }
				  else
				    {
					    //Black
					    x_from = start_x + (7 -
								item.from.x) *
						    (space + size);
					    y_from = start_y + (7 -
								item.from.y) *
						    (space + size);

					    x_to = start_x + (7 -
							      item.to.x) *
						    (space + size);
					    y_to = start_y + (7 -
							      item.to.y) *
						    (space + size);
				    }

				  x = x_from +
					  (int) ((x_to -
						  x_from) * item.progress);
				  y = y_from +
					  (int) ((y_to -
						  y_from) * item.progress);

				  DrawPiece (cairo, item.fig, x, y, size);
			  }
			return;
		}

		private void DrawPosition (Cairo.Context cairo)
		{
			for (int i = 0; i < 8; i++)
			  {
				  for (int j = 0; j < 8; j += 1)
				    {

					    FigureType fig =
						    position.GetFigureAt (i,
									  j);

					    if (fig != FigureType.None)
					      {
						      int x, y;

						      if (!side)
							{
								//White
								x = start_x +
									i *
									space
									+
									i *
									size;
								y = start_y +
									j *
									space
									+
									j *
									size;
							}
						      else
							{
								//Black
								x = start_x +
									(7 -
									 i) *
									(space
									 +
									 size);
								y = start_y +
									(7 -
									 j) *
									(space
									 +
									 size);
							}

						      DrawPiece (cairo, fig,
								 x, y, size);
					      }
				    }
			  }


			return;
		}

		private void DrawPiece (Cairo.Context cairo, FigureType fig,
					int x, int y, int size)
		{
			DrawPiece (cairo, fm, fig, x, y, size);
		}

		public static void DrawPiece (Cairo.Context cairo,
					      FigureManager fm,
					      FigureType fig, int x, int y,
					      int size)
		{
			Cairo.Color fill = new Cairo.Color (0, 0, 0, 1);

			ArrayList list = fm.GetPoints (fig);

			for (int i = 0; i < list.Count; i++)
			  {
				  SvgInfo info = (SvgInfo) list[i];
				  if (info.cmd == 'M')
				    {
					    cairo.MoveTo (x + info.x,
							  y + info.y);
					    continue;
				    }
				  else if (info.cmd == 'L')
				    {
					    cairo.LineTo (x + info.x,
							  y + info.y);
					    continue;
				    }
				  else if (info.cmd == 'z')
				    {
					    continue;
				    }

				  SvgInfo info2 = (SvgInfo) list[++i];
				  SvgInfo info3 = (SvgInfo) list[++i];

				  cairo.CurveTo (x + info.x, y + info.y,
						 x + info2.x, y + info2.y,
						 x + info3.x, y + info3.y);
			  }
			cairo.Color = fill;
			cairo.Fill ();
		}

		private void DrawMove (Cairo.Context cairo)
		{

			if (info.stage == MoveStage.Start)
			  {
				  int x = start_x +
					  info.start.x * (space + size);
				  int y = start_y +
					  info.start.y * (space + size);

				  cairo.Color = backgroundColor;
				  cairo.Rectangle (x, y, size, size);
				  cairo.Stroke ();
			  }

			if (info.cursorVisible)
			  {
				  int x = start_x +
					  info.cursor.x * (space + size);
				  int y = start_y +
					  info.cursor.y * (space + size);

				  cairo.Color = backgroundColor;
				  cairo.Rectangle (x, y, size, size);
				  cairo.Stroke ();
			  }

			return;
		}

		/////////////////////////////////////////////////////
		///
		/// Actual moving methods
		///
		////////////////////////////////////////////////////

		protected void Move (bool explicitly)
		{

			if (info.stage != MoveStage.Done)
			  {
				  return;
			  }


			if (side)
			  {
				  info.start.x = 7 - info.start.x;
				  info.start.y = 7 - info.start.y;
				  info.end.x = 7 - info.end.x;
				  info.end.y = 7 - info.end.y;
			  }


			char newFigure = ' ';

			position.Move (info.start, info.end, ref newFigure,
				       explicitly);

			string letter = "abcdefgh";
			string move = string.Format ("{0}{1}{2}{3}",
						     letter[info.start.x],
						     8 - info.start.y,
						     letter[info.end.x],
						     8 - info.end.y);

			if (newFigure != ' ')
			  {
				  move = move + newFigure;
			  }

			if (MoveEvent != null)
				MoveEvent (move);
		}

		public virtual void Move (int src_rank, int src_file,
					  int dest_rank, int dest_file,
					  char promotion_type)
		{
			SrcFile = src_file;	// file from left
			SrcRank = src_rank;
			DestFile = dest_file;
			DestRank = dest_rank;
			info.stage = MoveStage.Done;
			Move (true);
		}

		int SrcRank {
			get {
				return 7 - info.start.y;
			}
			set {
				info.start.y = 7 - value;
			}
		}
		int SrcFile {
			get {
				return info.start.x;
			}
			set {
				info.start.x = value;
			}
		}
		int DestRank {
			get {
				return 7 - info.end.y;
			}
			set {
				info.end.y = 7 - value;
			}
		}
		int DestFile {
			get {
				return info.end.x;
			}
			set {
				info.end.x = value;
			}
		}

		public void SetPosition (ArrayList pos)
		{
			info.cursorVisible = false;
			lastMove = null;

			if (showAnimations)
			  {
				  info.stage = MoveStage.SetPosition;
				  if (info.animate_timeout_id > 0)
				    {
					    GLib.Source.Remove (info.
								animate_timeout_id);
				    }
				  info.animate_timeout_id =
					  GLib.Timeout.Add (100,
							    new GLib.
							    TimeoutHandler
							    (on_animate_timeout));
				  info.animate_list =
					  position.SetPositionAnimate (pos);
			  }
			else
			  {
				  info.stage = MoveStage.Clear;
				  position.SetPosition (pos);
				  QueueDraw ();
			  }
		}

		private bool on_animate_timeout ()
		{

			bool task_finished = true;

			for (int i = 0; i < info.animate_list.Count; i++)
			  {
				  AnimationTaskItem item =
					  (AnimationTaskItem) info.
					  animate_list[i];
				  if (item.progress < 0.95)
				    {
					    item.progress += 0.2;
					    task_finished = false;
				    }
			  }


			if (task_finished)
			  {
				  info.stage = MoveStage.Clear;
				  info.animate_timeout_id = 0;
			  }

			QueueDraw ();

			return !task_finished;
		}

		public void SetMoveInfo (int sr, int sf, int dr,
					 int df)
		{
			SrcRank = sr;
			SrcFile = sf;
			DestRank = dr;
			DestFile = df;
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
				info.start.x = info.start.y = info.end.x = info.end.y =
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
	}
}
