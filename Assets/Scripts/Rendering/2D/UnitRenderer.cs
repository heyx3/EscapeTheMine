using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class UnitRenderer<UType> : MonoBehaviour
		where UType : GameLogic.Unit
	{
		public UType Target;


		public Transform MyTr { get; private set; }
		public SpriteRenderer MySprite { get; private set; }


		protected virtual void Awake()
		{
			MyTr = transform;
			MySprite = GetComponentInChildren<SpriteRenderer>();
		}
	}
}
