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
using System.Collections;

namespace CsBoard
{
	public class GameAdvertisement
	{
		public int gameHandle;
		public string username;
		public int rating;

		public int time_limit;
		public int time_increment;

		public bool rated;

		public string category;
		char color;
		bool automatic;	// automatic/manual
		bool formula;

		char rating_char;

		int ti;

		int[] rating_range;

		public string Color
		{
			get
			{
				return color == '?' ? "Any color" : color ==
					'w' ? "White" : "Black";
			}
		}

		public string Flags
		{
			get
			{
				string str =
					automatic ? "Automatic" : "Manual";
				  return str + ", Formula " +
					(formula ? "used" : "not used");
			}
		}

		public string Range
		{
			get
			{
				return rating_range == null ? "No range" :
					String.Format ("{0}-{1}",
						       rating_range[0],
						       rating_range[1]);
			}
		}

		public override string ToString ()
		{
			StringBuilder buffer = new StringBuilder ();
			buffer.Append (String.
				       Format ("Game Id          : {0}\n",
					       gameHandle));
			buffer.Append (String.
				       Format ("Name             : {0}\n",
					       username));
			buffer.Append (String.
				       Format ("Rating           : {0}\n",
					       rating));

			buffer.Append (String.
				       Format ("Time limit       : {0}\n",
					       time_limit));
			buffer.Append (String.
				       Format ("Time increment   : {0}\n",
					       time_increment));
			buffer.Append (String.
				       Format ("Rated            : {0}\n",
					       rated));

			buffer.Append (String.
				       Format ("Category         : {0}\n",
					       category));
			buffer.Append (String.
				       Format ("Color            : {0}\n",
					       color));
			buffer.Append (String.
				       Format ("Automatic        : {0}\n",
					       automatic));
			buffer.Append (String.
				       Format ("Formula          : {0}",
					       formula));

			return buffer.ToString ();
		}

		public string ToPango ()
		{
			StringBuilder buffer = new StringBuilder ();
			if (rated)
				buffer.Append (String.
					       Format
					       ("<span color=\"#802020\">"));
			buffer.Append (String.
				       Format ("<b>{0}</b> (<i>{1}</i>\n)",
					       username,
					       rated ? "rated" : "unrated"));
			buffer.Append (String.
				       Format
				       ("<i>Time limits:</i> <b>{0} +{1}</b>, <i>Color</i>: {2}, <i>Type</i>: {3}\n",
					time_limit, time_increment,
					color == '?' ? "No choice" : color ==
					'w' ? "White" : "Black", category));
			buffer.Append (String.
				       Format
				       ("<i>Acceptance</i>: {0}, <i>Formula</i>: {1}",
					automatic ? "Automatic" : "Manual",
					formula ? "yes" : "no"));
			if (rated)
				buffer.Append ("</span>");

			return buffer.ToString ();
		}

		static void SkipWhitespace (byte[]buffer, ref int idx,
					    int end)
		{
			while (buffer[idx] == ' ' && idx < end)
				idx++;
		}

		static void ReadNameValue (byte[]buffer, ref int idx, int end,
					   out string name, out string val)
		{
			ReadWord (buffer, '=', ref idx, end, out name);
			idx++;
			ReadWord (buffer, ' ', ref idx, end, out val);
		}

		public static void ReadCancellations (byte[]buffer, int start,
						      int end, ArrayList list)
		{
			while (start < end)
			  {
				  SkipWhitespace (buffer, ref start, end);
				  if (start >= end)
					  break;
				  string word;
				  ReadWord (buffer, ' ', ref start, end,
					    out word);
				  list.Add (Int32.Parse (word));
			  }
		}

		static void ReadWord (byte[]buffer, char delim, ref int idx,
				      int end, out string word)
		{
			System.Text.Decoder decoder =
				System.Text.Encoding.UTF8.GetDecoder ();
			for (int i = idx; i < end; i++)
			  {
				  if (buffer[i] == delim)
				    {
					    char[] chrs = new char[i - idx];
					    decoder.GetChars (buffer, idx,
							      i - idx, chrs,
							      0);
					    word = new string (chrs);
					    idx = i;
					    return;
				    }
			  }

			char[] chars = new char[end - idx];
			decoder.GetChars (buffer, idx, end - idx, chars, 0);
			word = new string (chars);
			idx = end;
		}

