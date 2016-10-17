﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_TestStructure : Window<GameLogic.Units.TestStructure>
	{
		public string FoodPrefix = "Food: ",
					  FoodSuffix = "";

		public UnityEngine.UI.Text Label_FoodValue;


		protected void Start()
		{
			Target.OnFoodChanged += OnFoodChanged;
			OnFoodChanged(Target, Target.Food, Target.Food);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			Target.OnFoodChanged -= OnFoodChanged;
		}

		private void OnFoodChanged(GameLogic.Units.TestStructure theChar, float oldVal, float newVal)
		{
			Label_FoodValue.text = FoodPrefix + Mathf.RoundToInt(newVal) + FoodSuffix;
		}
	}
}