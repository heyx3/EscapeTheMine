using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_TestChar : UnitRenderer<GameLogic.Units.TestChar>
	{
		public Sprite NormalSprite;

		private void Start()
		{
			MySprite.sprite = NormalSprite;
		}
	}
}