using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// For every cell in a 2D grid, stores a set of objects in that cell.
/// Optimized with the assumption that objects will be moving around a lot.
/// </summary>
public class GridSet<T>
	where T : class
{
	/// <summary>
	/// A linked-list node.
	/// </summary>
	private class Node
	{
		public Node Next;
		public T Value;
		public Node(T value, Node next = null)
		{
			Value = value;
			Next = next;
		}
	}

	private Node[,] grid;
	private HashSet<Vector2i> activeCells = new HashSet<Vector2i>();

	public GridSet(Vector2i size)
	{
		grid = new Node[size.x, size.y];
		foreach (Vector2i gridPos in grid.AllIndices())
			grid.Set(gridPos, null);
	}

	public void AddValue(T val, Vector2i pos)
	{
		if (grid.Get(pos) == null)
			grid.Set(pos, new Node(val));
		else
			InsertAtEnd(new Node(val), grid.Get(pos));

		activeCells.Add(pos);
	}
	public void MoveValue(T val, Vector2i oldPos, Vector2i newPos)
	{
		//Get the node and remove it from its current list.
		Node theNode;
		Node list = grid.Get(oldPos);
		list = Remove(val, list, out theNode);
		grid.Set(oldPos, list);

		if (list == null)
			activeCells.Remove(oldPos);

		//Add the node to its new list.
		theNode.Next = null;
		if (grid.Get(newPos) == null)
			grid.Set(newPos, theNode);
		else
			InsertAtEnd(theNode, grid.Get(newPos));

		activeCells.Add(newPos);
	}
	public void RemoveValue(T val, Vector2i pos)
	{
		Node theNode;
		Node list = grid.Get(pos);
		list = Remove(val, list, out theNode);
		grid.Set(pos, list);

		if (list == null)
			activeCells.Remove(pos);
	}


	public bool ContainsElements(Vector2i pos) { return grid.Get(pos) != null; }
	public int Count(Vector2i pos)
	{
		int count = 0;
		Node n = grid.Get(pos);

		while (n != null)
		{
			count += 1;
			n = n.Next;
		}

		return count;
	}
	public T Get(Vector2i pos, int index)
	{
		UnityEngine.Assertions.Assert.IsTrue(index >= 0);

		Node n = grid.Get(pos);

		while (index > 0)
			n = n.Next;

		return n.Value;
	}

	public void ForEach(Vector2i pos, Action<T, int> toDo)
	{
		int i = 0;
		Node n = grid.Get(pos);

		while (n != null)
		{
			toDo(n.Value, i);

			n = n.Next;
			i += 1;
		}
	}
	public IEnumerable<Vector2i> ActiveCells { get { return activeCells; } }
	public IEnumerable<T> AllValues
	{
		get
		{
			foreach (Vector2i pos in grid.AllIndices())
			{
				Node n = grid.Get(pos);
				while (n != null)
				{
					yield return n.Value;
					n = n.Next;
				}
			}
		}
	}

	/// <summary>
	/// Inserts the given node into the end of the given linked list.
	/// Assumes the node isn't already in the list.
	/// </summary>
	private void InsertAtEnd(Node n, Node listStart)
	{
		Node listEnd = listStart;
		while (listEnd.Next != null)
			listEnd = listEnd.Next;

		listEnd.Next = n;
		n.Next = null;
	}
	/// <summary>
	/// Removes the given value from the given list.
	/// Returns the new beginning of the list ("null" if the list is empty).
	/// </summary>
	/// <param name="theNode">
	/// When this method is finished,
	///     this variable will contain the linked list node for the given value.
	/// If the value wasn't actually found in the list, this will be "null".
	/// </param>
	private Node Remove(T val, Node listStart, out Node theNode)
	{
		if (listStart.Value == val)
		{
			theNode = listStart;
			return listStart.Next;
		}

		theNode = null;
		Node previous = listStart,
			 current = listStart.Next;
		while (current != null)
		{
			if (current.Value == val)
			{
				theNode = current;
				previous.Next = current.Next;
				break;
			}

			previous = current;
			current = current.Next;
		}

		return listStart;
	}
}