		// <s> 9 w=samochess ti=00 rt=1362  t=5 i=7 r=r tp=blitz c=? rr=0-9999 a=t f=t
		public static GameAdvertisement FromBuffer (byte[]buffer,
							    int start,
							    int end)
		{
			int tokbegin, tokend;
			SkipWhitespace (buffer, ref start, end);
			GameAdvertisement ad = new GameAdvertisement ();
			string str;
			ReadWord (buffer, ' ', ref start, end, out str);
			ad.gameHandle = Int32.Parse (str);
			while (start < end)
			  {
				  SkipWhitespace (buffer, ref start, end);
				  if (start >= end)
					  break;

				  string paramname, paramvalue;
				  ReadNameValue (buffer, ref start, end,
						 out paramname,
						 out paramvalue);
				  if (paramname.Equals ("w"))
					  ad.username = paramvalue;
				  else if (paramname.Equals ("rt"))
				    {
					    if (!System.Char.
						IsDigit (paramvalue
							 [paramvalue.Length -
							  1]))
					      {
						      ad.rating_char =
							      paramvalue
							      [paramvalue.
							       Length - 1];
						      paramvalue =
							      paramvalue.
							      Substring (0,
									 paramvalue.
									 Length
									 - 1);
					      }
					    ad.rating =
						    paramvalue[0] ==
						    '+' ? -1 : Int32.
						    Parse (paramvalue);
				    }
				  else if (paramname.Equals ("t"))
					  ad.time_limit =
						  Int32.Parse (paramvalue);
				  else if (paramname.Equals ("i"))
					  ad.time_increment =
						  Int32.Parse (paramvalue);
				  else if (paramname.Equals ("tp"))
					  ad.category = paramvalue;
				  else if (paramname.Equals ("r"))
					  ad.rated = paramvalue[0] == 'r';
				  else if (paramname.Equals ("c"))
					  ad.color = paramvalue[0];
				  else if (paramname.Equals ("a"))
					  ad.automatic = paramvalue[0] == 't';
				  else if (paramname.Equals ("f"))
					  ad.formula = paramvalue[0] == 't';
				  else if (paramname.Equals ("ti"))
					  ad.ti = Int32.Parse (paramvalue);
				  else if (paramname.Equals ("rr"))
				    {
					    string[]toks =
						    paramvalue.Split ('-');
					    ad.rating_range = new int[2];
					    ad.rating_range[0] =
						    Int32.Parse (toks[0]);
					    ad.rating_range[1] =
						    Int32.Parse (toks[1]);
				    }
			  }

			return ad;
		}
	}

	public class ParserException:Exception
	{
		public ParserException (string str):base (str)
		{
		}
	}

	public interface ICSParserListener
	{
		void GameAdvertisementMessage (GameAdvertisement msg);
	}

	public class ICSParser
	{
		public ICSParser ()
		{
		}

		public void StartProcessing (ICSParserListener listener)
		{
		}

		static void ParseGameRequestLine (string line,
						  out GameAdvertisement info)
		{
			// username (rating) seeking 5 3 unrated blitz [black] ("play 104" to respond)
			info = null;
			string[]tokens = line.Split ('(', ')');

			if (tokens.Length < 4)
				throw new ParserException (String.
							   Format
							   ("Invalid number of tokens ({0}) in the line:\n\t[{0}]",
							    tokens.Length,
							    line));

			string username = tokens[0].Trim ();
			string rating = tokens[1].Trim ();
			string gamestr = tokens[2];
			string option = tokens[3];

			// parse the game details
			tokens = gamestr.Split (' ');
			ArrayList toks = new ArrayList ();
			foreach (string token in tokens)
			{
				if (token.Length == 0)
					continue;
				toks.Add (token);
			}

			int time_limit = Int32.Parse ((string) toks[1]);
			int time_increment = Int32.Parse ((string) toks[2]);
			bool rated = toks[3].Equals ("rated");
			string category = (string) toks[4];

			string color = "[white]";

			if (toks.Count > 4)
				color = (string) toks[5];

			color = color.Substring (1, color.Length - 2);

			int gameHandle = -1;
			// Now parse the option
			tokens = option.Split (' ', '"');
			for (int i = 0; i < tokens.Length; i++)
			  {
				  if (tokens[i].Equals ("play"))
				    {
					    gameHandle =
						    Int32.
						    Parse (tokens[i + 1]);
					    break;
				    }
			  }

			if (gameHandle < 0)
				throw new ParserException (String.
							   Format
							   ("Couldn't find the game id in the string:\n\t[{0}]",
							    option));
			GameAdvertisement msg = new GameAdvertisement ();

			msg.username = username;
			msg.time_limit = time_limit;
			msg.time_increment = time_increment;
			msg.gameHandle = gameHandle;
			msg.rating =
				rating.IndexOf ('+') >=
				0 ? -1 : Int32.Parse (rating);
			msg.category = category;
			msg.rated = rated;

			info = msg;
		}
	}
}
