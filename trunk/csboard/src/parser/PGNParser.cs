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
using System.IO;
using System.Text;

namespace Chess
{
	namespace Parser
	{
		public class GameLoadedEventArgs:EventArgs
		{
			PGNChessGame game;
			public PGNChessGame Game
			{
				get
				{
					return game;
				}
			}

			public GameLoadedEventArgs (PGNChessGame game):base ()
			{
				this.game = game;
			}
		}

		public delegate void GameLoadedEvent (System.Object o,
						      GameLoadedEventArgs
						      args);

		public class PGNParser
		{
			public event GameLoadedEvent GameLoaded;

			PGNTokenizer tokenizer;

			public PGNParser (TextReader reader)
			{
				tokenizer = new PGNTokenizer (reader, false);
			}

			public void Parse ()
			{
				bool tagFound = false;
				while (ReadGame (ref tagFound))
					;
			}

			private bool ReadGame (ref bool tagFound)
			{
				ArrayList tagList = new ArrayList ();

				string token;

				if (tagFound)
				  {
					  token = "[";
					  tagFound = false;	// clear it now
				  }
				else
					token = tokenizer.nextToken ();

				if (token == null)
					return false;

				/* read tag-value pairs */
				while (token != null)
				  {
					  if (token.Equals ("["))
					    {
						    readTagValuePair (tagList,
								      tokenizer);
					    }
					  else
						  break;
					  token = tokenizer.nextToken ();
				  }

				/* now parse the game */
				return loadMoves (token, tagList,
						  ref tagFound);
			}

			private static void ignoreLine (string token,
							PGNTokenizer
							tokenizer)
			{
				tokenizer.ReturnDelimiterAsToken = true;
				while ((token =
					tokenizer.nextToken ()) != null)
				  {
					  if (token.Equals ("\n"))
						  break;
				  }
				tokenizer.ReturnDelimiterAsToken = false;
			}

			// This type of a comment starts with ';' and goes on till the end of the line
			private static string readLineComment (string token,
							       PGNTokenizer
							       tokenizer)
			{
				StringBuilder buffer = new StringBuilder ();
				tokenizer.ReturnDelimiterAsToken = true;
				while ((token =
					tokenizer.nextToken ()) != null)
				  {
					  if (token.Equals ("\n"))
						  break;
					  buffer.Append (token);
				  }
				tokenizer.ReturnDelimiterAsToken = false;
				return buffer.ToString ();
			}

			private static string readString (string token,
							  PGNTokenizer
							  tokenizer)
			{
				bool orig = tokenizer.ReturnDelimiterAsToken;
				tokenizer.ReturnDelimiterAsToken = true;
				StringBuilder buffer = new StringBuilder ();
				while ((token =
					tokenizer.nextToken ()) != null)
				  {
					  if (token.Equals ("\\"))
					    {	// escape char
						    string nextToken =
							    tokenizer.
							    nextToken ();
						    if (nextToken == null)
							    throw new
								    PGNParserException
								    ("No character after \\");
						    if (!nextToken.
							Equals ("\"")
							&& !nextToken.
							Equals ("\\"))
							    throw new
								    PGNParserException
								    ("Invalid escape char: "
								     +
								     nextToken
								     [0]);
						    buffer.Append (nextToken);
					    }
					  else if (token.Equals ("\""))	// end of the string
						  break;
					  else
						  buffer.Append (token);
				  }

				tokenizer.ReturnDelimiterAsToken = orig;
				return buffer.ToString ();
			}

