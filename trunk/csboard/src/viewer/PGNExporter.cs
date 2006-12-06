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

using System.Collections;
using Chess.Parser;
using Chess.Game;
using Gtk;
using Gnome;
using System;
using CsBoard;

namespace CsBoard
{
	namespace Viewer
	{
		struct PGNFonts
		{
			public Font regularFont;
			public Font titleFont;
			public Font commentFont;
			public Font moveFont;
		};

	public class PGNExporter
	{
		public static void WriteToPrinter (IList games,
						   PrintWrapper
						   printer)
			{
				PGNFonts fonts;

				fonts.regularFont =
					Font.
					FindClosestFromWeightSlant ("Sans",
								    FontWeight.
								    Regular,
								    false, 8);
				fonts.titleFont =
					Font.
					FindClosestFromWeightSlant ("Sans",
								    FontWeight.
								    Bold,
								    false,
								    12);
				fonts.commentFont =
					Font.
					FindClosestFromWeightSlant ("Sans",
								    FontWeight.
								    Regular,
								    true, 8);
				fonts.moveFont =
					Font.
					FindClosestFromWeightSlant ("Sans",
								    FontWeight.
								    Bold,
								    false, 9);

				printer.Start ();
				printer.LineSpaceRatio = 2;
				bool first = true;
				foreach (PGNChessGame game in games)
					{
						if (!first)
							printer.PageBreak ();
						WriteGameToPrinter (game, printer,
								    fonts);
						first = false;
					}
				printer.End ();
			}

		private static void PrintImageForPosition (PrintWrapper printer,
							   ArrayList
							   position)
			{
				int width = 200;
				int height = 200;
				PositionSnapshot ps = new PositionSnapshot(position, width, height);
				Gdk.Pixbuf image = Gdk.Pixbuf.FromDrawable(ps.Pixmap, ps.Pixmap.Colormap, 0, 0, 0, 0, width, height);
				printer.PrintImage(image);
			}

		private static void WriteGameToPrinter (PGNChessGame
							game,
							PrintWrapper
							printer,
							PGNFonts
							fonts)
			{
				GameSession session = new GameSession ();
				session.Set (game);
				string white = (string) game.Tags["White"];
				string black = (string) game.Tags["Black"];
				string result = (string) game.Tags["Result"];



				if (white == null)
					white = "[White]";
				if (black == null)
					black = "[Black]";
				if (result == null)
					result = "Unknown";

				printer.Font = fonts.titleFont;
				printer.PrintText (white + " vs " + black +
						   "\n\n");
				printer.Font = fonts.regularFont;
				printer.PrintText ("Result: " + result +
						   "\n\n");

				printer.Font = fonts.moveFont;
				int moveno = 1;
				foreach (ChessMove move in game.Moves)
					{
						// print move
						if (move.whitemove == null)
							break;
						printer.PrintText (moveno + ". " +
								   move.whitemove);
						if(session.HasNext()) {
							session.Next ();
							session.player.Move (session.CurrentMove);
						}
						if (move.whiteComment != null)
							{
								printer.LineBreak ();
								PrintImageForPosition(printer, session.
										      player.
										      GetPosition
										      ());
								printer.Font =
									fonts.commentFont;
								printer.PrintText (move.
										   whiteComment);
								printer.Font =
									fonts.moveFont;
								if (move.blackmove == null)
									break;
								printer.PrintText ("\n" +
										   moveno +
										   "...");
							}

						if (move.blackmove == null)
							break;
						if(session.HasNext()) {
							session.Next ();
							session.player.Move (session.CurrentMove);
						}
						printer.PrintText (" " +
								   move.blackmove +
								   " ");
						if (move.blackComment != null)
							{
								printer.LineBreak();
								PrintImageForPosition(printer, session.
										      player.
										      GetPosition
										      ());
								printer.Font =
									fonts.commentFont;
								printer.PrintText (move.
										   blackComment);
								printer.Font =
									fonts.moveFont;
								printer.PrintText ("\n");
							}
						moveno++;
					}
			}
	}
	}			// namespace Viewer
}
