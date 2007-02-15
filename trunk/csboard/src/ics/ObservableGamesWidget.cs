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

			ListStore store;

			Label infoLabel;
			int ngames;

			GameObservationManager obManager;

			static Pixbuf ComputerPixbuf =
				Gdk.Pixbuf.LoadFromResource ("computer.png");

			Expander expander;
			ComboBox ratedCombo, categoryCombo;
			Entry filterEntry;

			TreeModelFilter filter;

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

				store = new ListStore (typeof (object));
				gamesView.Model = store;
				gamesView.HeadersVisible = true;
				gamesView.HeadersClickable = true;

				/*
				filter = new TreeModelFilter(store, null);
				filter.VisibleFunc = FilterFunc;
				*/

				SetupColumns();
				SetupFilterWidgets();

				ScrolledWindow win = new ScrolledWindow ();
				  win.HscrollbarPolicy =
					win.VscrollbarPolicy =
					PolicyType.Automatic;
				  win.Add (gamesView);

				  UpdateInfoLabel ();

				  infoLabel.UseMarkup = true;
				  PackStart (infoLabel, false, true, 4);
				  PackStart(expander, false, true, 4);
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

			const int RATED_COMBO_ALL = 0;
			const int RATED_COMBO_RATED_ONLY = 1;

			const int CATEGORY_COMBO_ALL = 0;
			const int CATEGORY_COMBO_BLITZ = 1;
			const int CATEGORY_COMBO_LIGHTNING = 2;
			const int CATEGORY_COMBO_STANDARD = 3;
			const int CATEGORY_COMBO_OTHERS = 4;

			private void SetupFilterWidgets() {
				ratedCombo = new ComboBox(new string[] {
					Catalog.GetString("All"),
					Catalog.GetString("Rated")
				});

				categoryCombo = new ComboBox(new string[] {
					Catalog.GetString("Blitz"),
					Catalog.GetString("Lightning"),
					Catalog.GetString("Standard"),
					Catalog.GetString("Others")
				});

				expander = new Expander(Catalog.GetString("Filter"));
				expander.Expanded = false;

				VBox box = new VBox();
				filterEntry = new Entry();

				box.PackStart(filterEntry, true, false, 2);

				HBox hbox = new HBox();
				hbox.PackStart(categoryCombo, false, false, 2);
				hbox.PackStart(ratedCombo, false, false, 2);

				box.PackStart(hbox, false, false, 2);
				expander.Add(box);
				expander.ShowAll();

				filterEntry.Activated += OnFilter;
			}

			private void OnFilter(object o, EventArgs args) {
/*
				string str = filterEntry.Text.Trim();
				filterEntry.Text = str;

				if(str.Length == 0 && ratedCombo.Active == RATED_COMBO_ALL && categoryCombo.Active == CATEGORY_COMBO_ALL) {
					return;
				}

				gamesView.Model = filter;
				filter.Refilter();
*/
			}

			protected bool FilterFunc (TreeModel model,
						   TreeIter iter) {
				GameDetails details = (GameDetails) model.GetValue(iter, 0);

				if(ratedCombo.Active != RATED_COMBO_ALL && !details.Rated)
					return false;

				switch(categoryCombo.Active) {
				case CATEGORY_COMBO_BLITZ:
					if(details.GameCategory != GameCategory.Blitz)
						return false;
					break;
				case CATEGORY_COMBO_LIGHTNING:
					if(details.GameCategory != GameCategory.Lightning)
						return false;
					break;
				case CATEGORY_COMBO_STANDARD:
					if(details.GameCategory != GameCategory.Standard)
						return false;
					break;
				case CATEGORY_COMBO_OTHERS:
					if(details.GameCategory != GameCategory.Blitz &&
					   details.GameCategory != GameCategory.Lightning && details.GameCategory != GameCategory.Standard)
						return false;
					break;
				}

				if(details.white.ToLower().IndexOf(filterEntry.Text.ToLower()) < 0)
					return false;
				if(details.black.ToLower().IndexOf(filterEntry.Text.ToLower()) < 0)
					return false;

				return true;
			}


			private void SetupColumns() {
				TreeViewColumn col = new TreeViewColumn ();

				CellRendererText renderer =
					new CellRendererText ();
				  renderer.Yalign = 0;
				  col.Title = Catalog.GetString ("Games");
				  col.PackStart (renderer, false);
				  col.SetCellDataFunc (renderer,
						       new
						       TreeCellDataFunc
						       (GamesCellDataFunc));

				  gamesView.AppendColumn (col);

				  col = new TreeViewColumn ();
				  renderer = new CellRendererText ();
				  renderer.Yalign = 0;
				  col.Title = Catalog.GetString ("Time");
				  col.PackStart (renderer, false);
				  col.SetCellDataFunc (renderer,
						       new
						       TreeCellDataFunc
						       (TimeDetailsCellDataFunc));

				  gamesView.AppendColumn (col);

				  col = new TreeViewColumn ();
				  renderer = new CellRendererText ();
				  renderer.Yalign = 0;
				  col.Title = Catalog.GetString ("Category");
				  col.PackStart (renderer, false);
				  col.SetCellDataFunc (renderer,
						       new
						       TreeCellDataFunc
						       (CategoryCellDataFunc));
				  gamesView.AppendColumn (col);

				  col = new TreeViewColumn ();
				  renderer = new CellRendererText ();
				  renderer.Yalign = 0;
				  col.Title = Catalog.GetString ("Rated");
				  col.PackStart (renderer, false);
				  col.SetCellDataFunc (renderer,
						       new
						       TreeCellDataFunc
						       (RatedCellDataFunc));
				  gamesView.AppendColumn (col);

			}

			private void OnRowActivated (object o,
						     RowActivatedArgs args)
			{
				TreeIter iter;
				TreeView tree = (TreeView) o;
				  tree.Model.GetIter (out iter, args.Path);
				GameDetails details =
					(GameDetails) tree.Model.
					GetValue (iter, 0);
				  obManager.ObserveGame (details.gameId);
			}

			protected void GamesCellDataFunc (TreeViewColumn col,
							  CellRenderer r,
							  TreeModel model,
							  TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				GameDetails gd =
					(GameDetails) model.GetValue (iter,
								      0);
				if(gd == null) {
					return;
				}
				renderer.Markup = gd.ToPango ();
			}

			protected void CategoryCellDataFunc (TreeViewColumn
							     col,
							     CellRenderer r,
							     TreeModel model,
							     TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				GameDetails gd =
					(GameDetails) model.GetValue (iter,
								      0);
				if(gd == null)
					return;
				renderer.Text = gd.CategoryStr;
			}

			protected void RatedCellDataFunc (TreeViewColumn col,
							  CellRenderer r,
							  TreeModel model,
							  TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				GameDetails gd =
					(GameDetails) model.GetValue (iter,
								      0);
				if(gd == null)
					return;
				renderer.Text =
					gd.Rated ? "Rated" : "Unrated";
			}

			protected void TimeDetailsCellDataFunc (TreeViewColumn
								col,
								CellRenderer
								r,
								TreeModel
								model,
								TreeIter iter)
			{
				CellRendererText renderer =
					(CellRendererText) r;
				GameDetails gd =
					(GameDetails) model.GetValue (iter,
								      0);
				if(gd == null)
					return;
				renderer.Markup = gd.TimeDetailsAsMarkup ();
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
				store.AppendValues (details);
				UpdateInfoLabel ();
			}

			public void Clear ()
			{
				ngames = 0;
				store.Clear ();
				UpdateInfoLabel ();
			}
		}
	}
}
