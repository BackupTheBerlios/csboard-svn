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
using System.Collections;
using Mono.Unix;
using Gdk;

namespace CsBoard
{
	namespace ICS
	{
		public class GameAdvertisementGraph:VBox
		{
			Graph graph;
			Label infoLabel;
			  Gtk.Image image;
			ICSClient client;
			Hashtable categories;
			public GameAdvertisementGraph (ICSClient c)
			{
				graph = new Graph ();
				categories = new Hashtable ();
				categories["blitz"] = 1;
				categories["standard"] = 1;
				categories["lightning"] = 1;
				categories["untimed"] = 1;

				graph.GameFocusedEvent += OnGameFocused;
				graph.GameClickedEvent += OnGameClicked;

				infoLabel = new Label ();
				infoLabel.Xalign = 0;
				infoLabel.Xpad = 4;
				this.client = c;

				client.GameAdvertisementAddEvent +=
					OnGameAdvertisementAddEvent;
				client.GameAdvertisementRemoveEvent +=
					OnGameAdvertisementRemoveEvent;
				client.GameAdvertisementsClearedEvent +=
					OnGameAdvertisementsCleared;
				SetSizeRequest (600, 400);

				image = new Gtk.Image ();
				PackStart (graph, true, true, 4);

				HBox box = new HBox ();
				  box.PackStart (image, false, false, 4);
				  box.PackStart (infoLabel, true, true, 4);

				  PackStart (box, false, true, 4);
				  ShowAll ();
			}

			public void OnGameAdvertisementAddEvent (object o,
								 GameAdvertisement
								 ad)
			{
				if (!categories.
				    ContainsKey (ad.category.ToLower ()))
					return;
				graph.AddGameInfo (new
						   GameAdvertisementInfo
						   (ad));
			}

			private void OnGameAdvertisementsCleared (object o,
								  EventArgs
								  args)
			{
				graph.Clear ();
			}

			public void OnGameAdvertisementRemoveEvent (object o,
								    GameAdvertisement
								    ad)
			{
				graph.RemoveGame (ad.gameHandle);
			}

			uint timeoutHandlerID = 0;

			private bool FadeTimeout ()
			{
				if (showingSomething)
					return true;	// try again

				infoLabel.Markup = "";
				image.Pixbuf = null;
				timeoutHandlerID = 0;
				return false;
			}

			bool showingSomething = false;
			public void OnGameFocused (object o, IGameInfo info)
			{
				showingSomething = info != null;
				if (info == null)
					return;

				infoLabel.Markup =
					(info as
					 GameAdvertisementInfo).Markup;
				image.Pixbuf = info.Computer ?
					GameAdvertisements.
					ComputerPixbuf : null;
				if (timeoutHandlerID == 0)
				  {
					  timeoutHandlerID =
						  GLib.Timeout.Add (1000,
								    FadeTimeout);
				  }
			}

			public void OnGameClicked (object o, IGameInfo info)
			{
				client.CommandSender.SendCommand ("play " +
								  info.
								  GameId);
			}
		}

		class GameAdvertisementInfo:IGameInfo
		{
			public GameAdvertisement ad;
			public GameAdvertisementInfo (GameAdvertisement a)
			{
				ad = a;
			}
			public int GameId
			{
				get
				{
					return ad.gameHandle;
				}
			}

			public int Rating
			{
				get
				{
					return ad.rating;
				}
			}

			public int Time
			{
				get
				{
					return ad.time_limit;
				}
			}

			public int Increment
			{
				get
				{
					return ad.time_increment;
				}
			}

			public bool Rated
			{
				get
				{
					return ad.rated;
				}
			}

			public bool Computer
			{
				get
				{
					return ad.IsComputer;
				}
			}

			public string Markup
			{
				get
				{
					System.Text.StringBuilder buf =
						new System.Text.
						StringBuilder ();
					buf.Append (String.
						    Format ("<b>{0} ({1})",
							    ad.username,
							    ad.RatingStr));
					buf.Append (String.
						    Format
						    (" {0} {1} {2} {3}</b> ",
						     ad.time_limit,
						     ad.time_increment,
						     ad.rated ? Catalog.
						     GetString ("rated") :
						     Catalog.
						     GetString ("unrated"),
						     ad.category));
					buf.Append (String.
						    Format
						    ("<small><i>[{0}] [{1}] [{2}]</i></small>",
						     ad.Color, ad.Flags,
						     ad.Range));
					return buf.ToString ();
				}
			}
		}
	}
}
