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
using Gtk;
using Chess.Parser;

namespace CsBoard
{
	namespace Viewer
	{

		public class ChessGameWidget:VBox
		{
			PGNChessGame game;
			int highlightMoveIndex;
			bool highlightWhite;
			HTML infoHTML;
			HTML gameHTML;

			public ChessGameWidget ():base ()
			{
				highlightMoveIndex = -1;

				infoHTML = new HTML ();
				gameHTML = new HTML ();
				infoHTML.Show ();
				gameHTML.Show ();
				Show ();

				PackStart (infoHTML, false, false, 0);
				ScrolledWindow win = new ScrolledWindow ();
				  win.Child = gameHTML;
				  win.HscrollbarPolicy = PolicyType.Never;
				  win.VscrollbarPolicy = PolicyType.Automatic;
				  win.Show ();
				  PackStart (win, true, true, 0);
			}

			public void SetGame (PGNChessGame g)
			{
				game = g;
				GenerateHTML ();
			}

			public void HighlightMove (int moveIdx, bool white)
			{
				highlightWhite = white;
				highlightMoveIndex = moveIdx;
				//      GenerateHTML();
				gameHTML.JumpToAnchor ("" + moveIdx);
			}

			public void GenerateHTML ()
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

				HTMLStream stream = infoHTML.Begin ();
				stream.Write
					("<HTML><HEAD></HEAD><BODY><TABLE BORDER=0 WIDTH=100%>");
				stream.Write (String.
					      Format
					      ("<TR><TD COLSPAN=2><FONT SIZE=\"+1\"><B>{0} vs {1}</B></FONT></TD></TR>",
					       white, black));
				stream.Write (String.
					      Format
					      ("<TR><TD VALIGN=TOP><B>Result</B></TD><TD VALIGN=TOP>{0}</TD></TR>",
					       result));
				stream.Write (String.
					      Format
					      ("<TR><TD VALIGN=TOP><B>Date</B></TD><TD VALIGN=TOP>{0}</TD></TR>",
					       date));
				stream.Write (String.
					      Format
					      ("<TR><TD VALIGN=TOP><B>Event</B></TD><TD VALIGN=TOP>{0}</TD></TR>",
					       evnt));
				stream.Write (String.
					      Format
					      ("<TR><TD VALIGN=TOP><B>Site</B></TD><TD VALIGN=TOP>{0}</TD></TR>",
					       site));

				stream.Write ("</TABLE>");
				stream.Write ("</BODY></HTML>");
				infoHTML.End (stream, HTMLStreamStatus.Ok);

				stream = gameHTML.Begin ();
				stream.Write ("<HTML><HEAD></HEAD><BODY>");
				stream.Write
					("<TABLE BORDER=1 CELLSPACING=0 CELLPADDING=5 WIDTH=150>");
				stream.Write
					("<THEAD><TR BGCOLOR=\"#c0c0ff\"><TH>No</TH><TH>White</TH><TH>Black</TH></TR></THEAD><TBODY>");
				int i = 1;
				foreach (ChessMove move in game.Moves)
				{
					string whitemove =
						move.whitemove ==
						null ? "" : move.whitemove;
					string blackmove =
						move.blackmove ==
						null ? "" : move.blackmove;
					if (i == highlightMoveIndex)
					  {
						  if (highlightWhite)
							  whitemove =
								  "<B>" +
								  whitemove +
								  "</B>";
						  else
							  blackmove =
								  "<B>" +
								  blackmove +
								  "</B>";
					  }
					stream.Write (String.
						      Format
						      ("<TR><TD ALIGN=LEFT><A NAME=\"{0}\"/>{1}</TD><TD ALIGN=LEFT>{2}</TD><TD ALIGN=LEFT>{3}</TD></TR>\n",
						       i - 1, i, whitemove,
						       blackmove));
					i++;
				}
				stream.Write ("</TABLE>");
				stream.Write ("</BODY></HTML>");
				gameHTML.End (stream, HTMLStreamStatus.Ok);
			}
		}
	}
}
