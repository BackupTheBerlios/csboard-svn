using System;
using System.Collections;
using System.IO;

using Gtk;
using Gnome;

using Chess.Parser;
using Chess.Game;
using CsBoard.Plugin;

namespace CsBoard
{
	namespace Viewer
	{
		public class PGNPrinterPlugin:CsPlugin, IExporter,
			IPrintHandler
		{
			GameViewer viewer;
			MenuItem exportPsMenuItem;
			string file;
			bool loadingInProgress;

			public PGNPrinterPlugin ():base ("pgn-printer",
							 "PGN Printer",
							 "Prints PGN games and exports.")
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

				exportPsMenuItem = new MenuItem ("PS File");
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
				if (viewer.Games == null)
					return;
				string file =
					GameViewer.AskForFile (viewer.Window,
							       "Export as a PostScript document to file",
							       false);
				if (file == null)
					return;
				PrintWrapper printer = new PrintWrapper ();
				ProgressDialog prog =
					new ProgressDialog (viewer.Window,
							    "Exporting...");
				ExportHandler exp =
					new ExportHandler (prog, viewer.Games,
							   printer, file);
				prog.Run ();
				prog.Hide ();
				prog.Dispose ();
			}

			private void on_print_activate (object obj,
							EventArgs args)
			{
				if (viewer.Games == null)
					return;
				PrintWrapper printer = new PrintWrapper ();
				PrintDialog dialog =
					new PrintDialog (printer.PrintJob,
							 "Print PGN File", 0);
				int response = dialog.Run ();

				if (response == (int) PrintButtons.Cancel)
				  {
					  dialog.Hide ();
					  dialog.Dispose ();
					  return;
				  }
				ProgressDialog prog =
					new ProgressDialog (dialog,
							    "Printing...");
				prog.ShowAll ();
				new PrintHandler (prog, viewer.Games, printer,
						  response);
				prog.Run ();	// The PrintHandler will bail us out!
				prog.Hide ();
				prog.Dispose ();

				dialog.Hide ();
				dialog.Dispose ();
			}
		}

		abstract class PGNExportHandler
		{
			protected ProgressDialog dlg;
			protected ArrayList games;
			protected PrintWrapper printer;
			protected int totalgames;
			protected double ngames;	// so that a we can generate a fraction

			public PGNExportHandler (ProgressDialog d,
						 ArrayList games,
						 PrintWrapper printer)
			{
				dlg = d;
				this.games = games;
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
				dlg.UpdateProgress (ngames / totalgames);
			}

			private bool PGNExportIdleHandler ()
			{
				PGNPrinter pr =
					new PGNPrinter (games, printer);
				pr.GamePrinted += OnGamePrinted;
				pr.Print ();
				dlg.bar.Text = "Now printing...";
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
				HandlePrinted ();
				dlg.bar.Text = "Done.";
				dlg.Respond (ResponseType.None);
				return false;
			}

			protected abstract void HandlePrinted ();
		}

		class PrintHandler:PGNExportHandler
		{
			int response;
			public PrintHandler (ProgressDialog d,
					     ArrayList games,
					     PrintWrapper printer,
					     int response):base (d, games,
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
							       "Print Preview").
						  Show ();
					  break;
				  }
			}
		}


		class ExportHandler:PGNExportHandler
		{
			string file;

			public ExportHandler (ProgressDialog d,
					      ArrayList games,
					      PrintWrapper printer,
					      string file):base (d, games,
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
