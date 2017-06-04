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
		//TODO: Allow the player to add a "Note" to any unit. Show the note when mousing over.

		/// <summary>
		/// Raised when this unit is removed from the map.
		/// </summary>
		public event Action<Unit, Map> OnKilled;
		/// <summary>
		/// Raises the "OnKilled()" event.
		/// </summary>
		public void WasKilled()
		{
			if (OnKilled != null)
				OnKilled(this, TheMap);
		}


		public Stat<Vector2i, Unit> Pos { get; private set; }

		public Stat<float, Unit> MaxEnemyDistance { get; private set; }
		public float One_Over_MaxEnemyDistance { get; private set; }

		public Map TheMap { get; private set; }
		public ulong MyGroupID { get; private set; }
		public ulong ID { get; private set; }

		/// <summary>
		/// Whether this unit's ID has been assigned by the map yet.
		/// </summary>
		public bool IsIDRegistered { get; private set; }

		public Group MyGroup { get { return TheMap.Groups.Get(MyGroupID); } }

		public abstract string DisplayName { get; }

		/// <summary>
		/// If true, then the existence of this unit on a tile blocks structures from being built on it.
		/// "Structures" are a certain category of Unit, including Door, Workshop, Trap, etc.
		/// </summary>
		public abstract bool BlocksStructures { get; }
		/// <summary>
		/// If true, then the existence of this unit on a tile blocks other units from moving through it.
		/// </summary>
		public abstract bool BlocksMovement { get; }


		public Unit(Map theMap, Group g) : this(theMap, g, new Vector2i(0, 0)) { }
		public Unit(Map theMap, Group g, Vector2i pos)
		{
			TheMap = theMap;
			if (g != null)
				MyGroupID = g.ID;

			Pos = new Stat<Vector2i, Unit>(this, pos);

			MaxEnemyDistance = new Stat<float, Unit>(this, Units.Consts.DefaultMaxEnemyDistSqr);
			MaxEnemyDistance.OnChanged +=
				(_this, oldVal, newVal) => One_Over_MaxEnemyDistance = 1.0f / newVal;
			One_Over_MaxEnemyDistance = 1.0f / MaxEnemyDistance;

			ID = ulong.MaxValue;

			IsIDRegistered = false;
		}

		public Unit(Map theMap, ulong groupID) : this(theMap, theMap.Groups.TryGet(groupID)) { }
		public Unit(Map theMap, ulong groupID, Vector2i pos)
			: this(theMap, theMap.Groups.TryGet(groupID), pos) { }


		public void RegisterID(ulong myID)
		{
			UnityEngine.Assertions.Assert.IsFalse(IsIDRegistered);
			IsIDRegistered = true;
			ID = myID;
		}

		#region Pathfinding

		/// <summary>
		/// Outputs the shortest path from this Unit to the given goal
		///     into the "outPath" list.
		/// Does not include this Unit's own position in the list.
		/// Returns whether a path was actually found.
		/// </summary>
		public bool FindPath(Pathfinding.Goal<Vector2i> goal, List<Vector2i> outPath, bool avoidEnemies)
		{
			return TheMap.FindPath(Pos, goal, outPath, MakeHeuristic(avoidEnemies));
		}
		/// <summary>
		/// Finds the shortest path from this Unit to the given goal.
		/// Does not include this Unit's own position in the list.
		/// Returns "null" if a path wasn't found.
		/// IMPORTANT: The returned list is reused for other calls to this method,
		///     so treat it as a temp variable!
		/// </summary>
		public List<Vector2i> FindPath(Pathfinding.Goal<Vector2i> goal, bool avoidEnemies)
		{
			return TheMap.FindPath(Pos, goal, MakeHeuristic(avoidEnemies));
		}

		/// <summary>
		/// The heuristic function for pathfinding.
		/// Defaults to a method that includes the usual straight-line-distance heuristic,
		///     plus attempts to avoid nearby enemies.
		/// Note that to reduce garbage, this heuristic may reuse global variables,
		///		which means only one Unit can have a heuristic at any time.
		/// </summary>
		public virtual Pathfinding.PathFinder<Vector2i>.CostCalculator MakeHeuristic(bool avoidEnemies)
		{
			PrepareAStarEdgeCalc(avoidEnemies);
			return AStarEdgeCalc;
		}

		/// <summary>
		/// Prepares this unit to do pathfinding with the "AStarEdgeCalc()" heuristic method.
		/// Note that to reduce garbage, this heuristic may reuse global variables,
		///		which means only one Unit can have a heuristic at any time.
		/// </summary>
		private void PrepareAStarEdgeCalc(bool avoidEnemies)
		{
			_temp_enemies.Clear();
			foreach (ulong enemyGroupID in MyGroup.EnemiesByID)
				foreach (ulong enemyUnitID in TheMap.Groups.Get(enemyGroupID).UnitsByID)
					_temp_enemies.Add(TheMap.GetUnit(enemyUnitID));
		}
		private static HashSet<Unit> _temp_enemies = new HashSet<Unit>();

		/// <summary>
		/// The heuristic function for doing A* pathfinding for this unit.
		/// Note that to reduce garbage, this heuristic may reuse global variables,
		///		which means only one Unit can have a heuristic at any time.
		/// </summary>
		protected void AStarEdgeCalc(Pathfinding.Goal<Vector2i> goal,
								     Pathfinding.Edge<Vector2i> edge,
								     out float edgeLength, out float heuristic)
		{
			Graph.AStarEdgeCalc(goal, edge, out edgeLength, out heuristic);

			//Subtract enemy distances squared from the heuristic.
			foreach (Unit enemy in _temp_enemies)
			{
				float distSqr = enemy.Pos.Value.DistanceSqr(Pos);
				float distT = distSqr * One_Over_MaxEnemyDistance;
				distT = Math.Max(0.0f, Math.Min(1.0f, distT));

				heuristic += Units.Consts.EnemyDistanceHeuristicMax * distT;
			}
		}

		#endregion

		/// <summary>
		/// Runs a coroutine that has this unit take his turn.
		/// </summary>
		public abstract System.Collections.IEnumerable TakeTurn();

		public override int GetHashCode() { return (int)ID; }
		public override string ToString() { return DisplayName + ": " + MyType.ToString(); }


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
				case Types.PlayerChar: u = new Units.PlayerChar(theMap); break;
				case Types.Bed: u = new Units.Bed(theMap); break;
				default: throw new NotImplementedException(type.ToString());
			}

			reader.Structure(u, name + "_Value");

			return u;
		}

		public enum Types
		{
			PlayerChar,
			Bed,
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
	}
}