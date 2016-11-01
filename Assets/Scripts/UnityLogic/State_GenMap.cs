using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	public class State_GenMap : GameFSM.State
	{
		/// <summary>
		/// If true, new starting units for the player will be generated from scratch.
		/// If false, they will be taken from the GameFSM.WorldProgress struct.
		/// </summary>
		public bool FromScratch;

		public int Seed;
		public int NThreads;

		public MapGen.BiomeGenSettings BiomeSettings;
		public MapGen.RoomGenSettings RoomSettings;
		public MapGen.CAGenSettings CASettings;


		public State_GenMap(bool fromScratch, int seed, int nThreads,
							MapGen.BiomeGenSettings biomeSettings,
							MapGen.RoomGenSettings roomSettings,
							MapGen.CAGenSettings caSettings)
		{
			FromScratch = fromScratch;

			Seed = seed;
			NThreads = nThreads;

			BiomeSettings = biomeSettings;
			RoomSettings = roomSettings;
			CASettings = caSettings;
		}


		public override void Start(GameFSM.State previousState)
		{
			RunGenerator();

			FSM.SaveWorld();

			FSM.CurrentTurn = GameLogic.Unit.Teams.Player;
			FSM.CurrentState = new State_Turn();
		}

		private void RunGenerator()
		{
			int mapSize = FSM.Settings.Size;

			//Run the generators.
			var biomes = BiomeSettings.Generate(mapSize, mapSize, NThreads, Seed);
			var rooms = RoomSettings.Generate(biomes, NThreads, Seed);
			var ca = CASettings.Generate(biomes, rooms, NThreads, Seed);

			//Convert that to actual tiles.
			GameLogic.TileTypes[,] tiles = new GameLogic.TileTypes[mapSize, mapSize];
			for (int y = 0; y < mapSize; ++y)
				for (int x = 0; x < mapSize; ++x)
					if (y == 0 || y == mapSize - 1 || x == 0 || x == mapSize - 1)
						tiles[x, y] = GameLogic.TileTypes.Bedrock;
					else if (ca[x, y])
						tiles[x, y] = GameLogic.TileTypes.Wall;
					else
						tiles[x, y] = GameLogic.TileTypes.Empty;

			FSM.Map.Tiles = new GameLogic.TileGrid(tiles);

			//Generate units.
			if (FromScratch)
			{
				//Some debug units:
				Vector2i firstPos, secondPos;
				var emptyTiles = FSM.Map.Tiles.GetTiles((pos, tile) => tile == GameLogic.TileTypes.Empty);
				firstPos = emptyTiles.First();
				secondPos = emptyTiles.Skip(1).First();
				FSM.Map.Units.Add(new GameLogic.Units.TestChar(FSM.Map, firstPos));
				FSM.Map.Units.Add(new GameLogic.Units.TestStructure(FSM.Map, secondPos));

				//TODO: Generate actual units.
			}
			else
			{
				//Clone each unit and store it in a dictionary.
				Dictionary<GameLogic.Unit, GameLogic.Unit> oldUnitToNewUnit =
					new Dictionary<GameLogic.Unit, GameLogic.Unit>();
				foreach (GameLogic.Unit unit in FSM.Progress.ExitedUnits)
					oldUnitToNewUnit.Add(unit, unit.Clone(FSM.Map));

				//Give each new unit the proper allies/enemies.
				foreach (KeyValuePair<GameLogic.Unit, GameLogic.Unit> kvp in oldUnitToNewUnit)
				{
					foreach (GameLogic.Unit oldAlly in kvp.Key.Allies)
						kvp.Value.Allies.Add(oldUnitToNewUnit[oldAlly]);
					foreach (GameLogic.Unit oldEnemy in kvp.Key.Enemies)
						kvp.Value.Enemies.Add(oldUnitToNewUnit[oldEnemy]);
				}

				//Finally, add all the new units to the map.
				foreach (GameLogic.Unit newUnit in oldUnitToNewUnit.Values)
					FSM.Map.Units.Add(newUnit);
				
				FSM.Progress.ExitedUnits.Clear();
			}
		}
	}
}