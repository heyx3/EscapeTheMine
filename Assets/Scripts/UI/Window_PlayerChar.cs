using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_PlayerChar : Window<GameLogic.Units.PlayerChar>
	{
		public Localizer Label_FoodValue, Label_HealthValue,
						 Label_EnergyValue, Label_StrengthValue;

		
		private void Start()
		{
			Target.Food.OnChanged += OnFoodChanged;
			Label_FoodValue.Args = new object[] { Target.Food };
			OnFoodChanged(Target, Target.Food, Target.Food);

			Target.Health.OnChanged += OnHealthChanged;
			Label_HealthValue.Args = new object[] { Target.Health };
			OnHealthChanged(Target, Target.Health, Target.Health);

			Target.Energy.OnChanged += OnEnergyChanged;
			Label_EnergyValue.Args = new object[] { Target.Energy };
			OnEnergyChanged(Target, Target.Energy, Target.Energy);

			Target.Strength.OnChanged += OnStrengthChanged;
			Label_StrengthValue.Args = new object[] { Target.Strength };
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
			int newValI = Mathf.RoundToInt(newVal);
			if (newValI != (int)Label_FoodValue.Args[0])
			{
				Label_FoodValue.Args[0] = newValI;
				Label_FoodValue.OnValidate();
			}
		}
		private void OnHealthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			int newValI = Mathf.RoundToInt(newVal);
			if (newValI != (int)Label_HealthValue.Args[0])
			{
				Label_HealthValue.Args[0] = newValI;
				Label_HealthValue.OnValidate();
			}
		}
		private void OnEnergyChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			int newValI = Mathf.RoundToInt(newVal);
			if (newValI != (int)Label_EnergyValue.Args[0])
			{
				Label_EnergyValue.Args[0] = newValI;
				Label_EnergyValue.OnValidate();
			}
		}
		private void OnStrengthChanged(GameLogic.Unit theChar, float oldVal, float newVal)
		{
			int newValI = Mathf.RoundToInt(newVal);
			if (newValI != (int)Label_StrengthValue.Args[0])
			{
				Label_StrengthValue.Args[0] = newValI;
				Label_StrengthValue.OnValidate();
			}
		}
	}
}