using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A window for doing something with a tile.
	/// </summary>
	public class Window_Tile : Window<Vector2i>
	{
		public GameLogic.TileTypes TargetType {  get { return UnityLogic.EtMGame.Instance.Map.Tiles[Target]; } }


		public Dict.GameObjectsByTileType UIObjectsByTileType = new Dict.GameObjectsByTileType(true);


		protected override void Awake()
		{
			base.Awake();


		}
	}
}
