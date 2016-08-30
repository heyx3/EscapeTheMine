using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//Custom serialization system.
//Abstract enough that it can be used for any kind of stream.


namespace MyData
{
	public interface IReadWritable
	{
		void ReadData(Reader reader);
		void WriteData(Writer writer);
	}


	public abstract class Writer
	{
		public class WriteException : Exception { public WriteException(string msg) : base(msg) { } }


		public string ErrorMessage = "";


		public abstract void Bool(bool value, string name);
		public abstract void Byte(byte value, string name);
		public abstract void Int(int value, string name);
		public abstract void UInt(uint value, string name);
		public abstract void Float(float value, string name);
		public abstract void Double(double value, string name);
		public abstract void String(string value, string name);
		public abstract void Bytes(byte[] value, string name);
		
		public abstract void Structure(IReadWritable value, string name);
		

		public void Vec2f(UnityEngine.Vector2 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			Structure(floats, name);
		}
		public void Vec3f(UnityEngine.Vector3 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			floats.Add("z", v.z);
			Structure(floats, name);
		}
		public void Vec4f(UnityEngine.Vector4 v, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", v.x);
			floats.Add("y", v.y);
			floats.Add("z", v.z);
			floats.Add("w", v.w);
			Structure(floats, name);
		}
		public void Quaternion(UnityEngine.Quaternion q, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", q.x);
			floats.Add("y", q.y);
			floats.Add("z", q.z);
			floats.Add("w", q.w);
			Structure(floats, name);
		}
		public void Rect(UnityEngine.Rect r, string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", r.x);
			floats.Add("y", r.y);
			floats.Add("width", r.width);
			floats.Add("height", r.height);
			Structure(floats, name);
		}


		public delegate void ListElementWriter<T>(Writer w, T outVal, string name);

		public void List<T, ListType>(ListType data, string name,
									  ListElementWriter<T> writeElementWithName)
			where ListType : IList<T>
		{
			ListSerializerWrapper<T, ListType> sList = new ListSerializerWrapper<T, ListType>();
			sList.Data = data;
			sList.ElementWriter = writeElementWithName;
			Structure(sList, name);
		}

		public void Collection<T, CollectionType>(CollectionType data, string name,
												  ListElementWriter<T> writeElementWithName)
			where CollectionType : ICollection<T>
		{
			EnumerableSerializerWrapper<T, CollectionType> sColl =
				new EnumerableSerializerWrapper<T, CollectionType>();
			sColl.Data = data;
			sColl.ElementWriter = writeElementWithName;
			Structure(sColl, name);
		}
	}

	public abstract class Reader
	{
		public class ReadException : Exception { public ReadException(string msg) : base(msg) { } }


		public string ErrorMessage = "";

		
		public abstract bool Bool(string name);
		public abstract byte Byte(string name);
		public abstract int Int(string name);
		public abstract uint UInt(string name);
		public abstract float Float(string name);
		public abstract double Double(string name);
		public abstract string String(string name);
		public abstract byte[] Bytes(string name);
		
		public abstract void Structure(IReadWritable outValue, string name);
		

