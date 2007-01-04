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
			public int count;
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

				AddMove (((PGNChessMove) moves[0]).Move, null,
					 root, out node);
				node.count++;
				if (root == null)
					root = node;
				else {
					if (RearrangeList (root, node))
						root = node;
				}

				PGNTreeNode parent;
				bool first = true;
				foreach (PGNChessMove move in moves) {
					if (first) {
						first = false;
						continue;
					}

					parent = node;
					node = parent.firstChild;
					AddMove (move.Move, parent,
						 parent.firstChild, out node);
					node.count++;
					if (RearrangeList
					    (parent.firstChild, node))
						parent.firstChild = node;
				}

				node.value = value;
				leafnode = node;
			}
/*
			private void PrintList (PGNTreeNode node)
			{
				PGNTreeNode last = node.prev;
				for (;;) {
					Console.Write ("{0}[{1}] ", node.move,
						       node.count);
					if (node == last)
						break;
					node = node.next;
				}
				Console.WriteLine ();
			}
*/
			private bool RearrangeList (PGNTreeNode first,
						    PGNTreeNode node)
			{
				if (node == first)
					return false;
				if (node.count <= node.prev.count)
					return false;

				// no need to check the prev node as it was already checked above
				// also this avoids the case where node is the last element and
				// first.prev and node.prev will be the same
				PGNTreeNode tmpnode = node.prev.prev;

				RemoveNode (node);
				PGNTreeNode max = first.prev;
				while (tmpnode != max) {
					// note that 'node' is still part of the list
					if (node.count <= tmpnode.count)
						break;
					tmpnode = tmpnode.prev;
				}

				// node should be moved to the right of tmpnode
				InsertBetweenNodes (tmpnode, tmpnode.next,
						    node);
				if (node.count > first.count)
					return true;	// this should be the first child
				return false;
			}

			private void RemoveNode (PGNTreeNode node)
			{
				PGNTreeNode left = node.prev;
				PGNTreeNode right = node.next;
				left.next = right;
				right.prev = left;
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
/*
				if (first == null && parent != null)
					parent.firstChild = newnode;
				PrependToList (first, newnode);
*/
				if (parent != null
				    && parent.firstChild == null)
					parent.firstChild = newnode;
				AppendToList (first, newnode);

				slot = newnode;
			}

			private void AppendToList (PGNTreeNode first,
						   PGNTreeNode newnode)
			{
				if (first == null) {
					newnode.next = newnode.prev = newnode;
					return;
				}

				InsertBetweenNodes (first.prev, first,
						    newnode);
			}

			private void InsertBetweenNodes (PGNTreeNode left,
							 PGNTreeNode right,
							 PGNTreeNode newnode)
			{
				newnode.prev = left;
				newnode.next = right;
				right.prev = newnode;
				left.next = newnode;
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
