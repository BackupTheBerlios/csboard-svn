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

namespace CsBoard {

        using Gdk;
        using Gtk;
        using System;
	using System.Collections;

        public enum FigureType {
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

        public class Figure {

                private ArrayList pixbufs;

                public Figure () {
                } 
		
		public Pixbuf GetPixbuf (FigureType type) {

                        return (Pixbuf) pixbufs[(int) type];
                }
		
		public void SetSize (int s) {
            
	     	    pixbufs = new ArrayList ();
                    
		    string filename;
		    		    
		    s = Math.Max (s, 10);
		    
                    filename = "images/white-rook.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/white-king.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/white-queen.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/white-bishop.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/white-pawn.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/white-knight.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));

                    filename = "images/black-rook.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/black-king.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/black-queen.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/black-bishop.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/black-pawn.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));
                    filename = "images/black-knight.svg";
                    pixbufs.Add (Rsvg.Tool.PixbufFromFileAtSize (filename, s, s));

		}

        }
}
