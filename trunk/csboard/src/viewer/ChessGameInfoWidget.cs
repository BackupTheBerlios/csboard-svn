using Gtk;
using Chess.Parser;
using System;
using System.Collections;

namespace CsBoard
{
	namespace Viewer
	{

		public class ChessGameInfoWidget:Frame
		{
			VBox box;
			// expected tags
			Label resultLabel, resultValueLabel;
			Label dateLabel, dateValueLabel;
			Label eventLabel, eventValueLabel;
			Label siteLabel, siteValueLabel;
			Label titleLabel;

			Expander otherTagsWidget;

			PGNChessGame game;

			public ChessGameInfoWidget ()
			{
				box = new VBox ();
				titleLabel = new Label ();
				titleLabel.UseMarkup = true;

				resultLabel = new Label ("<b>Result</b>");
				resultLabel.UseMarkup = true;
				dateLabel = new Label ("<b>Date</b>");
				dateLabel.UseMarkup = true;
				eventLabel = new Label ("<b>Event</b>");
				eventLabel.UseMarkup = true;
				siteLabel = new Label ("<b>Site</b>");
				siteLabel.UseMarkup = true;

				resultValueLabel = new Label ();
				dateValueLabel = new Label ();
				eventValueLabel = new Label ();
				siteValueLabel = new Label ();

				titleLabel = new Label ();
				titleLabel.UseMarkup = true;

				VBox namesBox, valuesBox;
				  namesBox = new VBox ();
				  valuesBox = new VBox ();

				  box.PackStart (titleLabel, true, false, 2);
				  titleLabel.Xalign = 0;

				  namesBox.PackStart (resultLabel, true, true,
						      2);
				  resultLabel.Xalign = 0;
				  valuesBox.PackStart (resultValueLabel, true,
						       true, 2);
				  resultValueLabel.Xalign = 0;

				  namesBox.PackStart (dateLabel, true, true,
						      2);
				  dateLabel.Xalign = 0;
				  valuesBox.PackStart (dateValueLabel, true,
						       true, 2);
				  dateValueLabel.Xalign = 0;

				  namesBox.PackStart (eventLabel, true, true,
						      2);
				  eventLabel.Xalign = 0;
				  valuesBox.PackStart (eventValueLabel, true,
						       true, 2);
				  eventValueLabel.Xalign = 0;

				  namesBox.PackStart (siteLabel, true, true,
						      2);
				  siteLabel.Xalign = 0;
				  valuesBox.PackStart (siteValueLabel, true,
						       true, 2);
				  siteValueLabel.Xalign = 0;

				HBox hbox = new HBox ();
				  hbox.PackStart (namesBox, false, false, 10);
				  hbox.PackStart (valuesBox, true, true, 20);

				  box.PackStart (hbox, true, false, 2);

				  otherTagsWidget =
					new Expander ("Other details");
				  box.PackStart (otherTagsWidget, true, false,
						 2);

				  box.ShowAll ();
				  Child = box;
			}

			public void SetGame (PGNChessGame g)
			{
				game = g;
				UpdateGameDetails ();
			}

			private void UpdateGameDetails ()
			{
				string white =
					game.Tags.
					Contains ("White") ? (string) game.
					Tags["White"] : "[White]";
				string black =
					game.Tags.
					Contains ("Black") ? (string) game.
					Tags["Black"] : "[Black]";
				string evnt =
					game.Tags.
					Contains ("Event") ? (string) game.
					Tags["Event"] : "";
				string site =
					game.Tags.
					Contains ("Site") ? (string) game.
					Tags["Site"] : "";
				string date =
					game.Tags.
					Contains ("Date") ? (string) game.
					Tags["Date"] : "";
				string result =
					game.Tags.
					Contains ("Result") ? (string) game.
					Tags["Result"] : "";

				  titleLabel.Markup =
					"<b>" + white + " vs " + black +
					"</b>";
				  eventValueLabel.Text = evnt;
				  siteValueLabel.Text = site;
				  dateValueLabel.Text = date;
				  resultValueLabel.Text = result;

				IList ignoreTags = new ArrayList ();
				  ignoreTags.Add ("White");
				  ignoreTags.Add ("Black");
				  ignoreTags.Add ("Result");
				  ignoreTags.Add ("Date");
				  ignoreTags.Add ("Site");
				  ignoreTags.Add ("Event");

				  UpdateOtherTags (ignoreTags);
			}

			private void UpdateOtherTags (IList ignoreTags)
			{
				VBox namesBox, valuesBox;
				HBox hbox;

				namesBox = new VBox ();
				valuesBox = new VBox ();
				hbox = new HBox ();

				foreach (DictionaryEntry de in game.Tags)
				{
					if (ignoreTags.Contains (de.Key))
						continue;

					Label nameLabel =
						new Label ("<b>" + de.Key +
							   "</b>");
					Label valueLabel =
						new Label ((string) de.Value);
					nameLabel.UseMarkup = true;
					nameLabel.Xalign = 0;
					valueLabel.Xalign = 0;
					namesBox.PackStart (nameLabel, false,
							    false, 2);
					valuesBox.PackStart (valueLabel, true,
							     false, 2);
				}

				hbox.PackStart (namesBox, false, false, 2);
				hbox.PackStart (valuesBox, false, false, 20);

				hbox.ShowAll ();
				if (otherTagsWidget.Child != null)
				  {
					  otherTagsWidget.
						  Remove (otherTagsWidget.
							  Child);
				  }
				otherTagsWidget.Add (hbox);
			}
		}
	}
}
