using System;
using System.Collections.Generic;
using System.Linq;


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
		/// The minimum-allowable time it takes to run through every unit on the map.
		/// </summary>
		public float MinTurnWait = 0.5f;

		private ulong nextID = 0;
		private HashSet<Unit> units = new HashSet<Unit>();

		private Dictionary<ulong, Unit> idToUnit = new Dictionary<ulong, Unit>();
		private GridSet<Unit> unitsByPos;
		private Dictionary<Unit.Types, HashSet<Unit>> unitsByType = new Dictionary<Unit.Types, HashSet<Unit>>();

		private Graph pathingGraph;
		private Pathfinding.PathFinder<Vector2i> pathing;

		private static readonly HashSet<Unit> emptyUnitSet = new HashSet<Unit>();


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

			pathingGraph = new Graph(this);
			pathing = new Pathfinding.PathFinder<Vector2i>(pathingGraph, null);

			unitsByPos = new GridSet<Unit>(Tiles.Dimensions);

			//When the tile grid changes size, reset "unitsByPos".
			Tiles.OnTileGridReset += (grid, oldSize, newSize) =>
				{
					unitsByPos = new GridSet<Unit>(newSize);
					foreach (Unit u in units)
						unitsByPos.AddValue(u, u.Pos);
				};
		}


		public IEnumerable<Unit> GetUnits()
		{
			return units;
		}
		public IEnumerable<Unit> GetUnits(Unit.Types type)
		{
			if (unitsByType.ContainsKey(type))
				return unitsByType[type];
			else
				return emptyUnitSet;
		}
		public GridSet<Unit>.CellIterator GetUnits(Vector2i tilePos)
		{
			return unitsByPos.GetValuesAt(tilePos);
		}
		public GridSet<Unit>.PosIterator GetTilesWithUnits()
		{
			return unitsByPos.GetActiveCells();
		}

		public bool AnyUnitsAt(Vector2i tilePos, Predicate<Unit> predicate)
		{
			foreach (var unit in GetUnits(tilePos))
				if (predicate(unit))
					return true;
			return false;
		}
		public bool AllUnitsAt(Vector2i tilePos, Predicate<Unit> predicate)
		{
			foreach (var unit in GetUnits(tilePos))
				if (!predicate(unit))
					return false;
			return true;
		}
		public Unit FirstUnitAt(Vector2i tilePos, Predicate<Unit> predicate)
		{
			foreach (var unit in GetUnits(tilePos))
				if (predicate(unit))
					return unit;
			return null;
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

			//Add the unit's info to various cached data structures.
			if (!unitsByType.ContainsKey(u.MyType))
				unitsByType.Add(u.MyType, new HashSet<Unit>());
			unitsByType[u.MyType].Add(u);
			if (!unitsByPos.ContainsAt(u.Pos, u))
				unitsByPos.AddValue(u, u.Pos);
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

			unitsByType[u.MyType].Remove(u);
			unitsByPos.RemoveValue(u, u.Pos);

			u.Pos.OnChanged -= Callback_UnitMoved;

			u.WasKilled();
			if (OnUnitRemoved != null)
				OnUnitRemoved(this, u);

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
		/// NOTE: the map may take a few frames to actually end.
		/// </summary>
		public void Clear()
		{
			foreach (Unit u in units.ToList())
				RemoveUnit(u);
			units.Clear();

			Groups.Clear();
			Groups.NextID = 0;

            IsPaused.Value = true;
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
			//Note that this method requires some interaction with the Unity API.
			//However, it's worth it to have this method be encapsulated in the Map class.

			List<Group> groupsToUpdate = new List<Group>(Groups.Count);
			while (true)
			{
				float turnStartTime = UnityEngine.Time.time;

				//Prevent an infinite loop if there are no units or groups.
				yield return null;

				//Order groups based on their turn priority and have them take their turns.

				//Use a list so that new groups don't break the enumerator.
				groupsToUpdate.AddRange(Groups);
				groupsToUpdate.Sort((g1, g2) => g1.TurnPriority.Value - g2.TurnPriority.Value);

				//Run update logic for each Group.
				foreach (Group g in groupsToUpdate)
				{
					foreach (object o in g.TakeTurn())
					{
						yield return o;

						while (IsPaused)
							yield return null;
					}
				}
				groupsToUpdate.Clear();

				//Wait for the next turn.
				float elapsedTime = UnityEngine.Time.time - turnStartTime;
				if (elapsedTime < MinTurnWait)
					yield return new UnityEngine.WaitForSeconds(MinTurnWait - elapsedTime);
			}
		}


        public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Tiles, "tiles");
			writer.UInt64(nextID, "nextID");
			writer.Structure(Groups, "groups");
			writer.Collection<Unit, HashSet<Unit>>(units, "units",
												   (wr, unit, name) =>
													   Unit.Write(wr, name, unit));

		}
		public void ReadData(MyData.Reader reader)
		{
			Clear();

			reader.Structure(Tiles, "tiles");
			unitsByPos = new GridSet<Unit>(Tiles.Dimensions);

			nextID = reader.UInt64("nextID");

			reader.Structure(Groups, "groups");
			reader.Collection("units",
							  (MyData.Reader rd, ref Unit outUnit, string name) =>
							      outUnit = Unit.Read(rd, this, name),
							  (size) => units);

			foreach (Unit u in units)
				AddUnit(u);
			foreach (Group g in Groups)
				g.FinishDeserialization();
		}

		private void Callback_UnitMoved(Unit u, Vector2i oldP, Vector2i newP)
		{
			unitsByPos.MoveValue(u, oldP, newP);

			if (OnUnitMoved != null)
				OnUnitMoved(this, u, oldP, newP);
		}
	}
}