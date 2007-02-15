using Gtk;
using System;

namespace CsBoard
{
	public class ChessGameWidget:VBox
	{
		Gtk.Label whiteLabel, blackLabel;
		public ChessClock whiteClock, blackClock;
		  Gtk.Bin topBin, bottomBin;
		HBox whiteHBox, blackHBox;

		bool whiteAtBottom;

		public bool WhiteAtBottom
		{
			get
			{
				return whiteAtBottom;
			}
			set
			{
				if (whiteAtBottom != value)
				  {
					  SwitchSides ();
					  whiteAtBottom = value;
				  }
			}
		}

		public ChessGameWidget (Widget board)
		{
			whiteLabel = new Label ();
			blackLabel = new Label ();

			blackClock = new ChessClock ();
			blackHBox = new HBox ();
			blackHBox.PackStart (blackLabel, true, true, 2);
			blackHBox.PackStart (blackClock, false, false, 2);

			whiteClock = new ChessClock ();
			whiteHBox = new HBox ();
			whiteHBox.PackStart (whiteLabel, true, true, 2);
			whiteHBox.PackStart (whiteClock, false, false, 2);

			topBin = new Frame ();
			bottomBin = new Frame ();

			whiteAtBottom = true;
			topBin.Add (blackHBox);
			bottomBin.Add (whiteHBox);

			PackStart (topBin, false, false, 2);
			PackStart (board, true, true, 2);
			PackStart (bottomBin, false, false, 2);

			topBin.ShowAll ();
			bottomBin.ShowAll ();
		}

		public string White
		{
			set
			{
				whiteLabel.Markup = GetMarkup (value);
			}
		}

		public string Black
		{
			set
			{
				blackLabel.Markup = GetMarkup (value);
			}
		}

		static string GetMarkup (string str)
		{
			return String.
				Format
				("<big><big><big><b>{0}</b></big></big></big>",
				 str);
		}

		private void SwitchSides ()
		{
			Widget top, bottom;
			top = topBin.Child;
			bottom = bottomBin.Child;

			topBin.Remove (top);
			bottomBin.Remove (bottom);

			topBin.Add (bottom);
			bottomBin.Add (top);
		}
	}
}
