﻿using System;
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

			//Make sure we don't accidentally spawn the GameFSM object while shutting down.
			if (UnityLogic.GameFSM.InstanceExists)
			{
				FSM.Map.OnMapCleared -= Callback_MapCleared;
				FSM.Map.Units.OnElementAdded -= Callback_NewUnit;
				FSM.Map.Units.OnElementRemoved -= Callback_UnitDies;
				FSM.Map.Units.OnUnitMoved -= Callback_UnitMoves;
			}
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
			ContentUI.Instance.CreateUnitWindow(u);
			Callback_Button_Close();
		}

		private void AddUnit(GameLogic.Unit u)
		{
			Target.Add(u);

			//Get a display name for the given type.
			string typeName;
			switch (u.MyType)
			{
				case GameLogic.Unit.Types.TestChar: typeName = "Test Char"; break;
				case GameLogic.Unit.Types.TestStructure: typeName = "Test Structure"; break;
				case GameLogic.Unit.Types.PlayerChar: typeName = "Player Char"; break;
				default: throw new NotImplementedException(u.MyType.ToString());
			}

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
