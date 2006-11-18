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
using Chess.Parser;

namespace CsBoard
{
	namespace Viewer
	{

		public class GamesListWidget:HTML
		{
			ArrayList games;
			int highlightGameIndex = -1;

			public GamesListWidget ():base ()
			{
				GenerateHTML ();
				Show ();
			}

			public void SetGames (ArrayList g)
			{
				games = g;
				highlightGameIndex = -1;
				GenerateHTML ();
			}

			void GenerateHTML ()
			{
				HTMLStream stream = Begin ();
				  stream.Write ("<HTML><HEAD></HEAD><BODY>");
				if (games == null)
				  {
					  stream.Write ("</BODY></HTML>");
					  End (stream, HTMLStreamStatus.Ok);
					  return;
				  }
				stream.Write ("<H3>Available Games</H3>");

				stream.Write
					("<FONT SIZE=\"-1\"><TABLE BORDER=1 CELLSPACING=0><THEAD><TR><TH>No</TH><TH>White</TH><TH>Black</TH><TH>Moves</TH><TH>Result</TH></TR></THEAD><TBODY>");
				int i = 0;
				foreach (PGNChessGame game in games)
				{
					string white =
						game.Tags.
						Contains ("White") ? (string)
						game.Tags["White"] : "White";
					string black =
						game.Tags.
						Contains ("Black") ? (string)
						game.Tags["Black"] : "Black";
					string result =
						game.Tags.
						Contains ("Result") ? (string)
						game.
						Tags["Result"] : "Result";

					if (highlightGameIndex == i)
					  {
						  stream.Write (String.
								Format
								("<TR><TD><A NAME=\"{0}\" HREF=\"{0}\"><B>{1}</B></TD><TD><B>{2}</B></TD><TD><B>{3}</B></TD><TD><B>{4}</B></TD><TD><B>{5}</B></TD></TR>",
								 i, i + 1,
								 white, black,
								 game.Moves.
								 Count,
								 result));
					  }
					else
					  {
						  stream.Write (String.
								Format
								("<TR><TD><A NAME=\"{0}\" HREF=\"{0}\">{1}</TD><TD>{2}</TD><TD>{3}</TD><TD>{4}</TD><TD>{5}</TD></TR>",
								 i, i + 1,
								 white, black,
								 game.Moves.
								 Count,
								 result));
					  }
					i++;
				}
				stream.Write
					("</TABLE></FONT></BODY></HTML>");
				End (stream, HTMLStreamStatus.Ok);
			}

			public void HighlightGame (int idx)
			{
				highlightGameIndex = idx;
				GenerateHTML ();
				JumpToAnchor ("" + idx);
			}
		}
	}
}
