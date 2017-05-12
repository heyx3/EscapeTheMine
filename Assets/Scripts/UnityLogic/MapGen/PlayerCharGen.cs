using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using GameLogic;
using PlayerChar = GameLogic.Units.PlayerChar;
using PlayerConsts = GameLogic.Units.Player_Char.Consts;


namespace UnityLogic.MapGen
{
	/// <summary>
	/// Generates an entrance for PlayerChars to enter through, then places them there.
	/// Can optionally generate some PlayerChar units to place in the entrance.
	/// </summary>
	[Serializable]
	public class PlayerCharGenSettings : MyData.IReadWritable
	{
		public int NStartingChars = 5;

		/// <summary>
		/// The quality of the PlayerChars' starting stats, from 0 to 1.
		/// </summary>
		public float StartingStatAbilities = 1.5f;


		/// <summary>
		/// Runs the generator and returns the spaces in the generated entrance.
		/// </summary>
		/// <param name="rooms">
		/// The rooms that were generated. Used to choose a place to place an entrance.
		/// </param>
		/// <param name="theMap">
		/// The map that new units are being generated into.
		/// </param>
		/// <param name="unitsToKeep">
		/// The units to keep alive in the map, indexed by their ID's.
		/// Pass "null" if new units should be generated from scratch.
		/// </param>
		public List<Vector2i> Generate(Map theMap, EtMGame.WorldSettings genSettings,
									   List<Room> rooms, int nThreads, int seed,
									   UlongSet unitsToKeep = null)
		{
			//Choose a room and place the level's "entrance" into the middle of it.
			List<Vector2i> entranceSpaces = new List<Vector2i>();
			{
				PRNG roomPlacer = new PRNG(unchecked(seed * 8957));
				Vector2i entrance = rooms[roomPlacer.NextInt() % rooms.Count].OriginalBounds.Center;
				entrance = new Vector2i(Mathf.Clamp(entrance.x, 1, genSettings.Size - 2),
										Mathf.Clamp(entrance.y, 1, genSettings.Size - 2));

				//Carve a small circle out of the map for the entrance.
				const float entranceRadius = 1.75f,
							entranceRadiusSqr = entranceRadius * entranceRadius;
				int entranceRadiusCeil = Mathf.CeilToInt(entranceRadius);
				Vector2i entranceRegionMin = entrance - new Vector2i(entranceRadiusCeil,
																	 entranceRadiusCeil),
						 entranceRegionMax = entrance + new Vector2i(entranceRadiusCeil,
																	 entranceRadiusCeil);
				entranceRegionMin = new Vector2i(Mathf.Clamp(entranceRegionMin.x,
															 0, genSettings.Size - 1),
												 Mathf.Clamp(entranceRegionMin.y,
															 0, genSettings.Size - 1));
				entranceRegionMax = new Vector2i(Mathf.Clamp(entranceRegionMax.x,
															 0, genSettings.Size - 1),
												 Mathf.Clamp(entranceRegionMax.y,
															 0, genSettings.Size - 1));

				for (int y = entranceRegionMin.y; y <= entranceRegionMax.y; ++y)
					for (int x = entranceRegionMin.x; x <= entranceRegionMax.x; ++x)
						if (entrance.DistanceSqr(new Vector2i(x, y)) < entranceRadiusSqr)
						{
							entranceSpaces.Add(new Vector2i(x, y));
							theMap.Tiles[x, y] = GameLogic.TileTypes.Empty;
						}
			}


			//If we weren't given any units to place, generate some.
			if (unitsToKeep == null)
			{
				unitsToKeep = new UlongSet();

				//Find or create the PlayerGroup.
				var playerGroup = theMap.FindGroup<GameLogic.Groups.PlayerGroup>();
				if (playerGroup == null)
				{
					playerGroup = new GameLogic.Groups.PlayerGroup(theMap);
					theMap.Groups.Add(playerGroup);
				}

				//Generate a certain number of player units.
				PRNG prng = new PRNG(seed);
				for (int i = 0; i < NStartingChars; ++i)
				{
					//PlayerChar's have 3 stats to randomize.
					//Distribute "points" among them randomly.
					//Prefer energy, then food, then strength.

					float totalPoints = StartingStatAbilities;

					float p = prng.NextFloat() * Math.Min(1.0f, totalPoints);
					float energy = p;
					totalPoints -= p;
					
					p = prng.NextFloat() * Math.Min(1.0f, totalPoints);
					float food = p;
					totalPoints -= p;

					float strength = totalPoints;

					energy = Mathf.Lerp(PlayerConsts.MinStart_Energy,
										PlayerConsts.MaxStart_Energy,
										energy);
					food = Mathf.Lerp(PlayerConsts.MinStart_Food,
									  PlayerConsts.MaxStart_Food,
									  food);
					strength = Mathf.Lerp(PlayerConsts.MinStart_Strength,
										  PlayerConsts.MaxStart_Strength,
										  strength);

					var gender = (i % 2 == 0) ?
								      GameLogic.Units.Player_Char.Personality.Genders.Male :
								      GameLogic.Units.Player_Char.Personality.Genders.Female;

					PlayerChar chr = new PlayerChar(
						theMap, playerGroup.ID, food, energy, strength, 1.0f,
						GameLogic.Units.Player_Char.Personality.GenerateName(gender, prng.NextInt()),
						gender);
					theMap.AddUnit(chr);
					unitsToKeep.Add(chr.ID);
				}
			}

			//Position the units.
			UnityEngine.Assertions.Assert.IsTrue(entranceSpaces.Count > 0);
			int unitsPerSpace = unitsToKeep.Count / entranceSpaces.Count;
			int unitI = 0,
				posI = 0;
			foreach (Unit unit in unitsToKeep.Select(id => theMap.GetUnit(id)))
			{
				unit.Pos.Value = entranceSpaces[posI];

				unitI += 1;
				if (unitI >= unitsPerSpace)
				{
					unitI = 0;
					posI = (posI + 1) % entranceSpaces.Count;
				}
			}

			return entranceSpaces;
		}

		public void WriteData(MyData.Writer writer)
		{
			writer.Int(NStartingChars, "nStartingChars");
			writer.Float(StartingStatAbilities, "startingStatAbilities");
		}
		public void ReadData(MyData.Reader reader)
		{
			NStartingChars = reader.Int("nStartingChars");
			StartingStatAbilities = reader.Float("startingStatAbilities");
		}
	}
}
