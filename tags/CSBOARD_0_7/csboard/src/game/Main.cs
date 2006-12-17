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
using Chess.Game;

public class App
{
	private static void GetPositions (ChessSide white, ChessSide black)
	{
	}

	public static void Main (string[]args)
	{
		ChessGame game = ChessGame.CreateGame ();
		game.PrintPositions ();
		game.Move ("e4");
		game.PrintPositions ();
		game.Move ("e5");
		game.PrintPositions ();
		game.Move ("Nf3");
		game.PrintPositions ();
		game.Move ("Nc6");
		game.PrintPositions ();
		game.Move ("Bb5");
		game.PrintPositions ();
		game.Move ("Nf6");
		game.PrintPositions ();
		game.Move ("o-o");
		game.PrintPositions ();
		game.Move ("d6");
		game.PrintPositions ();
		game.Move ("a3");
		game.PrintPositions ();
		game.Move ("Bg4");
		game.PrintPositions ();
		game.Move ("Nc3");
		game.PrintPositions ();
		game.Move ("Qd7");
		game.PrintPositions ();
		game.Move ("d3");
		game.PrintPositions ();
		game.Move ("o-o-o");
		game.PrintPositions ();
	}
}
