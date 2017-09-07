using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using PlayerConsts = GameLogic.Units.Player_Char.Consts;


namespace Rendering.TwoD
{
	public class UnitRenderer_PlayerChar : UnitRenderer<GameLogic.Units.PlayerChar>
	{
		//TODO: Callbacks for taking damage/dealing damage. First have to set up a cross-type damage system.

		private UnitDispatcher.Data_PlayerChar spriteData { get { return UnitDispatcher.Instance.PlayerChar; } }

		private Coroutine animCoroutine = null;
        private BlinkSprite lowFoodWarning;


		private void ClearAnims()
		{
			if (animCoroutine != null)
			{
				StopCoroutine(animCoroutine);
				animCoroutine = null;
				MySprite.sprite = spriteData.GetSet(Target.Personality.AppearanceIndex).Idle;
			}
		}
		private void Play_Hurt()
		{
			ClearAnims();
			animCoroutine = StartCoroutine(spriteData.PlayAnim_Hurt(Target.Personality.AppearanceIndex,
																	MySprite));
		}
		private void Play_Attack()
		{
			ClearAnims();
			animCoroutine = StartCoroutine(spriteData.PlayAnim_Attack(Target.Personality.AppearanceIndex,
																	  MySprite));
		}

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

			MySprite.sprite = spriteData.GetSet(Target.Personality.AppearanceIndex).Idle;

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

            Target.Food.OnChanged -= Callback_FoodChanged;
            Target.Health.OnChanged -= Callback_HealthChanged;
            Target.Energy.OnChanged -= Callback_EnergyChanged;
            Target.Strength.OnChanged -= Callback_StrengthChanged;
        }

        private void Callback_FoodChanged(GameLogic.Units.PlayerChar u, float oldFood, float newFood)
        {
            lowFoodWarning.gameObject.SetActive(
				newFood <= PlayerConsts.InitialLowFoodThreshold);
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