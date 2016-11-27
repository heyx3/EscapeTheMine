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
		protected static UnityLogic.EtMGame Game { get { return UnityLogic.EtMGame.Instance; } }

		protected static GameLogic.Map Map { get { return Game.Map; } }
		protected static GameLogic.TileGrid Tiles { get { return Map.Tiles; } }

		protected static UnityLogic.EtMGame.WorldProgress WorldProgress { get { return Game.Progress; } }


		public Transform MyTr { get; private set; }


		protected virtual void Awake()
		{
			MyTr = transform;
		}
	}
}