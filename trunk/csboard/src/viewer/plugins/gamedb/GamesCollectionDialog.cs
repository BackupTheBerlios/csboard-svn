using Glade;
using Gtk;
using System.Collections;

namespace CsBoard {
	namespace Viewer {
		public class GamesCollectionDialog {
			[Glade.Widget] private Gtk.Dialog collectionDialog;
			[Glade.Widget] private Gtk.Entry titleEntry;
			[Glade.Widget] private Gtk.TextView descriptionTextView;
			
			public string Description {
				get
				{
					return descriptionTextView.Buffer.Text;
				}
			}

			public string Title {
				get
				{
					return titleEntry.Text;
				}
			}

			public Dialog Dialog {
				get
				{
					return collectionDialog;
				}
			}

			private GamesCollectionDialog()
			{
				Glade.XML xml =
					Glade.XML.
					FromAssembly ("gamedb.glade",
						      "collectionDialog", null);
				xml.Autoconnect (this);
			}

			public static GamesCollectionDialog FromGameCollection(GameCollection collection)
			{
				GamesCollectionDialog dlg = new GamesCollectionDialog();
				dlg.titleEntry.Text = collection.Title;
				dlg.descriptionTextView.Buffer.Text = collection.Description;
				
				return dlg;
			}

			public static GamesCollectionDialog CreateEmpty()
			{
				return new GamesCollectionDialog();
			}
		}

		public class EditGamesCollectionDialog {
			[Glade.Widget] private Gtk.Entry titleEntry;
			[Glade.Widget] private Gtk.TextView descriptionView;
			[Glade.Widget] private Gtk.TreeView gamesTreeView;
			[Glade.Widget] private Gtk.Button deleteGamesButton;
			[Glade.Widget] private Gtk.Dialog gamesCollectionDialog;

			public Dialog Dialog {
				get {
					return gamesCollectionDialog;
				}
			}

			GamesList collectionGamesList;

			public EditGamesCollectionDialog(GameCollection col) {
				Glade.XML xml = Glade.XML.FromAssembly("gamedb.glade", "gamesCollectionDialog", null);
				xml.Autoconnect(this);

				collectionGamesList = new GamesList(gamesTreeView);

				gamesTreeView.Selection.Mode =
					SelectionMode.Multiple;

				SetGamesCollection(col);
			}

			private void SetGamesCollection(GameCollection col) {
				titleEntry.Text = col.Title;
				descriptionView.Buffer.Text = col.Description;
				
				ArrayList games = new ArrayList();
				col.LoadGames(games);
				collectionGamesList.SetGames(games);
			}
		}
	}
}
