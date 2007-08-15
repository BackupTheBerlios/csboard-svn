using Gtk;
using System;

namespace CsBoard
{

	public interface IColorProvider
	{
		Cairo.Color WhiteSqColor
		{
			get;
		}

		Cairo.Color BlackSqColor
		{
			get;
		}

		Cairo.Color BackgroundColor
		{
			get;
		}

		Cairo.Color CoordColor
		{
			get;
		}

		Cairo.Color HighlightSqColor
		{
			get;
		}

		Cairo.Color MoveHintColor
		{
			get;
		}

		Cairo.Color ArrowColor
		{
			get;
		}

		Cairo.Color ForegroundColor
		{
			get;
		}
	}

	public class GtkBasedColorProvider:IColorProvider
	{
		Gtk.Widget widget;
		public GtkBasedColorProvider (Gtk.Widget w)
		{
			widget = w;
		}

		public static Cairo.Color GetCairoColor (Gdk.Color gclr,
							 double alpha)
		{
			double red, green, blue;
			red = gclr.Red;
			green = gclr.Green;
			blue = gclr.Blue;
			return new Cairo.Color (red / UInt16.MaxValue,
						green / UInt16.MaxValue,
						blue / UInt16.MaxValue,
						alpha);
		}

		public Cairo.Color WhiteSqColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Prelight],
						      1);
			}
		}

		public Cairo.Color BlackSqColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Active], 1);
			}
		}

		public Cairo.Color BackgroundColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Insensitive],
						      1);
			}
		}
		public Cairo.Color CoordColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Foregrounds[(int)
								  StateType.
								  Normal], 1);
			}
		}
		public Cairo.Color HighlightSqColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Selected],
						      0.7);
			}
		}
		public Cairo.Color MoveHintColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Selected],
						      0.4);
			}
		}
		public Cairo.Color ArrowColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Backgrounds[(int)
								  StateType.
								  Selected],
						      0.5);
			}
		}

		public Cairo.Color ForegroundColor
		{
			get
			{
				return GetCairoColor (widget.Style.
						      Foregrounds[(int)
								  StateType.
								  Normal], 1);
			}
		}
	}

	public class CustomColorProvider:IColorProvider
	{
		Cairo.Color whiteSqColor, blackSqColor, backgroundColor,
			coordColor, highlightSqColor, moveHintColor,
			foregroundColor, arrowColor;
		public CustomColorProvider ()
		{
			whiteSqColor = new Cairo.Color (1, 1, 1, 1);
			blackSqColor = new Cairo.Color (0.9, 0.8, 0.95, 1);
			backgroundColor = new Cairo.Color (1, 0.95, 0.95, 1);
			coordColor = new Cairo.Color (0.3, 0.1, 0.1, 1);
			highlightSqColor = new Cairo.Color (1, 0, 0, 0.7);
			moveHintColor = new Cairo.Color (1, 0, 0, 0.2);
			arrowColor = new Cairo.Color (0.5, 0.5, 0.8, 0.5);
			foregroundColor = new Cairo.Color (0, 0, 0, 1);
		}

		public Cairo.Color WhiteSqColor
		{
			get
			{
				return whiteSqColor;
			}
		}

		public Cairo.Color BlackSqColor
		{
			get
			{
				return blackSqColor;
			}
		}

		public Cairo.Color BackgroundColor
		{
			get
			{
				return backgroundColor;
			}
		}

		public Cairo.Color CoordColor
		{
			get
			{
				return coordColor;
			}
		}

		public Cairo.Color HighlightSqColor
		{
			get
			{
				return highlightSqColor;
			}
		}

		public Cairo.Color MoveHintColor
		{
			get
			{
				return moveHintColor;
			}
		}

		public Cairo.Color ForegroundColor
		{
			get
			{
				return foregroundColor;
			}
		}

		public Cairo.Color ArrowColor
		{
			get
			{
				return arrowColor;
			}
		}
	}
}
