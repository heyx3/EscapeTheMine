using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLogic;


namespace Rendering.TwoD
{
	/// <summary>
	/// Whever a unit is created, spawns a GameObject to represent it.
	/// </summary>
	public class UnitDispatcher : RendererComponent
	{
		public GameObject UnitPrefab_PlayerChar, UnitPrefab_Bed;

		private Dictionary<Unit, GameObject> unitToObj = new Dictionary<Unit, GameObject>();

		
		private void OnEnable()
		{
			Game.Map.OnUnitAdded += UnitAddedToMap;
			Game.Map.OnUnitRemoved += UnitRemovedFromMap;
		}
		private void OnDisable()
		{
			//Make sure we don't accidentally spawn the EtmGame object while shutting down.
			if (UnityLogic.EtMGame.InstanceExists)
			{
				Game.Map.OnUnitAdded -= UnitAddedToMap;
				Game.Map.OnUnitRemoved -= UnitRemovedFromMap;
			}
		}

		private void UnitAddedToMap(Map map, Unit unit)
		{
			GameObject obj;
			switch (unit.MyType)
			{
				case Unit.Types.PlayerChar:
					{
						obj = Instantiate(UnitPrefab_PlayerChar);
						obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.PlayerChar>>().Target =
							(GameLogic.Units.PlayerChar)unit;
					} break;
				case Unit.Types.Bed:
					{
						obj = Instantiate(UnitPrefab_Bed);
						obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.Bed>>().Target =
							(GameLogic.Units.Bed)unit;
					} break;

				default: throw new NotImplementedException(unit.GetType().Name);
			}
			unitToObj.Add(unit, obj);
		}
		private void UnitRemovedFromMap(Map map, Unit unit)
		{
			Destroy(unitToObj[unit]);
			unitToObj.Remove(unit);
		}
	}
}