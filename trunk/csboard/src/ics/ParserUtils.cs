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

namespace CsBoard
{
	namespace ICS
	{
		public class ParserException:Exception
		{
			public ParserException (string str):base (str)
			{
			}
		}

		public class ParserUtils
		{

			public static void SkipWhitespace (byte[]buffer,
							   ref int idx,
							   int end)
			{
				SkipThisChar (buffer, ' ', ref idx, end);
			}

			public static void SkipThisChar (byte[]buffer,
							 char ch, ref int idx,
							 int end)
			{
				while (buffer[idx] == ch && idx < end)
					idx++;
			}

			public static void GotoThisChar (byte[]buffer,
							 char ch, ref int idx,
							 int end)
			{
				while (buffer[idx] != ch && idx < end)
					idx++;
			}

			public static void ReadNameValue (byte[]buffer,
							  ref int idx,
							  int end,
							  out string name,
							  out string val)
			{
				ReadWord (buffer, '=', ref idx, end,
					  out name);
				idx++;
				ReadWord (buffer, ' ', ref idx, end, out val);
			}

			public static void ReadWord (byte[]buffer, char delim,
						     ref int idx, int end,
						     out string word)
			{
				int bufend = end;
				for (int i = idx; i < end; i++)
				  {
					  if (buffer[i] == delim)
					    {
						    bufend = i;
						    break;
					    }
				  }

				ReadString (buffer, ref idx, bufend,
					    out word);
			}

			public static void ReadString (byte[]buffer,
						       ref int idx,
						       int bufend,
						       out string str)
			{
				System.Text.Decoder decoder =
					System.Text.Encoding.UTF8.
					GetDecoder ();

				char[] chars = new char[bufend - idx];
				decoder.GetChars (buffer, idx, bufend - idx,
						  chars, 0);
				str = new string (chars);
				idx = bufend;
			}

			public static string GetNextToken (byte[]buffer,
							   ref int start,
							   int end)
			{
				return GetNextToken (buffer, ' ', ref start,
						     end);
			}

			public static string GetNextToken (byte[]buffer,
							   char delim,
							   ref int start,
							   int end)
			{
				ParserUtils.SkipWhitespace (buffer, ref start,
							    end);
				string token;
				ParserUtils.ReadWord (buffer, delim,
						      ref start, end,
						      out token);
				//Console.WriteLine("NEXTTOKEN: {0}", token);
				return token;
			}

			public static int ParseMoveTime (string str,
							 out bool
							 inMilliseconds)
			{
				int idx = str.IndexOf ('.');
				int millisecs = 0;
				int factor = 1;

				if (idx >= 0)
				  {
					  millisecs =
						  Int32.Parse (str.
							       Substring (idx
									  +
									  1));
					  str = str.Substring (0, idx);
					  inMilliseconds = true;
					  factor = 1000;
				  }
				else
					inMilliseconds = false;

				string[]toks = str.Split (':');
				int time = 0;
				for (int i = 0; i < toks.Length; i++)
				  {
					  time *= 60;
					  int val = Int32.Parse (toks[i]);
					  val *= factor;
					  time += val;
				  }
				time += millisecs;

				return time;
			}

			public static bool Matches (byte[]buffer, int start,
						    int end, string str)
			{
				if (str.Length > end - start)
					return false;
				int cur = start;
				foreach (char ch in str)
				{
					if (buffer[cur++] != ch)
						return false;
				}

				return true;
			}
		}
	}
}