			private bool loadMoves (string
						initialtoken,
						ArrayList tagList,
						ref bool tagFound)
			{
				string token;
				StringBuilder commentBuffer =
					new StringBuilder ();
				int moveidx = -1;
				ArrayList moves = new ArrayList ();
				ChessMove move = null;
				string initialComment = null;

				if (initialtoken == null)
					token = tokenizer.nextToken ();
				else
					token = initialtoken;

				for (; token != null;
				     token = tokenizer.nextToken ())
				  {
					  //      if(token.Equals("{") || token.Equals("(") || token.Equals("<")) {
					  if (token.Equals ("%"))
					    {
						    ignoreLine (token,
								tokenizer);
						    continue;
					    }
					  else if (token.Equals (";"))
					    {
						    string comment =
							    readLineComment
							    (token,
							     tokenizer);
						    commentBuffer.
							    Append (comment);
						    continue;
					    }
					  else if (token.Equals ("{")
						   || token.Equals ("("))
					    {
						    string comment =
							    readComment
							    (token,
							     tokenizer);
						    commentBuffer.
							    Append (comment);
						    continue;
					    }
					  else if (isNAG (token))
					    {
						    /* TODO: convert comment into a nag */
						    commentBuffer.
							    Append (" " +
								    token +
								    " ");
						    continue;
					    }
					  else if (tokenIsATermination
						   (token))
					    {
						    /* end of game */
						    break;
					    }
					  else if (token.Equals ("["))
					    {
						    Console.WriteLine
							    ("Abrupt end of the game. Didnt find the termination");
						    tagFound = true;
						    break;
					    }

					  if (moveidx > 0
					      && token.Equals ("."))
						  continue;

					  /* process moves */
					  bool token_is_a_number =
						  isNumber (token);
					  if (!token_is_a_number
					      && moveidx < 0)
						  throw new
							  PGNParserException
							  ("Line " +
							   tokenizer.
							   currentLine () +
							   ": Expecting a number. Got this token: ["
							   + token + "]");

					  if (token_is_a_number)
					    {
						    int val =
							    Int32.
							    Parse (token);
						    if (moveidx < 0)
						      {	// first time
							      moveidx = val;
							      move = new
								      ChessMove
								      (moveidx);
							      /* if there is a comment here.. add it to the previous move
							       * If there is no previous move.. then the comment is at the
							       * beginning of the game. So, create a dummy chess move.
							       */
							      if (commentBuffer.Length > 0)
								{
									if (moves.Count == 0)
									  {
										  initialComment
											  =
											  commentBuffer.
											  ToString
											  ();
										  commentBuffer.
											  Remove
											  (0,
											   commentBuffer.
											   Length);
									  }
									else
									  {
										  ChessMove
											  previousmove
											  =
											  (ChessMove)
											  moves
											  [moves.
											   Count
											   -
											   1];
										  previousmove.
											  blackComment
											  =
											  commentBuffer.
											  ToString
											  ();
										  commentBuffer.
											  Remove
											  (0,
											   commentBuffer.
											   Length);
									  }
								}
						      }
						    else if (moveidx != val)
							    throw new
								    PGNParserException
								    ("Line: "
								     +
								     tokenizer.
								     currentLine
								     ());
					    }
					  else if (move.whitemove == null)
					    {
						    /* first token after move number */
						    move.whitemove = token;
					    }
					  else if (move.blackmove == null)
					    {
						    move.blackmove = token;
						    if (commentBuffer.Length >
							0)
						      {
							      move.whiteComment = commentBuffer.ToString ();
							      commentBuffer.
								      Remove
								      (0,
								       commentBuffer.
								       Length);
						      }
						    /* at this point we have the moveidx, whitemove and blackmove
						     * Now create a chessmove. If there is any comment after this
						     * it will be added later.
						     */
						    moves.Add (move);
						    moveidx = -1;
						    move = null;
					    }
				  }

				if (commentBuffer.Length > 0)
				  {
					  if (move == null)
					    {
						    ChessMove previousmove =
							    (ChessMove)
							    moves[moves.
								  Count - 1];
						    previousmove.
							    blackComment =
							    commentBuffer.
							    ToString ();
						    commentBuffer.Remove (0,
									  commentBuffer.
									  Length);
					    }
					  else
					    {
						    if (move.blackmove ==
							null)
							    move.whiteComment
								    =
								    commentBuffer.
								    ToString
								    ();
						    else
							    move.blackComment
								    =
								    commentBuffer.
								    ToString
								    ();
					    }
				  }

				if (move != null)
					moves.Add (move);

				PGNChessGame game =
					new PGNChessGame (initialComment,
							  tagList, moves);
				if (GameLoaded != null)
				  {
					  GameLoaded (this,
						      new
						      GameLoadedEventArgs
						      (game));
				  }
				return true;
			}

			// Read the Numerical Annotated Glyph
			private static bool isNAG (string nag)
			{
				if (nag == null || nag.Length <= 1)
					return false;

				if (nag[0] != '$')
					return false;

				for (int i = 1; i < nag.Length; i++)
					if (!Char.IsDigit (nag[i]))
						return false;

				return true;
			}

			private static bool isNumber (string token)
			{
				if (token == null || token.Length < 1)
					return false;

				for (int i = 0; i < token.Length; i++)
					if (!Char.IsDigit (token[i]))
						return false;

				return true;
			}

			private static IDictionary matching_tokens =
				MatchingTokens ();
			private static IDictionary MatchingTokens ()
			{
				IDictionary matching_tokens =
					new Hashtable ();
				matching_tokens["{"] = "}";
				matching_tokens["["] = "]";
				matching_tokens["<"] = ">";
				matching_tokens["("] = ")";
				matching_tokens[";"] = "\n";
				matching_tokens["%"] = "\n";
				return matching_tokens;
			}

