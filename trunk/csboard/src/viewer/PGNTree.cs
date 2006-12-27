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
		public class PGNTreeNode
		{
			public string move;
			public PGNTreeNode prev, next;
			public PGNTreeNode firstChild, parent;

			public object value;

			public PGNTreeNode (string m)
			{
				move = m;
				next = prev = null;
				firstChild = null;
				parent = null;
			}
		}

		public delegate void ReadNodeHandler (PGNTreeNode node);

		public class PGNTree
		{
			protected PGNTreeNode root;
			public PGNTree ()
			{
			}

			protected virtual PGNTreeNode AllocNode (string move)
			{
				return new PGNTreeNode (move);
			}

			public void ForeachMove (PGNTreeNode leafnode,
						 ReadNodeHandler ReadNode)
			{
				Stack stack = new Stack ();
				for (PGNTreeNode node = leafnode;
				     node != null; node = node.parent)
					stack.Push (node);

				foreach (PGNTreeNode node in stack) {
					ReadNode (node);
				}
			}

			public void AddMoves (IList moves, object value,
					      out PGNTreeNode leafnode)
			{
				PGNTreeNode node;

				AddMove (((PGNChessMove) moves[0]).move, null,
					 root, out node);
				if (root == null)
					root = node;

				PGNTreeNode parent;
				bool first = true;
				foreach (PGNChessMove move in moves) {
					if (first) {
						first = false;
						continue;
					}

					parent = node;
					node = parent.firstChild;
					AddMove (move.move, parent,
						 parent.firstChild, out node);
				}

				node.value = value;
				leafnode = node;
			}

			// The 'node' will have the node which should parent the next insertion
			private void AddMove (string move, PGNTreeNode parent,
					      PGNTreeNode first,
					      out PGNTreeNode slot)
			{
				PGNTreeNode sibling;

				if (first != null
				    && FindNode (move, first, out sibling)) {
					slot = sibling;
					return;
				}

				// first is null or unable to find the move
				PGNTreeNode newnode = AllocNode (move);
				newnode.parent = parent;
				if (first == null && parent != null)
					parent.firstChild = newnode;
				PrependToList (first, newnode);
				slot = newnode;
			}

			private void PrependToList (PGNTreeNode first,
						    PGNTreeNode newnode)
			{
				if (first == null) {
					newnode.next = newnode.prev = newnode;
					return;
				}

				newnode.next = first;
				newnode.prev = first.prev;
				first.prev = newnode;
				newnode.prev.next = newnode;	// update the last node

			}

			private bool FindNode (string move, PGNTreeNode node,
					       out PGNTreeNode slot)
			{
				PGNTreeNode last = node.prev;
				for (;;) {
					if (move.Equals (node.move)) {
						slot = node;
						return true;
					}
					if (node == last)
						break;

					node = node.next;
				}
				slot = null;
				return false;
			}

			public void Dump ()
			{
				PrintNode (root, 0);
			}

			private void PrintNode (PGNTreeNode node, int level)
			{
				if (node == null)
					return;
				PGNTreeNode last = node.prev;
				for (;;) {
					for (int i = 0; i < level; i++)
						Console.Write ("=== ");
					Console.Write (node.move);
					if (node.value != null)
						Console.Write (" [{0}]",
							       node.value);
					Console.WriteLine ();
					PrintNode (node.firstChild,
						   level + 1);
					if (node == last)
						break;
					node = node.next;
				}
			}
		}
	}
}
