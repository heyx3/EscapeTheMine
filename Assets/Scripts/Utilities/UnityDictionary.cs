using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;


/// <summary>
/// A dictionary that is serializable and editable by Unity.
/// </summary>
[Serializable]
public abstract class UnityDictionary<KeyType, ValueType> : IDictionary<KeyType, ValueType>, ISerializationCallbackReceiver
{
	[SerializeField]
	private KeyType[] keys = new KeyType[0];
	[SerializeField]
	private ValueType[] values = new ValueType[0];

	private Dictionary<KeyType, ValueType> dict = null;


	public ValueType this[KeyType key]
	{
		get
		{
			if (dict == null)
				Init();
			return dict[key];
		}
		set
		{
			if (dict == null)
				Init();
			dict[key] = value;
		}
	}

	public UnityDictionary() { }
	public UnityDictionary(IEnumerable<KeyValuePair<KeyType, ValueType>> values)
	{
		foreach (var kvp in values)
			Add(kvp);
	}
	
	private void Init()
	{
		if (dict != null)
			return;

		UnityEngine.Assertions.Assert.AreEqual(keys.Length, values.Length,
											   "Keys and values don't line up!");

		dict = new Dictionary<KeyType, ValueType>();
		for (int i = 0; i < keys.Length; ++i)
			dict.Add(keys[i], values[i]);
	}

	
	//Implement IDictionary<>:

	public IEnumerator<KeyValuePair<KeyType, ValueType>> GetEnumerator() { Init(); return dict.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { Init(); return dict.GetEnumerator(); }

	public int Count { get { return keys.Length; } }
	public bool IsReadOnly { get { return false; } }

	public void Add(KeyValuePair<KeyType, ValueType> kvp)
	{
		Init();

		dict.Add(kvp.Key, kvp.Value);

		//Add an element to the arrays.
		var newKeys = new KeyType[keys.Length + 1];
		var newVals = new ValueType[keys.Length + 1];
		for (int i = 0; i < keys.Length; ++i)
		{
			newKeys[i] = keys[i];
			newVals[i] = values[i];
		}
		newKeys[newKeys.Length - 1] = kvp.Key;
		newVals[newVals.Length - 1] = kvp.Value;
		keys = newKeys;
		values = newVals;
	}
	public void Clear() { Init(); dict.Clear(); keys = new KeyType[0]; values = new ValueType[0]; }
	public bool Contains(KeyValuePair<KeyType, ValueType> kvp)
	{
		Init();
		return dict.Contains(kvp);
	}
	public void CopyTo(KeyValuePair<KeyType, ValueType>[] array, int arrayIndex)
	{
		Init();

		int i = 0;
		foreach (var kvp in dict)
		{
			array[arrayIndex + i] = kvp;
			i += 1;

			if (arrayIndex + i >= array.Length)
				break;
		}
	}
	public bool Remove(KeyValuePair<KeyType, ValueType> item)
	{
		Init();

		if (dict.ContainsKey(item.Key) && dict[item.Key].Equals(item.Value))
		{
			return Remove(item.Key);
		}
		else
			return false;
	}

	public ICollection<KeyType> Keys { get { Init(); return dict.Keys; } }
	public ICollection<ValueType> Values { get { Init(); return dict.Values; } }
	public void Add(KeyType key, ValueType value) { Add(new KeyValuePair<KeyType, ValueType>(key, value)); }
	public bool ContainsKey(KeyType key) { Init(); return dict.ContainsKey(key); }
	public bool Remove(KeyType key)
	{
		Init();

		if (dict.Remove(key))
		{
			//Remove the value from the arrays.

			var newKeys = new KeyType[keys.Length - 1];
			var newVals = new ValueType[keys.Length - 1];
			int offset = 0;
			for (int i = 0; i < newKeys.Length; ++i)
			{
				//If this is the key to delete, skip it.
				if (keys[i].Equals(key))
					offset += 1;

				newKeys[i] = keys[i + offset];
				newVals[i] = values[i + offset];
			}

			keys = newKeys;
			values = newVals;

			return true;
		}
		else
		{
			return false;
		}
	}
	public bool TryGetValue(KeyType key, out ValueType value) { Init(); return dict.TryGetValue(key, out value); }


	//Helper methods for the unity editor GUI:

	public abstract bool CanAddNewKey();
	public abstract KeyType GetNewKey();

	public KeyType GetKeyAt(int keyArrayIndex)
	{
		return keys[keyArrayIndex];
	}
	public void RemoveAt(int keyArrayIndex)
	{
		Remove(keys[keyArrayIndex]);
	}


	//Serialization callbacks:

	public void OnBeforeSerialize() { }
	public void OnAfterDeserialize()
	{
		dict = null;
		Init();
	}
}


//Below is all the sub-classes of this used for specific things.
namespace Dict
{
	[Serializable]
	public class DataByEnum<EnumType, ValueType> : UnityDictionary<EnumType, ValueType>
		where EnumType : struct
	{
		static DataByEnum()
		{
			if (!typeof(EnumType).IsEnum)
			{
				Debug.LogError("DataByEnum<" + typeof(EnumType).Name + ", " +
							       typeof(ValueType).Name + ">: key type isn't an enum!");
			}
		}
		
		private static List<EnumType> enumValues = null;
		private static List<EnumType> EnumValues
		{
			get
			{
				if (enumValues == null)
					enumValues = Enum.GetValues(typeof(EnumType)).Cast<EnumType>().ToList();
				return enumValues;
			}
		}


		public DataByEnum(bool autoFillWithDefaults): base()
		{
			if (autoFillWithDefaults)
			{
				for (int i = 0; i < EnumValues.Count; ++i)
					Add(EnumValues[i], default(ValueType));
			}
		}
		public DataByEnum(IEnumerable<KeyValuePair<EnumType, ValueType>> values) : base(values) { }

		public override bool CanAddNewKey()
		{
			return Count < EnumValues.Count;
		}
		public override EnumType GetNewKey()
		{
			for (int i = 0; i < EnumValues.Count; ++i)
				if (!ContainsKey(EnumValues[i]))
					return EnumValues[i];
			throw new NotImplementedException("No known unused tile types!");
		}
	}


	[Serializable]
	public class GameObjectsByTileType : DataByEnum<GameLogic.TileTypes, GameObject>
	{
		public GameObjectsByTileType(bool autoFillWithDefaults = false) : base(autoFillWithDefaults) { }
		public GameObjectsByTileType(IEnumerable<KeyValuePair<GameLogic.TileTypes, GameObject>> values) : base(values) { }
	}
	[Serializable]
	public class GameObjectsByViewMode : DataByEnum<UnityLogic.ViewModes, GameObject>
	{
		public GameObjectsByViewMode(bool autoFillWithDefaults = false) : base(autoFillWithDefaults) { }
		public GameObjectsByViewMode(IEnumerable<KeyValuePair<UnityLogic.ViewModes, GameObject>> values) : base(values) { }
	}

	[Serializable]
	public class UITabsByTabType : DataByEnum<MyUI.Window_PlayerChar.TabTypes, UITab>
	{
		public UITabsByTabType(bool autoFillWithDefaults = false) : base(autoFillWithDefaults) { }
		public UITabsByTabType(IEnumerable<KeyValuePair<MyUI.Window_PlayerChar.TabTypes, UITab>> values) : base(values) { }
	}
}