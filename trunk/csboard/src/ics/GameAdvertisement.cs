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
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class GameAdvertisement
		{
			const int SEEK_AD_TITLE_UNREGISTERED = 0x1;
			const int SEEK_AD_TITLE_COMPUTER = 0x2;
			const int SEEK_AD_TITLE_GM = 0x4;
			const int SEEK_AD_TITLE_IM = 0x8;
			const int SEEK_AD_TITLE_FM = 0x10;
			const int SEEK_AD_TITLE_WGM = 0x20;
			const int SEEK_AD_TITLE_WIM = 0x40;
			const int SEEK_AD_TITLE_WFM = 0x80;

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
					return color ==
						'?' ? "Any color" : color ==
						'w' ? "White" : "Black";
				}
			}

			public string Flags
			{
				get
				{
					string str =
						automatic ? "Automatic" :
						"Manual";
					  return str + ", Formula " +
						(formula ? "used" :
						 "not used");
				}
			}

			public string Range
			{
				get
				{
					return rating_range ==
						null ? "No range" : String.
						Format ("{0}-{1}",
							rating_range[0],
							rating_range[1]);
				}
			}

			public bool IsComputer
			{
				get
				{
					return (ti & SEEK_AD_TITLE_COMPUTER)
						!= 0;
				}
			}

			public bool IsUnregistered
			{
				get
				{
					return (ti &
						SEEK_AD_TITLE_UNREGISTERED) !=
						0;
				}
			}

			public bool IsGM
			{
				get
				{
					return (ti & SEEK_AD_TITLE_GM) != 0
						|| (ti & SEEK_AD_TITLE_WGM) !=
						0;
				}
			}

			public bool IsIMOrFM
			{
				get
				{
					return (ti & SEEK_AD_TITLE_IM) != 0
						|| (ti & SEEK_AD_TITLE_WIM) !=
						0
						|| (ti & SEEK_AD_TITLE_FM) !=
						0
						|| (ti & SEEK_AD_TITLE_WFM) !=
						0;
				}
			}

			public bool IsProvisional
			{
				get
				{
					return rating_char == 'P';
				}
			}

			public bool IsEstimated
			{
				get
				{
					return rating_char == 'E';
				}
			}

			public override string ToString ()
			{
				StringBuilder buffer = new StringBuilder ();
				buffer.Append (String.
					       Format
					       ("Game Id          : {0}\n",
						gameHandle));
				buffer.Append (String.
					       Format
					       ("Name             : {0}\n",
						username));
				buffer.Append (String.
					       Format
					       ("Rating           : {0}\n",
						rating));

				buffer.Append (String.
					       Format
					       ("Time limit       : {0}\n",
						time_limit));
				buffer.Append (String.
					       Format
					       ("Time increment   : {0}\n",
						time_increment));
				buffer.Append (String.
					       Format
					       ("Rated            : {0}\n",
						rated));

				buffer.Append (String.
					       Format
					       ("Category         : {0}\n",
						category));
				buffer.Append (String.
					       Format
					       ("Color            : {0}\n",
						color));
				buffer.Append (String.
					       Format
					       ("Automatic        : {0}\n",
						automatic));
				buffer.Append (String.
					       Format
					       ("Formula          : {0}",
						formula));

				return buffer.ToString ();
			}

			public string ToPango ()
			{
				StringBuilder buffer = new StringBuilder ();
				if (rated)
					buffer.Append (String.
						       Format
						       ("<span color=\"#202080\">"));
				buffer.Append (String.Format("<b>{0}</b>\n",username));
				buffer.Append (String.Format("<i>Color</i>: {0}, ",
						color ==
						'?' ? Catalog.GetString("Any") : color ==
						'w' ? Catalog.GetString("White") : Catalog.GetString("Black")));
				buffer.Append (String.
					       Format
					       (Catalog.GetString("<i>Acceptance</i>: {0}, <i>Formula</i>: {1}"),
						automatic ? Catalog.GetString("Automatic") :
						Catalog.GetString("Manual"),
						formula ? Catalog.GetString("yes") : Catalog.GetString("no")));
				if (rated)
					buffer.Append ("</span>");

				return buffer.ToString ();
			}

			public static void ReadCancellations (byte[]buffer,
							      int start,
							      int end,
							      ArrayList list)
			{
				while (start < end)
				  {
					  ParserUtils.SkipWhitespace (buffer,
								      ref
								      start,
								      end);
					  if (start >= end)
						  break;
					  string word;
					  ParserUtils.ReadWord (buffer, ' ',
								ref start,
								end,
								out word);
					  list.Add (Int32.Parse (word));
				  }
			}

			// <s> 9 w=samochess ti=00 rt=1362  t=5 i=7 r=r tp=blitz c=? rr=0-9999 a=t f=t
			public static GameAdvertisement
				FromBuffer (byte[]buffer, int start, int end)
			{
				int tokbegin, tokend;
				ParserUtils.SkipWhitespace (buffer, ref start,
							    end);
				GameAdvertisement ad =
					new GameAdvertisement ();
				string str;
				ParserUtils.ReadWord (buffer, ' ', ref start,
						      end, out str);
				ad.gameHandle = Int32.Parse (str);
				while (start < end)
				  {
					  ParserUtils.SkipWhitespace (buffer,
								      ref
								      start,
								      end);
					  if (start >= end)
						  break;

					  string paramname, paramvalue;
					  ParserUtils.ReadNameValue (buffer,
								     ref
								     start,
								     end,
								     out
								     paramname,
								     out
								     paramvalue);
					  if (paramname.Equals ("w"))
						  ad.username = paramvalue;
					  else if (paramname.Equals ("rt"))
					    {
						    if (!System.Char.
							IsDigit (paramvalue
								 [paramvalue.
								  Length -
								  1]))
						      {
							      ad.rating_char =
								      paramvalue
								      [paramvalue.
								       Length
								       - 1];
							      paramvalue =
								      paramvalue.
								      Substring
								      (0,
								       paramvalue.
								       Length
								       - 1);
						      }
						    ad.rating =
							    paramvalue[0] ==
							    '+' ? -1 : Int32.
							    Parse
							    (paramvalue);
					    }
					  else if (paramname.Equals ("t"))
						  ad.time_limit =
							  Int32.
							  Parse (paramvalue);
					  else if (paramname.Equals ("i"))
						  ad.time_increment =
							  Int32.
							  Parse (paramvalue);
					  else if (paramname.Equals ("tp"))
						  ad.category = paramvalue;
					  else if (paramname.Equals ("r"))
						  ad.rated =
							  paramvalue[0] ==
							  'r';
					  else if (paramname.Equals ("c"))
						  ad.color = paramvalue[0];
					  else if (paramname.Equals ("a"))
						  ad.automatic =
							  paramvalue[0] ==
							  't';
					  else if (paramname.Equals ("f"))
						  ad.formula =
							  paramvalue[0] ==
							  't';
					  else if (paramname.Equals ("ti"))
						  ad.ti = Int32.
							  Parse (paramvalue);
					  else if (paramname.Equals ("rr"))
					    {
						    string[]toks =
							    paramvalue.
							    Split ('-');
						    ad.rating_range =
							    new int[2];
						    ad.rating_range[0] =
							    Int32.
							    Parse (toks[0]);
						    ad.rating_range[1] =
							    Int32.
							    Parse (toks[1]);
					    }
				  }

				return ad;
			}
		}
	}
}
