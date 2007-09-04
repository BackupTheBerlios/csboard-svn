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
using System;
using System.Text;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		class ICSMenuBar:AppMenuBar
		{
			public ImageMenuItem connectMenuItem,
				disconnectMenuItem;
			public ICSMenuBar ():base ()
			{
				Menu menu = fileMenuItem.Submenu as Menu;
				  connectMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Connect"));
				  connectMenuItem.Image =
					new Image (Stock.Connect,
						   IconSize.Menu);
				  disconnectMenuItem =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_Disconnect"));
				  disconnectMenuItem.Image =
					new Image (Stock.Disconnect,
						   IconSize.Menu);
				int i = 0;
				  menu.Insert (connectMenuItem, i++);
				  menu.Insert (disconnectMenuItem, i++);

				  ShowAll ();
			}
		}
		public class ICSDetailsWidget:Frame, SubApp
		{
			public event TitleChangedEventHandler
				TitleChangedEvent;
			GameObservationManager obManager;
			ObservableGamesWidget observableGames;
			GameAdvertisements ads;
			ICSShell shell;
			GameAdvertisementGraph graph;

			ICSClient client;

			ICSMenuBar menubar;

			public MenuBar MenuBar
			{
				get
				{
					return menubar;
				}
			}

			ToolButton toolbutton;
			public ToolButton ToolButton
			{
				get
				{
					return toolbutton;
				}
			}

			public Widget Widget
			{
				get
				{
					return this;
				}
			}

			public string Title
			{
				get
				{
					return title;
				}
			}

			AccelGroup accel;
			public AccelGroup AccelGroup
			{
				get
				{
					return accel;
				}
			}

			Notebook book;
			public Notebook Book
			{
				get
				{
					return book;
				}
			}

			ICSConfigWidget configwidget;
			string title;
			bool app_visible = false;

			public ICSDetailsWidget ():base ()
			{
				menubar = new ICSMenuBar ();
				menubar.disconnectMenuItem.Activated +=
					on_disconnect_activate;
				menubar.connectMenuItem.Activated +=
					on_connect_activate;
				menubar.ShowAll ();

				Image img = new Image ();
				img.Stock = Stock.Network;
				toolbutton =
					new ToolButton (img,
							Catalog.
							GetString
							("Chess Server"));
				toolbutton.ShowAll ();

				client = new ICSClient ();
				title = String.Format (Catalog.GetString
						       ("ICS: {0}@{1}:{2}"),
						       client.User,
						       client.server,
						       client.port);
				book = new Notebook ();
				book.Show ();

				Add (book);

				obManager =
					new GameObservationManager (client,
								    this);

				observableGames =
					new ObservableGamesWidget (obManager);

				graph = new GameAdvertisementGraph (client);
				book.AppendPage (graph,
						 new Label (Catalog.
							    GetString
							    ("Seek Graph")));
				ads = new GameAdvertisements (client);
				book.AppendPage (ads,
						 new Label (Catalog.
							    GetString
							    ("Game Seeks")));

				book.AppendPage (observableGames,
						 new Label (Catalog.
							    GetString
							    ("Watch Games")));

				shell = new ICSShell (client);
				book.AppendPage (shell,
						 new Label (Catalog.
							    GetString
							    ("Shell")));

				client.ChallengeEvent += OnChallengeEvent;

				client.AuthEvent += OnAuth;
				client.ConnectionErrorEvent +=
					OnConnectionError;

				ShowConfigWidget ();

				menubar.disconnectMenuItem.Sensitive = false;
				GLib.Idle.Add (delegate ()
					       {
					       Authenticate (); return false;}
				);

				accel = new AccelGroup ();
				menubar.quitMenuItem.
					AddAccelerator ("activate", accel,
							new AccelKey (Gdk.Key.
								      q,
								      Gdk.
								      ModifierType.
								      ControlMask,
								      AccelFlags.
								      Visible));
				ShowAll ();
				CsBoardApp.Instance.QuitEvent += OnQuitEvent;
			}

			private void OnQuitEvent (System.Object b,
						  EventArgs e)
			{
				client.Stop ();
			}

			public void SetVisibility (bool visible)
			{
				app_visible = visible;
			}

			public void MakeVisible ()
			{
				if (!app_visible)
					CsBoardApp.Instance.ShowApp (this);
			}

			private void OnAuth (object o, bool successful)
			{
				if (successful)
				  {
					  menubar.disconnectMenuItem.
						  Sensitive = true;
					  title = String.Format (Catalog.
								 GetString
								 ("ICS: {0}@{1}:{2}"),
								 client.User,
								 client.
								 server,
								 client.port);

					  RemoveConfigWidget ();
					  if (TitleChangedEvent != null)
						  TitleChangedEvent (this,
								     EventArgs.
								     Empty);
					  return;
				  }

				// on auth failure, reauthenticate
				configwidget.Sensitive = true;
				Authenticate ();
			}

			private void ShowConfigWidget ()
			{
				if (configwidget != null)
					return;

				configwidget = new ICSConfigWidget (client);
				int pageidx = book.NPages;
				book.ShowTabs = false;
				book.AppendPage (configwidget,
						 new Label (Catalog.
							    GetString
							    ("Login")));
				book.CurrentPage = pageidx;
				configwidget.ShowAll ();
			}

			private void RemoveConfigWidget ()
			{
				book.ShowTabs = true;

				book.RemovePage (book.CurrentPage);
				book.CurrentPage = 0;
				configwidget = null;
			}

			private void Authenticate ()
			{
				menubar.connectMenuItem.Sensitive = false;
				ShowConfigWidget ();
				//align.Show();

				if (configwidget.Run () ==
				    (int) ResponseType.Ok)
				  {
					  client.Start ();
					  configwidget.Sensitive = false;

				  }
				else
					menubar.connectMenuItem.Sensitive =
						true;

			}

			private void OnConnectionError (object o,
							string reason)
			{
				client.Stop ();
				// show error
				MessageDialog md = new MessageDialog (null,
								      DialogFlags.
								      DestroyWithParent,
								      MessageType.
								      Error,
								      ButtonsType.
								      Close,
								      String.
								      Format
								      ("<b>{0}</b>",
								       reason));

				md.Run ();
				md.Hide ();
				md.Dispose ();

				menubar.disconnectMenuItem.Sensitive = false;
				menubar.connectMenuItem.Sensitive = true;
				//configwidget.Sensitive = true;
				//Authenticate ();
			}

			protected void on_quit_activate (object o,
							 EventArgs args)
			{
				App.Close ();
			}

			protected void on_about_activate (object o,
							  EventArgs args)
			{
				CsBoardApp.ShowAboutDialog (null);
			}

			protected void on_edit_engines_activate (object o,
								 EventArgs
								 args)
			{
				CsBoardApp.ShowEngineChooser ();
			}

			protected void on_connect_activate (object o,
							    EventArgs args)
			{
				menubar.disconnectMenuItem.Sensitive = false;
				menubar.connectMenuItem.Sensitive = false;
				configwidget.Sensitive = true;
				Authenticate ();
			}

			protected void on_disconnect_activate (object o,
							       EventArgs args)
			{
				menubar.disconnectMenuItem.Sensitive = false;
				menubar.connectMenuItem.Sensitive = true;
				client.Stop ();
				Authenticate ();
			}

			private void OnChallengeEvent (object o,
						       MatchChallenge mc)
			{
				Console.WriteLine (mc);
				ShowChallengeDialog (mc);
			}

			private void ShowChallengeDialog (MatchChallenge mc)
			{
				StringBuilder buf = new StringBuilder ();
				string rating;

				if (mc.OpponentsRating != 0)
					rating = mc.OpponentsRating.
						ToString ();
				else
					rating = "----";
				buf.Append (String.
					    Format
					    ("<big><b>{0} ({1}) wants to play a {2} game</b></big>\n",
					     mc.Opponent, rating,
					     mc.Category));
				buf.Append (String.
					    Format
					    ("<b><u>Time:</u> {0} </b><i>mins</i>, <b><u>Increment:</u></b> {1}\n",
					     mc.Time, mc.Increment));
				if (mc.Color != null)
					buf.Append (String.
						    Format
						    ("\n<b><u>Color:</u></b> {0}\n",
						     mc.Color));

				buf.Append
					("\n\n<b>Do you want to play?</b>");

				MessageDialog dlg = new MessageDialog (null,
								       DialogFlags.
								       Modal,
								       MessageType.
								       Question,
								       ButtonsType.
								       YesNo,
								       true,
								       buf.
								       ToString
								       ());
				dlg.Modal = false;
				dlg.GrabFocus ();
				int ret = dlg.Run ();
				if (ret == (int) ResponseType.Yes)
					client.CommandSender.
						SendCommand ("accept");
				else if (ret == (int) ResponseType.No)
					client.CommandSender.
						SendCommand ("decline");
				dlg.Hide ();
				dlg.Dispose ();
			}
		}
	}
}
