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
using System.IO;
using Gnome;
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		struct Tag
		{
			public string word;
			public Font font;
			public double width;
		}

		public class PrintWrapper
		{
			const bool SPLIT_IF_NEEDED = false;
			const bool DONT_SPLIT = true;
			PrintJob job;
			double curx, cury;
			double page_width, page_height;
			double marginx, marginy;
			double width, height;
			int pageno;
			ArrayList tags;
			string tab_string;
			Font font;
			double line_space_ratio;

			public double LineSpaceRatio
			{
				get
				{
					return line_space_ratio;
				}
				set
				{
					line_space_ratio = value;
				}
			}

			public Font Font
			{
				get
				{
					return font;
				}

				set
				{
					font = value;
				}
			}

			public PrintJob PrintJob
			{
				get
				{
					return job;
				}
			}

			public PrintWrapper ()
			{
				marginx = 50;
				marginy = 50;
				job = new PrintJob (PrintConfig.Default ());
				job.GetPageSize (out page_width,
						 out page_height);
				width = page_width - marginx - marginx;
				height = page_height - marginy - marginy;
				tags = new ArrayList ();
				tab_string = "        ";
				font = Font.
					FindClosestFromWeightSlant
					("monospace", FontWeight.Regular,
					 false, 8);
				line_space_ratio = 1.8;
			}

			public void Start ()
			{
				curx = marginx;
				cury = marginy + height;
				pageno = 1;
				Print.Beginpage (job.Context,
						 Catalog.GetString("Page ") + pageno++);
				Print.Moveto (job.Context, curx, cury);
			}

			public void PrintText (string text)
			{
				int begin = 0;
				for (int i = 0; i < text.Length; i++)
				  {
					  char ch = text[i];
					  if (Char.IsLetterOrDigit (ch))
						  continue;
					  if (begin < i)
					    {
						    PrintWord (text.
							       Substring
							       (begin,
								i - begin),
							       font,
							       SPLIT_IF_NEEDED);
					    }
					  PrintChar (ch, font);
					  begin = i + 1;	// the word might begin at the next offset
				  }
				if (begin < text.Length - 1)
				  {
					  PrintWord (text.Substring (begin),
						     font, SPLIT_IF_NEEDED);
				  }
			}

			private void SplitWord (string word, Font font,
						double available_width,
						out string left,
						out string right)
			{
				double total_width = 0;
				for (int i = 0; i < word.Length; i++)
				  {
					  char ch = word[i];
					  double char_width =
						  font.GetWidthUtf8 ("" + ch);
					  total_width += char_width;
					  if (total_width > available_width)
					    {
						    left = word.Substring (0,
									   i);
						    right = word.
							    Substring (i);
						    return;
					    }
				  }

				left = null;
				right = null;
			}

			private void PrintWord (string word, Font font,
						bool ignore_split)
			{
				double width_required =
					font.GetWidthUtf8 (word);
				Tag tag;

				// word fits in the line
				if ((curx + width_required) <
				    (marginx + width))
				  {
					  tag.word = word;
					  tag.font = font;
					  tag.width = width_required;
					  tags.Add (tag);
					  curx += width_required;
					  return;
				  }

				// this is a loooong word.
				if (width_required > width)
				  {
					  if (ignore_split)
					    {	// for tabs we dont need to split
						    FlushLine ();
					    }
					  else
					    {
						    string part, extra;
						    SplitWord (word, font,
							       marginx +
							       width - curx,
							       out part,
							       out extra);
						    tag.word = part;
						    tag.font = font;
						    tag.width =
							    font.
							    GetWidthUtf8
							    (part);
						    tags.Add (tag);
						    FlushLine ();
						    PrintWord (extra, font, ignore_split);	// recursive!
					    }
					  return;
				  }

				// the 'word' has to go to the next line
				FlushLine ();
				tag.word = word;
				tag.font = font;
				tag.width = width_required;
				curx += width_required;
				tags.Add (tag);
			}

			// returns true if a new page is created
			private void FlushLine ()
			{
				PrintContext ctx = job.Context;
				double max_line_space = 0;
				foreach (Tag tag in tags)
				{
					Font tagfont = tag.font;
					double line_space =
						(tagfont.Descender +
						 tagfont.Ascender) *
						line_space_ratio;
					if (line_space > max_line_space)
						max_line_space = line_space;
				}

				if (max_line_space == 0)
				  {
					  // blank line. get the height from the current font
					  max_line_space =
						  (font.Descender +
						   font.Ascender) *
						  line_space_ratio;
				  }

				// now print the line
				curx = marginx;
				cury -= max_line_space;
				if (cury < marginy)
				  {
					  Print.Showpage (ctx);
					  Print.Beginpage (ctx,
							   Catalog.GetString("Page ") +
							   pageno++);
					  cury = marginy + height;
				  }

				foreach (Tag tag in tags)
				{
					Print.Moveto (ctx, curx, cury);
					Print.Setfont (ctx, tag.font);
					Print.Show (ctx, tag.word);
					curx += tag.width;
				}

				tags.Clear ();
				curx = marginx;
			}

			public void LineBreak ()
			{
				FlushLine ();
			}

			public void HorizontalLineBreak ()
			{
				if (tags.Count > 0)
					FlushLine ();

				LineBreak ();

				PrintContext ctx = job.Context;
				curx = marginx;
				Print.Moveto (ctx, curx, cury);
				Print.Lineto (ctx, curx + width, cury);
				Print.Stroke (ctx);

				LineBreak ();
			}

			public void PageBreak ()
			{
				if (tags.Count > 0)
					FlushLine ();
				PrintContext ctx = job.Context;
				Print.Showpage (ctx);
				curx = marginx;
				cury = marginy + height;
				Print.Beginpage (ctx, Catalog.GetString("Page ") + pageno++);
			}

			public void PrintImage (Gdk.Pixbuf image)
			{
				if (tags.Count > 0)
					FlushLine ();
				LineBreak ();

				PrintContext ctx = job.Context;
				if (cury - marginy < image.Height)
				  {
					  Print.Showpage (ctx);
					  Print.Beginpage (ctx,
							   Catalog.GetString("Page ") +
							   pageno++);
					  curx = marginx;
					  cury = marginy + height;
				  }
				cury -= image.Height;
				Print.Moveto (ctx, curx, cury);

				Print.Gsave (ctx);
				Print.Translate (ctx, curx, cury);
				Print.Scale (ctx, image.Width, image.Height);
				Print.Pixbuf (ctx, image);
				Print.Grestore (ctx);

				LineBreak ();
			}

			void PrintChar (char ch, Font font)
			{
				if (ch == '\n')
				  {
					  FlushLine ();
				  }
				else if (ch == '\t')
				  {
					  PrintWord (tab_string, font,
						     DONT_SPLIT);
				  }
				else
				  {
					  PrintWord ("" + ch, font,
						     SPLIT_IF_NEEDED);
				  }
			}

			public void End ()
			{
				if (tags.Count > 0)
					FlushLine ();
				Print.Showpage (job.Context);
				job.Close ();
			}

			public void Export (string file)
			{
				job.PrintToFile (file);
				job.Print ();
			}
		}
	}
}
