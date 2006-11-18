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
        using CsBoard.Viewer;

        public class App {

		public static Session session;	

		static int StartViewer(string[] args) {

                Application.Init ();

		Catalog.Init (Config.packageName, Config.prefix + "/share/locale");

			try {
				session = new Session ();
	                        new GameViewer (args.Length > 1 ? args[1] : null);
	                        Application.Run ();
			} catch (ApplicationException) {
				return 1;
			} catch (System.Exception e) {	

				 try {
					 MessageDialog md =
		                    	   new MessageDialog (null,
	    	                                              DialogFlags.DestroyWithParent,
	                	                              MessageType.Error,
	                    	                              ButtonsType.Close, 
				    	   	              Catalog.GetString ("<b>Unexpected exception occured</b>\n\n") +
				    	   	              GLib.Markup.EscapeText (e.ToString()) +
				    	   	              "\n" +
							      Catalog.GetString ("Please send this bug report to\n") +
							      "Nickolay V. Shmyrev  &lt;nshmyrev@yandex.ru&gt;\n");
					 md.Run ();
	        	                 md.Hide ();
	    	        	         md.Dispose ();
    
				 } catch (Exception ex) {

					 throw e;

		    		 }
			}
			
			return 0;
		}

                public static int Main (string [] args) {
		  if(args.Length > 0 && args[0].Equals("-viewer"))
		    return StartViewer(args);
		  return StartPlayer(args);
		}

                public static int StartPlayer (string [] args) {

                Application.Init ();

		Catalog.Init (Config.packageName, Config.prefix + "/share/locale");

			try {
				session = new Session ();
				string filename = null;
				if (args.Length == 1 && System.IO.File.Exists (args[0])) {
				   filename = args[0];
				} 
	                        new ChessWindow (filename);
	                        Application.Run ();
			} catch (ApplicationException) {
				return 1;
			} catch (System.Exception e) {	
				 try {
					 MessageDialog md =
		                    	   new MessageDialog (null,
	    	                                              DialogFlags.DestroyWithParent,
	                	                              MessageType.Error,
	                    	                              ButtonsType.Close, 
				    	   	              Catalog.GetString ("<b>Unexpected exception occured</b>\n\n") +
				    	   	              GLib.Markup.EscapeText (e.ToString()) +
				    	   	              "\n" +
							      Catalog.GetString ("Please send this bug report to\n") +
							      "Nickolay V. Shmyrev  &lt;nshmyrev@yandex.ru&gt;\n");
					 md.Run ();
	        	                 md.Hide ();
	    	        	         md.Dispose ();
    
				 } catch {
					 throw e;
		    		 }
			}
			
			return 0;
		}
	}
}
