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
using System.IO;

namespace Chess
{
	namespace Parser
	{
		public class PGNTokenizer
		{
			TextReader reader;
			bool returnDelimiterAsToken = false;

			string lastToken;
			int linesRead;

			public PGNTokenizer (TextReader rdr,
					     bool return_whitespace)
			{
				reader = rdr;
				returnDelimiterAsToken = return_whitespace;
				lastToken = null;
				linesRead = 0;
			}

			bool checkStringEscape = false;
			public bool CheckStringEscape
			{
				get
				{
					return checkStringEscape;
				}
				set
				{
					checkStringEscape = value;
				}
			}

			public bool ReturnDelimiterAsToken
			{
				set
				{
					returnDelimiterAsToken = value;
				}
				get
				{
					return returnDelimiterAsToken;
				}
			}

			public string nextToken ()
			{
				if (lastToken != null)
				  {
					  string ret = lastToken;
					  lastToken = null;
					  return ret;
				  }

				return __nextToken ();
			}

			public bool pushBackToken (string token)
			{
				if (lastToken != null)
					return false;
				lastToken = token;
				return true;
			}

			private string __nextToken ()
			{
				StringBuilder buffer = new StringBuilder ();
				int ch;
				/* skip whitespaces */
				while ((ch = reader.Read ()) >= 0)
				  {
					  //                      System.out.println((char)ch);
					  if (ch == '\n')
						  linesRead++;
					  if (Char.IsWhiteSpace ((char) ch))
					    {
						    if (returnDelimiterAsToken)
							    return (Char.
								    ToString ((char) ch));
					    }
					  else
						  break;
				  }

				/* first character already read above */
				if (ch < 0)
					return null;
				if (ch == '"')
					return returnDelimiterAsToken ? Char.
						ToString ((char) ch) :
						readString (reader);
				if (ch == '$')
					return '$' + readNAG (reader);

				/* self delimiting characters */
				if (".[](){}<>".IndexOf ((char) ch) >= 0)
					return Char.ToString ((char) ch);

				if (!Char.IsLetterOrDigit ((char) ch))
				  {
					  /* 
					   * TODO: throw a parsing exception
					   * for now just return the token and let the caller decide
					   */
					  //                      System.err.println("Excepting a letter or digit. Got this " + ((char) ch));
					  return Char.ToString ((char) ch);
				  }

				buffer.Append ((char) ch);

				/* read the remaining chars of the symbol */
				while ((ch = reader.Peek ()) >= 0)
				  {
					  if (!isSymbolChar ((char) ch))
					    {
						    break;
					    }
					  ch = reader.Read ();	/* call Read to actually read the char */
					  buffer.Append ((char) ch);
				  }

				return buffer.ToString ();
			}

			public int currentLine ()
			{
				return linesRead;
			}

			private string readNAG (TextReader reader)
			{
				StringBuilder buffer = new StringBuilder ();
				int ch;

				while (true)
				  {
					  ch = reader.Peek ();
					  if (ch < 0)
						  break;
					  if (!Char.IsDigit ((char) ch))
					    {
						    break;
					    }
					  else
					    {
						    ch = reader.Read ();
						    buffer.Append ((char) ch);
					    }
				  }

				return buffer.ToString ();
			}
			private string readString (TextReader reader)
			{
				int ch;
				StringBuilder buffer = new StringBuilder ();
				bool escape_char = false;

				while ((ch = reader.Read ()) >= 0)
				  {
					  if (checkStringEscape
					      && escape_char)
					    {
						    if (ch == '"')
							    buffer.Append
								    ('"');
						    else if (ch == '\\')
							    buffer.Append
								    ('\\');

						    escape_char = false;
						    continue;
					    }
					  if (checkStringEscape && ch == '\\')
					    {
						    escape_char = true;
						    continue;
					    }

					  if (ch == '"')
						  break;
					  buffer.Append ((char) ch);
				  }

				if (ch < 0)
				  {
					  /* TODO: throw a parsing exception here */
					  // Console.WriteLine("Parsing error: no \" found to end the string");
					  return null;
				  }

				return buffer.ToString ();
			}

			private bool isSymbolChar (char ch)
			{
				if (Char.IsLetterOrDigit (ch))
					return true;
				if ("!?_-+#=:/".IndexOf (ch) >= 0)
					return true;

				return false;
			}
		}
	}
}
