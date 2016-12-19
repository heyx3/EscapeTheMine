using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer_Bed : UnitRenderer<GameLogic.Units.Bed>
	{
		//TODO: Bed can get messy, less effective until cleaned. Add it to the design doc.

		public Sprite NormalSprite;


		protected override void Start()
		{
			base.Start();

			MySprite.sprite = NormalSprite;
		}
	}
}
