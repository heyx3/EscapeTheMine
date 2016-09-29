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
		public enum Teams
		{
			Player,
			Environment,
			Enemies,
		}


		/// <summary>
		/// The parameters are this unit, its old position, and its new position, respectively.
		/// </summary>
		public event Action<Unit, Vector2i, Vector2i> OnPosChanged;

		public Teams Team;

		public Vector2i Pos
		{
			get { return pos; }
			set
			{
				Vector2i oldP = pos;
				pos = value;
				if (OnPosChanged != null)
					OnPosChanged(this, oldP, value);
			}
		}
		private Vector2i pos;

		public UnitSet Allies, Enemies;
		
		public Map Owner { get; private set; }


		public Unit(Map map, Teams team) : this(map, team, new Vector2i(-1, -1)) { }
		public Unit(Map map, Teams team, Vector2i _pos)
		{
			Team = team;
			pos = _pos;

			Owner = map;

			Allies = new UnitSet(Owner);
			Allies.OnElementAdded += (allies, ally) =>
				{
					Enemies.Remove(ally);
				};

			Enemies = new UnitSet(Owner);
			Enemies.OnElementAdded += (enemies, enemy) =>
				{
					Allies.Remove(enemy);
				};
		}

		protected Unit(Map map, Unit copyFrom)
		{
			Owner = map;
			Team = copyFrom.Team;
			pos = copyFrom.pos;
		}

		/// <summary>
		/// Creates a clone of this unit that belongs to the given map.
		/// Note that this does NOT copy over events or allies/enemies.
		/// </summary>
		public abstract Unit Clone(Map newOwner);


		//Serialization stuff:

		public static void Write(MyData.Writer writer, string name, Unit u)
		{
			writer.Int((int)u.MyType, name + "_Type");
			writer.Structure(u, name + "_Value");
		}
		public static Unit Read(MyData.Reader reader, Map map, string name)
		{
			Unit u = null;
			Types type = (Types)reader.Int(name + "_Type");
			switch (type)
			{
				case Types.TestChar: u = new Units.TestChar(map); break;
				default: throw new NotImplementedException(type.ToString());
			}

			reader.Structure(u, name + "_Value");

			return u;
		}

		protected enum Types
		{
			TestChar = 0,
			TestStructure = 0,
		}
		protected abstract Types MyType { get; }

		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Int((int)Team, "team");
			writer.Int(pos.x, "posX");
			writer.Int(pos.y, "posY");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			Team = (Teams)reader.Int("team");
			Pos = new Vector2i(reader.Int("posX"),
							   reader.Int("posY"));
		}
	}


	public class UnitSet : LockedSet<Unit>
	{
		public Map Owner { get; private set; }

		public UnitSet(Map owner) { Owner = owner; }


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