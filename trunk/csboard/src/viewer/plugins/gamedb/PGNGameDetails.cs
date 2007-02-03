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

using Chess.Parser;
using Chess.Game;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public enum GameRating
		{
			Unknown = 0,
			Ignore = -1,
			Average = 1,
			Good = 2,
			Excellent = 3,
			MustHave = 4
		}

		public class PGNGameDetails:ChessGame
		{
			int nmoves;
			string white;
			string black;
			string result;

			  string[] tags;
			public string[] Tags
			{
				get
				{
					return tags;
				}
			}

			int id;
			public int ID
			{
				get
				{
					return id;
				}
				set
				{
					id = value;
				}
			}

			public GameRating Rating
			{
				get
				{
					return (GameRating) rating;
				}
				set
				{
					rating = (int) value;
				}
			}

			public string RatingStr
			{
				get
				{
					switch (rating)
					  {
					  case (int) GameRating.Unknown:
						  return "Unknown";
						  case (int) GameRating.
							  Ignore:return
							  "Ignore";
						  case (int) GameRating.
							  Average:return
							  "Average";
						  case (int) GameRating.
							  Good:return "Good";
						  case (int) GameRating.
							  Excellent:return
							  "Excellent";
						  case (int) GameRating.
							  MustHave:return
							  "MustHave";
					  }

					return "";
				}
			}

			int rating = (int) GameRating.Unknown;
			string hash;
			public string Hash
			{
				get
				{
					if (hash == null)
						hash = GenerateHash (this);
					return hash;
				}
			}

			public bool AddTag (string tag)
			{
				if (tags == null)
				  {
					  tags = new string[]
					  {
					  tag};
					  return true;
				  }

				foreach (string t in tags)
				{
					if (t.Equals (tag))
						return false;
				}

				string[]newtags = new string[tags.Length + 1];
				int i = 0;
				foreach (string t in tags)
				{
					newtags[i++] = t;
				}

				newtags[i] = tag;
				tags = newtags;
				return true;
			}

			public bool RemoveTag (string tag)
			{
				if (tags == null)
					return false;
				int i = 0;
				string[]newtags = new string[tags.Length - 1];
				bool found = false;
				foreach (string t in tags)
				{
					if (!found && t.Equals (tag))
					  {
						  found = true;
						  continue;
					  }

					newtags[i++] = t;
				}

				if (found)
					tags = newtags.Length ==
						0 ? null : newtags;
				return found;
			}

			public PGNGameDetails (PGNChessGame game):base (game)
			{
				nmoves = game.Moves.Count;
				white = game.GetTagValue ("White", "");
				black = game.GetTagValue ("Black", "");
				result = game.GetTagValue ("Result", "*");
			}

			protected static string GenerateHash (PGNChessGame
							      game)
			{
				StringBuilder buffer = new StringBuilder ();
				int nmoves = game.Moves.Count;
				buffer.Append (String.
					       Format ("{0}:", nmoves));
				ChessGamePlayer player;
				player = game.HasTag ("FEN") ?
					ChessGamePlayer.
					CreateFromFEN (game.GetTagValue
						       ("FEN",
							null))
					: ChessGamePlayer.CreatePlayer ();
				player = Chess.Game.
					ChessGamePlayer.CreatePlayer ();
				foreach (PGNChessMove move in game.Moves)
				{
					player.Move (move.Move);
				}

				buffer.Append (player.GetPositionAsFEN ());
				buffer.Append (((nmoves + 1) / 2));
				return buffer.ToString ();
			}

			public override string ToPango ()
			{
				StringBuilder buffer = new StringBuilder ();
				string[]rating_colors =
				{
					"#808080",	// Ignore
						"#000000",	// unknown
						"#000000",	// average
						"#881010",	// good
						"#108810",	// excellent
						"#101088"	// must have
				};
				string rating_color =
					rating_colors[(int) rating -
						      (int) GameRating.
						      Ignore];
				buffer.Append (String.
					       Format ("<span color=\"{0}\">",
						       rating_color));
				buffer.Append (base.ToPango ());
				string tagsStr = "";
				if (tags == null || tags.Length == 0)
					tagsStr = "";
				else
				  {
					  foreach (string tag in tags)
					  {
						  if (tagsStr == null)
							  tagsStr = tag;
						  else
							  tagsStr =
								  String.
								  Format
								  ("{0}, {1}",
								   tagsStr,
								   tag);
					  }
				  }
				buffer.Append (String.
					       Format
					       ("\n<small><i>{0}</i>: {1}, <i>{2}</i>: {3}</small>",
						Catalog.
						GetString ("Game Rating"),
						RatingStr,
						Catalog.GetString ("Tags"),
						tagsStr));

				buffer.Append ("</span>");
				return buffer.ToString ();
			}
		}
	}
}
