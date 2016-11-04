using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameLogic;
using GameLogic.Units.Player_Char;


namespace MyUI
{
	public class Window_PlayerChar : Window<GameLogic.Units.PlayerChar>
	{
		private static string FormatFood(float f)
		{
			return Mathf.RoundToInt(f).ToString();
		}
		private static string FormatHealth(float f)
		{
			float maxHealth = Consts.Instance.Max_Health;
			int healthPercent = Mathf.RoundToInt(100.0f * f / maxHealth);
			return healthPercent.ToString() + "%";
		}
		private static string FormatEnergy(float f)
		{
			return Mathf.RoundToInt(f).ToString();
		}
		private static string FormatStrength(float f)
		{
			return string.Format("{0:0.00}", f);
		}


		public Localizer Label_FoodValue, Label_HealthValue,
						 Label_EnergyValue, Label_StrengthValue;


		#region Helper types/field for Editor serialization of tabTypeToObj
		public enum TabTypes
		{
			JobSelection,
			Stats,
		}
		[Serializable]
		private class TabTypeAndObj
		{
			public TabTypes Type;
			public UITab Obj;
			public TabTypeAndObj() { }
			public TabTypeAndObj(TabTypes t, UITab o) { Type = t; Obj = o; }
		}
		[SerializeField]
		private List<TabTypeAndObj> tabs = new List<TabTypeAndObj>()
		{
			new TabTypeAndObj(TabTypes.JobSelection, null),
			new TabTypeAndObj(TabTypes.Stats, null),
		};
		#endregion

		[SerializeField]
		private TabTypes firstTab = TabTypes.Stats;

		private Dictionary<TabTypes, UITab> tabTypeToObj;

		
		public void SwitchToTab(TabTypes type)
		{
			foreach (var kvp in tabTypeToObj)
				if (kvp.Key == type)
					kvp.Value.SelectMe();
				else
					kvp.Value.DeselectMe();
		}

		protected override void Awake()
		{
			base.Awake();

			tabTypeToObj = new Dictionary<TabTypes, UITab>();
			foreach (var tabTypeAndObj in tabs)
			{
				tabTypeToObj.Add(tabTypeAndObj.Type, tabTypeAndObj.Obj);
				tabTypeAndObj.Obj.OnClicked += Callback_TabClicked;
			}
			tabs = null;
		}
		private void Start()
		{
			Target.Food.OnChanged += OnFoodChanged;
			Label_FoodValue.Args = new object[] { FormatFood(Target.Food.Value) };
			OnFoodChanged(Target, Target.Food, Target.Food);

			Target.Health.OnChanged += OnHealthChanged;
			Label_HealthValue.Args = new object[] { FormatHealth(Target.Health.Value) };
			OnHealthChanged(Target, Target.Health, Target.Health);

			Target.Energy.OnChanged += OnEnergyChanged;
			Label_EnergyValue.Args = new object[] { FormatEnergy(Target.Energy.Value) };
			OnEnergyChanged(Target, Target.Energy, Target.Energy);

			Target.Strength.OnChanged += OnStrengthChanged;
			Label_StrengthValue.Args = new object[] { FormatStrength(Target.Strength.Value) };
			OnStrengthChanged(Target, Target.Strength, Target.Strength);

			SwitchToTab(firstTab);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.Food.OnChanged -= OnFoodChanged;
			Target.Health.OnChanged -= OnHealthChanged;
			Target.Energy.OnChanged -= OnEnergyChanged;
			Target.Strength.OnChanged -= OnStrengthChanged;
		}

		private void OnFoodChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_FoodValue.Args[0] = FormatFood(newVal);
			Label_FoodValue.OnValidate();
		}
		private void OnHealthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_HealthValue.Args[0] = FormatHealth(newVal);
			Label_HealthValue.OnValidate();
		}
		private void OnEnergyChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_EnergyValue.Args[0] = FormatEnergy(newVal);
			Label_EnergyValue.OnValidate();
		}
		private void OnStrengthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_StrengthValue.Args[0] = FormatStrength(newVal);
			Label_StrengthValue.OnValidate();
		}

		public void Callback_TabClicked(UITab tab)
		{
			//Find the type of tab that was clicked on and select it.
			foreach (var kvp in tabTypeToObj)
				if (kvp.Value == tab)
				{
					SwitchToTab(kvp.Key);
					return;
				}

			throw new ArgumentException(tab.name);
		}

		public void Callback_NewJob_MoveToPos()
		{
			//Ask the player to select a tile to move to.
			var data = new Window_SelectTile.TileSelectionData(
				(tilePos) =>
				{
					if (tilePos.HasValue)
						Target.AddJob(new Job_MoveToPos(tilePos.Value, false, FSM.Map)); //TODO: Optionally make it an emergency.
				},
				(tilePos) => { return !FSM.Map.Tiles[tilePos].BlocksMovement(); },
				"WINDOW_MOVETOPOS_TITLE", "WINDOW_MOVETOPOS_MESSAGE");

			var wnd = ContentUI.Instance.CreateWindow(ContentUI.Instance.Window_SelectTile, data);
		}
	}
}