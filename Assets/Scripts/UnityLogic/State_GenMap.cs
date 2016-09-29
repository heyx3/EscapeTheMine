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

		public MapGen.BiomeGenSettings BiomeSettings;
		public MapGen.RoomGenSettings RoomSettings;


		public State_GenMap(bool fromScratch,
							MapGen.BiomeGenSettings biomeSettings,
							MapGen.RoomGenSettings roomSettings)
		{
			FromScratch = fromScratch;
			BiomeSettings = biomeSettings;
			RoomSettings = roomSettings;
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
			const int mapSize = 500;

			//Run the generators.
			var biomes = BiomeSettings.Generate(mapSize, mapSize);
			var rooms = RoomSettings.Generate(biomes);

			//Convert that to actual tiles.
			GameLogic.TileTypes[,] tiles = new GameLogic.TileTypes[mapSize, mapSize];
			for (int y = 0; y < mapSize; ++y)
				for (int x = 0; x < mapSize; ++x)
					if (rooms.Any(room => room.Spaces.Contains(new Vector2i(x, y))))
						tiles[x, y] = GameLogic.TileTypes.Empty;
					else
						tiles[x, y] = GameLogic.TileTypes.Wall;

			FSM.Map.Tiles = new GameLogic.TileGrid(tiles);

			//Bring in any units from the previous level.
			if (!FromScratch)
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