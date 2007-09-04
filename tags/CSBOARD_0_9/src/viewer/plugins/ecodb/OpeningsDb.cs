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
			bool reverseTraversal = false;

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
				if (!openings.ContainsKey (eco))
				  {
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
				PGNTreeNode last;
				if (reverseTraversal)
				  {
					  last = node;
					  node = node.prev;
				  }
				else
					last = node.prev;

				for (;;)
				  {
					  TreeIter newiter;
					  Opening opening =
						  (Opening) node.value;
					  string name = opening == null ? "" :
						  String.
						  Format
						  ("<b><span color=\"#222266\">{2}</span> {0}:{1} </b>",
						   opening.name,
						   opening.variation,
						   opening.ecoName);
					  newiter =
						  iter.Equals (TreeIter.
							       Zero) ? store.
						  AppendValues (node.move,
								node.count,
								name) : store.
						  AppendValues (iter,
								node.move,
								node.count,
								name);
					  AppendMove (store, newiter,
						      node.firstChild);
					  if (node == last)
						  break;
					  if (reverseTraversal)
						  node = node.prev;
					  else
						  node = node.next;
				  }
			}
		}
	}
}
