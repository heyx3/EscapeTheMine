using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_TestChar : Window<GameLogic.Units.TestChar>
	{
		public string FoodPrefix = "Food: ",
					  FoodSuffix = "";

		public UnityEngine.UI.Text Label_FoodValue;

		
		private void Start()
		{
			Target.OnFoodChanged += OnFoodChanged;
			OnFoodChanged(Target, Target.Food, Target.Food);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			Target.OnFoodChanged -= OnFoodChanged;
		}
		
		private void OnFoodChanged(GameLogic.Units.TestChar theChar, float oldVal, float newVal)
		{
			Label_FoodValue.text = FoodPrefix + Mathf.RoundToInt(newVal) + FoodSuffix;
		}
	}
}