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

	public delegate void TitleChangedEventHandler (object o,
						       EventArgs args);
	public class CsBoardApp:ChessWindowUI, MainApp, SubApp
	{
		public event TitleChangedEventHandler TitleChangedEvent;
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

		public string ID
		{
			get
			{
				return "player";
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
	  Hashtable subAppsMap;

		public Gtk.Window Window
		{
			get
			{
				return csboardWindow;
			}
		}

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
			subAppsMap = new Hashtable();
			accel = new AccelGroup ();
			csboardWindow.AddAccelGroup (accel);
			Gtk.Image img =
				new Gtk.Image (Gdk.Pixbuf.
					       LoadFromResource
					       ("computer.png"));
			img.Show ();
			playerToolButton.IconWidget = img;


			subapps.Add (this);
			playerToolButton.Sensitive = false;
			TitleChangedEvent += OnAppTitleChanged;

			AddApp (new CsBoard.ICS.ICSDetailsWidget ());
			CsBoard.Viewer.GameViewer.CreateInstance ();
			playerToolButton.Clicked += OnToolButtonClicked;

			if (filename == null) {
				control.OpenGame (App.Session.Filename);
				ShowAppFromLastSession();
			}
			else
			  {
				  ShowApp (CsBoard.Viewer.GameViewer.
					   Instance);
				  GLib.Idle.Add (delegate
						 {
						 CsBoard.Viewer.GameViewer.
						 Instance.Load (filename);
						 return false;
						 }
				  );
			  }

			chessGameWidget.Show ();
			boardWidget.Show ();
			csboardWindow.Show ();
		}

		public void SetVisibility (bool visible)
		{
		}

	  private void ShowAppFromLastSession() {
	    string lastappname = null;
	    try {
	      lastappname = App.Session.LastAppName;
	    }
	    catch(Exception) {
	    }
	    if(lastappname == null || !subAppsMap.ContainsKey(lastappname))
	      return;
	    ShowApp(subAppsMap[lastappname] as SubApp);
	  }

		public void AddApp (SubApp app)
		{
			if(subAppsMap.ContainsKey(app.ID))
				return;
			subAppsMap[app.ID] = app;
			SeparatorToolItem separator =
				new SeparatorToolItem ();
			separator.Show ();
			if (subapps.Count == 0)
			  {
				  appsBar.Insert (app.ToolButton,
						  appsBar.NItems);
				  appsBar.Insert (separator, appsBar.NItems);
			  }
			else
			  {
				  int index =
					  appsBar.
					  GetItemIndex ((subapps
							 [subapps.Count -
							  1] as SubApp).
							ToolButton);
				  index++;
				  appsBar.Insert (separator, index++);
				  appsBar.Insert (app.ToolButton, index);
			  }

			menusBook.AppendPage (app.MenuBar, new Label ());

			app.ToolButton.Clicked += OnToolButtonClicked;
			appsBook.AppendPage (app.Widget, new Label ());
			app.TitleChangedEvent += OnAppTitleChanged;

			subapps.Add (app);
		}

		private void OnAppTitleChanged (object o, EventArgs args)
		{
			if (subapps[appsBook.CurrentPage].Equals (o))
				csboardWindow.Title = (o as SubApp).Title;
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
			if (i == curappIndex)
				return;
			SubApp app = subapps[curappIndex] as SubApp;
			app.ToolButton.Sensitive = true;
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
			app.ToolButton.Sensitive = false;
			App.Session.LastAppName = app.ID;
		}
	}
}
