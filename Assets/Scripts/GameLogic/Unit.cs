using System;
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

        public Group MyGroup { get; private set; }
        public Map TheMap { get { return MyGroup.TheMap; } }


		public Unit(Group g) : this(g, new Vector2i(-1, -1)) { }
		public Unit(Group g, Vector2i pos)
		{
            MyGroup = g;
			Pos = new Stat<Vector2i, Unit>(this, pos);
		}

		protected Unit(Group g, Unit copyFrom)
		{
            MyGroup = g;
			Pos = copyFrom.Pos;
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
		public static Unit Read(MyData.Reader reader, Map map, string name)
		{
			Unit u = null;
			Types type = (Types)reader.UInt(name + "_Type");
			switch (type)
			{
				case Types.TestChar: u = new Units.TestChar(map); break;
				case Types.TestStructure: u = new Units.TestStructure(map); break;
				case Types.PlayerChar: u = new Units.PlayerChar(map); break;
				default: throw new NotImplementedException(type.ToString());
			}

			reader.Structure(u, name + "_Value");

			return u;
		}

		public enum Types
		{
			TestChar = 0,
			TestStructure,
			PlayerChar,
		}
		public abstract Types MyType { get; }

		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Vec2i(Pos, "pos");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			Pos.Value = reader.Vec2i("pos");
		}

		#endregion
	}


	public class UnitSet : LockedSet<Unit>
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

		//Serialization stuff.
		protected override void Write(MyData.Writer writer, Unit value, string name)
		{
			Unit.Write(writer, name, value);
		}
		protected override Unit Read(MyData.Reader reader, string name)
		{
			return Unit.Read(reader, Owner, name);
		}
	}
}