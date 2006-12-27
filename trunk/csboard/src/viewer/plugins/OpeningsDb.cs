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

using System;
using System.Collections;
using Gtk;

using Chess.Parser;

namespace CsBoard
{
	namespace Viewer
	{
		public class Opening
		{
			public string ecoName;
			public string name;
			public string variation;
			public IList moves;
		}

		public class OpeningsDb:PGNTree
		{
			Hashtable openings;
			public OpeningsDb ():base ()
			{
				openings = new Hashtable ();
			}

			public void PopulateTree (TreeStore store)
			{
				AppendMove (store, TreeIter.Zero, root);
			}

			public void AddOpening (Opening opening)
			{
				IList moves = opening.moves;
				PGNTreeNode leafnode;
				  AddMoves (moves, opening, out leafnode);

				ArrayList list;
				if (openings.ContainsKey (opening.ecoName))
					  list = (ArrayList) openings[opening.
								      ecoName];
				else
					  list = new ArrayList ();
				  list.Add (leafnode);
				  openings[opening.ecoName] = list;
			}

			public string GetName (string eco)
			{
				if (!openings.ContainsKey (eco)) {
					return null;
				}
				ArrayList list = (ArrayList) openings[eco];
				if (list.Count == 0)
					return null;
				PGNTreeNode node = (PGNTreeNode) list[0];
				Opening opening = (Opening) node.value;
				return opening.name;
			}

			private void AppendMove (TreeStore store,
						 TreeIter iter,
						 PGNTreeNode node)
			{
				if (node == null)
					return;
				PGNTreeNode last = node.prev;
				for (;;) {
					TreeIter newiter;
					Opening opening =
						(Opening) node.value;
					if (iter.Equals (TreeIter.Zero))
						newiter =
							store.
							AppendValues (node.
								      move,
								      opening
								      ==
								      null ?
								      "" :
								      String.
								      Format
								      ("{0}:{1}",
								       opening.
								       name,
								       opening.
								       variation));
					else
						newiter =
							store.
							AppendValues (iter,
								      node.
								      move,
								      opening
								      ==
								      null ?
								      "" :
								      String.
								      Format
								      ("{0}:{1}",
								       opening.
								       name,
								       opening.
								       variation));
					AppendMove (store, newiter,
						    node.firstChild);
					if (node == last)
						break;
					node = node.next;
				}
			}
		}
	}
}
