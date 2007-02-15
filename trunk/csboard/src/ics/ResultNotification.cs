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

namespace CsBoard
{
	namespace ICS
	{
		public class ResultNotification
		{
			public int gameid;
			public string result;
			public string reason;

			public static ResultNotification
				FromBuffer (byte[]buffer, int start, int end)
			{
				string token;
				  token =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);
				if (!token.Equals ("Game"))
				  {
					  Console.WriteLine
						  ("Not a result notification");
					  return null;
				  }

				ResultNotification res =
					new ResultNotification ();
				  res.gameid =
					Int32.Parse (ParserUtils.
						     GetNextToken (buffer,
								   ref start,
								   end));
				int i = start;
				while (buffer[i] != ')' && i < end)
					  i++;
				if (i == end)
					return null;
				  start = i + 1;
				  ParserUtils.SkipWhitespace (buffer,
							      ref start, end);
				  ParserUtils.ReadWord (buffer, '}',
							ref start, end,
							out res.reason);
				  start++;
				  res.result =
					ParserUtils.GetNextToken (buffer,
								  ref start,
								  end);

				  return res;
			}

			public override string ToString ()
			{
				StringBuilder buf = new StringBuilder ();
				  buf.Append (String.
					      Format ("GameId: {0}\n",
						      gameid));
				  buf.Append (String.
					      Format ("Result: {0}\n",
						      result));
				  buf.Append (String.
					      Format ("Reason: {0}", reason));

				  return buf.ToString ();
			}
		}
	}
}
