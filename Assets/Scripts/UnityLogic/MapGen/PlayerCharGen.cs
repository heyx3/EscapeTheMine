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
			ICollection<Unit> newUnits;
			if (toClone == null)
			{
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

					energy = Mathf.Lerp(PlayerConsts.Instance.MinStart_Energy,
										PlayerConsts.Instance.MaxStart_Energy,
										energy);
					food = Mathf.Lerp(PlayerConsts.Instance.MinStart_Food,
									  PlayerConsts.Instance.MaxStart_Food,
									  food);
					strength = Mathf.Lerp(PlayerConsts.Instance.MinStart_Strength,
										  PlayerConsts.Instance.MaxStart_Strength,
										  strength);


					PlayerChar chr = new PlayerChar(newMap, food, energy, strength);
					newMap.Units.Add(chr);
					units.Add(chr);
				}

				//Make all these units allies with each other.
				foreach (Unit u in units)
				{
					foreach (Unit u2 in units)
						if (u != u2)
							u.Allies.Add(u2);
				}

				newUnits = units;
			}
			else
			{
				//Clone each unit and associate each original with its clone.
				Dictionary<Unit, Unit> oldUnitToNew = new Dictionary<Unit, Unit>(toClone.Count);
				foreach (Unit unit in toClone)
					oldUnitToNew.Add(unit, unit.Clone(newMap));

				//Give each new unit the proper allies/enemies.
				foreach (KeyValuePair<GameLogic.Unit, GameLogic.Unit> kvp in oldUnitToNew)
				{
					foreach (GameLogic.Unit oldAlly in kvp.Key.Allies)
						kvp.Value.Allies.Add(oldUnitToNew[oldAlly]);
					foreach (GameLogic.Unit oldEnemy in kvp.Key.Enemies)
						kvp.Value.Enemies.Add(oldUnitToNew[oldEnemy]);
				}

				newUnits = oldUnitToNew.Values;
			}

			//Position the units.
			UnityEngine.Assertions.Assert.IsTrue(entranceSpaces.Count > 0);
			int unitsPerSpace = newUnits.Count / entranceSpaces.Count;
			int unitI = 0,
				posI = 0;
			foreach (Unit unit in newUnits)
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
