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
using Cairo;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public interface IGameInfo
		{
			int GameId
			{
				get;
			}

			int Rating
			{
				get;
			}

			int Time
			{
				get;
			}

			int Increment
			{
				get;
			}

			bool Rated
			{
				get;
			}

			bool Computer
			{
				get;
			}
		}

		struct GraphPoint
		{
			public IGameInfo info;
			public int x;
			public int y;
			public GraphPoint (IGameInfo i)
			{
				info = i;
				x = -1;
				y = -1;
			}

			public static GraphPoint Zero = new GraphPoint (null);
		}

		public delegate void GameFocusedHandler (object o,
							 IGameInfo info);
		public delegate void GameClickedHandler (object o,
							 IGameInfo info);

		public class Graph:Gtk.DrawingArea
		{
			private Pango.Layout layout;

			private Cairo.Color backgroundColor, coordColor,
				ratedColor, unratedColor, pointBorderColor,
				computerColor;
			private int start_x, start_y;
			  GraphMark[] x_marks;
			  GraphMark[] y_marks;

			ArrayList points;
			int point_size = 10;
			int inter_point_gap = 1;
			  Gdk.Cursor handCursor, regularCursor;
			public event GameFocusedHandler GameFocusedEvent;
			public event GameClickedHandler GameClickedEvent;

			Pixmap pixmap;

			public Graph ():base ()
			{
				handCursor =
					new Gdk.Cursor (Gdk.CursorType.Hand2);
				regularCursor =
					new Gdk.Cursor (Gdk.CursorType.Arrow);

				Events = EventMask.ButtonPressMask
					| EventMask.ButtonReleaseMask
					| EventMask.PointerMotionMask
					| EventMask.ButtonMotionMask;

				backgroundColor =
					new Cairo.Color (1, 1, 1, 1);
				coordColor =
					new Cairo.Color (0.5, 0.1, 0.1, 1);
				ratedColor =
					new Cairo.Color (0.9, 0.6, 0.7, 1);
				computerColor =
					new Cairo.Color (0.7, 0.6, 0.9, 1);
				unratedColor =
					new Cairo.Color (0.6, 0.9, 0.7, 1);
				pointBorderColor =
					new Cairo.Color (0, 0, 0, 1);

				layout = new Pango.Layout (PangoContext);
				x_marks = new GraphMark[3];
				y_marks = new GraphMark[2];

				points = new ArrayList ();
				ConfigureEvent += OnConfigured;
				WidgetEventAfter += OnEventAfter;
				MotionNotifyEvent += OnMotionNotify;
				ExposeEvent += OnExpose;
				//CanFocus = true;
			}

			void __AddGameInfo (IGameInfo info)
			{
				int x, y;
				  try
				{
					FindSlotToPlot (info, out x, out y);
				}
				catch (Exception)
				{
					x = -1;
					y = -1;
				}
				GraphPoint point = new GraphPoint (info);
				point.x = x;
				point.y = y;
				points.Add (point);
			}

			public void AddGameInfo (IGameInfo info)
			{
				__AddGameInfo (info);
				QueueDraw ();
			}

			public void RemoveGame (int id)
			{
				int i = 0;
				bool found = false;
				foreach (GraphPoint point in points)
				{
					if (point.info.GameId == id)
					  {
						  found = true;
						  break;
					  }
					i++;
				}
				if (found)
				  {
					  points.RemoveAt (i);
					  QueueDraw ();
				  }
			}

			public void Clear ()
			{
				points.Clear ();
				QueueDraw ();
			}

			private void PlotPoint (Cairo.Context cairo,
						GraphPoint point)
			{
				if (point.x < 0 || point.y < 0)
					return;
				int size = point_size - inter_point_gap;
				int x = start_x + point.x;
				int y = start_y - point.y;

				if (point.info.Rated)
				  {
					  cairo.Color = pointBorderColor;
					  cairo.Arc (x + size / 2, y + size / 2, size / 2, 0, 44.0 / 7.0);	// 2 pi = 360 degrees
					  cairo.Stroke ();
					  cairo.Color =
						  point.info.
						  Computer ? computerColor :
						  ratedColor;
					  cairo.Arc (x + size / 2, y + size / 2, size / 2, 0, 44.0 / 7.0);	// 2 pi = 360 degrees
					  cairo.Fill ();
				  }
				else
				  {
					  cairo.Color = pointBorderColor;
					  cairo.Rectangle (x, y, size, size);
					  cairo.Stroke ();
					  cairo.Color =
						  point.info.
						  Computer ? computerColor :
						  unratedColor;
					  cairo.Rectangle (x, y, size, size);
					  cairo.Fill ();
				  }
			}

			bool IsPointWithinArea (int x, int y)
			{
				if (x < 0
				    || (x + point_size) > graph_area_width)
					return false;
				if (y < point_size || y > graph_area_height)
					return false;
				return true;
			}

			bool FindASlotAroundThis (int center_x, int center_y,
						  int side_size, out int x,
						  out int y)
			{
				int x1;
				int y1;

				int points_within_area = 0;
				int size = point_size;

				// top horizontal
				x1 = center_x - (side_size / 2) * size;
				y1 = center_y + (side_size / 2) * size;
				for (int i = 0; i < side_size; i++)
				  {
					  if (!IsPointWithinArea (x1, y1))
						  continue;
					  points_within_area++;
					  GraphPoint matching;
					  if (!OverlappingPoint
					      (x1, y1, point_size,
					       out matching))
					    {
						    // found a free slot!
						    x = x1;
						    y = y1;
						    return true;
					    }
					  x1 += size;
				  }

				// bottom horizontal
				x1 = center_x - (side_size / 2) * size;
				y1 = center_y - (side_size / 2) * size;
				for (int i = 0; i < side_size; i++)
				  {
					  if (!IsPointWithinArea (x1, y1))
						  continue;
					  points_within_area++;
					  GraphPoint matching;
					  if (!OverlappingPoint
					      (x1, y1, point_size,
					       out matching))
					    {
						    // found a free slot!
						    x = x1;
						    y = y1;
						    return true;
					    }
					  x1 += size;
				  }

				// left vertical
				x1 = center_x - (side_size / 2) * size;
				y1 = center_y - (side_size / 2) * size;
				// top horizontal
				for (int i = 0; i < side_size; i++)
				  {
					  if (!IsPointWithinArea (x1, y1))
						  continue;
					  points_within_area++;
					  GraphPoint matching;
					  if (!OverlappingPoint
					      (x1, y1, point_size,
					       out matching))
					    {
						    // found a free slot!
						    x = x1;
						    y = y1;
						    return true;
					    }
					  y1 += size;
				  }

				// right vertical
				x1 = center_x + (side_size / 2) * size;
				y1 = center_y - (side_size / 2) * size;
				// top horizontal
				for (int i = 0; i < side_size; i++)
				  {
					  if (!IsPointWithinArea (x1, y1))
						  continue;
					  points_within_area++;
					  GraphPoint matching;
					  if (!OverlappingPoint
					      (x1, y1, point_size,
					       out matching))
					    {
						    // found a free slot!
						    x = x1;
						    y = y1;
						    return true;
					    }
					  y1 += size;
				  }

				x = -1;
				y = -1;
				if (points_within_area == 0)
				  {
					  throw new
						  Exception
						  ("Graph area is full!");
				  }

				return false;
			}

			void FindSlotToPlot (IGameInfo info, out int x,
					     out int y)
			{
				x = GetXPos (info);
				y = GetYPos (info.Rating);
				if (IsPointWithinArea (x, y))
				  {
					  GraphPoint matching =
						  GraphPoint.Zero;
					  if (!OverlappingPoint
					      (x, y, point_size,
					       out matching))
						  return;
				  }

				bool found = false;
				int side_width = 3;
				int x1, y1;
				while (!found)
				  {
					  if (FindASlotAroundThis
					      (x, y, side_width, out x1,
					       out y1))
					    {
						    x = x1;
						    y = y1;
						    return;
					    }
					  side_width += 2;
				  }
			}

			bool PointInsideRect (int x1, int y1, int x2, int y2,
					      int width,
					      bool compare_downwards)
			{
				if (compare_downwards)
				  {
					  // This is when (x2, y2) is the top left corner
					  if (x1 >= x2 && x1 < x2 + width &&
					      y1 >= (y2 - width) && y1 < y2)
						  return true;
				  }
				else
				  {
					  // This is when (x2, y2) is the bottom left corner
					  if (x1 >= x2 && x1 < x2 + width &&
					      y1 >= y2 && y1 < y2 + width)
						  return true;
				  }
				return false;
			}

			bool PointInsideRect (int x1, int y1, int x2, int y2,
					      int width)
			{
				return PointInsideRect (x1, y1, x2, y2, width,
							false);
			}

			bool IsOverlapping (int x1, int y1, int width, int x2,
					    int y2)
			{
				if (PointInsideRect (x1, y1, x2, y2, width))
					return true;
				if (PointInsideRect
				    (x1, y1 + point_size, x2, y2, width))
					return true;
				if (PointInsideRect
				    (x1 + point_size, y1, x2, y2, width))
					return true;
				if (PointInsideRect
				    (x1 + point_size, y1 + point_size, x2,
				     y2, width))
					return true;
				return false;
			}

			bool OverlappingPoint (int x, int y, int width,
					       out GraphPoint matching)
			{
				foreach (GraphPoint point in points)
				{
					if (IsOverlapping
					    (x, y, width, point.x, point.y))
					  {
						  matching = point;
						  return true;
					  }
				}

				matching = GraphPoint.Zero;
				return false;
			}

			bool MatchingPoint (int x, int y, int width,
					    out GraphPoint matching)
			{
				foreach (GraphPoint point in points)
				{
					if (PointInsideRect
					    (x, y, point.x, point.y, width,
					     true))
					  {
						  matching = point;
						  return true;
					  }
				}

				matching = GraphPoint.Zero;
				return false;
			}

			void OnConfigured (object o, ConfigureEventArgs args)
			{
				pixmap = new Pixmap (args.Event.Window,
						     Allocation.Width,
						     Allocation.Height);
				ArrayList oldlist = points;
				points = new ArrayList ();

				ComputePositions ();
				foreach (GraphPoint point in oldlist)
				{
					__AddGameInfo (point.info);
				}
			}

			int offset_x, offset_y;
			int graph_area_width, graph_area_height;
			int line_thickness;

			int max_rating = 2500;
			int min_rating = 900;
			int offset = 10;

			private void ComputePositions ()
			{
				int width = Allocation.Width;
				int height = Allocation.Height;

				x_marks[0].name =
					Catalog.GetString ("Lightning");
				x_marks[1].name = Catalog.GetString ("Blitz");
				x_marks[2].name =
					Catalog.GetString ("Standard");

				int y_name_width = 0;
				int x_name_height = 0;

				double scale = Pango.Scale.PangoScale;

				for (int i = 0; i < x_marks.Length; i++)
				  {
					  layout.SetText (x_marks[i].name);
					  Pango.Rectangle logical, ink;
					  layout.GetExtents (out ink,
							     out logical);
					  int txt_height =
						  (int) Math.Round (logical.
								    Height /
								    scale);
					  if (txt_height > x_name_height)
						  x_name_height = txt_height;
				  }

				int rating;
				rating = 1500;
				y_marks[0].name = rating.ToString ();
				rating = 2000;
				y_marks[1].name = rating.ToString ();
				for (int i = 0; i < y_marks.Length; i++)
				  {
					  layout.SetText (y_marks[i].name);
					  Pango.Rectangle logical, ink;
					  layout.GetExtents (out ink,
							     out logical);
					  int txt_width =
						  (int) Math.Round (logical.
								    Width /
								    scale);
					  if (txt_width > y_name_width)
						  y_name_width = txt_width;
				  }

				offset_x = offset + y_name_width + offset;
				offset_y = offset + x_name_height + offset;

				start_x = offset_x;
				start_y = height - offset_y;
				start_x = offset_x;
				graph_area_width =
					width - 2 * offset_x - line_thickness;
				graph_area_height =
					height - 2 * offset_y -
					line_thickness;

				// lightning
				x_marks[0].x = 0;
				x_marks[0].y = 0;

				// blitz
				x_marks[1].x =
					(int) Math.Round (graph_area_width *
							  0.15);
				x_marks[1].y = 0;

				// standard
				x_marks[2].x =
					(int) Math.Round (graph_area_width *
							  0.4);
				x_marks[2].y = 0;

				// 1500
				rating = 1500;
				y_marks[0].x = 0;
				y_marks[0].y = GetYPos (rating);

				// 2000
				rating = 2000;
				y_marks[1].x = 0;
				y_marks[1].y = GetYPos (rating);
			}

			int GetYPos (int rating)
			{
				double val = rating;

				int ret =
					(int) Math.
					Round (((val -
						 min_rating) / (max_rating -
								min_rating)) *
					       graph_area_height);
				if (ret >= graph_area_height)
					ret = graph_area_height - 1;
				if (ret < 0)
					ret = 0;
				return ret;
			}

			int GetXPos (IGameInfo info)
			{
				double time =
					(info.Time * 60 +
					 info.Increment * 12) / 60.0;
				// 15% = 3 mins
				int ret =
					(int) Math.Round (graph_area_width *
							  time / 20.0);
				if (ret >= graph_area_width)
					ret = graph_area_width - 1;
				return ret;
			}

			struct GraphMark
			{
				public int x, y;
				public string name;
				public int width, height;
			};

			private void DrawCoords (Cairo.Context cairo)
			{
				cairo.Color = coordColor;
				// x-axis
				cairo.MoveTo (start_x, start_y);
				cairo.LineTo (start_x + graph_area_width,
					      start_y);
				cairo.Stroke ();

				// y-axis
				cairo.MoveTo (start_x, start_y);
				cairo.LineTo (start_x, offset_y);
				cairo.Stroke ();

				int max_height = 0;
				for (int i = 0; i < x_marks.Length; i++)
				  {
					  TextExtents extents =
						  cairo.
						  TextExtents (x_marks[i].
							       name);
					  int height =
						  (int) Math.Round (extents.
								    Height);
					  if (height > max_height)
						  max_height = height;
				  }

				for (int i = 0; i < x_marks.Length; i++)
				  {
					  if (i != 0)
					    {
						    cairo.MoveTo (start_x +
								  x_marks[i].
								  x, start_y);
						    cairo.LineTo (start_x +
								  x_marks[i].
								  x,
								  offset_y);
						    cairo.Stroke ();
					    }

					  cairo.MoveTo (start_x +
							x_marks[i].x,
							start_y + offset +
							max_height);
					  cairo.ShowText (x_marks[i].name);
					  cairo.Stroke ();
				  }

				for (int i = 0; i < y_marks.Length; i++)
				  {
					  TextExtents extents =
						  cairo.
						  TextExtents (y_marks[i].
							       name);
					  int width =
						  (int) Math.Round (extents.
								    Width);
					  int height =
						  (int) Math.Round (extents.
								    Height);

					  cairo.MoveTo (start_x - offset / 2,
							start_y -
							y_marks[i].y);
					  cairo.LineTo (start_x + offset / 2,
							start_y -
							y_marks[i].y);
					  cairo.Stroke ();

					  cairo.MoveTo (start_x - offset -
							width,
							start_y -
							y_marks[i].y +
							height / 2);
					  cairo.ShowText (y_marks[i].name);
					  cairo.Stroke ();
				  }
			}

			private void DrawBackground (Cairo.Context cairo)
			{
				cairo.Color = backgroundColor;
				cairo.Rectangle (0, 0, Allocation.Width,
						 Allocation.Height);
				cairo.Fill ();
			}

			bool hoveringOverPoint = false;
			void OnEventAfter (object sender,
					   WidgetEventAfterArgs args)
			{
				if (args.Event.Type !=
				    Gdk.EventType.ButtonRelease)
					return;

				Gdk.EventButton evt =
					(Gdk.EventButton) args.Event;

				if (evt.Button != 1)
					return;

				int x, y;
				ConvertToGraphCoords ((int) evt.X,
						      (int) evt.Y, out x,
						      out y);
				GraphPoint matching;
				if (MatchingPoint
				    (x, y, point_size - inter_point_gap,
				     out matching))
				  {
					  if (GameClickedEvent != null)
						  GameClickedEvent (this,
								    matching.
								    info);
				  }
			}

			void OnMotionNotify (object sender,
					     MotionNotifyEventArgs args)
			{
				int x, y;
				int px, py;

				GetPointer (out px, out py);
				ConvertToGraphCoords (px, py, out x, out y);
				SetCursorIfAppropriate (x, y);
			}

			// Also update the cursor image if the window becomes visible
			// (e.g. when a window covering it got iconified).
			void OnExpose (object sender, ExposeEventArgs args)
			{
				Cairo.Context cairo =
					Gdk.CairoHelper.Create (pixmap);
				Gdk.Rectangle area = args.Event.Area;
				cairo.Rectangle (area.X, area.Y,
						 area.Width, area.Height);
				cairo.Clip ();

				DrawBackground (cairo);
				DrawCoords (cairo);

				if (graph_area_width < point_size
				    || graph_area_height < point_size)
					return;

				foreach (GraphPoint point in points)
				{
					PlotPoint (cairo, point);
				}

				args.Event.Window.DrawDrawable (Style.WhiteGC,
								pixmap,
								area.X,
								area.Y,
								area.X,
								area.Y,
								area.Width,
								area.Height);

				int wx, wy;
				int x, y;

				GetPointer (out wx, out wy);
				ConvertToGraphCoords (wx, wy, out x, out y);
				SetCursorIfAppropriate (x, y);
			}

			void SetCursorIfAppropriate (int x, int y)
			{
				bool hover = false;
				GraphPoint matching;
				hover = MatchingPoint (x, y,
						       point_size -
						       inter_point_gap,
						       out matching);
				if (hover == hoveringOverPoint)
					return;

				hoveringOverPoint = hover;
				GdkWindow.Cursor =
					hoveringOverPoint ? handCursor :
					regularCursor;
				if (GameFocusedEvent != null)
					GameFocusedEvent (this,
							  hoveringOverPoint ?
							  matching.
							  info : null);
			}

			void ConvertToGraphCoords (int x, int y, out int x1,
						   out int y1)
			{
				x1 = x - start_x;
				y1 = start_y - y;
			}
		}
	}
}
