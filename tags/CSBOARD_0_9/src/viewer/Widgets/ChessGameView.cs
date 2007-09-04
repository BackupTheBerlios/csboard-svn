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
using System.Text;
using Gtk;
using Chess.Parser;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public class MoveEventArgs:EventArgs
		{
			public int nthMove;
			public MoveEventArgs (int n)
			{
				nthMove = n;
			}
		}

		public delegate void NthMoveEvent (object o,
						   MoveEventArgs args);

		public class ChessGameView:VBox
		{
			ChessGame game;
			int curMoveIdx;

			public event NthMoveEvent ShowNthMove;

			TextView view;
			public Widget Widget
			{
				get
				{
					return view;
				}
			}

			Gdk.Cursor handCursor, regularCursor;

			public ChessGameView ():base ()
			{
				handCursor =
					new Gdk.Cursor (Gdk.CursorType.Hand2);
				regularCursor =
					new Gdk.Cursor (Gdk.CursorType.Xterm);

				marks = new Hashtable ();
				tag_links = new Hashtable ();
				taglinks = new ArrayList ();
				curMoveIdx = -1;

				view = new TextView ();
				view.WrapMode = WrapMode.Word;
				view.Editable = false;
				view.WidgetEventAfter += EventAfter;
				view.MotionNotifyEvent += MotionNotify;
				view.VisibilityNotifyEvent +=
					VisibilityNotify;

				ScrolledWindow win = new ScrolledWindow ();
				  win.SetPolicy (PolicyType.Never,
						 PolicyType.Automatic);
				  win.Add (view);

				  PackStart (win, true, true, 0);
				  view.WidthRequest = 150;

				  CreateTags ();
				  ShowAll ();
			}

			const string NORMAL_TAG = "normal_tag";
			const string HEADING_TAG = "heading_tag";
			const string BOLD_TAG = "bold_tag";
			const string GAMETAGNAME_TAG = "gametagname_tag";
			const string HIGHLIGHTED_TAG = "highlighted_tag";
			const string COMMENT_TAG = "comment_tag";
			const string MOVENUMBER_TAG = "movenumber_tag";
			const string MOVE_TAG = "move_tag";

			protected TextTag normalTag;
			protected TextTag headingTag;
			protected TextTag boldTag;
			protected TextTag gametagnameTag;
			protected TextTag highlightedTag;
			protected TextTag commentTag;
			protected TextTag movenumberTag;
			protected TextTag moveTag;

			Hashtable marks;
			Hashtable tag_links;
			ArrayList taglinks;

			private void CreateTags ()
			{
				TextTag tag;
				tag = new TextTag (NORMAL_TAG);
				tag.Style = Pango.Style.Normal;
				view.Buffer.TagTable.Add (tag);
				normalTag = tag;

				tag = new TextTag (MOVENUMBER_TAG);
				tag.Weight = Pango.Weight.Bold;
				tag.Foreground = "#404040";
				tag.Style = Pango.Style.Normal;
				view.Buffer.TagTable.Add (tag);
				movenumberTag = tag;

				tag = new TextTag (MOVE_TAG);
				tag.Style = Pango.Style.Normal;
				tag.FontDesc =
					Pango.FontDescription.
					FromString ("monospace");
				//tag.Weight = Pango.Weight.Bold;
				view.Buffer.TagTable.Add (tag);
				moveTag = tag;

				tag = new TextTag (HEADING_TAG);
				tag.Weight = Pango.Weight.Bold;
				tag.Foreground = "brown";
				tag.PixelsBelowLines = 10;
				tag.Scale = Pango.Scale.XLarge;
				view.Buffer.TagTable.Add (tag);
				headingTag = tag;

				tag = new TextTag (BOLD_TAG);
				tag.Weight = Pango.Weight.Bold;
				view.Buffer.TagTable.Add (tag);
				boldTag = tag;

				tag = new TextTag (GAMETAGNAME_TAG);
				tag.Weight = Pango.Weight.Bold;
				tag.RightMargin = 20;
				view.Buffer.TagTable.Add (tag);
				gametagnameTag = tag;

				tag = new TextTag (HIGHLIGHTED_TAG);
				tag.Weight = Pango.Weight.Bold;
				tag.Scale = Pango.Scale.XLarge;
				//tag.Foreground = "#803030";
				view.Buffer.TagTable.Add (tag);
				highlightedTag = tag;

				tag = new TextTag (COMMENT_TAG);
				tag.Style = Pango.Style.Italic;
				tag.PixelsBelowLines = 5;
				tag.PixelsAboveLines = 5;
				tag.Foreground = "#303060";
				commentTag = tag;

				view.Buffer.TagTable.Add (tag);
			}

			private void GetItersForMove (int idx,
						      TextBuffer buffer,
						      out TextIter start,
						      out TextIter end)
			{
				string markstr = idx.ToString ();
				int len = (int) marks[markstr];
				start = buffer.GetIterAtMark (buffer.
							      GetMark
							      (markstr));
				end = start;
				end.ForwardChars (len);
			}

			public void SetMoveIndex (int idx)
			{
				TextIter start, end;
				string text;
				TextBuffer buffer = view.Buffer;

				if (curMoveIdx >= 0)
				  {
					  GetItersForMove (curMoveIdx, buffer,
							   out start,
							   out end);
					  text = buffer.GetText (start, end,
								 false);
					  buffer.Delete (ref start, ref end);
					  buffer.InsertWithTags (ref start,
								 text,
								 moveTag,
								 taglinks
								 [curMoveIdx]
								 as TextTag);
				  }

				if (idx >= 0)
				  {
					  GetItersForMove (idx, buffer,
							   out start,
							   out end);
					  text = buffer.GetText (start, end,
								 false);
					  buffer.Delete (ref start, ref end);
					  buffer.InsertWithTags (ref start,
								 text,
								 highlightedTag,
								 taglinks[idx]
								 as TextTag);
				  }
				view.ScrollToMark (buffer.
						   GetMark (idx.ToString ()),
						   0, false, 0, 0);
				// jump to anchor
				curMoveIdx = idx;
				//html.JumpToAnchor(idx.ToString());
			}

			public void SetGame (ChessGame game)
			{
				this.game = game;
				curMoveIdx = -1;
				Refresh ();
			}

			public void Refresh ()
			{
				foreach (TextTag tag in taglinks)
				{
					view.Buffer.TagTable.Remove (tag);
				}
				tag_links.Clear ();
				taglinks.Clear ();
				view.Buffer.Clear ();
				TextIter iter = view.Buffer.StartIter;
				UpdateGameDetails (view.Buffer, ref iter);
			}

			private void UpdateGameDetails (TextBuffer buffer,
							ref TextIter iter)
			{
				PrintTitle (buffer, ref iter);
				if (game == null)
					return;

				if(game.Comment != null) {
					buffer.InsertWithTags (ref
							       iter,
							       game.Comment,
							       commentTag);
					buffer.Insert (ref iter, "\n");
				}

				int i = 0;
				int moveno = 1;
				foreach (PGNChessMove move in game.Moves)
				{
					if (i % 2 == 0)
					  {
						  buffer.InsertWithTags (ref
									 iter,
									 moveno.
									 ToString
									 (),
									 movenumberTag);
						  moveno++;
						  buffer.Insert (ref iter,
								 ". ");
					  }

					string markstr = i.ToString ();
					string text = move.DetailedMove;
					buffer.CreateMark (markstr, iter, true);	// left gravity
					TextTag link_tag = new TextTag (null);
					tag_links[link_tag] = i;
					taglinks.Add (link_tag);
					buffer.TagTable.Add (link_tag);
					buffer.InsertWithTags (ref iter, text,
							       moveTag,
							       link_tag);
					marks[markstr] = text.Length;
					buffer.Insert (ref iter, " ");

					if (move.comment != null)
					  {
						  buffer.Insert (ref iter,
								 "\n");
						  buffer.InsertWithTags (ref
									 iter,
									 move.
									 comment,
									 commentTag);
						  buffer.Insert (ref iter,
								 "\n");
					  }
					i++;
				}
			}

			bool hoveringOverLink = false;

			private void SetCursorIfAppropriate (TextView view,
							     int x, int y)
			{
				bool hover = false;
				TextIter iter = view.GetIterAtLocation (x, y);

				foreach (TextTag tag in iter.Tags)
				{
					if (tag_links[tag] is int)
					  {
						  hover = true;
						  break;
					  }
				}

				if (hover == hoveringOverLink)
					return;
				Gdk.Window window =
					view.GetWindow (Gtk.TextWindowType.
							Text);
				hoveringOverLink = hover;
				window.Cursor =
					hoveringOverLink ? handCursor :
					regularCursor;
			}

			private void PrintTitle (TextBuffer buffer,
						 ref TextIter iter)
			{
				if (game == null)
					return;
				string title = String.Format ("{0} vs {1}",
							      game.White,
							      game.Black);

				buffer.CreateMark ("-1", iter, true);
				buffer.InsertWithTagsByName (ref iter, title,
							     HEADING_TAG);
				buffer.Insert (ref iter, "\n");

				Widget tagdetails = GetTagDetailsWidget ();
				TextChildAnchor anchor =
					buffer.CreateChildAnchor (ref iter);
				view.AddChildAtAnchor (tagdetails, anchor);
				buffer.Insert (ref iter, "\n\n");
			}

			private Widget GetTagDetailsWidget ()
			{
				string eco;
				GameViewer.GetOpeningName (game.
							   GetTagValue ("ECO",
									""),
							   out eco);
				string[,] values = new string[,]
				{
					{
					Catalog.GetString ("Result"),
							game.Result}
					,
					{
					Catalog.GetString ("Date"),
							game.Date}
					,
					{
					Catalog.GetString ("Event"),
							game.Event}
					,
					{
					Catalog.GetString ("Site"),
							game.Site}
					,
					{
					Catalog.GetString ("ECO"), eco}
				};
				uint count = (uint) values.Length / 2;
				Table table = new Table (count, 2, false);
				table.ColumnSpacing = 10;
				for (uint i = 0; i < count; i++)
				  {
					  Label name = new Label ();
					  name.Xalign = 0;
					  name.Markup =
						  String.Format ("<b>{0}</b>",
								 values[i,
									0]);
					  Label value = new Label ();
					  value.Xalign = 0;
					  value.Text = values[i, 1];
					  table.Attach (name, 0, 1, i, i + 1);
					  table.Attach (value, 1, 2, i,
							i + 1);
				  }

				table.ShowAll ();
				return table;
			}

			void FollowIfLink (TextView view, TextIter iter)
			{
				foreach (TextTag tag in iter.Tags)
				{
					object move = tag_links[tag];
					if (move is int)
					  {
						  if (ShowNthMove != null)
							  ShowNthMove (this,
								       new
								       MoveEventArgs
								       ((int)
									move));
					  }
				}
			}

			void EventAfter (object sender,
					 WidgetEventAfterArgs args)
			{
				if (args.Event.Type !=
				    Gdk.EventType.ButtonRelease)
					return;

				Gdk.EventButton evt =
					(Gdk.EventButton) args.Event;

				if (evt.Button != 1)
					return;

				TextView view = sender as TextView;
				TextIter start, end, iter;
				int x, y;

				// we shouldn't follow a link if the user has selected something
				view.Buffer.GetSelectionBounds (out start,
								out end);
				if (start.Offset != end.Offset)
					return;

				view.WindowToBufferCoords (TextWindowType.
							   Widget,
							   (int) evt.X,
							   (int) evt.Y, out x,
							   out y);
				iter = view.GetIterAtLocation (x, y);

				FollowIfLink (view, iter);
			}

			void MotionNotify (object sender,
					   MotionNotifyEventArgs args)
			{
				TextView view = sender as TextView;
				int x, y;
				Gdk.ModifierType state;

				view.WindowToBufferCoords (TextWindowType.
							   Widget,
							   (int) args.Event.X,
							   (int) args.Event.Y,
							   out x, out y);
				SetCursorIfAppropriate (view, x, y);

				view.GdkWindow.GetPointer (out x, out y,
							   out state);
			}

			// Also update the cursor image if the window becomes visible
			// (e.g. when a window covering it got iconified).
			void VisibilityNotify (object sender,
					       VisibilityNotifyEventArgs a)
			{
				TextView view = sender as TextView;
				int wx, wy, bx, by;

				view.GetPointer (out wx, out wy);
				view.WindowToBufferCoords (TextWindowType.
							   Widget, wx, wy,
							   out bx, out by);
				SetCursorIfAppropriate (view, bx, by);
			}
		}
	}
}
