using System;

namespace CsBoard {
	namespace ICS {
	public class CommandResponseLineEventArgs : EventArgs {
		public int commandId;
		public byte[] buffer;
		public int start;
		public int end;

		public CommandResponseLineEventArgs(int commandId, byte[] buffer, int start, int end) {
			this.commandId = commandId;
			this.buffer = buffer;
			this.start = start;
			this.end = end;
		}
	}

	public delegate void CommandResponseLineEventHandler(object o, CommandResponseLineEventArgs args);
	public delegate void CommandCompletedEventHandler(object o, int commandId);

	public class CommandSender {
		ICSClient client;
		int commandId;
		int pending;

		int currentCommand;
		const int MAX_COMMANDS = 9;
		int blockCount;

		public event CommandResponseLineEventHandler CommandResponseLineEvent;
		public event CommandCompletedEventHandler CommandCompletedEvent;

		public CommandSender(ICSClient client) {
			this.client = client;
			commandId = 1;
			currentCommand = -1;
			blockCount = 0;

			client.BlockCodeEvent += OnBlockCodeEvent;
			client.CommandIdentifierEvent += OnCommandIdentifierEvent;
			client.CommandCodeEvent += OnCommandCodeEvent;

			client.LineBufferReceivedEvent += OnLineBufferReceivedEvent;
		}

		private void OnCommandIdentifierEvent(object o, string str) {
			currentCommand = Int32.Parse(str);
		}

		private void OnCommandCodeEvent(object o, CommandCode code) {
			// TODO: do some verification. there might be some error messages
		}

		private void OnBlockCodeEvent(object o, BlockCode code) {
			if(code == BlockCode.BlockStart) {
				blockCount++;
				return;
			}
			if(code == BlockCode.BlockEnd) {
				blockCount--;
				if(blockCount > 0)
					return;
				if(CommandCompletedEvent != null)
					CommandCompletedEvent(this, currentCommand);
				currentCommand = -1;
				pending--;
			}
		}

		private void OnLineBufferReceivedEvent(object o, LineBufferReceivedEventArgs args) {
			if(currentCommand < 0)
				return;
			if(CommandResponseLineEvent != null) {
				CommandResponseLineEvent(this, new CommandResponseLineEventArgs(currentCommand,
												args.Buffer,
												args.Start,
												args.End));
			}
		}

		private int NextCommandId() {
			int ret = commandId;
			if(++commandId == 10)
				commandId = 1;
			return ret;
		}

		public int SendCommand(string str) {
			if(pending > MAX_COMMANDS)
				return -1;
			int cmd = NextCommandId();
			pending++;
			client.WriteLine(String.Format("{0} {1}", cmd, str));
			return cmd;
		}
	}
	}
}
