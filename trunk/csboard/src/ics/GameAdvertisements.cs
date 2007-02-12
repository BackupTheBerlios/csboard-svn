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


namespace CsBoard
{
	public class GameAdvertisements:VBox
	{
		private Gtk.TreeView adList;

		ListStore store;

		ICSClient client;
		Label infoLabel;
		int ngames;
		int nrated;

		public GameAdvertisements (ICSClient client)
		{
			adList = new TreeView ();
			infoLabel = new Label ();
			infoLabel.Xalign = 0;
			infoLabel.Xpad = 4;
			this.client = client;

			client.GameAdvertisementAddEvent +=
				OnGameAdvertisementAddEvent;
			client.GameAdvertisementRemoveEvent +=
				OnGameAdvertisementRemoveEvent;
			client.AuthEvent += OnAuthEvent;

			store = new ListStore (typeof (object), typeof (int));
			  adList.Model = store;
			  adList.HeadersVisible = true;
			  adList.HeadersClickable = true;

			TreeViewColumn col;

			  col = new TreeViewColumn ();
			CellRendererText renderer = new CellRendererText ();
			  renderer.Yalign = 0;
			  col.Title = Catalog.GetString ("Games");
			  col.PackStart (renderer, false);
			  col.SetCellDataFunc (renderer,
					       new
					       TreeCellDataFunc
					       (GamesCellDataFunc));

			  adList.AppendColumn (col);
			  adList.AppendColumn (Catalog.GetString ("Rating"),
					       new CellRendererText (),
					       "text", 1);

			ScrolledWindow win = new ScrolledWindow ();
			  win.HscrollbarPolicy = win.VscrollbarPolicy =
				PolicyType.Automatic;
			  win.Add (adList);

			  UpdateInfoLabel ();

			  infoLabel.UseMarkup = true;
			  PackStart (infoLabel, false, true, 4);
			  PackStart (win, true, true, 4);

			  SetSizeRequest (600, 400);
			  ShowAll ();
		}

		protected void GamesCellDataFunc (TreeViewColumn col,
						  CellRenderer r,
						  TreeModel model,
						  TreeIter iter)
		{
			CellRendererText renderer = (CellRendererText) r;
			GameAdvertisement ad =
				(GameAdvertisement) model.GetValue (iter, 0);
			  renderer.Markup = ad.ToPango ();
		}

		public void OnGameAdvertisementAddEvent (object o,
							 GameAdvertisement ad)
		{
			store.AppendValues (ad, ad.rating);
			ngames++;
			if (ad.rated)
				nrated++;
			UpdateInfoLabel ();
		}

		public void OnGameAdvertisementRemoveEvent (object o,
							    GameAdvertisement
							    ad)
		{
			TreeIter iter;
			bool found = false;

			for (bool ret = store.GetIterFirst (out iter); ret;
			     ret = store.IterNext (ref iter))
			  {
				  GameAdvertisement a =
					  (GameAdvertisement) store.
					  GetValue (iter, 0);
				  if (a.gameHandle == ad.gameHandle)
				    {
					    store.Remove (ref iter);
					    ngames--;
					    if (ad.rated)
						    nrated--;
					    UpdateInfoLabel ();
					    break;
				    }
			  }
		}

		private void UpdateInfoLabel ()
		{
			infoLabel.Markup =
				String.Format ("<b>{0}: {1}, {2}: {3}</b>",
					       Catalog.
					       GetString ("Total game seeks"),
					       ngames,
					       Catalog.GetString ("Rated"),
					       nrated);
		}

		public void OnAuthEvent (object o, bool success)
		{
		}
	}
}
