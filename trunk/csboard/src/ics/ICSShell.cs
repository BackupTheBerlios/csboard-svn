using System;
using Gtk;
using Mono.Unix;

namespace CsBoard {
	namespace ICS {
	public class ICSShell : VBox {
		Button sendButton;
		Entry commandEntry;
		TextView textView;
		int max_chars;

		ICSClient client;

		public ICSShell(ICSClient client) : base() {
			this.client = client;
			max_chars = 16 * 1024;
			textView = new TextView();
			textView.ModifyFont(Pango.FontDescription.FromString("Monospace 9"));
			client.LineReceivedEvent += OnLineReceived;

			commandEntry = new Entry();
			sendButton = new Button(Catalog.GetString("Send"));

			ScrolledWindow win = new ScrolledWindow();
			win.HscrollbarPolicy = win.VscrollbarPolicy = PolicyType.Automatic;
			win.Add(textView);

			PackStart(win, true, true, 4);
			HBox box = new HBox();
			box.PackStart(commandEntry, true, true, 4);
			box.PackStart(sendButton, false, false, 4);
			PackStart(box, false, true, 4);

			textView.Editable = false;

			commandEntry.Activated += OnCommand;
			sendButton.Clicked += OnCommand;
			ShowAll();
		}

		private void OnCommand(object o, EventArgs args) {
			string cmd = commandEntry.Text.Trim();
			commandEntry.Text = "";
			if(cmd.Length == 0)
				return;

			client.WriteLine(cmd);
			AddLineToBuffer(cmd);
		}

		private void OnLineReceived(object o, LineReceivedEventArgs args) {
			if(args.LineType != LineType.Normal && args.LineType != LineType.Talk)
				return;
			string line = args.Line;
			AddLineToBuffer(line);
		}

		private void AddLineToBuffer(string line) {
			TextBuffer buffer = textView.Buffer;
			int len = line.Length;

			while(buffer.CharCount + len > max_chars) {
				// remove a line from the beginning of the buffer
				TextIter startIter = buffer.StartIter;
				TextIter endIter = startIter.Copy();
				if(!endIter.ForwardToLineEnd())
					break;
				buffer.Delete(startIter, endIter);
			}

			buffer.Insert(buffer.EndIter, line);
			buffer.Insert(buffer.EndIter, "\n");
			textView.ScrollToIter(buffer.EndIter, 0, false, 0, 0);
		}
	}
	}
}
