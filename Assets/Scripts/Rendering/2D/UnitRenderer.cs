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
			UnityLogic.GameFSM.Instance.Map.Units.OnElementRemoved += Callback_OnElementRemoved;
		}
		protected virtual void OnDestroy()
		{
			//Make sure we don't accidentally spawn the GameFSM object while shutting down.
			if (UnityLogic.GameFSM.InstanceExists)
				UnityLogic.GameFSM.Instance.Map.Units.OnElementRemoved -= Callback_OnElementRemoved;
		}

		private void Callback_OnElementRemoved(LockedSet<GameLogic.Unit> worldUnits,
											   GameLogic.Unit unit)
		{
			Destroy(gameObject);
		}
	}
}
