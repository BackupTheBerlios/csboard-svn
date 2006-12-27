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
// Copyright (C) 2004 Nickolay V. Shmyrev


// FIXME: implement caching

namespace CsBoard
{

	using Gdk;
	using Gtk;
	using System;
	using System.IO;
	using System.Collections;

	public enum FigureType
	{
		WhiteRook,
		WhiteKing,
		WhiteQueen,
		WhiteBishop,
		WhitePawn,
		WhiteKnight,
		BlackRook,
		BlackKing,
		BlackQueen,
		BlackBishop,
		BlackPawn,
		BlackKnight,
		None
	};

	public class Figure
	{

		protected ArrayList pixbufs;

		  string[] files = {
		"white-rook.svg",
				"white-king.svg",
				"white-queen.svg",
				"white-bishop.svg",
				"white-pawn.svg",
				"white-knight.svg",
				"black-rook.svg",
				"black-king.svg",
				"black-queen.svg",
				"black-bishop.svg",
				"black-pawn.svg", "black-knight.svg"};

		public Figure ()
		{
		}

		public Pixbuf GetPixbuf (FigureType type)
		{
			return (Pixbuf) pixbufs[(int) type];
		}

		public virtual void SetSize (int s)
		{
			pixbufs = new ArrayList ();

			s = Math.Max (s, 10);

			foreach (string filename in files) {
				pixbufs.Add (GetPixbuf (filename, s));
			}
		}

		protected virtual Gdk.Pixbuf GetPixbuf (string filename,
							int size)
		{
			return Rsvg.Tool.PixbufFromFileAtSize (Path.
							       Combine
							       ("images",
								filename),
							       size, size);
		}
	}
}
