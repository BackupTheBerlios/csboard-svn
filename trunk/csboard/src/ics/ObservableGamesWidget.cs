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
		public class ObservableGamesWidget:VBox
		{
			private Gtk.TreeView gamesView;

			TreeStore store;

			Label infoLabel;
			int ngames;

			GameObservationManager obManager;

			static protected Pixbuf ComputerPixbuf =
				Gdk.Pixbuf.LoadFromResource ("computer.png");

			Entry filterEntry;

			TreeModelFilter filter;
			TreeIter ratedGamesIter;
			TreeIter unratedGamesIter;
			TreeIter examGamesIter;

			public ObservableGamesWidget (GameObservationManager
						      observer)
			{
				obManager = observer;
				gamesView = new TreeView ();
				infoLabel = new Label ();
				infoLabel.Xalign = 0;
				infoLabel.Xpad = 4;
				observer.ObservableGameEvent +=
					OnObservableGameEvent;

				store = new TreeStore (typeof (string),	// used for filtering
						       typeof (int),	// gameid
						       typeof (string),	// markup
						       typeof (string),	// 
						       typeof (string));

				  AddParentIters ();

				  gamesView.HeadersVisible = true;
				  gamesView.HeadersClickable = true;

				  gamesView.AppendColumn (Catalog.
							  GetString ("Games"),
							  new
							  CellRendererText (),
							  "markup", 2);
				  gamesView.AppendColumn (Catalog.
							  GetString ("Time"),
							  new
							  CellRendererText (),
							  "markup", 3);
				  gamesView.AppendColumn (Catalog.
							  GetString
							  ("Category"),
							  new
							  CellRendererText (),
							  "markup", 4);

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy =
					win.VscrollbarPolicy =
					PolicyType.Automatic;
				  win.Add (gamesView);

				  UpdateInfoLabel ();

				  filterEntry = new Entry ();
				  filterEntry.Changed += OnFilter;

				  filter = new TreeModelFilter (store, null);
				  filter.VisibleFunc = FilterFunc;
				  gamesView.Model = filter;

				  infoLabel.UseMarkup = true;
				  PackStart (infoLabel, false, true, 4);
				  PackStart (filterEntry, false, true, 4);
				  PackStart (win, true, true, 4);

				Button refreshButton =
					new Button (Stock.Refresh);
				Alignment align = new Alignment (0, 1, 0, 0);
				  align.Add (refreshButton);
				  PackStart (align, false, false, 4);
				  refreshButton.Clicked +=
					delegate (object o, EventArgs args)
				{
					Clear ();
					obManager.GetGames ();
				};

				  gamesView.RowActivated += OnRowActivated;
				  SetSizeRequest (600, 400);
				  ShowAll ();
			}

			private void AddParentIters ()
			{
				ratedGamesIter = store.AppendValues ("",
								     0,
								     String.
								     Format
								     ("<b>{0}</b>",
								      Catalog.
								      GetString
								      ("Rated Games")),
								     "", "");
				unratedGamesIter =
					store.AppendValues ("", 0,
							    String.
							    Format
							    ("<b>{0}</b>",
							     Catalog.
							     GetString
							     ("Unrated Games")),
							    "", "");
				examGamesIter =
					store.AppendValues ("", 0,
							    String.
							    Format
							    ("<b>{0}</b>",
							     Catalog.
							     GetString
							     ("Examined Games")),
							    "", "");
			}

			private void OnFilter (object o, EventArgs args)
			{
				filter.Refilter ();
			}

			protected bool FilterFunc (TreeModel model,
						   TreeIter iter)
			{
				if (model.IterHasChild (iter))
					return true;

				string filterstr = filterEntry.Text.Trim ();
				if (filterstr.Length == 0)
					return true;

				string str =
					model.GetValue (iter, 0) as string;

				if (str.IndexOf (filterstr.ToLower ()) >= 0)
					return true;

				return false;
			}

			private void OnRowActivated (object o,
						     RowActivatedArgs args)
			{
				TreeIter iter;
				TreeView tree = o as TreeView;
				tree.Model.GetIter (out iter, args.Path);
				int gameId =
					(int) tree.Model.GetValue (iter, 1);
				if (gameId > 0)
					obManager.ObserveGame (gameId);
			}

			private void UpdateInfoLabel ()
			{
				infoLabel.Markup =
					String.Format ("<b>{0}: {1}</b>",
						       Catalog.
						       GetString
						       ("Number of games"),
						       ngames);
			}

			public void OnObservableGameEvent (object o,
							   GameDetails
							   details)
			{
				if (details.PrivateGame)
				  {
					  return;
				  }
				ngames++;
				string filter_string =
					String.Format ("{0} {1}",
						       details.white,
						       details.black);
				filter_string = filter_string.ToLower ();

				UpdateInfoLabel ();
				TreeIter iter =
					details.
					Examined ? examGamesIter : details.
					Rated ? ratedGamesIter :
					unratedGamesIter;

				store.AppendValues (iter,
						    filter_string,
						    details.gameId,
						    details.
						    ToPango (),
						    details.
						    TimeDetailsAsMarkup
						    (), details.CategoryStr);
			}

			public void Clear ()
			{
				ngames = 0;
				store.Clear ();
				UpdateInfoLabel ();
				filterEntry.Text = "";
				AddParentIters ();
			}
		}
	}
}
