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

	using System;
	using Gtk;
	using CsBoard.Viewer;
	using Mono.Unix;
	public class App
	{

		public static Session session;

		public static int StartViewer (string[]args)
		{
			Catalog.Init (Config.packageName,
				      Config.prefix + "/share/locale");

			try
			{
				session = new Session ();
				GameViewer.CreateInstance ();
				CsBoard.Plugin.PluginManager.Instance.
					StartPlugins ();
				if (args.Length > 1)
					GameViewer.Instance.Load (args[1]);
			}
			catch (ApplicationException)
			{
				return 1;
			}
			catch (System.Exception e)
			{

				try
				{
					MessageDialog md =
						new MessageDialog (null,
								   DialogFlags.
								   DestroyWithParent,
								   MessageType.
								   Error,
								   ButtonsType.
								   Close,
								   Catalog.
								   GetString
								   ("<b>Unexpected exception occured</b>\n\n")
								   +
								   GLib.
								   Markup.
								   EscapeText
								   (e.
								    ToString
								    ()) +
								   "\n" +
								   Catalog.
								   GetString
								   ("Please send this bug report to\n")
								   +
								   "Nickolay V. Shmyrev  &lt;nshmyrev@yandex.ru&gt;\n");
					md.Run ();
					md.Hide ();
					md.Dispose ();

				}
				catch
				{

					throw e;

				}
			}

			return 0;
		}

		public static int StartApp(string[] args) {
			CsApp app = new CsApp(args);
			return 0;
		}

		public static int Main (string[]args)
		{
			Application.Init ();
			if (args.Length > 0 && args[0].Equals ("-viewer"))
				StartViewer (args);
			else if (args.Length > 0 && args[0].Equals ("-player"))
				StartPlayer (args);
			else
				StartApp (args);
			Application.Run ();
			return 0;
		}

		public static int StartPlayer (string[]args)
		{
			Catalog.Init (Config.packageName,
				      Config.prefix + "/share/locale");

			try
			{
				session = new Session ();
				string filename = null;
				if (args.Length == 1
				    && System.IO.File.Exists (args[0]))
				  {
					  filename = args[0];
				  }
				new ChessWindow (filename);
			}
			catch (System.Exception e)
			{
				try
				{
					MessageDialog md =
						new MessageDialog (null,
								   DialogFlags.
								   DestroyWithParent,
								   MessageType.
								   Error,
								   ButtonsType.
								   Close,
								   Catalog.
								   GetString
								   ("<b>Unexpected exception occured</b>\n\n")
								   +
								   GLib.
								   Markup.
								   EscapeText
								   (e.
								    ToString
								    ()) +
								   "\n" +
								   Catalog.
								   GetString
								   ("Please send this bug report to\n")
								   +
								   "Nickolay V. Shmyrev  &lt;nshmyrev@yandex.ru&gt;\n");
					md.Run ();
					md.Hide ();
					md.Dispose ();

				}
				catch
				{
					throw e;
				}
			}

			return 0;
		}

		class CsApp {
			string[] args;
			[Glade.Widget] private Gtk.Button startPlayerButton, startViewerButton;
			[Glade.Widget] private Gtk.Window csAppWindow;
			public CsApp(string[] args) {
				this.args = args;
				Glade.XML xml = Glade.XML.FromAssembly("csboard.glade", "csAppWindow", null);
				xml.Autoconnect(this);

				startPlayerButton.Clicked += OnButtonClicked;
				startViewerButton.Clicked += OnButtonClicked;

				csAppWindow.DeleteEvent += delegate (object o, DeleteEventArgs e)
				{
					Application.Quit();
				};
			}

			private void OnButtonClicked(object o, EventArgs evargs) {
				csAppWindow.Hide();
				if(o.Equals(startPlayerButton)) {
					App.StartPlayer(args);
				}
				else {
					App.StartViewer(args);
				}
				csAppWindow.Dispose();
			}
		}
	}
}
