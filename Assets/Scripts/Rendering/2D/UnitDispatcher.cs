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
		public static UnitDispatcher Instance { get; private set; }


		/// <summary>
		/// The sorting layers of the unit sprites get cycled regularly
		///     so that all sprites can be visible.
		/// </summary>
		public int MinSortLayer = 1,
				   MaxSortLayer = 10;
		public float WaitBetweenCycles = 0.5f; //TODO: Tie the wait between cycles to the number of active grid cells.

		public GameObject UnitPrefab_PlayerChar, UnitPrefab_Bed;
		
		[Serializable]
		public class SpriteSet_PlayerChar
		{
			public Sprite Idle;
		}
		public SpriteSet_PlayerChar[] Sprites_PlayerChar = new SpriteSet_PlayerChar[1];

		
		private Dictionary<Unit, SpriteRenderer> unitToSprite = new Dictionary<Unit, SpriteRenderer>();
		private Coroutine spriteCycler;


		protected override void Awake()
		{
			base.Awake();
			Instance = this;
		}
		private void OnEnable()
		{
			Map.OnUnitAdded += Callback_UnitAddedToMap;
			Map.OnUnitRemoved += Callback_UnitRemovedFromMap;

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
			}

			StopCoroutine(spriteCycler);
		}

		private System.Collections.IEnumerator RunSpriteCycle()
		{
			List<Vector2i> activeTiles = new List<Vector2i>();
			var waitAfterEach = new WaitForSeconds(WaitBetweenCycles);

			while (true)
			{
				//Get the cells to cycle through.
				activeTiles.Clear();
				foreach (Vector2i tilePos in Map.GetTilesWithUnits())
					activeTiles.Add(tilePos);

				foreach (Vector2i tilePos in activeTiles)
				{
					//Only cycle sprites in cells with at least two units.
					bool cycle = false,
						 foundFirst = false;
					foreach (var unit in Map.GetUnits(tilePos))
					{
						if (foundFirst)
						{
							cycle = true;
							break;
						}
						else
						{
							foundFirst = true;
						}
					}

					if (cycle)
					{
						//Find the number of units, and the unit that's currently active.
						int count = 0;
						int activeUnitI = -1;
						foreach (Unit u in Map.GetUnits(tilePos))
						{
							if (unitToSprite[u].sortingOrder == MaxSortLayer)
								activeUnitI = count;
							count += 1;
						}

						if (count == 0)
							continue;

						//Cycle the sprite layers.
						activeUnitI = (activeUnitI + 1) % count;
						int i = 0;
						foreach (Unit u in Map.GetUnits(tilePos))
						{
							unitToSprite[u].sortingOrder = (i == activeUnitI) ?
														       MaxSortLayer :
															   MinSortLayer;
							i += 1;
						}
						
						yield return waitAfterEach;
					}
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
		}
		private void Callback_UnitRemovedFromMap(Map map, Unit unit)
		{
			Destroy(unitToSprite[unit].gameObject);
			unitToSprite.Remove(unit);
		}
	}
}