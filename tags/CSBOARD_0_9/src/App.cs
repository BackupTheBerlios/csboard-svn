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
		private static int appCount = 0;

		public static void Close ()
		{
			appCount--;
			if (appCount <= 0)
			  {
				  Application.Quit ();
			  }
		}

		private static Session session;
		public static Session Session
		{
			get
			{
				if (session == null)
					session = new Session ();
				return session;
			}
		}

		public static bool StartApp (string[]args)
		{
			try
			{
				StartPlayer (args.Length >
					     0 ? args[0] : null);
			}
			catch (ApplicationException)
			{
				return false;
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
				return false;
			}

			return true;
		}

		public static int Main (string[]args)
		{
			Application.Init ();
			Catalog.Init (Config.packageName,
				      Config.prefix + "/share/locale");

			if (StartApp (args))
				Application.Run ();
			return 0;
		}

		static void StartPlayer (string filename)
		{
			App.StartPlayer (App.Session.Engine, filename);
		}

		static void StartPlayer (string engine, string filename)
		{
			new CsBoardApp (engine, filename);
			appCount++;
		}
	}
}
