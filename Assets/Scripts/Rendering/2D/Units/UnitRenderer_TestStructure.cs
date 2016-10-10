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
		}
	}
}