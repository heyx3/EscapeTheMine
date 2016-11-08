﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic
{
	/// <summary>
	/// An entity from the game map.
	/// This includes things like people, monsters, and buildings.
	/// </summary>
	public abstract class Unit : MyData.IReadWritable
	{
		public Stat<Vector2i, Unit> Pos { get; private set; }

		public Map TheMap { get; private set; }
		public ulong MyGroupID { get; private set; }
		public ulong ID { get; private set; }

		/// <summary>
		/// Whether this unit's ID has been assigned by the map yet.
		/// </summary>
		public bool IsIDRegistered { get; private set; }

		public Group MyGroup { get { return TheMap.Groups.Get(MyGroupID); } }


		public Unit(Map theMap, Group g) : this(theMap, g, new Vector2i(-1, -1)) { }
		public Unit(Map theMap, Group g, Vector2i pos)
		{
			TheMap = theMap;
			MyGroupID = g.ID;
			Pos = new Stat<Vector2i, Unit>(this, pos);

			ID = ulong.MaxValue;

			IsIDRegistered = false;
		}

		public Unit(Map theMap, ulong groupID) : this(theMap, theMap.Groups.Get(groupID)) { }
		public Unit(Map theMap, ulong groupID, Vector2i pos)
			: this(theMap, theMap.Groups.Get(groupID), pos) { }


		public void RegisterID(ulong myID)
		{
			UnityEngine.Assertions.Assert.IsFalse(IsIDRegistered);
			IsIDRegistered = true;
			ID = myID;
		}


		/// <summary>
		/// Runs a coroutine that has this unit take his turn.
		/// </summary>
		public abstract System.Collections.IEnumerable TakeTurn();


		#region Serialization

		public static void Write(MyData.Writer writer, string name, Unit u)
		{
			writer.UInt((uint)u.MyType, name + "_Type");
			writer.Structure(u, name + "_Value");
		}
		public static Unit Read(MyData.Reader reader, Map theMap, string name)
		{
			Unit u = null;
			Types type = (Types)reader.UInt(name + "_Type");
			switch (type)
			{
				case Types.PlayerChar: u = new Units.PlayerChar(theMap, ulong.MaxValue); break;
				default: throw new NotImplementedException(type.ToString());
			}

			reader.Structure(u, name + "_Value");

			return u;
		}

		public enum Types
		{
			PlayerChar,
		}
		public abstract Types MyType { get; }

		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Vec2i(Pos, "pos");

			writer.UInt64(ID, "id");
			writer.UInt64(MyGroupID, "myGroup");

			writer.Bool(IsIDRegistered, "isIDRegistered");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			Pos.Value = reader.Vec2i("pos");

			ID = reader.UInt64("id");
			MyGroupID = reader.UInt64("myGroup");

			IsIDRegistered = reader.Bool("isIDRegistered");
		}

		#endregion


		#region UnitSet class

		public class UnitSet : IDCollection<Unit>
		{
			public Map Owner { get; private set; }
			public event Action<UnitSet, Unit, Vector2i, Vector2i> OnUnitMoved;

			public UnitSet(Map owner)
			{
				Owner = owner;

				OnElementAdded += Callback_UnitAdded;
				OnElementRemoved += Callback_UnitRemoved;

				foreach (Unit u in this)
					Callback_UnitAdded(this, u);
			}

			private void Callback_UnitAdded(LockedSet<Unit> thisSet, Unit u)
			{
				u.Pos.OnChanged += Callback_UnitMoved;
			}
			private void Callback_UnitRemoved(LockedSet<Unit> thisSet, Unit u)
			{
				u.Pos.OnChanged -= Callback_UnitMoved;
			}
			private void Callback_UnitMoved(Unit u, Vector2i oldPos, Vector2i newPos)
			{
				if (OnUnitMoved != null)
					OnUnitMoved(this, u, oldPos, newPos);
			}
			
			protected override ulong GetID(Unit owner)
			{
				return owner.ID;
			}
			protected override void SetID(ref Unit owner, ulong id)
			{
				owner.ID = id;
			}
			protected override void Write(MyData.Writer writer, Unit value, string name)
			{
				Unit.Write(writer, name, value);
			}
			protected override Unit Read(MyData.Reader reader, string name)
			{
				return Unit.Read(reader, Owner, name);
			}
		}

		#endregion
	}
}