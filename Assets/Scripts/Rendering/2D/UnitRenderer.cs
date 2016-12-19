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
			UnityLogic.EtMGame.Instance.Map.OnUnitRemoved += Callback_UnitRemoved;

			Target.Pos.OnChanged += Callback_UnitMoved;
			MyTr.position = new Vector3(Target.Pos.Value.x + 0.5f, Target.Pos.Value.y + 0.5f,
										MyTr.position.z);
		}
		protected virtual void OnDestroy()
		{
			//Make sure we don't accidentally spawn the EtMGame object while shutting down.
			if (UnityLogic.EtMGame.InstanceExists)
				UnityLogic.EtMGame.Instance.Map.OnUnitRemoved -= Callback_UnitRemoved;

			Target.Pos.OnChanged -= Callback_UnitMoved;
		}

		protected virtual void Callback_UnitRemoved(GameLogic.Map map, GameLogic.Unit unit)
		{
			if (unit == Target)
				Destroy(gameObject);
		}
		protected virtual void Callback_UnitMoved(GameLogic.Unit u, Vector2i oldPos, Vector2i newPos)
		{
			UnityEngine.Assertions.Assert.IsTrue(u == Target);
			MyTr.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, MyTr.position.z);
		}
	}
}
