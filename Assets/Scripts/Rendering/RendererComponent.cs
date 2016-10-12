using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering
{
	/// <summary>
	/// A script that handles some aspect of rendering the game.
	/// </summary>
	public abstract class RendererComponent : MonoBehaviour
	{
		protected static UnityLogic.GameFSM GameFSM { get { return UnityLogic.GameFSM.Instance; } }

		protected static GameLogic.Map Map { get { return GameFSM.Map; } }
		protected static GameLogic.TileGrid Tiles { get { return Map.Tiles; } }

		protected static UnityLogic.GameFSM.WorldProgress WorldProgress { get { return GameFSM.Progress; } }


		public Transform MyTr { get; private set; }


		protected virtual void Awake()
		{
			MyTr = transform;
		}
	}
}