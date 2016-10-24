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
		public TileGrid Tiles;
		public Graph PathingGraph;

		private Dictionary<Vector2i, List<Unit>> posToUnits = new Dictionary<Vector2i, List<Unit>>();
		private static List<Unit> emptyUnitList = new List<Unit>();


		public Map(int mapSizeX, int mapSizeY)
		{
			Units = new UnitSet(this);
			Tiles = new TileGrid(mapSizeX, mapSizeY);
			PathingGraph = new Graph(this);

			RegisterCallbacks();
		}
		public Map(TileTypes[,] tileGrid)
		{
			Units = new UnitSet(this);
			Tiles = new TileGrid(tileGrid);
			PathingGraph = new Graph(this);

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

		public void Clear()
		{
			Units.Clear();

			if (OnMapCleared != null)
				OnMapCleared(this);
		}


		//Serialization stuff:
		public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Units, "units");
			writer.Structure(Tiles, "tiles");
		}
		public void ReadData(MyData.Reader reader)
		{
			Units.Clear();
			reader.Structure(Units, "units");

			reader.Structure(Tiles, "tiles");
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