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

using CsBoard.Viewer;
using Gtk;
using System;
using System.Collections;
using Mono.Unix;

namespace CsBoard
{
	namespace ICS
	{

		public class PlayerPage:ObservingGamePage,
			IAsyncCommandResponseListener
		{
			Button drawButton, resignButton, adjournButton,
				abortButton, takebackButton;

			public PlayerPage (ICSGameObserverWindow win,
					   MoveDetails details):base (win,
								      details)
			{
				HButtonBox box = new HButtonBox ();

				  drawButton =
					new Button (Catalog.
						    GetString ("Draw"));
				  resignButton =
					new Button (Catalog.
						    GetString ("Resign"));
				  abortButton =
					new Button (Catalog.
						    GetString ("Abort"));
				  adjournButton =
					new Button (Catalog.
						    GetString ("Adjourn"));
				  takebackButton =
					new Button (Catalog.
						    GetString ("Takeback"));

				  drawButton.Clicked += OnClicked;
				  resignButton.Clicked += OnClicked;
				  abortButton.Clicked += OnClicked;
				  adjournButton.Clicked += OnClicked;
				  takebackButton.Clicked += OnClicked;

				  box.LayoutStyle = ButtonBoxStyle.Start;
				  box.PackStart (drawButton, false, false, 2);
				  box.PackStart (resignButton, false, false,
						 2);
				  box.PackStart (abortButton, false, false,
						 2);
				  box.PackStart (adjournButton, false, false,
						 2);
				  box.PackStart (takebackButton, false, false,
						 2);
				  box.ShowAll ();
				  PackStart (box, false, true, 2);
			}

			public override void Update (MoveDetails details)
			{
				if (details.relation ==
				    Relation.IamPlayingAndMyMove)
					board.Sensitive = true;
				else if (details.relation ==
					 Relation.IamPlayingAndMyOppsMove)
					board.Sensitive = false;

				base.Update (details);
			}

			private void OnClicked (object o, EventArgs args)
			{
				string cmd;
				if (o.Equals (resignButton))
				  {
					  if (!AskForConfirmation
					      (win,
					       Catalog.
					       GetString
					       ("Do you really want to resign?")))
						  return;
					  cmd = "resign";
				  }
				else if (o.Equals (drawButton))
					  cmd = "draw";
				else if (o.Equals (abortButton))
					cmd = "abort";
				else if (o.Equals (adjournButton))
					cmd = "adjourn";
				else if (o.Equals (takebackButton))
					cmd = "takeback";
				else
					return;
				win.Client.CommandSender.SendCommand (cmd);
			}

			private static bool AskForConfirmation (Window win,
								string text)
			{
				MessageDialog md = new MessageDialog (win,
								      DialogFlags.
								      DestroyWithParent,
								      MessageType.
								      Question,
								      ButtonsType.
								      YesNo,
								      String.
								      Format
								      ("<b>{0}</b>",
								       text));

				int res = md.Run ();
				md.Hide ();
				md.Dispose ();
				return res == (int) ResponseType.Yes;
			}

			protected override void InitGameWidget (MoveDetails
								details)
			{
				board = new CairoPlayerBoard (details.pos);
				board.MoveEvent += OnMoveEvent;
				gameWidget = new ChessGameWidget (board);
			}

			private void OnMoveEvent (string move)
			{
				if (move.Length == 5
				    && !Char.IsDigit (move[4]))
				  {
					  // promotion case. insert '='
					  char lastchar = move[4];
					  if (lastchar == 'K'
					      || lastchar == 'k')
						  lastchar = 'N';
					  move = move.Substring (0,
								 4) + '=' +
						  lastchar;
				  }
				win.Client.CommandSender.SendCommand (move,
								      this);
			}

			public void CommandResponseLine (int id, byte[]buffer,
							 int start, int end)
			{
				board.SetPosition (lastMove.pos);
				SetMoveInfo (board, lastMove);
				board.QueueDraw ();
			}

			public void CommandCodeReceived (int id,
							 CommandCode code)
			{
			}

			public void CommandCompleted (int id)
			{
			}
		}
	}
}
