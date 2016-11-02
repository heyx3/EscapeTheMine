using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	//TODO: Remove TestStruct and TestChar.

	public class State_GenMap : GameFSM.State
	{
		/// <summary>
		/// If true, new starting units for the player will be generated from scratch.
		/// If false, they will be taken from the GameFSM.WorldProgress struct.
		/// </summary>
		public bool FromScratch;

		public int Seed;
		public int NThreads;


		public State_GenMap(bool fromScratch, int seed, int nThreads)
		{
			FromScratch = fromScratch;

			Seed = seed;
			NThreads = nThreads;
		}


		public override void Start(GameFSM.State previousState)
		{
			RunGenerator(FSM.Settings, FSM.Progress, FSM.Map);

			FSM.SaveWorld();

			FSM.CurrentTurn = GameLogic.Unit.Teams.Player;
			FSM.CurrentState = new State_Turn();
		}

		private void RunGenerator(GameFSM.WorldSettings settings, GameFSM.WorldProgress progress,
								  GameLogic.Map map)
		{
			int mapSize = settings.Size;

			//Run the generators.
			var biomes = settings.Biome.Generate(mapSize, mapSize, NThreads, unchecked(Seed * 462315));
			var deposits = settings.Deposits.Generate(mapSize, mapSize, NThreads, unchecked(Seed * 123));
			var rooms = settings.Rooms.Generate(biomes, NThreads, Seed);
			var finalWalls = settings.CA.Generate(biomes, rooms, NThreads, unchecked(Seed * 3468));

			//Convert that to actual tiles.
			GameLogic.TileTypes[,] tiles = new GameLogic.TileTypes[mapSize, mapSize];
			for (int y = 0; y < mapSize; ++y)
			{
				for (int x = 0; x < mapSize; ++x)
				{
					if (y == 0 || y == mapSize - 1 || x == 0 || x == mapSize - 1)
						tiles[x, y] = GameLogic.TileTypes.Bedrock;
					else if (finalWalls[x, y])
						tiles[x, y] = (deposits[x, y] ? GameLogic.TileTypes.Deposit : GameLogic.TileTypes.Wall);
					else
						tiles[x, y] = GameLogic.TileTypes.Empty;
				}
			}

			map.Tiles = new GameLogic.TileGrid(tiles);


			//Choose a room and place the level's "entrance" into the middle of it.
			List<Vector2i> entranceSpaces = new List<Vector2i>();
			{
				PRNG roomPlacer = new PRNG(unchecked(Seed * 8957));
				Vector2i entrance = rooms[roomPlacer.NextInt() % rooms.Count].OriginalBounds.Center;
				entrance = new Vector2i(Mathf.Clamp(entrance.x, 1, mapSize - 2),
										Mathf.Clamp(entrance.y, 1, mapSize - 2));

				//Carve a small circle out of the map for the entrance.
				const float entranceRadius = 1.75f,
							entranceRadiusSqr = entranceRadius * entranceRadius;
				int entranceRadiusCeil = Mathf.CeilToInt(entranceRadius);
				Vector2i entranceRegionMin = entrance - new Vector2i(entranceRadiusCeil,
																	 entranceRadiusCeil),
						 entranceRegionMax = entrance + new Vector2i(entranceRadiusCeil,
																	 entranceRadiusCeil);
				entranceRegionMin = new Vector2i(Mathf.Clamp(entranceRegionMin.x, 0, mapSize - 1),
												 Mathf.Clamp(entranceRegionMin.y, 0, mapSize - 1));
				entranceRegionMax = new Vector2i(Mathf.Clamp(entranceRegionMax.x, 0, mapSize - 1),
												 Mathf.Clamp(entranceRegionMax.y, 0, mapSize - 1));

				for (int y = entranceRegionMin.y; y <= entranceRegionMax.y; ++y)
					for (int x = entranceRegionMin.x; x <= entranceRegionMax.x; ++x)
						if (entrance.DistanceSqr(new Vector2i(x, y)) < entranceRadiusSqr)
						{
							entranceSpaces.Add(new Vector2i(x, y));
							map.Tiles[x, y] = GameLogic.TileTypes.Empty;
						}
			}

			//Generate units.
			var newUnits = settings.PlayerChars.Generate(
							   entranceSpaces,
							   (FromScratch ? null : progress.ExitedUnits),
							   FSM.Map, NThreads, Seed);
			progress.ExitedUnits.Clear();


			//TODO: Make the camera focus in on the units. Provide some kind of "ZoomToUnits()" method on the Content2D/3D classes.
		}
	}
}