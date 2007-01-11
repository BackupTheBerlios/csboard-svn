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

using Glade;
using Gtk;
using System;
using System.Collections;
using Mono.Unix;

namespace CsBoard {
	namespace Viewer {
	public class GamesEditorDialog {

		[Glade.Widget] private Gtk.ComboBox ratingComboBox;
		[Glade.Widget] private Gtk.TreeView tagsListView;
		[Glade.Widget] private Gtk.TreeView gamesListView;
		[Glade.Widget] private Gtk.HPaned splitPane;
		[Glade.Widget] private Gtk.Dialog editGamesDialog;
		[Glade.Widget] private Gtk.Entry addTagEntry;

		public Dialog Dialog {
			get {
				return editGamesDialog;
			}
		}

		public HPaned SplitPane {
			get {
				return splitPane;
			}
		}

		ListStore tagsStore;

		GamesList gamesList;

		Hashtable ratingMap;

		GameRating[] ratingValues;

		PGNGameDetails selectedGame;

		ArrayList modifiedGames;

		public GamesEditorDialog(ArrayList games, ArrayList modifiedGames) {
			this.modifiedGames = modifiedGames;

			Glade.XML xml = Glade.XML.FromAssembly("gamedb.glade", "editGamesDialog", null);
			xml.Autoconnect(this);

			ratingMap = new Hashtable();

			/* Note: this order should match the order in the glade file */
			ratingValues = new GameRating[] {
				GameRating.Average,
				GameRating.Good,
				GameRating.Excellent,
				GameRating.MustHave,
				GameRating.Unknown,
				GameRating.Ignore
			};

			int i = 0;
			foreach(GameRating r in ratingValues) {
				ratingMap[r] = i++;
			}

			tagsStore = new ListStore(typeof(string));

			tagsListView.Model = tagsStore;
			gamesList = new GamesList(gamesListView);

			gamesList.SetGames(games);

			tagsListView.AppendColumn(new TreeViewColumn("Tags", new CellRendererText(), "text", 0));
			gamesListView.CursorChanged += OnCursorChanged;
		}

		private void OnCursorChanged(object o, EventArgs args) {
			TreePath path;
			TreeViewColumn col;
			gamesListView.GetCursor(out path, out col);

			TreeIter iter;
			gamesListView.Model.GetIter(out iter, path);
			PGNGameDetails info = (PGNGameDetails) gamesListView.Model.GetValue(iter, 0);

			selectedGame = info;
			RefreshGameInfo();
		}

		public void RefreshGameInfo() {
			if(selectedGame == null)
				return;

			ratingComboBox.Active = (int) ratingMap[selectedGame.Rating];
			tagsStore.Clear();
			if(selectedGame.Tags == null)
				return;

			foreach(string tag in selectedGame.Tags) {
				tagsStore.AppendValues(tag);
			}
		}

		public void OnDeleteTagClicked(object o, EventArgs args) {
			if(selectedGame == null)
				return;
			TreePath path;
			TreeViewColumn col;
			tagsListView.GetCursor(out path, out col);
			if(path == null)
				return;

			TreeIter iter;
			tagsStore.GetIter(out iter, path);
			string tag = (string) tagsStore.GetValue(iter, 0);
			if(selectedGame.RemoveTag(tag))
				MarkCurrentGameDirty();
		}

		public void OnAddTagActivated(object o, EventArgs args) {
			if(selectedGame == null)
				return;
			string tag = addTagEntry.Text.Trim();
			if(tag.Length == 0)
				return;
			selectedGame.AddTag(tag);
			addTagEntry.Text = "";
			MarkCurrentGameDirty();
		}

		void MarkCurrentGameDirty() {
			if(!modifiedGames.Contains(selectedGame))
				modifiedGames.Add(selectedGame);
			RefreshGameInfo();
		}
	}
	}
}
