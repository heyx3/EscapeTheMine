using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
			float maxHealth = GameLogic.Units.Player_Char.Consts.Instance.Max_Health;
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
	}
}