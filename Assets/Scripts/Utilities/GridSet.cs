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

	public bool HasAtLeastOneAt(Vector2i pos) { return grid.Get(pos) != null; }
	public int CountAt(Vector2i pos)
	{
		int count = 0;
		foreach (var value in GetValuesAt(pos))
			count += 1;
		return count;
	}
	public T GetAt(Vector2i pos, int index)
	{
		int count = 0;
		foreach (var value in GetValuesAt(pos))
		{
			if (index == count)
				return value;
			count += 1;
		}

		throw new IndexOutOfRangeException(index.ToString() + "/" + count.ToString());
	}
	
	public CellIterator GetValuesAt(Vector2i pos) { return new CellIterator(grid.Get(pos)); }
	public SetIterator GetAllValues() { return new SetIterator(this); }
	public PosIterator GetActiveCells() { return new PosIterator(activeCells); }
	
	public bool Contains(T t)
	{
		foreach (T value in GetAllValues())
			if (value == t)
				return true;
		return false;
	}
	public bool ContainsAt(Vector2i pos, T t)
	{
		foreach (T value in GetValuesAt(pos))
			if (value == t)
				return true;
		return false;
	}
	public bool AnyAt(Vector2i pos, Func<T, int, bool> predicate)
	{
		int i = 0;
		foreach (var value in GetValuesAt(pos))
		{
			if (predicate(value, i))
				return true;
			i += 1;
		}
		return false;
	}
	public bool AllAt(Vector2i pos, Func<T, int, bool> predicate)
	{
		int i = 0;
		foreach (var value in GetValuesAt(pos))
		{
			if (!predicate(value, i))
				return false;
			i += 1;
		}
		return true;
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


	#region Iterators

	/// <summary>
	/// Iterates across a HashSet of Vector2i.
	/// </summary>
	public struct PosIterator
	{
		public Vector2i Current { get { return enumerator.Current; } }

		private HashSet<Vector2i> set;
		private HashSet<Vector2i>.Enumerator enumerator;

		public PosIterator(HashSet<Vector2i> _set)
		{
			set = _set;
			enumerator = set.GetEnumerator();
		}

		public bool MoveNext() { return enumerator.MoveNext(); }
		public void Reset() { enumerator = set.GetEnumerator(); }
		public void Dispose() { enumerator.Dispose(); }

		public PosIterator GetEnumerator() { Reset(); return this; }
	}

	/// <summary>
	/// Iterates across all items in a single grid cell.
	/// </summary>
	public struct CellIterator
	{
		public T Current { get { return currentNode.Value; } }

		private bool reset;
		private Node startNode, currentNode;

		public CellIterator(object _startNode)
		{
			startNode = (Node)_startNode;
			currentNode = startNode;
			reset = true;
		}

		public bool MoveNext()
		{
			if (!reset && currentNode != null)
				currentNode = currentNode.Next;
			reset = false;

			return currentNode != null;
		}
		public void Reset() { currentNode = startNode; reset = true; }
		public void Dispose() { }

		public CellIterator GetEnumerator() { Reset(); return this; }
	}

	/// <summary>
	/// Iterates across all items in the set.
	/// </summary>
	public struct SetIterator
	{
		public T Current { get { return cellIterator.Current; } }

		private bool reset;
		private Vector2i.Iterator posIterator;
		private CellIterator cellIterator;
		GridSet<T> set;

		public SetIterator(GridSet<T> _set)
		{
			set = _set;
			posIterator = new Vector2i.Iterator(set.grid.SizeXY());
			cellIterator = new CellIterator();
			reset = true;
		}

		public bool MoveNext()
		{
			bool done = false;
			if (reset)
			{
				done = posIterator.MoveNext();
				if (!done)
					cellIterator = new CellIterator(set.grid.Get(posIterator.Current));
			}

			return !done && !cellIterator.MoveNext();
		}
		public void Reset()
		{
			posIterator.Reset();
			reset = true;
		}
		public void Dispose() { posIterator.Dispose(); cellIterator.Dispose(); }

		public SetIterator GetEnumerator() { Reset(); return this; }
	}

	#endregion
}