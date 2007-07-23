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
	using System.Collections;
	using Gtk;
	using Gdk;
	using Mono.Unix;

	public delegate void QuitEventHandler (object o, EventArgs args);
	public interface MainApp
	{
		void AddApp (SubApp app);
		void ShowApp (int i);
		void ShowApp (SubApp app);
		event QuitEventHandler QuitEvent;
	}

	public interface SubApp
	{
		MenuBar MenuBar
		{
			get;
		}

		Widget Widget
		{
			get;
		}

		ToolButton ToolButton
		{
			get;
		}

		AccelGroup AccelGroup
		{
			get;
		}

		string Title
		{
			get;
		}

		void SetVisibility (bool visible);
	}

	public class CsBoardApp:ChessWindowUI, MainApp, SubApp
	{

		AccelGroup accel;
		public AccelGroup AccelGroup
		{
			get
			{
				return accel;
			}
		}

		public Widget Widget
		{
			get
			{
				return appBox;
			}
		}

		string title;
		public string Title
		{
			get
			{
				return title;
			}
		}

		public ToolButton ToolButton
		{
			get
			{
				return playerToolButton;
			}
		}

		public MenuBar MenuBar
		{
			get
			{
				return menubar;
			}
		}

		private static CsBoardApp instance;
		public static CsBoardApp Instance
		{
			get
			{
				return instance;
			}
		}
		ArrayList subapps;

		public CsBoardApp (string engine,
				   string filename):base (engine, filename)
		{
			title = String.Format (Catalog.
					       GetString
					       ("Welcome to CS Board ({0})"),
					       control.Name);
			csboardWindow.Title = title;
			instance = this;
			subapps = new ArrayList ();
			accel = new AccelGroup ();
			csboardWindow.AddAccelGroup (accel);
			Gtk.Image img =
				new Gtk.Image (Gdk.Pixbuf.
					       LoadFromResource
					       ("computer.png"));
			img.Show ();
			playerToolButton.IconWidget = img;


			subapps.Add (this);
			AddApp (new CsBoard.ICS.ICSDetailsWidget ());
			AddApp (CsBoard.Viewer.GameViewer.Instance);
			playerToolButton.Clicked += OnToolButtonClicked;

			if (filename == null)
				control.OpenGame (App.Session.Filename);
			else
				CsBoard.Viewer.GameViewer.Instance.
					Load (filename);

			chessGameWidget.Show ();
			boardWidget.Show ();
			csboardWindow.Show ();
		}

		public void SetVisibility (bool visible)
		{
		}

		public void AddApp (SubApp app)
		{
			subapps.Add (app);

			menusBook.AppendPage (app.MenuBar, new Label ());
			int i = appsBar.NItems;
			appsBar.Insert (app.ToolButton, i++);
			SeparatorToolItem separator =
				new SeparatorToolItem ();
			separator.Show ();
			appsBar.Insert (separator, i);
			app.ToolButton.Clicked += OnToolButtonClicked;
			appsBook.AppendPage (app.Widget, new Label ());
		}

		private void OnToolButtonClicked (object o, EventArgs args)
		{
			int i = 0;
			foreach (SubApp app in subapps)
			{
				if (app.ToolButton.Equals (o))
				  {
					  ShowApp (i);
					  break;
				  }
				i++;
			}
		}

		public void ShowApp (SubApp app)
		{
			int i = 0;
			foreach (SubApp subapp in subapps)
			{
				if (subapp.Equals (app))
				  {
					  ShowApp (i);
					  return;
				  }
				i++;
			}
		}

		public void ShowApp (int i)
		{
			int curappIndex = appsBook.CurrentPage;
			SubApp app = subapps[curappIndex] as SubApp;
			app.SetVisibility (false);
			if (app.AccelGroup != null)
			  {
				  csboardWindow.RemoveAccelGroup (app.
								  AccelGroup);
			  }

			menusBook.CurrentPage = i;
			appsBook.CurrentPage = i;
			app = subapps[i] as SubApp;
			if (app.AccelGroup != null)
				csboardWindow.AddAccelGroup (app.AccelGroup);
			csboardWindow.Title = app.Title;
			app.SetVisibility (true);
		}
	}
}
