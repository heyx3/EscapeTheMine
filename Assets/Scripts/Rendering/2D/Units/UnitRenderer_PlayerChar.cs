using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_PlayerChar : UnitRenderer<GameLogic.Units.PlayerChar>
	{
		public Sprite NormalSprite;
        public float LowFoodThreshold = 30.0f; //TODO: Make this a setting in the unit itself.

        private BlinkSprite lowFoodWarning;


        protected override void Awake()
        {
            base.Awake();

            GameObject lowFoodGO = Instantiate(Content2D.Instance.Prefab_HungerWarning);
            lowFoodWarning = lowFoodGO.GetComponent<BlinkSprite>();

            Transform lowFoodTr = lowFoodGO.transform;
            lowFoodTr.SetParent(MyTr, false);
            lowFoodTr.localPosition = Vector3.zero;
            lowFoodTr.localScale = Vector3.one;
            lowFoodTr.localRotation = Quaternion.identity;
        }
        protected override void Start()
		{
			base.Start();

			MySprite.sprite = NormalSprite;

			Target.Pos.OnChanged += Callback_PosChanged;
			Callback_PosChanged(Target, Vector2i.Zero, Target.Pos);
			
            Target.Food.OnChanged += Callback_FoodChanged;
            Callback_FoodChanged(Target, 0.0f, Target.Food);
			
            Target.Health.OnChanged += Callback_HealthChanged;
            Callback_HealthChanged(Target, 0.0f, Target.Health);
			
            Target.Energy.OnChanged += Callback_EnergyChanged;
            Callback_EnergyChanged(Target, 0.0f, Target.Energy);
			
            Target.Strength.OnChanged += Callback_StrengthChanged;
            Callback_StrengthChanged(Target, 0.0f, Target.Strength);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.Pos.OnChanged -= Callback_PosChanged;
            Target.Food.OnChanged -= Callback_FoodChanged;
            Target.Health.OnChanged -= Callback_HealthChanged;
            Target.Energy.OnChanged -= Callback_EnergyChanged;
            Target.Strength.OnChanged -= Callback_StrengthChanged;
        }

		private void Callback_PosChanged(GameLogic.Unit u, Vector2i oldPos, Vector2i newPos)
		{
			MyTr.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, MyTr.position.z);
		}
        private void Callback_FoodChanged(GameLogic.Units.PlayerChar u, float oldFood, float newFood)
        {
            lowFoodWarning.gameObject.SetActive(newFood <= LowFoodThreshold);
        }
        private void Callback_HealthChanged(GameLogic.Units.PlayerChar u, float oldFood, float newFood)
        {

        }
        private void Callback_EnergyChanged(GameLogic.Units.PlayerChar u, float oldFood, float newFood)
        {

        }
        private void Callback_StrengthChanged(GameLogic.Units.PlayerChar u, float oldFood, float newFood)
        {

        }
	}
}