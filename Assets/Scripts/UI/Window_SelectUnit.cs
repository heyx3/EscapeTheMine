using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_SelectUnit : Window<List<GameLogic.Unit>>
	{
		public Transform ContentParent;
		public GameObject OptionButtonPrefab;

		public Vector2i TilePos { get; private set; }


		protected override void Awake()
		{
			base.Awake();

			Game.Map.OnUnitAdded += Callback_NewUnit;
			Game.Map.OnUnitRemoved += Callback_UnitDies;
			Game.Map.OnUnitMoved += Callback_UnitMoves;
		}
        private void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(Target.Count > 0, "No items to select from");
            TilePos = Target.First().Pos;

            //Do the necessary things to initialize each targeted unit.
            List<GameLogic.Unit> units = new List<GameLogic.Unit>(Target);
            Target.Clear();
            foreach (GameLogic.Unit u in units)
                AddUnit(u);
        }
		protected override void OnDestroy()
		{
			base.OnDestroy();

			//Make sure we don't accidentally spawn the EtMGame object while shutting down.
			if (UnityLogic.EtMGame.InstanceExists)
			{
				Game.Map.OnUnitAdded -= Callback_NewUnit;
				Game.Map.OnUnitRemoved -= Callback_UnitDies;
				Game.Map.OnUnitMoved -= Callback_UnitMoves;
			}
		}

		public void Callback_NewUnit(GameLogic.Map theMap, GameLogic.Unit unit)
		{
			UnityEngine.Assertions.Assert.IsFalse(Target.Contains(unit));

			if (unit.Pos == TilePos)
				AddUnit(unit);
		}
		public void Callback_UnitDies(GameLogic.Map theMap, GameLogic.Unit unit)
		{
			if (Target.Contains(unit))
				RemoveUnit(unit);
		}
		public void Callback_UnitMoves(GameLogic.Map theMap, GameLogic.Unit unit, Vector2i oldPos, Vector2i newPos)
		{
			if (oldPos == TilePos && newPos != TilePos)
			{
				UnityEngine.Assertions.Assert.IsTrue(Target.Contains(unit));
				RemoveUnit(unit);
			}
			else if (oldPos != TilePos && newPos == TilePos)
			{
				UnityEngine.Assertions.Assert.IsFalse(Target.Contains(unit));
				AddUnit(unit);
			}
		}

		protected override void Callback_MapChanging()
		{
			base.Callback_MapChanging();
			Callback_Button_Close();
		}

		public void Callback_UnitSelected(GameLogic.Unit u)
		{
			ContentUI.Instance.CreateUnitWindow(u);
			Callback_Button_Close();
		}

		private void AddUnit(GameLogic.Unit u)
		{
			Target.Add(u);

			//Set up the button for selecting this option.
			Transform button = Instantiate(OptionButtonPrefab).transform;
			button.SetParent(ContentParent, false);
			button.GetComponentInChildren<UnityEngine.UI.Text>().text = u.DisplayName;
			button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(
				() => Callback_UnitSelected(u));
		}
		private void RemoveUnit(GameLogic.Unit u)
		{
			int i = Target.IndexOf(u);
			Target.RemoveAt(i);

			Destroy(ContentParent.GetChild(i).gameObject);

			if (Target.Count < 1)
				Callback_Button_Close();
		}
	}
}