			private static bool isSpecialToken (string token)
			{
				if (token.Equals ("{") || token.Equals ("\\"))
					return true;
				return false;
			}

			private static string readComment (string begintoken,
							   PGNTokenizer
							   tokenizer)
			{
				string token = begintoken;
				StringBuilder commentBuffer =
					new StringBuilder ();
				Stack stack = new Stack ();
				string expected_token =
					(string) matching_tokens[token];

				stack.Push (token);
				//      commentBuffer.Append(token);
				tokenizer.ReturnDelimiterAsToken = true;

				while ((token =
					tokenizer.nextToken ()) != null)
				  {
					  if (token.Equals ("\\"))
					    {
						    string nextToken =
							    tokenizer.
							    nextToken ();
						    if (nextToken == null)
							    throw new
								    PGNParserException
								    ("Unable to find the next token after the escape char");
						    if (!isSpecialToken
							(nextToken))
							    throw new
								    PGNParserException
								    ("Invalid escape char: "
								     +
								     nextToken);
						    commentBuffer.
							    Append
							    (nextToken);
						    continue;
					    }
					  //      if(token.Equals("{") || token.Equals("(") || token.Equals("[") || token.Equals("<")) {
					  if (token.Equals ("{")
					      || token.Equals (begintoken))
					    {
						    stack.Push (token);
						    expected_token =
							    (string)
							    matching_tokens
							    [token];
						    if (commentBuffer.Length >
							0)
						      {
							      // Another comment within a comment. Add one extra space to the comment buffer since
							      // we are not adding the delims
							      commentBuffer.
								      Append
								      (" ");
						      }
					    }
					  else if (token.
						   Equals (expected_token))
					    {
						    stack.Pop ();
						    if (stack.Count == 0)
							    break;
						    expected_token =
							    (string)
							    matching_tokens
							    [stack.Peek ()];
					    }
					  else if (token.Equals ("\""))
					    {
						    commentBuffer.
							    Append (token);
						    commentBuffer.
							    Append (readString
								    (token,
								     tokenizer));
						    commentBuffer.
							    Append (token);
					    }
					  else
						  commentBuffer.
							  Append (token);
				  }

				if (token == null)
				  {
					  StringBuilder stackinfo =
						  new StringBuilder ();
					  foreach (string str in stack)
						  stackinfo.Append (str +
								    ", ");
					  throw new
						  PGNParserException
						  ("Waiting for delimiter tokens for: "
						   + stackinfo);
				  }

				tokenizer.ReturnDelimiterAsToken = false;
				return commentBuffer.ToString ();
			}

			/*  
			   private static void loadTokensTillDelim(string delim, StringBuilder buf, PGNTokenizer tokenizer) {
			   string token;
			   while((token = tokenizer.nextToken()) != null) {
			   buf.Append(token);
			   if(token.Equals(delim))
			   return;
			   }

			   throw new PGNParserException();
			   }
			 */

			private static void readTagValuePair (IList
							      tagList,
							      PGNTokenizer
							      tokenizer)
			{
				string name, value;
				if ((name = tokenizer.nextToken ()) == null)
					throw new
						PGNParserException
						("Reached the end after starting a token begin!");
				if (name.Equals ("]"))	/* empty tag */
					return;

				StringBuilder value_buf =
					new StringBuilder ();
				while (true)
				  {
					  value = tokenizer.nextToken ();
					  if (value == null)
					    {
						    throw new
							    PGNParserException
							    ("No more tokens but i'm trying to read the tag value");
					    }
					  if (value.Equals ("]"))
					    {
						    PGNTag tag =
							    new PGNTag (name,
									extractTagValue
									(value_buf.
									 ToString
									 ()));
						    if (!tagList.
							Contains (tag))
							    tagList.Add (tag);
						    break;
					    }
					  value_buf.Append (value);
				  }
			}

			private static string extractTagValue (string str)
			{
				str = str.Trim ();
				if (str.Length == 0)
					return str;

				if (str[0] == '"'
				    && str[str.Length - 1] == '"')
					str = str.Substring (1,
							     str.Length - 2);
				return str;
			}

			private static bool tokenIsATermination (string token)
			{
				if (token.Equals ("1-0")
				    || token.Equals ("0-1")
				    || token.Equals ("1/2-1/2")
				    || token.Equals ("*"))
					return true;
				return false;
			}
		}
	}
}
