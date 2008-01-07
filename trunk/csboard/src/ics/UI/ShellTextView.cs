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
using Gtk;
using Mono.Unix;
using System.Text.RegularExpressions;

namespace CsBoard
{
	namespace ICS
	{
		public class ShellTextView:TextView
		{
			int max_chars;
			const string NORMAL_TAG = "normal_tag";
			const string TALK_TAG = "talk_tag";
			const string COMMAND_TAG = "command_tag";
			const string GAMEAD_TAG = "gamead_tag";
			const string MESSAGE_TAG = "message_tag";
			const string USERNAME_TAG = "user_tag";
			const string USERMSG_TAG = "usermsg_tag";

			const string GAME_AD_PATTERN =
				"[\\w()\\d]+\\s+\\([\\s\\+\\d-]+\\)\\s+seeking*";
			protected TextTag normalTag;
			protected TextTag talkTag;
			protected TextTag commandTag,
				gameadTag, messageTag, usernameTag,
				usermsgTag;

			public ShellTextView (ICSClient client):base ()
			{
				max_chars = 16 * 1024;
				WrapMode = WrapMode.Word;
				Editable = false;
				ModifyFont (Pango.FontDescription.
					    FromString ("Monospace 9"));
				client.LineReceivedEvent += OnLineReceived;
				CreateTags ();
			}


			private void CreateTags ()
			{
				TextTag tag;
				  tag = new TextTag (NORMAL_TAG);
				  tag.Style = Pango.Style.Normal;
				  Buffer.TagTable.Add (tag);
				  normalTag = tag;

				  tag = new TextTag (TALK_TAG);
				  tag.Weight = Pango.Weight.Bold;
				  tag.Foreground = "#560303";
				  tag.Style = Pango.Style.Normal;
				  Buffer.TagTable.Add (tag);
				  talkTag = tag;

				  tag = new TextTag (COMMAND_TAG);
				  tag.Weight = Pango.Weight.Bold;
				  tag.Foreground = "#030356";
				  tag.Style = Pango.Style.Normal;
				  Buffer.TagTable.Add (tag);
				  commandTag = tag;

				  tag = new TextTag (GAMEAD_TAG);
				  tag.Weight = Pango.Weight.Normal;
				  tag.Foreground = "#909090";
				  tag.Style = Pango.Style.Italic;
				  Buffer.TagTable.Add (tag);
				  gameadTag = tag;

				  tag = new TextTag (MESSAGE_TAG);
				  tag.Weight = Pango.Weight.Bold;
				  tag.Foreground = "#309030";
				  tag.Style = Pango.Style.Normal;
				  Buffer.TagTable.Add (tag);
				  messageTag = tag;

				  tag = new TextTag (USERNAME_TAG);
				  tag.Weight = Pango.Weight.Bold;
				  tag.Foreground = "#801010";
				  tag.Underline = Pango.Underline.Single;
				  Buffer.TagTable.Add (tag);
				  usernameTag = tag;

				  tag = new TextTag (USERMSG_TAG);
				  tag.Weight = Pango.Weight.Normal;
				  tag.Foreground = "#808020";
				  tag.Style = Pango.Style.Normal;
				  Buffer.TagTable.Add (tag);
				  usermsgTag = tag;
			}

			private void trim_extra_chars (string line)
			{
				TextBuffer buffer = Buffer;
				int len = line.Length;

				while (buffer.CharCount + len > max_chars)
				  {
					  // remove a line from the beginning of the buffer
					  TextIter startIter =
						  buffer.StartIter;
					  TextIter endIter = startIter;
					  if (!endIter.ForwardToLineEnd ())
						    break;
					    buffer.Delete (ref startIter,
							   ref endIter);
				  }
			}

			public void AddCommandToBuffer (string line)
			{
				trim_extra_chars (line);
				append (line, commandTag, true);
			}

			private void add_line_to_buffer (string line,
							 LineType linetype)
			{
				trim_extra_chars (line);
				switch (linetype)
				  {
				  case LineType.Normal:
					  handle_normalline (line);
					  break;
				  case LineType.Talk:
					  append (line, talkTag, true);
					  break;
				  default:
					  append (line, normalTag, true);
					  break;
				  }
			}

			private void handle_normalline (string line)
			{
				if (Regex.IsMatch (line, GAME_AD_PATTERN))
				  {
					  append (line, gameadTag, true);
					  return;
				  }

				Regex reg = new Regex ("(?<user>^[\\w\\d]+)" +
						       "(?<usrxtra>[()\\d\\w*]+)?"
						       +
						       "(?<xtra> (tells you)|(shouts)|(c-shouts))?"
						       + ":(?<msg>.+)");
				Match m = reg.Match (line);
				if (m.Success)
				  {
					  append (m.Groups["user"].Value,
						  usernameTag, false);
					  append (m.Groups["usrxtra"].Value,
						  normalTag, false);
					  append (m.Groups["xtra"].Value,
						  normalTag, false);
					  append (":" + m.Groups["msg"].Value,
						  usermsgTag, true);
					  return;
				  }

				if (line[0] == '\\')
					append (line.Substring (1),
						usermsgTag, true);
				else
					append (line, normalTag, true);
			}

			private void append (string line, TextTag tag,
					     bool newline)
			{
				TextIter iter = Buffer.EndIter;
				Buffer.InsertWithTags (ref iter, line, tag);
				if (newline)
					Buffer.InsertWithTags (ref iter, "\n",
							       tag);
				ScrollToIter (Buffer.EndIter, 0, false, 0, 0);
			}

			private void OnLineReceived (object o,
						     LineReceivedEventArgs
						     args)
			{
				if (args.LineType != LineType.Normal
				    && args.LineType != LineType.Talk)
					return;
				string line = args.Line;
				add_line_to_buffer (line, args.LineType);
			}
		}
	}
}
