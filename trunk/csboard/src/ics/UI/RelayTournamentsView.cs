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
using CsBoard.Viewer;

namespace CsBoard
{
	namespace ICS
	{
		public class RelayTournamentsView:VBox
		{
			TreeStore store;
			TreeView tree;
			ICSClient client;
			bool relay_pending = false;
			Button refreshButton;
			Label infoLabel;
			int ntourneys, ngames;
			TournamentsGetter getter;
			bool sendnotification = false;
			const int TOURNAMENTS_NOTIFICATION_TIMEOUT = 10;
			public RelayTournamentsView (ICSClient c)
			{
				client = c;
				store = new TreeStore (typeof (int),
						       typeof (string),
						       typeof (string));
				  create_tree ();
				  refreshButton = new Button (Stock.Refresh);
				  refreshButton.Clicked += OnClicked;
				  infoLabel = new Label ();
				  infoLabel.UseMarkup = true;
				  infoLabel.Xalign = 0;
				  infoLabel.Xpad = 4;
				  //infoLabel.Yalign = 0;
				  //infoLabel.Ypad = 4;
				HBox box = new HBox ();
				  box.PackStart (infoLabel, true, true, 4);
				  box.PackStart (refreshButton, false, false,
						 4);
				  PackStart (box, false, true, 4);

				ScrolledWindow scroll = new ScrolledWindow ();
				  scroll.HscrollbarPolicy =
					scroll.VscrollbarPolicy =
					PolicyType.Automatic;
				  scroll.Add (tree);
				  PackStart (scroll, true, true, 4);
				  client.AuthEvent += OnAuth;
				  ShowAll ();
			}

			void OnAuth (object o, bool successful)
			{
				if (!successful)
					return;
				tree.Hide();
				UpdateTournaments ();
				sendnotification = true;
			}

			private void OnClicked (object o, EventArgs args)
			{
				UpdateTournaments ();
			}

			private void create_tree ()
			{
				tree = new TreeView ();
				tree.Model = store;
				tree.HeadersVisible = false;
				//tree.HeadersClickable = true;

				CellRendererText renderer =
					new CellRendererText ();
				renderer.Yalign = 0;

				TreeViewColumn col = new TreeViewColumn ();
				//col.Title = Catalog.GetString ("Tournament");
				col.PackStart (renderer, false);
				col.SetAttributes (renderer, "markup", 1);
				CellRendererText resultRenderer =
					new CellRendererText ();
				resultRenderer.Xpad = 5;
				resultRenderer.Yalign = 0;
				col.PackStart (resultRenderer, false);
				col.SetAttributes (resultRenderer, "markup",
						   2);

				tree.AppendColumn (col);
				tree.RowActivated += OnRowActivated;
			}

			public void UpdateTournaments ()
			{
				if (relay_pending)
					return;
				relay_pending = true;
				store.Clear ();
				ntourneys = 0;
				ngames = 0;
				infoLabel.Markup =
					Catalog.
					GetString
					("<b>Getting tournament info...</b>");

				getter = new TournamentsGetter (client);
				getter.RelayTournamentEvent +=
					OnRelayTournament;
				getter.RelayTournamentGameEvent +=
					OnRelayTournamentGame;
				getter.Start ();
			}
			void OnRelayTournament (object o,
						RelayTournamentEventArgs args)
			{
				//addsample();
				if (args.IsEndOfList)
				  {
					  getter.RelayTournamentEvent -=
						  OnRelayTournament;
					  getter.RelayTournamentGameEvent -=
						  OnRelayTournamentGame;
					  if (ntourneys == 0) {
						  infoLabel.Markup =
							  String.
							  Format
							  ("<span color=\"#800000\"><big><b>{0}</b></big></span>",
							   Catalog.
							   GetString
							   ("There are no relay tournaments"));
						  tree.Hide();
						  relay_pending = false;
						  return;
					  }
					  infoLabel.Markup =
					    String.
					    Format (Catalog.
						    GetString
						    ("<b>Tournaments: {0}, Games {1}</b>"),
						    ntourneys,
						    ngames);
					  relay_pending = false;
					  if(sendnotification) {
					    ICSDetailsWidget ics = ICSDetailsWidget.Instance;
					    ics.NotificationWidget.SetNotification(new TournamentInfoNotification(ics, ntourneys + " tournaments available. Show?"), TOURNAMENTS_NOTIFICATION_TIMEOUT);
					    sendnotification = false;
					  }
					  return;
				  }

				tree.Show();

				store.AppendValues (args.Tournament.ID,
						    String.
						    Format
						    ("<span color=\"#702020\"><b>{0}</b>\n<small><i>{1}</i></small></span>",
						     args.Tournament.Name,
						     args.Tournament.
						     RoundInfo), "");
				ntourneys++;
				infoLabel.Markup =
					String.Format (Catalog.
						       GetString
						       ("<b>Tournaments: {0}, Games {1} ...</b>"),
						       ntourneys, ngames);
			}

			private bool find_iter (int tournamentid,
						out TreeIter iter)
			{
				TreeIter curiter;
				bool ret;
				for (ret = store.GetIterFirst (out curiter);
				     ret; ret = store.IterNext (ref curiter))
				  {
					  if (tournamentid ==
					      (int) store.GetValue (curiter,
								    0))
					    {
						    iter = curiter;
						    return true;
					    }
				  }

				iter = TreeIter.Zero;
				return false;
			}

			void OnRelayTournamentGame (object o,
						    RelayTournamentGameEventArgs
						    args)
			{
				TreeIter iter;
				if (!find_iter (args.Tournament.ID, out iter))
				  {
					  return;
				  }
				ngames++;
				CsBoard.ICS.Relay.Game game = args.Game;
				string opening = null;
				if (GameViewer.EcoDb != null)
					opening =
						GameViewer.EcoDb.
						GetOpeningName (game.Opening);
				if (opening == null)
					opening = game.Opening;
				else
					opening =
						String.
						Format ("<b>{0}</b> ({1})",
							opening,
							game.Opening);

				string result;
				if (game.Result.Equals ("*"))
				  {
					  result = String.
						  Format
						  ("<span color=\"#808080\">{0}</span>",
						   Catalog.
						   GetString ("In Progress"));
				  }
				else if (game.Result.Equals ("1/2-1/2"))
				  {
					  result = Catalog.GetString ("Draw");
				  }
				else if (game.Result.Equals ("1-0"))
				  {
					  result = Catalog.
						  GetString ("White Won");
				  }
				else if (game.Result.Equals ("0-1"))
				  {
					  result = Catalog.
						  GetString ("Black Won");
				  }
				else
				  {
					  result = game.Result;
				  }

				store.AppendValues (iter, game.ID,
						    String.
						    Format
						    ("<b>{0}</b> - <b>{1}</b>\n"
						     + "<small>{2}</small>",
						     game.White, game.Black,
						     opening),
						    String.
						    Format ("<b>{0}</b>",
							    result));
				infoLabel.Markup =
					String.Format (Catalog.
						       GetString
						       ("<b>Tournaments: {0}, Games {1} ...</b>"),
						       ntourneys, ngames);
			}

			private void OnRowActivated (object o,
						     RowActivatedArgs args)
			{
				TreeIter iter;
				if (args.Path.Depth <= 1)
					return;
				tree.Model.GetIter (out iter, args.Path);
				int gameId =
					(int) tree.Model.GetValue (iter, 0);
				if (gameId > 0)
				  {
					  client.CommandSender.
						  SendCommand ("observe " +
							       gameId);
				  }
			}
		}
	}
}
