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
		/// <summary>
		/// NOTE: This enum only determines turn order, not allies/enemies!
		/// </summary>
		public enum Teams
		{
			Player,
			Environment,
			Monsters,
		}
		

		/// <summary>
		/// NOTE: This value only determines turn order, not allies/enemies!
		/// </summary>
		public Stat<Teams, Unit> Team { get; private set; }

		public Stat<Vector2i, Unit> Pos { get; private set; }

		public UnitSet Allies, Enemies;
		
		public Map Owner { get; private set; }


		public Unit(Map map, Teams team) : this(map, team, new Vector2i(-1, -1)) { }
		public Unit(Map map, Teams team, Vector2i pos)
		{
			Team = new Stat<Teams, Unit>(this, team);
			Pos = new Stat<Vector2i, Unit>(this, pos);

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
			Pos = copyFrom.Pos;
		}

		/// <summary>
		/// Creates a clone of this unit that belongs to the given map.
		/// Note that this does NOT copy over events or allies/enemies.
		/// </summary>
		public abstract Unit Clone(Map newOwner);


		public abstract void TakeTurn();


		//Serialization stuff:

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

		protected enum Types
		{
			TestChar = 0,
			TestStructure,
			PlayerChar,
		}
		protected abstract Types MyType { get; }

		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Int((int)Team.Value, "team");
			writer.Int(Pos.Value.x, "posX");
			writer.Int(Pos.Value.y, "posY");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			Team.Value = (Teams)reader.Int("team");
			Pos.Value = new Vector2i(reader.Int("posX"),
							  		 reader.Int("posY"));
		}
	}


	public class UnitSet : LockedSet<Unit>
	{
		public Map Owner { get; private set; }

		public event Action<UnitSet, Unit, Vector2i, Vector2i> OnUnitMoved;
		public event Action<UnitSet, Unit, Unit.Teams, Unit.Teams> OnUnitTeamChanged;


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
			u.Team.OnChanged += Callback_UnitTeamChanged;
		}
		private void Callback_UnitRemoved(LockedSet<Unit> thisSet, Unit u)
		{
			u.Pos.OnChanged -= Callback_UnitMoved;
		}
		private void Callback_UnitTeamChanged(Unit u, Unit.Teams oldTeam, Unit.Teams newTeam)
		{
			if (OnUnitTeamChanged != null)
				OnUnitTeamChanged(this, u, oldTeam, newTeam);
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