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

namespace CsBoard
{

	using System;
	using System.Collections;

	public class Point
	{

		public int x;
		public int y;

		public Point (int a, int b)
		{
			x = a;
			y = b;
		}

		public Point ()
		{
		}

		public void Set (Point from)
		{
			x = from.x;
			y = from.y;
		}

		public void Center ()
		{
			x = 4;
			y = 4;
		}
	};

	public class AnimationTaskItem
	{

		public FigureType fig;

		public Point from;
		public Point to;

		public double progress;

		public AnimationTaskItem (FigureType fig_p, Point from_p,
					  Point to_p)
		{
			fig = fig_p;
			from = from_p;
			to = to_p;
			progress = 0.0;
		}
	}

	public class Position
	{

		public FigureType takenFig;

		private ArrayList position;
		private ArrayList takenPosition;
		public ArrayList Pos
		{
			get
			{
				return position;
			}
		}

		bool askForPromotion = true;
		public bool AskForPromotion
		{
			get
			{
				return askForPromotion;
			}
			set
			{
				askForPromotion = value;
			}
		}

		public Position (ArrayList pos)
		{
			position = pos;
		}

		public void SetPosition (ArrayList pos)
		{
			position = pos;
		}

		public ArrayList SetPositionAnimate (ArrayList pos)
		{
			int i = 0, j = 0;
			int k = 0, l = 0;

			ArrayList result = new ArrayList ();

			FigureType[,] board_start = new FigureType[8, 8];
			FigureType[,] board_end = new FigureType[8, 8];

			for (i = 0; i < 8; i++)
				for (j = 0; j < 8; j++)
					board_start[i, j] =
						GetFigureAt (i, j);

			position = pos;

			for (i = 0; i < 8; i++)
				for (j = 0; j < 8; j++)
					board_end[i, j] = GetFigureAt (i, j);

			for (i = 0; i < 8; i++)
				for (j = 0; j < 8; j++)
				  {

					  if (board_start[i, j] ==
					      board_end[i, j]
					      && board_start[i,
							     j] !=
					      FigureType.None)
					    {
						    AnimationTaskItem item =
							    new
							    AnimationTaskItem
							    (board_start
							     [i, j],
							     new Point (i, j),
							     new Point (i,
									j));

						    board_start[i, j] =
							    FigureType.None;
						    board_end[i, j] =
							    FigureType.None;
						    result.Add (item);
					    }
				  }

			bool finished = false;

			while (true)
			  {

				  finished = true;

				  for (i = 0; i < 8; i++)
				    {
					    for (j = 0; j < 8; j++)
					      {
						      if (board_end[i, j] !=
							  FigureType.None)
							{
								finished =
									false;
								break;
							}
					      }
					    if (!finished)
						    break;
				    }

				  if (finished)
					  break;

				  bool found = false;
				  FigureType fig = board_end[i, j];

				  for (k = 0; k < 8; k++)
				    {
					    for (l = 0; l < 8; l++)
					      {
						      if (board_start[k, l] ==
							  fig)
							{

								found = true;
								board_start[k,
									    l]
									=
									FigureType.
									None;
								break;

							}
					      }
					    if (found)
					      {
						      break;
					      }
				    }

				  AnimationTaskItem item = null;

				  if (found)
				    {
					    item = new
						    AnimationTaskItem
						    (board_end[i, j],
						     new Point (k, l),
						     new Point (i, j));
				    }
				  else
				    {
					    item = new
						    AnimationTaskItem
						    (board_end[i, j],
						     new Point (k, l),
						     new Point (i, j));
				    }

				  board_end[i, j] = FigureType.None;

				  result.Add (item);
			  }


			return result;
		}

		public FigureType GetFigureAt (int x, int y)
		{

			string str = position[y + 2].ToString ();
			FigureType result;

			switch (str[x * 2])
			  {
			  case 'p':
				  result = FigureType.BlackPawn;
				  break;
			  case 'P':
				  result = FigureType.WhitePawn;
				  break;
			  case 'r':
				  result = FigureType.BlackRook;
				  break;
			  case 'R':
				  result = FigureType.WhiteRook;
				  break;
			  case 'k':
				  result = FigureType.BlackKing;
				  break;
			  case 'K':
				  result = FigureType.WhiteKing;
				  break;
			  case 'b':
				  result = FigureType.BlackBishop;
				  break;
			  case 'B':
				  result = FigureType.WhiteBishop;
				  break;
			  case 'q':
				  result = FigureType.BlackQueen;
				  break;
			  case 'Q':
				  result = FigureType.WhiteQueen;
				  break;
			  case 'n':
				  result = FigureType.BlackKnight;
				  break;
			  case 'N':
				  result = FigureType.WhiteKnight;
				  break;

			  default:
				  result = FigureType.None;
				  break;
			  }
			return result;
		}

