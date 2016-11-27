using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic
{
	public class Map : MyData.IReadWritable
	{
		public event Action<Map, Unit> OnUnitAdded;
		public event Action<Map, Unit> OnUnitRemoved;
		public event Action<Map, Unit, Vector2i, Vector2i> OnUnitMoved;


		public Group.GroupSet Groups { get; private set; }
		public TileGrid Tiles { get; private set; }

		/// <summary>
		/// Indicates that units should not take their turns.
		/// </summary>
        public Stat<bool, Map> IsPaused { get; private set; }

		/// <summary>
		/// Indicates that this map should stop running the update logic coroutine
		///     as soon as possible.
		/// </summary>
		public Stat<bool, Map> ShouldQuit { get; private set; }


		private ulong nextID = 0;
		private HashSet<Unit> units = new HashSet<Unit>();


		private Dictionary<ulong, Unit> idToUnit = new Dictionary<ulong, Unit>();
		private Dictionary<Vector2i, List<Unit>> posToUnits = new Dictionary<Vector2i, List<Unit>>();

		private Graph pathingGraph;
		private Pathfinding.PathFinder<Vector2i> pathing;
		private static List<Unit> emptyUnitList = new List<Unit>();


		public Map(int mapSizeX, int mapSizeY)
		{
			Tiles = new TileGrid(mapSizeX, mapSizeY);
			CommonInit();
		}
		public Map(TileTypes[,] tileGrid)
        {
            Tiles = new TileGrid(tileGrid);
			CommonInit();
		}
		public Map() : this(1, 1) { }

		private void CommonInit()
		{
            Groups = new Group.GroupSet(this);

            IsPaused = new Stat<bool, Map>(this, false);
			ShouldQuit = new Stat<bool, Map>(this, false);

			pathingGraph = new Graph(this);
			pathing = new Pathfinding.PathFinder<Vector2i>(pathingGraph, null);
		}


		/// <summary>
		/// Returns null if there's no unit at the given position.
		/// </summary>
		public IEnumerable<Unit> GetUnitsAt(Vector2i tilePos)
		{
			return (posToUnits.ContainsKey(tilePos) ?
						posToUnits[tilePos] :
						emptyUnitList);
		}
		
		/// <summary>
		/// Adds the given unit to this map.
		/// Returns its new ID.
		/// </summary>
		public ulong AddUnit(Unit u)
		{
			u.Pos.OnChanged += Callback_UnitMoved;

			if (!u.IsIDRegistered)
			{
				u.RegisterID(nextID);
				nextID += 1;
			}

			idToUnit.Add(u.ID, u);
			units.Add(u);
			u.MyGroup.UnitsByID.Add(u.ID);

			if (OnUnitAdded != null)
				OnUnitAdded(this, u);

			return u.ID;
		}
		/// <summary>
		/// Kills the given unit.
		/// </summary>
		public void RemoveUnit(Unit u)
		{
			u.MyGroup.UnitsByID.Remove(u.ID);

			idToUnit.Remove(u.ID);
			units.Remove(u);

			if (OnUnitRemoved != null)
				OnUnitRemoved(this, u);

			u.Pos.OnChanged -= Callback_UnitMoved;
		}

		public Unit GetUnit(ulong id) { return idToUnit[id]; }
		public bool DoesUnitExist(ulong id) { return idToUnit.ContainsKey(id); }

		public GroupType FindGroup<GroupType>()
			where GroupType : Group
		{
			return (GroupType)Groups.FirstOrDefault(g => (g is GroupType));
		}
		public IEnumerable<GroupType> FindGroups<GroupType>()
			where GroupType : Group
		{
			return Groups.Where(g => (g is GroupType)).Select(g => (GroupType)g);
		}

		
		/// <summary>
		/// Wipes out all units and jobs.
		/// </summary>
		public void Clear()
		{
			foreach (Unit u in units)
				RemoveUnit(u);
			units.Clear();

			Groups.Clear();

            IsPaused.Value = true;
			ShouldQuit.Value = true;
		}

		/// <summary>
		/// Outputs the shortest path from the given start to the given goal
		///     into the "outPath" list.
		/// Does not include the "start" pos in the list.
		/// Returns whether a path was actually found.
		/// </summary>
		/// <param name="heuristicCalc">
		/// The heuristic/path length calculator,
		///     or "null" if the default one (manhattan distance) should be used.
		/// </param>
		public bool FindPath(Vector2i start, Pathfinding.Goal<Vector2i> goal,
							 List<Vector2i> outPath,
							 Pathfinding.PathFinder<Vector2i>.CostCalculator heuristicCalc = null)
		{
			if (heuristicCalc == null)
				pathing.CalcCosts = Graph.AStarEdgeCalc;
			else
				pathing.CalcCosts = heuristicCalc;

			return pathing.FindPath(start, goal, float.PositiveInfinity, false, outPath);
		}
		/// <summary>
		/// Finds the shortest path from the given start to the given goal.
		/// Does not include the "start" pos in the list.
		/// Returns "null" if a path wasn't found.
		/// IMPORTANT: The returned list is reused for other calls to this method,
		///     so treat it as a temp variable!
		/// </summary>
		/// <param name="heuristicCalc">
		/// The heuristic/path length calculator,
		///     or "null" if the default one (manhattan distance) should be used.
		/// </param>
		public List<Vector2i> FindPath(Vector2i start, Pathfinding.Goal<Vector2i> goal,
									   Pathfinding.PathFinder<Vector2i>.CostCalculator heuristicCalc = null)
		{
			return (FindPath(start, goal, tempPath, heuristicCalc) ?
						tempPath :
						null);
		}
		private List<Vector2i> tempPath = new List<Vector2i>();

		/// <summary>
		/// Keeps running this map until the "ShouldQuit" field is set to true.
		/// </summary>
		public System.Collections.IEnumerator RunGameCoroutine()
		{
			while (true)
			{
				foreach (Group g in Groups.OrderByDescending(g => g.TurnPriority.Value))
				{
					foreach (object o in g.TakeTurn())
					{
						yield return o;

						while (IsPaused)
							yield return null;

						if (ShouldQuit.Value)
							yield break;
					}
				}
			}
		}


        public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Groups, "groups");
			writer.Structure(Tiles, "tiles");

			writer.UInt64(nextID, "nextID");

			writer.Collection<Unit, HashSet<Unit>>(units, "units",
												   (wr, unit, name) =>
													   Unit.Write(wr, name, unit));
		}
		public void ReadData(MyData.Reader reader)
		{
			Clear();

			reader.Structure(Groups, "groups");
			reader.Structure(Tiles, "tiles");

			nextID = reader.UInt64("nextID");

			reader.Collection("units",
							  (MyData.Reader rd, ref Unit outUnit, string name) =>
							      outUnit = Unit.Read(rd, this, name),
							  (size) => units);
			foreach (Unit u in units)
				AddUnit(u);
		}
		
		private void Callback_UnitMoved(Unit u, Vector2i oldP, Vector2i newP)
		{
			//Remove the unit's current entry in "posToUnits".
			if (posToUnits.ContainsKey(oldP))
				posToUnits[oldP].Remove(u);

			//Add its new entry in "posToUnits".
			if (!posToUnits.ContainsKey(newP))
				posToUnits.Add(newP, new List<Unit>());
			posToUnits[newP].Add(u);

			if (OnUnitMoved != null)
				OnUnitMoved(this, u, oldP, newP);
		}
	}
} 