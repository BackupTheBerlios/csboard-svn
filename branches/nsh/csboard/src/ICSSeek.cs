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
// Copyright (C) 2004 Jamin Gray


namespace CsBoard {
	
        using System;
        using System.Collections;
        using Gtk;
        using Gdk;

	
	public class SeekDialog : Dialog {

                [Glade.Widget] Gtk.Widget seek_vbox;
                [Glade.Widget] Gtk.TreeView seek_treeview;
		
		private ICSClient client;
		private ListStore store;

		public SeekDialog (ICSClient c) {			
			
			client = c;	
		
			Glade.XML gXML =
                                new Glade.
                                XML ("resource/csboard.glade",
                                     "seek_vbox", null);
                        gXML.Autoconnect (this);

                        HasSeparator = false;
                        Title = Catalog.GetString("Start new ICS game");
                        SetSizeRequest (550, 300);
        
	                VBox.Add (seek_vbox);
                        AddButton (Stock.Refresh, (int) ResponseType.Apply);
                        AddButton (Stock.Cancel, (int) ResponseType.Close);
                        AddButton (Stock.New, (int) ResponseType.Accept);


			store = new ListStore (typeof (string), typeof (string), 
					       typeof (string), typeof (string),
					       typeof (string), typeof (string),
					       typeof (string), typeof (string),
					       typeof (string), typeof (string));
  			seek_treeview.Model = store;
  			seek_treeview.HeadersVisible = true;
  			seek_treeview.HeadersClickable = true;
  			seek_treeview.AppendColumn (Catalog.GetString("Index"), new CellRendererText (), "text", 0);
  			seek_treeview.AppendColumn (Catalog.GetString("Rating"), new CellRendererText (), "text", 1);
  			seek_treeview.AppendColumn (Catalog.GetString("Player"), new CellRendererText (), "text", 2);
  			seek_treeview.AppendColumn (Catalog.GetString("Time"), new CellRendererText (), "text", 3);
  			seek_treeview.AppendColumn (Catalog.GetString("Increment"), new CellRendererText (), "text", 4);
  			seek_treeview.AppendColumn (Catalog.GetString("R/U"), new CellRendererText (), "text", 5);
  			seek_treeview.AppendColumn (Catalog.GetString("Type"), new CellRendererText (), "text", 6);
  			seek_treeview.AppendColumn (Catalog.GetString("Color"), new CellRendererText (), "text", 7);
  			seek_treeview.AppendColumn (Catalog.GetString("Range"), new CellRendererText (), "text", 8);
	                seek_treeview.AppendColumn (Catalog.GetString("Flags"), new CellRendererText (), "text", 9);
			
			seek_treeview.RowActivated += new RowActivatedHandler (on_line_activate);			
			seek_treeview.Selection.Changed += on_selection_changed;			
		}
		
		public bool SeekNewGame () {
			Gtk.ResponseType response;

			do {
				RefreshModel();
				
				response = (Gtk.ResponseType)Run ();

			} while (response == ResponseType.Apply);
			
			Hide ();
			
			if (response == ResponseType.Close) 
				return false;

			TreeIter iter;
			TreeModel model;
			
			if (seek_treeview.Selection.GetSelected (out model, out iter)) {
				string game_num = (string) store.GetValue (iter, 0);
				client.Write ("play " + game_num);
				return true;
			}
			return false;	
	    	}
		
		private void RefreshModel () {
			store.Clear ();
			SetResponseSensitive (ResponseType.Accept, false);
			client.Write ("seek");			
		}
		
		private void on_line_activate (object o, Gtk.RowActivatedArgs args) {
			Respond (ResponseType.Accept);
		}

		private void on_selection_changed (object o, EventArgs args) {
			TreeIter iter;
			TreeModel model;
			
			if (seek_treeview.Selection.GetSelected (out model, out iter)) {
				SetResponseSensitive (ResponseType.Accept, true);
			}
		}
	}
}
