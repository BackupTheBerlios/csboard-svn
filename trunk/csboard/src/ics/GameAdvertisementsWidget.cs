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
using Mono.Unix;
using Gdk;
using System.Reflection;


namespace CsBoard
{
	namespace ICS
	{
		public class GameAdvertisements:VBox
		{
			private Gtk.TreeView adList;

			TreeStore store;

			ICSClient client;
			Label infoLabel;
			int ngames;
			int nrated;

			static Pixbuf ComputerPixbuf =
				Gdk.Pixbuf.LoadFromResource ("computer.png");

			TreeIter ratedGamesIter, unratedGamesIter;

			public GameAdvertisements (ICSClient client)
			{
				adList = new TreeView ();
				infoLabel = new Label ();
				infoLabel.Xalign = 0;
				infoLabel.Xpad = 4;
				this.client = client;

				client.GameAdvertisementAddEvent +=
					OnGameAdvertisementAddEvent;
				client.GameAdvertisementRemoveEvent +=
					OnGameAdvertisementRemoveEvent;
				client.GameAdvertisementsClearedEvent +=
					OnGameAdvertisementsCleared;
				client.AuthEvent += OnAuthEvent;

				store = new TreeStore (typeof (int), typeof (Gdk.Pixbuf),	// computer?
						       typeof (string),	// details
						       typeof (string),	// rating
						       typeof (string),	// timing
						       typeof (string)	// category
					);
				  adList.Model = store;
				  adList.HeadersVisible = true;
				  adList.HeadersClickable = true;

				  AddParentIters ();

				TreeViewColumn col = new TreeViewColumn ();

				CellRendererPixbuf title_renderer =
					new CellRendererPixbuf ();
				  title_renderer.Yalign = 0;
				  col.PackStart (title_renderer, false);
				  col.SetAttributes (title_renderer, "pixbuf",
						     1);

				CellRendererText renderer =
					new CellRendererText ();
				  renderer.Yalign = 0;
				  col.Title = Catalog.GetString ("Games");
				  col.PackStart (renderer, false);
				  col.SetAttributes (renderer, "markup", 2);

				  adList.AppendColumn (col);
				  adList.AppendColumn (Catalog.
						       GetString ("Rating"),
						       new
						       CellRendererText (),
						       "text", 3);

				  adList.AppendColumn (Catalog.
						       GetString ("Timing"),
						       new
						       CellRendererText (),
						       "markup", 4);
				  adList.AppendColumn (Catalog.
						       GetString ("Category"),
						       new
						       CellRendererText (),
						       "text", 5);

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy =
					win.VscrollbarPolicy =
					PolicyType.Automatic;
				  win.Add (adList);

				  UpdateInfoLabel ();

				  infoLabel.UseMarkup = true;
				  PackStart (infoLabel, false, true, 4);
				  PackStart (win, true, true, 4);

				  adList.RowActivated += OnRowActivated;
				  SetSizeRequest (600, 400);
				  ShowAll ();
			}

			public void OnGameAdvertisementAddEvent (object o,
								 GameAdvertisement
								 ad)
			{
				store.AppendValues (ad.
						    rated ? ratedGamesIter :
						    unratedGamesIter,
						    ad.gameHandle,
						    ad.
						    IsComputer ?
						    ComputerPixbuf : null,
						    ad.ToPango (),
						    ad.rating.ToString (),
						    String.
						    Format
						    ("<b>{0}  +{1}</b>",
						     ad.time_limit,
						     ad.time_increment),
						    ad.category);
				ngames++;
				if (ad.rated)
					nrated++;
				UpdateInfoLabel ();
			}

			private void OnGameAdvertisementsCleared (object o,
								  EventArgs
								  args)
			{
				store.Clear ();
				ngames = 0;
				nrated = 0;
				UpdateInfoLabel ();
				AddParentIters ();
			}

			public void OnGameAdvertisementRemoveEvent (object o,
								    GameAdvertisement
								    ad)
			{
				ngames--;
				if (ad.rated)
					nrated--;
				RemoveAdvertisement (ad.gameHandle);
				UpdateInfoLabel ();
			}

			int gameHandleToBeRemoved;

			private bool TreeModelForeach (TreeModel model,
						       TreePath path,
						       TreeIter iter)
			{
				if (model.IterHasChild (iter))
				  {
					  return false;
				  }
				int gameHandle =
					(int) store.GetValue (iter, 0);
				if (gameHandle != gameHandleToBeRemoved)
				  {
					  return false;
				  }
				// remove it
				store.Remove (ref iter);
				return true;
			}

			private void RemoveAdvertisement (int gameHandle)
			{
				gameHandleToBeRemoved = gameHandle;
				store.Foreach (TreeModelForeach);
			}

			private void UpdateInfoLabel ()
			{
				infoLabel.Markup =
					String.
					Format ("<b>{0}: {1}, {2}: {3}</b>",
						Catalog.
						GetString
						("Total game seeks"), ngames,
						Catalog.GetString ("Rated"),
						nrated);
			}

			private void AddParentIters ()
			{
				ratedGamesIter = store.AppendValues (0,
								     null,
								     String.
								     Format
								     ("<b>{0}</b>",
								      Catalog.
								      GetString
								      ("Rated")),
								     "");
				unratedGamesIter =
					store.AppendValues (0, null,
							    String.
							    Format
							    ("<b>{0}</b>",
							     Catalog.
							     GetString
							     ("Unrated Games")),
							    "");
			}

			public void OnAuthEvent (object o, bool success)
			{
			}

			private void OnRowActivated (object o,
						     RowActivatedArgs args)
			{
				TreeIter iter;
				TreeView tree = o as TreeView;
				adList.Model.GetIter (out iter, args.Path);
				int gameId =
					(int) tree.Model.GetValue (iter, 0);
				if (gameId > 0)
				  {
					  client.CommandSender.
						  SendCommand ("play " +
							       gameId);
				  }
			}
		}
	}
}
