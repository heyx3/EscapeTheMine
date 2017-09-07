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

		#region Sprite data

		[Serializable]
		public class Data_PlayerChar
		{
			[Serializable]
			public class SpriteSet
			{
				public Sprite Idle, Hurt, Attacking;
			}
			public SpriteSet[] Sprites;

			public float SpriteTime_Hurt = 0.25f;
			public float SpriteTime_Attack = 0.1f;

			public SpriteSet GetSet(int appearanceIndex) { return Sprites[appearanceIndex % Sprites.Length]; }
			public System.Collections.IEnumerator PlayAnim_Hurt(int appearanceIndex,
																SpriteRenderer rnd)
			{
				var set = GetSet(appearanceIndex);
				rnd.sprite = set.Hurt;
				yield return new WaitForSeconds(SpriteTime_Hurt);
				rnd.sprite = set.Idle;
			}
			public System.Collections.IEnumerator PlayAnim_Attack(int appearanceIndex,
																  SpriteRenderer rnd)
			{
				var set = GetSet(appearanceIndex);
				rnd.sprite = set.Attacking;
				yield return new WaitForSeconds(SpriteTime_Attack);
				rnd.sprite = set.Idle;
			}
		}
		public Data_PlayerChar PlayerChar;

		[Serializable]
		public class Data_LizardMan
		{
			public Sprite Spr_Idle, Spr_Hurt, Spr_Attacking;

			public float SpriteTime_Hurt = 0.25f;
			public float SpriteTime_Attack = 0.1f;

			public System.Collections.IEnumerator PlayAnim_Hurt(SpriteRenderer rnd)
			{
				rnd.sprite = Spr_Hurt;
				yield return new WaitForSeconds(SpriteTime_Hurt);
				rnd.sprite = Spr_Idle;
			}
			public System.Collections.IEnumerator PlayAnim_Attack(SpriteRenderer rnd)
			{
				rnd.sprite = Spr_Attacking;
				yield return new WaitForSeconds(SpriteTime_Attack);
				rnd.sprite = Spr_Idle;
			}
		}
		public Data_LizardMan LizardMan;

		#endregion

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