		public UnityEngine.Vector2 Vec2f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector2(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value);
		}
		public UnityEngine.Vector3 Vec3f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector3(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value,
										   floats.ValuesByName[2].Value);
		}
		public UnityEngine.Vector4 Vec4f(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			floats.Add("w", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Vector4(floats.ValuesByName[0].Value,
										   floats.ValuesByName[1].Value,
										   floats.ValuesByName[2].Value,
										   floats.ValuesByName[3].Value);
		}
		public UnityEngine.Quaternion Quaternion(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("z", 0.0f);
			floats.Add("w", 0.0f);
			Structure(floats, name);
			return new UnityEngine.Quaternion(floats.ValuesByName[0].Value,
											  floats.ValuesByName[1].Value,
											  floats.ValuesByName[2].Value,
											  floats.ValuesByName[3].Value);
		}
		public UnityEngine.Rect Rect(string name)
		{
			FloatSerializerWrapper floats = new FloatSerializerWrapper();
			floats.Add("x", 0.0f);
			floats.Add("y", 0.0f);
			floats.Add("width", 0.0f);
			floats.Add("height", 0.0f);
			Structure(floats, name);

			return new UnityEngine.Rect(floats.ValuesByName[0].Value, floats.ValuesByName[1].Value,
										floats.ValuesByName[2].Value, floats.ValuesByName[3].Value);
		}


		public delegate void ListElementReader<T>(Reader r, ref T outVal, string name);
		
		public ListType List<T, ListType>(string name, ListElementReader<T> readElementWithName,
										  Func<int, ListType> createListWithCapacity)
			where ListType : IList<T>
		{
			ListSerializerWrapper<T, ListType> sList = new ListSerializerWrapper<T, ListType>();
			sList.ElementReader = readElementWithName;
			sList.ListFactoryFromCapacity = createListWithCapacity;
			Structure(sList, name);
			return sList.Data;
		}
		public CollectionType Collection<T, CollectionType>(string name,
													  ListElementReader<T> readElementWithName,
													  Func<int, CollectionType> createWithCapacity)
			where CollectionType : ICollection<T>
		{
			EnumerableSerializerWrapper<T, CollectionType> sColl =
				new EnumerableSerializerWrapper<T, CollectionType>();
			sColl.ElementReader = readElementWithName;
			sColl.CollectionFactoryFromCapacity = createWithCapacity;
			Structure(sColl, name);
			return sColl.Data;
		}
	}

	
	#region Helpers for reading/writing special types.
	/// <summary>
	/// Helper class for DataReader/DataWriter. Please ignore.
	/// </summary>
	public class FloatSerializerWrapper : IReadWritable
	{
		public List<KeyValuePair<string, float>> ValuesByName = new List<KeyValuePair<string, float>>();
		public void Add(string name, float val) { ValuesByName.Add(new KeyValuePair<string, float>(name, val)); }
		public void ReadData(Reader reader)
		{
			for (int i = 0; i < ValuesByName.Count; ++i)
			{
				float f = reader.Float(ValuesByName[i].Key);
				ValuesByName[i] = new KeyValuePair<string, float>(ValuesByName[i].Key, f);
			}
		}
		public void WriteData(Writer writer)
		{
			foreach (KeyValuePair<string, float> kvp in ValuesByName)
				writer.Float(kvp.Value, kvp.Key);
		}
	}
	/// <summary>
	/// Helper class for DataReader/DataWriter. Please ignore.
	/// </summary>
	public class ListSerializerWrapper<T, ListType> : IReadWritable
		where ListType : IList<T>
	{
		public ListType Data;

		public Writer.ListElementWriter<T> ElementWriter = null;

		public Reader.ListElementReader<T> ElementReader = null;
		public Func<int, ListType> ListFactoryFromCapacity = null;

		public void ReadData(Reader reader)
		{
			int size = (int)reader.UInt("NValues");

			Data = ListFactoryFromCapacity(size);
			for (int i = 0; i < size; ++i)
			{
				T t = default(T);
				ElementReader(reader, ref t, (i + 1).ToString());
				Data.Add(t);
			}
		}
		public void WriteData(Writer writer)
		{
			writer.UInt((uint)Data.Count, "NValues");
			for (int i = 0; i < Data.Count; ++i)
				ElementWriter(writer, Data[i], (i + 1).ToString());
		}
	}
	/// <summary>
	/// Helper class for DataReader/DataWriter. Please ignore.
	/// </summary>
	public class EnumerableSerializerWrapper<T, CollectionType> : IReadWritable
		where CollectionType : ICollection<T>
	{
		public CollectionType Data;

		public Writer.ListElementWriter<T> ElementWriter = null;

		public Reader.ListElementReader<T> ElementReader = null;
		public Func<int, CollectionType> CollectionFactoryFromCapacity = null;

		public void ReadData(Reader reader)
		{
			int size = (int)reader.UInt("NValues");

			Data = CollectionFactoryFromCapacity(size);
			for (int i = 0; i < size; ++i)
			{
				T t = default(T);
				ElementReader(reader, ref t, (i + 1).ToString());
				Data.Add(t);
			}
		}
		public void WriteData(Writer writer)
		{
			writer.UInt((uint)Data.Count, "NValues");
			int count = 1;
			foreach (T t in Data)
			{
				ElementWriter(writer, t, count.ToString());
				count += 1;
			}
		}
	}
	#endregion
}
