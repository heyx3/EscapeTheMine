using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_TestStructure : UnitRenderer<GameLogic.Units.TestStructure>
	{
		public Sprite Spr;


		private void Start()
		{
			MySprite.sprite = Spr;

			Target.OnPosChanged += Callback_PosChanged;
			Callback_PosChanged(Target, Vector2i.Zero, Target.Pos);
		}

		private void Callback_PosChanged(GameLogic.Unit u, Vector2i oldPos, Vector2i newPos)
		{
			MyTr.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, MyTr.position.z);
		}
	}
}