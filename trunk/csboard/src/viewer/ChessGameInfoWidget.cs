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
				string white = game.White;
				string black = game.Black;
				string evnt = game.Event;
				string site = game.Site;
				string date = game.Date;
				string result = game.Result;

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

				foreach (PGNTag tag in game.TagList)
				{
					if (ignoreTags.Contains (tag.Name))
						continue;

					Label nameLabel =
						new Label ("<b>" + tag.Name +
							   "</b>");
					Label valueLabel =
						new Label ((string) tag.Value);
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
