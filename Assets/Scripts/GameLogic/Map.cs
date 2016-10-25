using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic
{
	public class Map : MyData.IReadWritable
	{
		public event Action<Map> OnMapCleared;

		public UnitSet Units;
		public GlobalJobs Jobs;
		public TileGrid Tiles;

		private Graph pathingGraph;
		private Pathfinding.PathFinder<Vector2i> pathing;

		private Dictionary<Vector2i, List<Unit>> posToUnits = new Dictionary<Vector2i, List<Unit>>();
		private static List<Unit> emptyUnitList = new List<Unit>();


		public Map(int mapSizeX, int mapSizeY)
		{
			Units = new UnitSet(this);
			Tiles = new TileGrid(mapSizeX, mapSizeY);
			Jobs = new GlobalJobs(this);

			pathingGraph = new Graph(this);
			pathing = new Pathfinding.PathFinder<Vector2i>(pathingGraph, null);

			RegisterCallbacks();
		}
		public Map(TileTypes[,] tileGrid)
		{
			Units = new UnitSet(this);
			Tiles = new TileGrid(tileGrid);
			Jobs = new GlobalJobs(this);

			pathingGraph = new Graph(this);
			pathing = new Pathfinding.PathFinder<Vector2i>(pathingGraph, null);

			RegisterCallbacks();
		}
		public Map() : this(1, 1) { }

		private void RegisterCallbacks()
		{
			Units.OnElementAdded += OnUnitAdded;
			Units.OnElementRemoved += OnUnitRemoved;
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
		/// Wipes out all units and jobs.
		/// </summary>
		public void Clear()
		{
			Units.Clear();
			Jobs.Clear();

			if (OnMapCleared != null)
				OnMapCleared(this);
		}


		//Serialization stuff:
		public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Units, "units");
			writer.Structure(Tiles, "tiles");
			writer.Structure(Jobs, "jobs");
		}
		public void ReadData(MyData.Reader reader)
		{
			Clear();

			reader.Structure(Units, "units");
			reader.Structure(Tiles, "tiles");
			reader.Structure(Jobs, "jobs");
		}


		#region Callbacks
		
		private void OnUnitAdded(LockedSet<Unit> units, Unit unit)
		{
			//Add the unit to the "posToUnits" lookup.
			if (!posToUnits.ContainsKey(unit.Pos))
				posToUnits.Add(unit.Pos, new List<Unit>());
			posToUnits[unit.Pos].Add(unit);

			//Register callbacks for the unit.
			unit.Pos.OnChanged += OnUnitMoved;
		}
		private void OnUnitRemoved(LockedSet<Unit> units, Unit unit)
		{
			//Remove the unit from the "posToUnits" lookup.
			//If there's no units left at that position, remove that entry entirely.
			if (posToUnits[unit.Pos].Count == 1)
			{
				UnityEngine.Assertions.Assert.IsTrue(posToUnits[unit.Pos][0] == unit);
				posToUnits.Remove(unit.Pos);
			}
			else
			{
				posToUnits[unit.Pos].Remove(unit);
			}

			//Unregister callbacks for the unit.
			unit.Pos.OnChanged -= OnUnitMoved;
		}

		private void OnUnitMoved(Unit u, Vector2i oldP, Vector2i newP)
		{
			//Move the unit from its current entry in the "posToUnits" lookup to a new entry.

			posToUnits[oldP].Remove(u);

			if (!posToUnits.ContainsKey(newP))
				posToUnits.Add(newP, new List<Unit>());
			posToUnits[newP].Add(u);
		}

		#endregion
	}
} 