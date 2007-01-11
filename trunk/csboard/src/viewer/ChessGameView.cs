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
using System.Text;
using Gtk;
using Chess.Parser;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public class MoveEventArgs:EventArgs
		{
			public int nthMove;
			public MoveEventArgs (int n)
			{
				nthMove = n;
			}
		}

		public delegate void NthMoveEvent (object o,
						   MoveEventArgs args);

		public class ChessGameWidget:VBox
		{
			PGNGameDetails details;
			int curMoveIdx;

			public event NthMoveEvent ShowNthMove;

			public HTML HTML
			{
				get
				{
					return html;
				}
			}
			HTML html;
			public ChessGameWidget ():base ()
			{
				curMoveIdx = -1;

				html = new HTML ();
				ScrolledWindow win = new ScrolledWindow ();
				  win.SetPolicy (PolicyType.Never,
						 PolicyType.Automatic);
				  win.Add (html);

				  PackStart (win, true, true, 0);
				  html.WidthRequest = 150;
				  html.LinkClicked += OnLinkClicked;

				  ShowAll ();
			}

			public void SetMoveIndex (int idx)
			{
				// jump to anchor
				curMoveIdx = idx;
				//html.JumpToAnchor(idx.ToString());
			}

			public void SetGame (PGNGameDetails d)
			{
				curMoveIdx = -1;
				details = d;
				UpdateGameDetails ();
			}

			public void Refresh ()
			{
				UpdateGameDetails ();
			}

			private bool UpdateGameDetails ()
			{
				HTMLStream stream = html.Begin ();
				StringBuilder buffer = new StringBuilder ();
				buffer.Append
					("<HTML><HEAD></HEAD><BODY LINK=\"#000000\" VLINK=\"#000000\">");

				FillDetails (buffer);

				buffer.Append ("</BODY></HTML>");
				stream.Write (buffer.ToString ());
				html.End (stream, HTMLStreamStatus.Ok);
				return false;
			}

			private void FillDetails (StringBuilder buffer)
			{
				PrintTitle (buffer);
				if (details == null)
					return;

				PGNChessGame game = details.Game;

				int i = 0;
				int moveno = 1;
				foreach (PGNChessMove move in game.Moves)
				{
					if (i % 2 == 0)
						buffer.Append (String.
							       Format
							       ("<b>{0}. </b>",
								moveno++));
					buffer.Append (String.
						       Format
						       ("<b><a name=\"{0}\" href=\"#{0}\">{1}</a> </b>",
							i,
							move.DetailedMove));
					if (move.comment != null)
					  {
						  buffer.Append ("<p>");
						  buffer.Append (move.comment);	// TODO: format the markup
						  buffer.Append ("</p>");
					  }
					i++;
				}
			}

			private void PrintTitle (StringBuilder buffer)
			{
				if (details == null)
					return;
				PGNChessGame game = details.Game;

				buffer.Append (String.
					       Format
					       ("<H3><FONT COLOR=\"{0}\">{1} vs {2}</FONT></H3>",
						"#600000", game.White,
						game.Black));
				buffer.Append
					("<TABLE BORDER=0 CELLSPACING=4>");
				string format =
					"<TR><TD><B>{0}</B></TD><TD>{1}</TD></TR>";
				string eco;
				GameViewer.GetOpeningName (game.
							   GetTagValue ("ECO",
									""),
							   out eco);

				buffer.Append (String.Format (format,
							      Catalog.
							      GetString
							      ("Result"),
							      game.Result));
				buffer.Append (String.
					       Format (format,
						       Catalog.
						       GetString ("Date"),
						       game.Date));
				buffer.Append (String.
					       Format (format,
						       Catalog.
						       GetString ("Event"),
						       game.Event));
				buffer.Append (String.
					       Format (format,
						       Catalog.
						       GetString ("Site"),
						       game.Site));
				buffer.Append (String.
					       Format (format,
						       Catalog.
						       GetString ("Opening"),
						       eco));
				buffer.Append ("</TABLE><BR>");
			}

			private void OnLinkClicked (object o,
						    LinkClickedArgs args)
			{
				if (ShowNthMove == null)
					return;
				string url = args.Url;
				if (!url.StartsWith ("#"))
					return;
				int idx = Int32.Parse (url.Substring (1));
				ShowNthMove (this, new MoveEventArgs (idx));
			}
		}
	}
}
