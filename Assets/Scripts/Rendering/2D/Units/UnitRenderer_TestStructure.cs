using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_TestStructure : UnitRenderer<GameLogic.Units.TestStructure>
	{
		public Sprite Spr;

		
		protected override void Start()
		{
			base.Start();

			MySprite.sprite = Spr;

			Target.OnPosChanged += Callback_PosChanged;
			Callback_PosChanged(Target, Vector2i.Zero, Target.Pos);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.OnPosChanged -= Callback_PosChanged;
		}

		private void Callback_PosChanged(GameLogic.Unit u, Vector2i oldPos, Vector2i newPos)
		{
			MyTr.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, MyTr.position.z);
		}
	}
}