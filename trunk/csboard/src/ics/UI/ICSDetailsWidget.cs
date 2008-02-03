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
using System.Collections;
using System.Text;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{
		public class ICSDetailsWidget:Frame, SubApp
		{
			public event TitleChangedEventHandler
				TitleChangedEvent;
			GameObservationManager obManager;
			ObservableGamesView observableGames;
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

			public string ID
			{
				get
				{
					return "icsplayer";
				}
			}

			public AccelGroup AccelGroup
			{
				get
				{
					return menubar.AccelGroup;
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
				book.TabPos = PositionType.Left;
				book.Show ();

				Add (book);

				obManager =
					new GameObservationManager (client,
								    this);

				observableGames =
					new ObservableGamesView (obManager,
								 client);

				add_seek_graph_page ();
				add_game_seeks_page ();
				add_watch_games_page ();
				add_shell_page ();

				client.ChallengeEvent += OnChallengeEvent;

				client.AuthEvent += OnAuth;
				client.ConnectionErrorEvent +=
					OnConnectionError;

				ShowConfigWidget ();

				menubar.disconnectMenuItem.Sensitive = false;
				GLib.Idle.Add (delegate ()
					       {
					       Authenticate ();
					       return false;
					       }
				);

				ShowAll ();
				CsBoardApp.Instance.QuitEvent += OnQuitEvent;
				menubar.showTabsMenuItem.Activated +=
					on_showtabs_activated;
			}

			public void AddPage (Widget page, Widget label,
					     MenuItem item)
			{
				Menu menu =
					menubar.viewMenuItem.Submenu as Menu;
				book.AppendPage (page, label);
				menu.Append (item);
			}

			public void RemovePage (Widget page, MenuItem item)
			{
				int index = book.PageNum (page);
				book.RemovePage (index);
				(menubar.viewMenuItem.Submenu as Menu).
					Remove (item);
			}

			private void add_seek_graph_page ()
			{
				graph = new GameAdvertisementGraph (client);
				MenuItem item =
					new PageMenuItem (Catalog.
							  GetString
							  ("Seek _Graph"),
							  graph, book);
				AddPage (graph,
					 new Label (Catalog.
						    GetString ("Seek Graph")),
					 item);
			}

			private void add_game_seeks_page ()
			{
				/*
				   book.AppendPage (graph,
				   GetLabelImage
				   ("graphicon.png"));
				 */
				ads = new GameAdvertisements (client);
				MenuItem item =
					new PageMenuItem (Catalog.
							  GetString
							  ("Game _Seeks"),
							  ads, book);
				AddPage (ads,
					 new Label (Catalog.
						    GetString ("Game Seeks")),
					 item);
			}

			private void add_watch_games_page ()
			{
				MenuItem item =
					new PageMenuItem (Catalog.
							  GetString
							  ("_Watch Games"),
							  observableGames,
							  book);
				AddPage (observableGames,
					 new Label (Catalog.
						    GetString
						    ("Watch Games")), item);
			}

			private void add_shell_page ()
			{
				shell = new ICSShell (client);
				MenuItem item =
					new PageMenuItem (Catalog.
							  GetString
							  ("S_hell"), shell,
							  book);
				AddPage (shell,
					 new Label (Catalog.
						    GetString ("Shell")),
					 item);
			}

			private static Image GetLabelImage (string name)
			{
				Image img = Image.LoadFromResource (name);
				return new Image (img.Pixbuf.
						  ScaleSimple (30, 30,
							       Gdk.InterpType.
							       Bilinear));
			}

			private void OnQuitEvent (System.Object b,
						  EventArgs e)
			{
				client.Stop ();
			}

			private void on_showtabs_activated (object o,
							    EventArgs args)
			{
				book.ShowTabs =
					menubar.showTabsMenuItem.Active;
				App.Session.ICSShowTabs =
					menubar.showTabsMenuItem.Active;
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
				DisableSubmenu (menubar.viewMenuItem);
			}

			private void RemoveConfigWidget ()
			{
				book.ShowTabs =
					menubar.showTabsMenuItem.Active;

				book.RemovePage (book.CurrentPage);
				book.CurrentPage = 0;
				configwidget = null;
				EnableSubmenu (menubar.viewMenuItem);
			}

			private void DisableSubmenu (MenuItem item)
			{
				foreach (Widget child
					 in (item.Submenu as Menu))
				{
					child.Sensitive = false;
				}
			}

			private void EnableSubmenu (MenuItem item)
			{
				foreach (Widget child
					 in (item.Submenu as Menu))
				{
					child.Sensitive = true;
				}
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
