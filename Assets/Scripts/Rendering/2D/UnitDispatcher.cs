using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;


namespace Rendering.TwoD
{
	/// <summary>
	/// Spawns GameObjects that represent units in the map.
	/// </summary>
	public class UnitDispatcher : RendererComponent
	{
		public GameObject UnitPrefab_TestChar, UnitPrefab_TestStructure;

		private Dictionary<GameLogic.Unit, GameObject> unitToObj = new Dictionary<Unit, GameObject>();


		protected override void UnitAddedToMap(LockedSet<Unit> collection, Unit unit)
		{
			base.UnitAddedToMap(collection, unit);

			GameObject obj;
			if (unit is GameLogic.Units.TestChar)
			{
				obj = Instantiate(UnitPrefab_TestChar);
				obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.TestChar>>().Target =
					(GameLogic.Units.TestChar)unit;
			}
			else if (unit is GameLogic.Units.TestStructure)
			{
				obj = Instantiate(UnitPrefab_TestStructure);
				obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.TestStructure>>().Target =
					(GameLogic.Units.TestStructure)unit;
			}
			else
			{
				throw new NotImplementedException(unit.GetType().Name);
			}

			unitToObj.Add(unit, obj);
		}
		protected override void UnitRemovedFromMap(LockedSet<Unit> collection, Unit unit)
		{
			base.UnitRemovedFromMap(collection, unit);

			Destroy(unitToObj[unit]);
			unitToObj.Remove(unit);
		}
	}
}