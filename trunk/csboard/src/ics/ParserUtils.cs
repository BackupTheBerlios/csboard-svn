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

namespace CsBoard {
	namespace ICS {
	public class ParserException:Exception
	{
		public ParserException (string str):base (str)
		{
		}
	}

	public class ParserUtils {

		public static void SkipWhitespace (byte[]buffer, ref int idx,
					    int end)
		{
			while (buffer[idx] == ' ' && idx < end)
				idx++;
		}

		public static void ReadNameValue (byte[]buffer, ref int idx, int end,
					   out string name, out string val)
		{
			ReadWord (buffer, '=', ref idx, end, out name);
			idx++;
			ReadWord (buffer, ' ', ref idx, end, out val);
		}

		public static void ReadWord (byte[]buffer, char delim, ref int idx,
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
	}
	}
}