		public void Move (Point start, Point end, ref char figure,
				  bool explicitly)
		{
			MoveAnimate (start, end, ref figure, explicitly);
		}

		public ArrayList MoveAnimate (Point start, Point end,
					      ref char figure,
					      bool explicitly)
		{

			Promotion dialog;
			ArrayList list = new ArrayList ();
			AnimationTaskItem item = null;

			FigureType ft;
			ft = GetFigureAt (start.x, start.y);
			item = new AnimationTaskItem (ft, start, end);
			list.Add (item);

			for (int i = 0; i < 8; i++)
				for (int j = 0; j < 8; j++)
				  {
					  if (i == start.x && j == start.y)
						  continue;
					  ft = GetFigureAt (i, j);
					  if (ft == FigureType.None)
						  continue;
					  Point p = new Point (i, j);
					  list.Add (new
						    AnimationTaskItem (ft, p,
								       p));
				  }

			// castling case
			bool castling = false;
			if (ft == FigureType.WhiteKing && start.y == 0
			    && end.y == 0 && start.x == 4 && (end.x == 2
							      || end.x == 6))
				castling = true;
			else if (ft == FigureType.BlackKing && start.y == 7
				 && end.y == 7 && start.x == 4 && (end.x == 2
								   || end.x ==
								   6))
				castling = true;
			if (castling)
			  {
				  int src_file, dst_file;
				  if (end.x == 2)
				    {
					    src_file = 0;
					    dst_file = 3;
				    }
				  else
				    {
					    src_file = 7;
					    dst_file = 5;
				    }
				  item = new
					  AnimationTaskItem (GetFigureAt
							     (src_file,
							      start.y),
							     new
							     Point (src_file,
								    start.y),
							     new
							     Point (dst_file,
								    end.y));
				  list.Add (item);
			  }

			if (askForPromotion && start.y == 1
			    && end.y == 0 &&
			    GetFigureAt (start.x,
					 start.y) == FigureType.WhitePawn)
			  {

				  dialog = new Promotion ();

				  dialog.Run ();

				  figure = dialog.GetResult ();
				  dialog.Hide ();
				  dialog.Dispose ();

			  }

			if (askForPromotion && start.y == 6
			    && end.y == 7 &&
			    GetFigureAt (start.x,
					 start.y) == FigureType.BlackPawn)
			  {

				  dialog = new Promotion ();

				  dialog.Run ();

				  figure = dialog.GetResult ();
				  dialog.Hide ();
				  dialog.Dispose ();

			  }

			if (explicitly)
			  {
				  // Find figure to take
				  string str = (string) position[start.y + 2];
				  char fig = str[start.x * 2];
				  // Clear from point
				  string str_clear = str.Substring (0,
								    start.x *
								    2) + '.' +
					  str.Substring (start.x * 2 + 1);

				  position[start.y + 2] = str_clear;
				  // Add figure to destination 
				  str = (string) position[end.y + 2];
				  string str_new = str.Substring (0,
								  end.x * 2) +
					  fig + str.Substring (end.x * 2 + 1);
				  position[end.y + 2] = str_new;
			  }

			return list;
		}

		public void Take (Point point)
		{

			takenFig = GetFigureAt (point.x, point.y);
			takenPosition = new ArrayList (position);

			string str = (string) position[point.y + 2];

			string str_new = str.Substring (0,
							point.x * 2) + '.' +
				str.Substring (point.x * 2 + 1,
					       str.Length - point.x * 2 - 1);
			position[point.y + 2] = str_new;
		}

		public void Cancel ()
		{
			position = takenPosition;
		}

		public void Put ()
		{
			position = takenPosition;
		}

		public void Dump ()
		{
			for (int i = 0; i < position.Count; i++)
			  {
				  Console.WriteLine (position[i]);
			  }
			Console.WriteLine ("");
		}

	}
}
