using Glade;
using Gtk;
using System.Collections;

namespace CsBoard
{
	namespace Viewer
	{
		public class GamesCollectionDialog
		{
			[Glade.Widget] private Gtk.Dialog collectionDialog;
			[Glade.Widget] private Gtk.Entry titleEntry;
			[Glade.Widget] private Gtk.
				TextView descriptionTextView;

			public string Description
			{
				get
				{
					return descriptionTextView.Buffer.
						Text;
				}
			}

			public string Title
			{
				get
				{
					return titleEntry.Text;
				}
			}

			public Dialog Dialog
			{
				get
				{
					return collectionDialog;
				}
			}

			private GamesCollectionDialog ()
			{
				Glade.XML xml =
					Glade.XML.
					FromAssembly ("gamedb.glade",
						      "collectionDialog",
						      null);
				xml.Autoconnect (this);
			}

			public static GamesCollectionDialog
				FromGameCollection (GameCollection collection)
			{
				GamesCollectionDialog dlg =
					new GamesCollectionDialog ();
				dlg.titleEntry.Text = collection.Title;
				dlg.descriptionTextView.Buffer.Text =
					collection.Description;

				return dlg;
			}

			public static GamesCollectionDialog CreateEmpty ()
			{
				return new GamesCollectionDialog ();
			}
		}

		public class EditGamesCollectionDialog
		{
			[Glade.Widget] private Gtk.Entry titleEntry;
			[Glade.Widget] private Gtk.TextView descriptionView;
			[Glade.Widget] private Gtk.VBox gamesListBox;
			[Glade.Widget] private Gtk.
				Button removeSelectedGamesButton;
			[Glade.Widget] private Gtk.
				Dialog gamesCollectionDialog;

			public Dialog Dialog
			{
				get
				{
					return gamesCollectionDialog;
				}
			}

			GamesListWidget collectionGamesListWidget;
			GameCollection col;

			public EditGamesCollectionDialog (GameCollection col)
			{
				Glade.XML xml =
					Glade.XML.
					FromAssembly ("gamedb.glade",
						      "gamesCollectionDialog",
						      null);
				xml.Autoconnect (this);

				removeSelectedGamesButton.Clicked +=
					OnRemoveSelectedGamesButtonClicked;

				collectionGamesListWidget =
					new GamesListWidget ();
				gamesListBox.Add (collectionGamesListWidget);

				collectionGamesListWidget.View.SelectionMode =
					SelectionMode.Multiple;

				this.col = col;
				SetGamesCollection (col);
			}

			private void
				OnRemoveSelectedGamesButtonClicked (object o,
								    System.
								    EventArgs
								    args)
			{
				TreePath[]selected =
					collectionGamesListWidget.View.
					SelectedItems;
				if (selected == null || selected.Length == 0)
					return;

				TreeIter iter;
				ArrayList iters = new ArrayList ();
				foreach (TreePath path in selected)
				{
					collectionGamesListWidget.View.Model.
						GetIter (out iter, path);
					iters.Add (iter);
					PGNGameDetails info =
						(PGNGameDetails)
						collectionGamesListWidget.
						View.Model.GetValue (iter, 0);
					col.RemoveGame (info);
				}

				for (int i = 0; i < iters.Count; i++)
				  {
					  iter = (TreeIter) iters[i];
					  ((ListStore)
					   collectionGamesListWidget.View.
					   Model).Remove (ref iter);
				  }

				GameDb.Instance.AddCollection (col);	// this will actually save the collection
			}

			private void SetGamesCollection (GameCollection col)
			{
				titleEntry.Text = col.Title;
				descriptionView.Buffer.Text = col.Description;

				ArrayList games = new ArrayList ();
				col.LoadGames (games);
				collectionGamesListWidget.SetGames (games);
			}
		}
	}
}
