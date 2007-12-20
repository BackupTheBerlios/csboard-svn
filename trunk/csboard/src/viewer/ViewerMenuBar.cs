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

using Gtk;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{

		public class ViewerMenuBar:AppMenuBar
		{
			public MenuItem actionMenuItem, moveCommentMenuItem,
				exportAsMenuItem;
			public CheckMenuItem highlightMoveMenuItem;
			public ImageMenuItem switchSideMenuItem,
				saveAsMenuItem, printMenuItem;
			public SeparatorMenuItem fileOpenSeparator,
				saveAsSeparator;

			public ViewerMenuBar ():base ()
			{
				/* File menu */
				fileOpenSeparator = new SeparatorMenuItem ();
				saveAsSeparator = new SeparatorMenuItem ();
				saveAsMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("Save _As"));
				saveAsMenuItem.Image =
					new Image (Stock.Refresh,
						   IconSize.Menu);
				printMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Print"));
				printMenuItem.Image =
					new Image (Stock.Print,
						   IconSize.Menu);

				Menu menu = fileMenuItem.Submenu as Menu;
				int i = 0;
				  menu.Insert (fileOpenSeparator, i++);
				  menu.Insert (saveAsMenuItem, i++);
				  menu.Insert (saveAsSeparator, i++);
				  menu.Insert (printMenuItem, i++);

				/* Action menu */
				  actionMenuItem =
					new MenuItem (Catalog.
						      GetString ("_Action"));
				  AppendAfter (fileMenuItem, actionMenuItem);
				  menu = new Menu ();
				  actionMenuItem.Submenu = menu;
				  switchSideMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Switch Side"));
				  switchSideMenuItem.Image =
					new Image (Stock.Refresh,
						   IconSize.Menu);
				  menu.Append (switchSideMenuItem);

				/* Edit menu */
				  menu = editMenuItem.Submenu as Menu;
				  moveCommentMenuItem =
					new MenuItem (Catalog.
						      GetString
						      ("Move _Comment"));
				  menu.Append (moveCommentMenuItem);

				/* Export As menu */
				  exportAsMenuItem =
					new MenuItem (Catalog.
						      GetString
						      ("_Export As"));
				  AppendAfter (editMenuItem,
					       exportAsMenuItem);

				/* View menu */
				  menu = viewMenuItem.Submenu as Menu;
				  highlightMoveMenuItem =
					new CheckMenuItem (Catalog.
							   GetString
							   ("_Highlight Move"));
				  menu.Append (highlightMoveMenuItem);
				  ShowAll ();
			}
		}
	}
}
