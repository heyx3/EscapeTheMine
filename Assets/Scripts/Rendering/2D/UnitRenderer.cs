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
		protected virtual void Start()
		{
			UnityLogic.EtMGame.Instance.Map.OnUnitRemoved += Callback_OnUnitRemoved;
		}
		protected virtual void OnDestroy()
		{
			//Make sure we don't accidentally spawn the EtMGame object while shutting down.
			if (UnityLogic.EtMGame.InstanceExists)
				UnityLogic.EtMGame.Instance.Map.OnUnitRemoved -= Callback_OnUnitRemoved;
		}

		private void Callback_OnUnitRemoved(GameLogic.Map map, GameLogic.Unit unit)
		{
			if (unit == Target)
				Destroy(gameObject);
		}
	}
}
