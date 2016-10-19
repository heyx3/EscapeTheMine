using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_PlayerChar : Window<GameLogic.Units.PlayerChar>
	{
		public UnityEngine.UI.Text Label_FoodValue, Label_HealthValue,
								   Label_EnergyValue, Label_StrengthValue;

		
		private void Start()
		{
			Target.Food.OnChanged += OnFoodChanged;
			OnFoodChanged(Target, Target.Food, Target.Food);

			Target.Health.OnChanged += OnHealthChanged;
			OnHealthChanged(Target, Target.Health, Target.Health);

			Target.Energy.OnChanged += OnEnergyChanged;
			OnEnergyChanged(Target, Target.Energy, Target.Energy);

			Target.Strength.OnChanged += OnStrengthChanged;
			OnStrengthChanged(Target, Target.Strength, Target.Strength);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.Food.OnChanged -= OnFoodChanged;
		}
		
		private void OnFoodChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_FoodValue.text = "Food: " + Mathf.RoundToInt(newVal);
		}
		private void OnHealthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_HealthValue.text = "Health: " + Mathf.RoundToInt(newVal);
		}
		private void OnEnergyChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_EnergyValue.text = "Energy: " + Mathf.RoundToInt(newVal);
		}
		private void OnStrengthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			Label_StrengthValue.text = "Strength: " + Mathf.RoundToInt(newVal);
		}
	}
}