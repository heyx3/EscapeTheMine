using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyData;


/// <summary>
/// A HashSet whose elements are carefully controlled.
/// Events are exposed to react to any changes.
/// </summary>
public abstract class LockedSet<T> : MyData.IReadWritable, ICollection<T>
{
	public event Action<LockedSet<T>, T> OnElementAdded;
	public event Action<LockedSet<T>, T> OnElementRemoved;


	private HashSet<T> elements = new HashSet<T>();


	public int Count { get { return elements.Count; } }


	public bool Contains(T t)
	{
		return elements.Contains(t);
	}

	/// <summary>
	/// If the given element doesn't exist in this set already:
	///     1. It gets added to this collection.
	///     2. The OnElementAdded" event gets raised.
	/// </summary>
	public virtual void Add(T t)
	{
		if (elements.Add(t))
		{
			if (OnElementAdded != null)
				OnElementAdded(this, t);
		}
	}

	/// <summary>
	/// If the given element exists in this set:
	///     1. It gets removed.
	///     2. The OnElementRemoved" event gets raised.
	/// Returns whether it was removed.
	/// </summary>
	public virtual bool Remove(T t)
	{
		if (elements.Remove(t))
		{
			if (OnElementRemoved != null)
				OnElementRemoved(this, t);
			return true;
		}
		else
			return false;
	}
	/// <summary>
	/// Removes all elements that pass the given predicate.
	/// Returns the number of elements that were removed this way.
	/// </summary>
	public int RemoveWhere(Predicate<T> predicate)
	{
		//Copy elements to a temp array so that we can safely iterate
		//    over every element while removing those elements.
		T[] tempArray = elements.ToArray();

		int count = 0;
		for (int i = 0; i < tempArray.Length; ++i)
			if (predicate(tempArray[i]))
			{
				count += 1;
				Remove(tempArray[i]);
			}
		return count;
	}
	/// <summary>
	/// Removes all elements from this set.
	/// </summary>
	public void Clear()
	{
		RemoveWhere(t => true);
	}

	public void CopyTo(T[] array, int startIndex)
	{
		elements.CopyTo(array, startIndex);
	}


	//Serialization:

	public virtual void WriteData(MyData.Writer writer)
	{
		writer.Int(Count, "count");
		int count = 0;
		foreach (T t in elements)
		{
			Write(writer, t, count.ToString());
			count += 1;
		}
	}
	public virtual void ReadData(MyData.Reader reader)
	{
		int count = reader.Int("count");

		Clear();
		for (int i = 0; i < count; ++i)
			Add(Read(reader, i.ToString()));
	}

	protected abstract void Write(MyData.Writer writer, T value, string name);
	protected abstract T Read(MyData.Reader reader, string name);


	//Extra stuff needed to finish the ICollection<T> interface:
	public IEnumerator<T> GetEnumerator() { return elements.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return elements.GetEnumerator(); }
	public bool IsReadOnly { get { return false; } }
}


public class IntSet : LockedSet<int>
{
	protected override void Write(MyData.Writer writer, int value, string name)
	{
		writer.Int(value, name);
	}
	protected override int Read(MyData.Reader reader, string name)
	{
		return reader.Int(name);
	}
}
public class UlongSet : LockedSet<ulong>
{
    protected override void Write(Writer writer, ulong value, string name)
    {
        writer.UInt64(value, name);
    }
    protected override ulong Read(Reader reader, string name)
    {
        return reader.UInt64(name);
    }
}
public class FloatSet : LockedSet<float>
{
	protected override void Write(MyData.Writer writer, float value, string name)
	{
		writer.Float(value, name);
	}
	protected override float Read(MyData.Reader reader, string name)
	{
		return reader.Float(name);
	}
}
public class StringSet : LockedSet<string>
{
	protected override void Write(MyData.Writer writer, string value, string name)
	{
		writer.String(value, name);
	}
	protected override string Read(MyData.Reader reader, string name)
	{
		return reader.String(name);
	}
}
public class Vector2iSet : LockedSet<Vector2i>
{
	protected override void Write(Writer writer, Vector2i value, string name)
	{
		writer.Int(value.x, name + "_x");
		writer.Int(value.y, name + "_y");
	}
	protected override Vector2i Read(Reader reader, string name)
	{
		return new Vector2i(reader.Int(name + "_x"), reader.Int(name + "_y"));
	}
}