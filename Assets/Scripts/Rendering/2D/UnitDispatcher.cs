using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using UnityEngine;


namespace Rendering.TwoD
{
	/// <summary>
	/// Whever a unit is created, spawns a GameObject to represent it.
	/// </summary>
	public class UnitDispatcher : RendererComponent
	{
		public GameObject UnitPrefab_TestChar, UnitPrefab_TestStructure,
						  UnitPrefab_PlayerChar;

		private Dictionary<GameLogic.Unit, GameObject> unitToObj = new Dictionary<Unit, GameObject>();

		
		private void OnEnable()
		{
			GameFSM.Map.Units.OnElementAdded += UnitAddedToMap;
			GameFSM.Map.Units.OnElementRemoved += UnitRemovedFromMap;
		}
		private void OnDisable()
		{
			GameFSM.Map.Units.OnElementAdded -= UnitAddedToMap;
			GameFSM.Map.Units.OnElementRemoved -= UnitRemovedFromMap;
		}

		protected void UnitAddedToMap(LockedSet<Unit> collection, Unit unit)
		{
			GameObject obj;
			switch (unit.MyType)
			{
				case Unit.Types.TestChar:
					{
						obj = Instantiate(UnitPrefab_TestChar);
						obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.TestChar>>().Target =
							(GameLogic.Units.TestChar)unit;
					} break;
				case Unit.Types.TestStructure:
					{
						obj = Instantiate(UnitPrefab_TestStructure);
						obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.TestStructure>>().Target =
							(GameLogic.Units.TestStructure)unit;
					} break;
				case Unit.Types.PlayerChar:
					{
						obj = Instantiate(UnitPrefab_PlayerChar);
						obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.PlayerChar>>().Target =
							(GameLogic.Units.PlayerChar)unit;
					} break;
				default:
					{
						throw new NotImplementedException(unit.GetType().Name);
					} break;
			}
			unitToObj.Add(unit, obj);
		}
		protected void UnitRemovedFromMap(LockedSet<Unit> collection, Unit unit)
		{
			Destroy(unitToObj[unit]);
			unitToObj.Remove(unit);
		}
	}
}