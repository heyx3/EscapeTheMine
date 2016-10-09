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


		public override GameFSM.State Start(GameFSM.State previousState)
		{
			RunGenerator();

			FSM.SaveWorld();

			//TODO: Change to the normal gameplay state.
			return base.Start(previousState);
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
						tiles[x, y] = GameLogic.TileTypes.Empty;
					else
						tiles[x, y] = GameLogic.TileTypes.Wall;

			FSM.Map.Tiles = new GameLogic.TileGrid(tiles);

			//Generate units.
			if (FromScratch)
			{
				//TODO: Implement.
			}
			else
			{
				Dictionary<GameLogic.Unit, GameLogic.Unit> oldUnitToNewUnit =
					new Dictionary<GameLogic.Unit, GameLogic.Unit>();
				foreach (GameLogic.Unit unit in FSM.Progress.ExitedUnits)
				{
					GameLogic.Unit newUnit = unit.Clone(FSM.Map);

					oldUnitToNewUnit.Add(unit, newUnit);
					FSM.Map.Units.Add(newUnit);
				}
				
				FSM.Progress.ExitedUnits.Clear();
			}
		}
	}
}