using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rendering.TwoD
{
	public class UnitRenderer_LizardMan : UnitRenderer<GameLogic.Units.LizardMan>
	{
		//TODO: Once PlayerChar implements hurt/attack, this class should too.

		private UnitDispatcher.Data_LizardMan spriteData { get { return UnitDispatcher.Instance.LizardMan; } }

		private Coroutine animCoroutine = null;


		private void ClearAnims()
		{
			if (animCoroutine != null)
			{
				StopCoroutine(animCoroutine);
				animCoroutine = null;
				MySprite.sprite = spriteData.Spr_Idle;
			}
		}
		private void Play_Hurt()
		{
			ClearAnims();
			animCoroutine = StartCoroutine(spriteData.PlayAnim_Hurt(MySprite));
		}
		private void Play_Attack()
		{
			ClearAnims();
			animCoroutine = StartCoroutine(spriteData.PlayAnim_Attack(MySprite));
		}

		protected override void Start()
		{
			base.Start();

			MySprite.sprite = spriteData.Spr_Idle;

			Target.Health.OnChanged += Callback_HealthChanged;
			Callback_HealthChanged(Target, 0.0f, Target.Health);

			Target.Strength.OnChanged += Callback_StrengthChanged;
			Callback_StrengthChanged(Target, 0.0f, Target.Strength);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.Health.OnChanged -= Callback_HealthChanged;
			Target.Strength.OnChanged -= Callback_StrengthChanged;
		}

		private void Callback_HealthChanged(GameLogic.Units.LizardMan l,
											float oldHealth, float newHealth)
		{

		}
		private void Callback_StrengthChanged(GameLogic.Units.LizardMan l,
											  float oldStrength, float newStrength)
		{

		}
	}
}
