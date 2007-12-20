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
// Copyright (C) 2004 Nickolay V. Shmyrev

namespace CsBoard
{

	using Gtk;
	using System;
	using System.Collections;
	using Mono.Unix;

	public class BookDialog:Dialog
	{

		[Glade.Widget] TreeView treeview;
		[Glade.Widget] Widget vbox_contents;

		public BookDialog (ArrayList result)
		{

			Glade.XML gXML =
				Glade.XML.FromAssembly ("csboard.glade",
							"vbox_contents",
							null);
			gXML.Autoconnect (this);

			HasSeparator = false;
			this.Title = Catalog.GetString ("Book Opening");
			this.SetSizeRequest (380, 260);

			VBox.Add (vbox_contents);
			AddButton (Stock.Close, (int) ResponseType.Close);
			AddButton (Stock.GoForward, (int) ResponseType.Apply);

			Gtk.ListStore store =
				new ListStore (typeof (string),
					       typeof (int),
					       typeof (int),
					       typeof (int), typeof (int));

			  treeview.Model = store;
			  treeview.RulesHint = true;
			  foreach (BookMove m in result)
			{

				int score =
					100 * (m.Wins +
					       (m.Draws / 2)) /
					Math.Max (m.Wins + m.Loses + m.Draws,
						  1) + m.Wins / 2;

				  store.AppendValues (m.Move, score,
						      m.Wins, m.Loses,
						      m.Draws);
			} TreeViewColumn column;
			  column =
				treeview.AppendColumn (Catalog.
						       GetString ("Move"),
						       new
						       CellRendererText (),
						       "text", 0);
			  column.SortColumnId = 0;

			  column =
				treeview.AppendColumn (Catalog.
						       GetString ("Score"),
						       new
						       CellRendererText (),
						       "text", 1);
			  column.SortColumnId = 1;

			  column =
				treeview.AppendColumn (Catalog.
						       GetString ("Wins"),
						       new
						       CellRendererText (),
						       "text", 2);
			  column.SortColumnId = 2;

			  column =
				treeview.AppendColumn (Catalog.
						       GetString ("Loses"),
						       new
						       CellRendererText (),
						       "text", 3);
			  column.SortColumnId = 3;

			  column =
				treeview.AppendColumn (Catalog.
						       GetString ("Draws"),
						       new
						       CellRendererText (),
						       "text", 4);
			  column.SortColumnId = 4;


			  ((TreeSortable) store).SetSortColumnId (1,
								  SortType.
								  Descending);

			  vbox_contents.ShowAll ();

			  treeview.RowActivated +=
				new RowActivatedHandler (activate);
		} public string GetMove ()
		{

			TreeSelection selection = treeview.Selection;
			TreeIter iter;
			TreeModel model;

			if (selection.GetSelected (out model, out iter))
			  {
				  return model.GetValue (iter, 0).ToString ();
			  }

			return null;

		}

		private void activate (System.Object b, RowActivatedArgs e)
		{
			this.Respond (ResponseType.Apply);
		}
	}
}
