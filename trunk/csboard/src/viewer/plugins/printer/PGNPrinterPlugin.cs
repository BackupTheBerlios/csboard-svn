using System;
using System.Collections;
using System.IO;

using Gtk;
using Gnome;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;
using Mono.Unix;

namespace CsBoard
{
	namespace Viewer
	{
		public class PGNPrinterPlugin:CsPlugin, IExporter,
			IPrintHandler
		{
			GameViewer viewer;
			MenuItem exportPsMenuItem;

			public PGNPrinterPlugin ():base ("pgn-printer",
							 Catalog.
							 GetString
							 ("PGN Printer"),
							 Catalog.
							 GetString
							 ("Prints PGN games and exports."))
			{
			}

			public EventHandler OnPrintActivated
			{
				get
				{
					return on_print_activate;
				}
			}

			public override bool Initialize ()
			{
				viewer = GameViewer.Instance;
				if (viewer == null)
					return false;

				viewer.RegisterPrintHandler (this);

				ImageMenuItem item =
					new ImageMenuItem (Catalog.
							   GetString
							   ("_PS File"));
				  item.Image =
					new Image (Gtk.Stock.SaveAs,
						   IconSize.Menu);
				  exportPsMenuItem = item;
				  exportPsMenuItem.Activated +=
					on_export_ps_activate;
				  exportPsMenuItem.Show ();
				  viewer.RegisterExporter (this,
							   exportPsMenuItem);
				  return true;
			}

			public override bool Shutdown ()
			{
				viewer.UnregisterPrintHandler (this);
				viewer.UnregisterExporter (this,
							   exportPsMenuItem);
				return true;
			}

			public bool Export (IList games)
			{
				return false;
			}

			private void on_export_ps_activate (object obj,
							    EventArgs args)
			{
				if (viewer.ChessGameWidget.Games == null)
					return;
				string file = viewer.AskForFile (null,
								 Catalog.
								 GetString
								 ("Export as a PostScript document to file"),
								 false);
				if (file == null)
					return;
				PrintWrapper printer = new PrintWrapper ();
				new ExportHandler (viewer,
						   viewer.ChessGameWidget.
						   Games, printer, file);
			}

			private void on_print_activate (object obj,
							EventArgs args)
			{
				if (viewer.ChessGameWidget.Games == null)
					return;
				PrintWrapper printer = new PrintWrapper ();
				PrintDialog dialog =
					new PrintDialog (printer.PrintJob,
							 Catalog.
							 GetString
							 ("Print PGN File"),
							 0);
				int response = dialog.Run ();

				if (response == (int) PrintButtons.Cancel)
				  {
					  dialog.Hide ();
					  dialog.Dispose ();
					  return;
				  }
				new PrintHandler (viewer,
						  viewer.ChessGameWidget.
						  Games, printer, response);

				dialog.Hide ();
				dialog.Dispose ();
			}
		}

		abstract class PGNExportHandler
		{
			protected IList games;
			protected PrintWrapper printer;
			protected int totalgames;
			protected double ngames;	// so that a we can generate a fraction
			protected GameViewer viewer;

			public PGNExportHandler (GameViewer viewer,
						 IList games,
						 PrintWrapper printer)
			{
				this.games = games;
				this.viewer = viewer;
				this.printer = printer;
				totalgames = games.Count;
				ngames = 0;
				GLib.Idle.
					Add (new
					     GLib.IdleHandler
					     (PGNExportIdleHandler));
			}

			protected void OnGamePrinted (System.Object o,
						      EventArgs args)
			{
				ngames++;
				viewer.UpdateProgress (ngames / totalgames);
			}

			private bool PGNExportIdleHandler ()
			{
				PGNPrinter pr =
					new PGNPrinter (games, printer);
				pr.GamePrinted += OnGamePrinted;
				viewer.StartProgress ();
				pr.Print ();
				viewer.ProgressBar.Text =
					Catalog.GetString ("Now printing...");
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
				HandlePrinted ();
				viewer.ProgressBar.Text =
					Catalog.GetString ("Done.");
				viewer.StopProgress ();
				return false;
			}

			protected abstract void HandlePrinted ();
		}

		class PrintHandler:PGNExportHandler
		{
			int response;
			public PrintHandler (GameViewer viewer,
					     IList games,
					     PrintWrapper printer,
					     int response):base (viewer,
								 games,
								 printer)
			{
				this.response = response;
			}

			protected override void HandlePrinted ()
			{
				switch (response)
				  {
				  case (int) PrintButtons.Print:
					  printer.PrintJob.Print ();
					  break;
				  case (int) PrintButtons.Preview:
					  new PrintJobPreview (printer.
							       PrintJob,
							       Catalog.
							       GetString
							       ("Print Preview")).
						  Show ();
					  break;
				  }
			}
		}


		class ExportHandler:PGNExportHandler
		{
			string file;

			public ExportHandler (GameViewer viewer,
					      IList games,
					      PrintWrapper printer,
					      string file):base (viewer,
								 games,
								 printer)
			{
				this.file = file;
			}

			protected override void HandlePrinted ()
			{
				printer.Export (file);
			}
		}
	}
}
