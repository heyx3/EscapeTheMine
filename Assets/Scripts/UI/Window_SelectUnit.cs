using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_SelectUnit : Window<IList<GameLogic.Unit>>
	{
		public Transform ContentParent;
		public GameObject OptionButtonPrefab;

		public Vector2i TilePos { get; private set; }


		protected override void Awake()
		{
			base.Awake();

			UnityEngine.Assertions.Assert.IsTrue(Target.Count > 0, "No items to select from");
			TilePos = Target.First().Pos;

			FSM.Map.OnMapCleared += Callback_MapCleared;
			FSM.Map.Units.OnElementAdded += Callback_NewUnit;
			FSM.Map.Units.OnElementRemoved += Callback_UnitDies;
			FSM.Map.Units.OnUnitMoved += Callback_UnitMoves;
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (!UnityLogic.GameFSM.InstanceExists)
				return;

			FSM.Map.OnMapCleared -= Callback_MapCleared;
			FSM.Map.Units.OnElementAdded -= Callback_NewUnit;
			FSM.Map.Units.OnElementRemoved -= Callback_UnitDies;
			FSM.Map.Units.OnUnitMoved -= Callback_UnitMoves;
		}

		public void Callback_NewUnit(LockedSet<GameLogic.Unit> units, GameLogic.Unit unit)
		{
			UnityEngine.Assertions.Assert.IsFalse(Target.Contains(unit));

			if (unit.Pos == TilePos)
				AddUnit(unit);
		}
		public void Callback_UnitDies(LockedSet<GameLogic.Unit> units, GameLogic.Unit unit)
		{
			if (Target.Contains(unit))
				RemoveUnit(unit);
		}
		public void Callback_UnitMoves(GameLogic.UnitSet units, GameLogic.Unit unit, Vector2i oldPos, Vector2i newPos)
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

		public void Callback_MapCleared(GameLogic.Map map)
		{
			Callback_Button_Close();
		}

		public void Callback_UnitSelected(GameLogic.Unit u)
		{
			if (u is GameLogic.Units.TestChar)
				ContentUI.Instance.CreateWindowFor(ContentUI.Instance.Window_TestChar,
												   (GameLogic.Units.TestChar)u);
			else if (u is GameLogic.Units.TestStructure)
				ContentUI.Instance.CreateWindowFor(ContentUI.Instance.Window_TestStructure,
												   (GameLogic.Units.TestStructure)u);
			else
				throw new NotImplementedException("Unknown type " + u.GetType().Name);

			Callback_Button_Close();
		}

		private void AddUnit(GameLogic.Unit u)
		{
			Target.Add(u);

			//Get a display name for the given type.
			string typeName;
			if (u is GameLogic.Units.TestChar)
				typeName = "Test Char";
			else if (u is GameLogic.Units.TestStructure)
				typeName = "Test Structure";
			else
				throw new NotImplementedException("Unknown unit type: " + u.GetType().FullName);

			//Set up the button for selecting this option.
			Transform button = Instantiate(OptionButtonPrefab).transform;
			button.SetParent(ContentParent, false);
			button.GetComponentInChildren<UnityEngine.UI.Text>().text = typeName;
			button.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(
				() => RemoveUnit(u));
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
