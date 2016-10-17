using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_TestChar : UnitRenderer<GameLogic.Units.TestChar>
	{
		public Sprite NormalSprite;
        public float LowFoodThreshold = 30.0f;

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

			Target.OnPosChanged += Callback_PosChanged;
			Callback_PosChanged(Target, Vector2i.Zero, Target.Pos);

            Target.OnFoodChanged += Callback_FoodChanged;
            Callback_FoodChanged(Target, 0.0f, Target.Food);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.OnPosChanged -= Callback_PosChanged;
            Target.OnFoodChanged -= Callback_FoodChanged;
        }

		private void Callback_PosChanged(GameLogic.Unit u, Vector2i oldPos, Vector2i newPos)
		{
			MyTr.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, MyTr.position.z);
		}
        private void Callback_FoodChanged(GameLogic.Units.TestChar u, float oldFood, float newFood)
        {
            lowFoodWarning.gameObject.SetActive(newFood <= LowFoodThreshold);
        }
	}
}