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
	/// Generates new PlayerChar units for a new level.
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
		/// Runs the generator and returns the new units.
		/// </summary>
		/// <param name="entranceSpaces">
		/// The empty spaces in the map to place units into.
		/// </param>
		/// <param name="toPlace">
		/// The units to clone into this new map.
		/// Pass "null" if new units should be generated from scratch.
		/// </param>
		public HashSet<Unit> Generate(List<Vector2i> entranceSpaces, ICollection<Unit> toClone,
									  Map newMap, int nThreads, int seed)
		{
			HashSet<Unit> chars = new HashSet<Unit>();

			//Either generate new units or clone the old ones.
			GameLogic.Groups.PlayerGroup playerGroup = null;
			if (toClone == null)
			{
				playerGroup = new GameLogic.Groups.PlayerGroup(newMap);
				HashSet<Unit> units = new HashSet<Unit>();

				PRNG prng = new PRNG(seed);
				for (int i = 0; i < NStartingChars; ++i)
				{
					//PlayerChar's have 3 stats to randomize.
					//Distribute "points" among them randomly.
					//Prefer energy, then food, then strength.

					float totalPoints = StartingStatAbilities;

					float f = float.PositiveInfinity;
					while (f > totalPoints)
						f = prng.NextFloat();
					float energy = f;
					totalPoints -= f;

					f = float.PositiveInfinity;
					while (f > totalPoints)
						f = prng.NextFloat();
					float food = f;
					totalPoints -= f;

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


					PlayerChar chr = new PlayerChar(playerGroup, food, energy, strength);
					playerGroup.Units.Add(chr);
					units.Add(chr);
				}
			}
			else
			{
				playerGroup = newMap.FindPlayerGroup();
				UnityEngine.Assertions.Assert.IsNotNull(playerGroup);

				playerGroup.Clear();
			}

			//Position the units.
			UnityEngine.Assertions.Assert.IsTrue(entranceSpaces.Count > 0);
			int unitsPerSpace = playerGroup.Units.Count / entranceSpaces.Count;
			int unitI = 0,
				posI = 0;
			foreach (Unit unit in playerGroup.Units)
			{
				unit.Pos.Value = entranceSpaces[posI];

				unitI += 1;
				if (unitI >= unitsPerSpace)
				{
					unitI = 0;
					posI = (posI + 1) % entranceSpaces.Count;
				}
			}

			return chars;
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
