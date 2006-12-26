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

		private ArrayList pixbufs;

		public Figure ()
		{
		}

		public Pixbuf GetPixbuf (FigureType type)
		{

			return (Pixbuf) pixbufs[(int) type];
		}

		public void SetSize (int s)
		{

			pixbufs = new ArrayList ();

			string filename;

			s = Math.Max (s, 10);

			filename = "white-rook.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "white-king.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "white-queen.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "white-bishop.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "white-pawn.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "white-knight.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));

			filename = "black-rook.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "black-king.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "black-queen.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "black-bishop.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "black-pawn.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));
			filename = "black-knight.svg";
			pixbufs.Add (GetPixbuf (filename, s, s));

		}

		static Gdk.Pixbuf GetPixbuf (string file, int width,
					     int height)
		{
			Gdk.Pixbuf pix = Rsvg.Pixbuf.LoadFromResource (file);
			return pix.ScaleSimple (width, height,
						Gdk.InterpType.Bilinear);
		}

	}
}
