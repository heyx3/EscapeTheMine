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

		public UlongSet SleepingUnitsByID = new UlongSet();


		public Bed(Map theMap, ulong groupID, Vector2i pos)
			: base(theMap, groupID, pos)
		{
			
		}
		public Bed(Map theMap) : this(theMap, ulong.MaxValue, new Vector2i(-1, -1)) { }


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

			//TODO: Chance to reproduce for everybody in the bed.

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
