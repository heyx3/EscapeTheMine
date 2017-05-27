using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLogic;


namespace Rendering.TwoD
{
	/// <summary>
	/// Whever a unit is created, spawns a GameObject to represent it.
	/// Also handles displaying multiple units that share a tile.
	/// </summary>
	public class UnitDispatcher : RendererComponent
	{
		/// <summary>
		/// The sorting layers of the unit sprites get cycled regularly
		///     so that all sprites can be visible.
		/// </summary>
		public int MinSortLayer = 1,
				   MaxSortLayer = 10;

		//TODO: Tie the wait between cycles to the number of active grid cells.
		public float WaitBetweenCycles = 0.5f;

		public GameObject UnitPrefab_PlayerChar, UnitPrefab_Bed;


		private Vector2i.Iterator posCycler;
		private Dictionary<Unit, SpriteRenderer> unitToSprite = new Dictionary<Unit, SpriteRenderer>();

		private GridSet<Unit> unitsByPos;
		private Coroutine spriteCycler;

		
		private void OnEnable()
		{
			unitsByPos = new GridSet<Unit>(Map.Tiles.Dimensions);

			posCycler = new Vector2i.Iterator(Map.Tiles.Dimensions);
			posCycler.MoveNext();

			Map.OnUnitAdded += Callback_UnitAddedToMap;
			Map.OnUnitRemoved += Callback_UnitRemovedFromMap;
			Map.Tiles.OnTileGridReset += Callback_TileGridReset;

			//"Add" the units that already exist.
			foreach (Unit u in Map.GetUnits())
				Callback_UnitAddedToMap(Map, u);

			spriteCycler = StartCoroutine(RunSpriteCycle());
		}
		private void OnDisable()
		{
			//Make sure we don't accidentally spawn the EtmGame script while shutting down.
			if (UnityLogic.EtMGame.InstanceExists)
			{
				//"Remove" all units.
				foreach (Unit u in Map.GetUnits())
					Callback_UnitRemovedFromMap(Map, u);

				Map.OnUnitAdded -= Callback_UnitAddedToMap;
				Map.OnUnitRemoved -= Callback_UnitRemovedFromMap;
				Map.Tiles.OnTileGridReset -= Callback_TileGridReset;
			}

			StopCoroutine(spriteCycler);
		}

		private System.Collections.IEnumerator RunSpriteCycle()
		{
			//Only cycle sprites in cells with at least two units.
			Func<Vector2i, bool> isCellCycleable = (tilePos) =>
				unitsByPos.Count(tilePos) > 1;

			List<Vector2i> toCycleThrough = new List<Vector2i>();
			var waitAfterEach = new WaitForSeconds(WaitBetweenCycles);

			int activeUnitI = -1;
			int count = -1;
			Action<Unit, int> countUnit = (unit, unitI) =>
			{
				count += 1;
				if (unitToSprite[unit].sortingOrder == MaxSortLayer)
					activeUnitI = unitI;
			};
			Action<Unit, int> cycleSprite = (unit, unitI) =>
			{
				unitToSprite[unit].sortingOrder = (unitI == activeUnitI) ?
												      MaxSortLayer :
												      MinSortLayer;
			};

			while (true)
			{
				//Get the cells to cycle through.
				toCycleThrough.Clear();
				toCycleThrough.AddRange(unitsByPos.ActiveCells.Where(isCellCycleable));
				
				foreach (Vector2i tilePos in toCycleThrough)
				{
					//Find the number of units, and the unit that's currently active.
					count = 0;
					activeUnitI = -1;
					unitsByPos.ForEach(tilePos, countUnit);

					if (count == 0)
						continue;
					
					//Cycle the sprite layers.
					activeUnitI = (activeUnitI + 1) % count;
					unitsByPos.ForEach(tilePos, cycleSprite);

					yield return waitAfterEach;
				}

				//Prevent infinite loops if there's nothing to cycle through.
				yield return null;
			}
		}


		private void Callback_UnitAddedToMap(Map map, Unit unit)
		{
			SpriteRenderer spr;
			switch (unit.MyType)
			{
				case Unit.Types.PlayerChar: {
						var obj = Instantiate(UnitPrefab_PlayerChar);
						var rend = obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.PlayerChar>>();
						rend.Target = (GameLogic.Units.PlayerChar)unit;
						spr = rend.MySprite;
					} break;
				case Unit.Types.Bed: {
						var obj = Instantiate(UnitPrefab_Bed);
						var rend = obj.GetComponentInChildren<UnitRenderer<GameLogic.Units.Bed>>();
						rend.Target = (GameLogic.Units.Bed)unit;
						spr = rend.MySprite;
					} break;

				default: throw new NotImplementedException(unit.GetType().Name);
			}
			unitToSprite.Add(unit, spr);

			unitsByPos.AddValue(unit, unit.Pos);
			unit.Pos.OnChanged += Callback_UnitPosChanged;
		}
		private void Callback_UnitRemovedFromMap(Map map, Unit unit)
		{
			Destroy(unitToSprite[unit].gameObject);
			unitToSprite.Remove(unit);

			unit.Pos.OnChanged -= Callback_UnitPosChanged;
			unitsByPos.RemoveValue(unit, unit.Pos);
		}

		private void Callback_TileGridReset(TileGrid grid, Vector2i oldSize, Vector2i newSize)
		{
			unitsByPos = new GridSet<Unit>(newSize);
			foreach (Unit u in unitToSprite.Keys)
				unitsByPos.AddValue(u, u.Pos);

			posCycler = new Vector2i.Iterator(newSize);
			posCycler.MoveNext();
		}

		private void Callback_UnitPosChanged(Unit u, Vector2i oldPos, Vector2i newPos)
		{
			unitsByPos.MoveValue(u, oldPos, newPos);
		}
	}
}