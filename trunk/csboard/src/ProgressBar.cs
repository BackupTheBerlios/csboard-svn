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

	public class ProgressBar: Gtk.ProgressBar {
	
	    static GLib.GType gtype = GLib.GType.Invalid;
	    private uint timeout_id = 0;

	    public ProgressBar ():base (GType) {
	    }
	    
	    public static new GLib.GType GType {
            
	                get {
                                if (gtype == GLib.GType.Invalid)
                                        gtype = RegisterGType
                                                (typeof (ProgressBar));
                                return gtype;
	                    }
	    }

	    
	    public void Start () {
	      timeout_id = GLib.Timeout.Add (300, new GLib.TimeoutHandler(timeout_cb)); 
	      Show ();
	    }
	    
	    public void Stop () {
	      if (timeout_id > 0) {
		      GLib.Source.Remove (timeout_id);
		      timeout_id = 0;
	      }
	      Hide ();
	    }
	    
	    private bool timeout_cb ()
	    {
		Pulse ();
		return true;
	    }
	    
	}
}