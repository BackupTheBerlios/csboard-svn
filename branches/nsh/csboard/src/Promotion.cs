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

namespace CsBoard {

        using Gtk;
        using System;

        public class Promotion:Dialog {

                Glade.XML gXML;

                [Glade.Widget] ToggleButton radiobutton4;
                [Glade.Widget] ToggleButton radiobutton5;
                [Glade.Widget] ToggleButton radiobutton6;
                [Glade.Widget] ToggleButton radiobutton7;
                [Glade.Widget] Widget hbox_contents;



                public Promotion () {

                        gXML = new Glade.XML ("resource/csboard.glade",
                                              "hbox_contents", null);
                        gXML.Autoconnect (this);

                        HasSeparator = false;
                        this.Title = Catalog.GetString("Select a figure to promote");
                        this.SetSizeRequest (300, 150);

                        VBox.PackStart (hbox_contents, true, true, 10);
                        VBox.Show ();

                        AddButton (Stock.Close, (int) ResponseType.Close);
                } 
		
		public char GetResult () {

                        char result;

                          result = ' ';

                        if (radiobutton4.Active)
                                  result = 'Q';
                        if (radiobutton5.Active)
                                  result = 'K';
                        if (radiobutton6.Active)
                                  result = 'B';
                        if (radiobutton7.Active)
                                  result = 'R';

                          return result;
	         }
        }		
}
