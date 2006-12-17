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
using Mono.Unix;

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

				resultLabel =
					new Label (Catalog.
						   GetString
						   ("<b>Result</b>"));
				resultLabel.UseMarkup = true;
				dateLabel =
					new Label (Catalog.
						   GetString ("<b>Date</b>"));
				dateLabel.UseMarkup = true;
				eventLabel =
					new Label (Catalog.
						   GetString
						   ("<b>Event</b>"));
				eventLabel.UseMarkup = true;
				siteLabel =
					new Label (Catalog.
						   GetString ("<b>Site</b>"));
				siteLabel.UseMarkup = true;

				resultValueLabel = new Label ();
				dateValueLabel = new Label ();
				eventValueLabel = new Label ();
				siteValueLabel = new Label ();

				titleLabel = new Label ();
				titleLabel.UseMarkup = true;

				Table table = new Table (5, 2, false);

				uint row = 0;
				  table.Attach (titleLabel, 0, 2, row,
						row + 1);
				  titleLabel.Xalign = 0;

				  row++;
				  table.Attach (resultLabel, 0, 1, row,
						row + 1);
				  resultLabel.Xalign = 0;
				  table.Attach (resultValueLabel, 1, 2, row,
						row + 1);
				  resultValueLabel.Xalign = 0;

				  row++;
				  table.Attach (dateLabel, 0, 1, row,
						row + 1);
				  dateLabel.Xalign = 0;
				  table.Attach (dateValueLabel, 1, 2, row,
						row + 1);
				  dateValueLabel.Xalign = 0;

				  row++;
				  table.Attach (eventLabel, 0, 1, row,
						row + 1);
				  eventLabel.Xalign = 0;
				  table.Attach (eventValueLabel, 1, 2, row,
						row + 1);
				  eventValueLabel.Xalign = 0;

				  row++;
				  table.Attach (siteLabel, 0, 1, row,
						row + 1);
				  siteLabel.Xalign = 0;
				  table.Attach (siteValueLabel, 1, 2, row,
						row + 1);
				  siteValueLabel.Xalign = 0;

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy = PolicyType.Automatic;
				  win.VscrollbarPolicy = PolicyType.Never;
				  win.AddWithViewport (table);

				  box.PackStart (win, false, false, 2);

				  otherTagsWidget =
					new Expander (Catalog.
						      GetString
						      ("Other details"));
				  box.PackStart (otherTagsWidget, false,
						 false, 2);

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
					"<b>" + white +
					Catalog.GetString (" vs ") + black +
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
				Table table =
					new Table ((uint) game.TagList.Count,
						   2, false);

				uint i = 0;
				foreach (PGNTag tag in game.TagList)
				{
					if (ignoreTags.Contains (tag.Name))
						continue;

					Label nameLabel =
						new Label ("<b>" + tag.Name +
							   "</b>");
					Label valueLabel =
						new Label ((string) tag.
							   Value);
					nameLabel.UseMarkup = true;
					nameLabel.Xalign = 0;
					valueLabel.Xalign = 0;
					table.Attach (nameLabel, 0, 1, i,
						      i + 1);
					table.Attach (valueLabel, 1, 2, i,
						      i + 1);
					i++;
				}

				ScrolledWindow win = new ScrolledWindow ();
				win.HscrollbarPolicy = PolicyType.Automatic;
				win.VscrollbarPolicy = PolicyType.Automatic;
				win.AddWithViewport (table);
				win.ShowAll ();
				if (otherTagsWidget.Child != null)
				  {
					  otherTagsWidget.
						  Remove (otherTagsWidget.
							  Child);
				  }
				otherTagsWidget.Add (win);
			}
		}
	}
}
