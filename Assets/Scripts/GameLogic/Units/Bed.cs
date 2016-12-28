using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyData;

namespace GameLogic.Units
{
	public class Bed : Unit
	{
		public override string DisplayName { get { return Localization.Get("WINDOW_SELECTUNIT_BED"); } }

		/// <summary>
		/// Raised when a male/female pair made a baby in this bed.
		/// The PlayerChar arguments are as follows:
		///     1. The father.
		///     2. The mother.
		///     3. The baby.
		/// </summary>
		public event Action<Bed, PlayerChar, PlayerChar, PlayerChar> OnMakeBaby;

		public UlongSet SleepingUnitsByID = new UlongSet();


		public Bed(Map theMap, ulong groupID, Vector2i pos)
			: base(theMap, groupID, pos)
		{
			//Keep track of when sleeping units die.
			SleepingUnitsByID.OnElementAdded += (ids, id) =>
			{
				TheMap.GetUnit(id).OnKilled += Callback_SleeperKilled;
			};
			SleepingUnitsByID.OnElementRemoved += (ids, id) =>
			{
				TheMap.GetUnit(id).OnKilled -= Callback_SleeperKilled;
			};
		}
		public Bed(Map theMap) : this(theMap, ulong.MaxValue, new Vector2i(-1, -1)) { }


		private void Callback_SleeperKilled(Unit sleeper, Map theMap)
		{
			SleepingUnitsByID.Remove(sleeper.ID);
		}

		public override IEnumerable TakeTurn()
		{
			//Gain stats for everybody in bed.
			float energyImprovement =
					  Player_Char.Consts.EnergyIncreaseFromBedPerTurn(SleepingUnitsByID.Count),
				  healthImprovement =
					  Player_Char.Consts.HealthIncreaseFromBedPerTurn(SleepingUnitsByID.Count);
			foreach (ulong unitID in SleepingUnitsByID)
			{
				var pChar = (PlayerChar)TheMap.GetUnit(unitID);
				pChar.Energy.Value += energyImprovement;
				pChar.Health.Value += healthImprovement;
			}

			//Count the number of male/female pairs.
			int nMales = 0,
				nFemales = 0;
			foreach (Unit unit in SleepingUnitsByID.Select(id => TheMap.GetUnit(id)))
			{
				switch (((PlayerChar)unit).Personality.Gender.Value)
				{
					case Player_Char.Personality.Genders.Male:
						nMales += 1;
						break;
					case Player_Char.Personality.Genders.Female:
						nFemales += 1;
						break;
					default:
						throw new NotImplementedException(((PlayerChar)unit).Personality.Gender.ToString());
				}
			}
			int nPairs = Math.Min(nMales, nFemales);

			//For each pair, give them a chance to make a baby.
			for (int i = 0; i < nPairs; ++i)
			{
				if (UnityEngine.Random.value < Player_Char.Consts.ReproductionChance)
				{
					//Get the father and mother.
					ulong fatherID = SleepingUnitsByID.Where(id =>
					{
						PlayerChar pc = (PlayerChar)TheMap.GetUnit(id);
						return pc.Personality.Gender.Value == Player_Char.Personality.Genders.Male;
					}).ElementAt(i);
					ulong motherID = SleepingUnitsByID.Where(id =>
					{
						PlayerChar pc = (PlayerChar)TheMap.GetUnit(id);
						return pc.Personality.Gender.Value == Player_Char.Personality.Genders.Female;
					}).ElementAt(i);

					PlayerChar father = (PlayerChar)TheMap.GetUnit(fatherID),
							   mother = (PlayerChar)TheMap.GetUnit(motherID);
					
					//Create the baby.
					var gender = (UnityEngine.Random.value > 0.5f ?
									  Player_Char.Personality.Genders.Male :
									  Player_Char.Personality.Genders.Female);
					string name =
						Player_Char.Personality.GenerateName(gender,
															 UnityEngine.Random.Range(0, int.MaxValue));
					PlayerChar baby = new PlayerChar(TheMap, father.MyGroupID,
													 0.0f, 0.0f, 0.0f, //TODO: Choose stats.
													 name, gender, 0); //TODO: How about UnitRenderer_PlayerChar chooses an appearance index once it's hooked up to the unit?
					TheMap.AddUnit(baby);

					if (OnMakeBaby != null)
						OnMakeBaby(this, father, mother, baby);
				}
			}

			yield break;
		}

		public override Types MyType { get { return Types.Bed; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);

			writer.Collection<ulong, UlongSet>(SleepingUnitsByID, "sleepingUnits",
											   (wr, inVal, name) => wr.UInt64(inVal, name));
		}
		public override void ReadData(MyData.Reader reader)
		{
			base.ReadData(reader);

			SleepingUnitsByID.Clear();
			reader.Collection("sleepingUnits",
							  (MyData.Reader r, ref ulong outVal, string name) =>
							      { outVal = r.UInt64(name); },
							  (capacity) => SleepingUnitsByID);
		}
	}
}
