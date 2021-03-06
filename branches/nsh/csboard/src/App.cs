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

        using System;
        using Gtk;

        public class App {

		public static Session session;	
	
                public static int Main (string [] args) {

                Application.Init ();
		
		Catalog.Init (Config.packageName, Config.prefix + "/share/locale");

			try {
		
				session = new Session ();
				
				string filename = null;
				
				if (args.Length == 1 && System.IO.File.Exists (args[0])) {
				   filename = args[0];
				} 

	                        ChessWindow win = new ChessWindow (filename);

	                        Application.Run ();

			} catch (ApplicationException) {
				return 1;
			} catch (System.Exception e) {	
				
				 try {
					 MessageDialog md =
		                                        new MessageDialog (null,
	    	                                                       DialogFlags.
	        	                                               DestroyWithParent,
	                	                                       MessageType.Error,
	                    	                                       ButtonsType.Close, 
				    				       Catalog.GetString ("An unexpected exception occured:\n\n") +
								       e.ToString() + "\n" +
				                                       Catalog.GetString ("Please report about this exception to \n") +
								      "Nickolay V. Shmyrev <nshmyrev@yandex.ru>");
	    							   
					 md.Run ();
	        	                 md.Hide ();
	    	        	         md.Dispose ();
    
				 } catch (Exception ex) {

					 throw e;

		    		 }
			}
			
			return 0;
		}
	}
}
