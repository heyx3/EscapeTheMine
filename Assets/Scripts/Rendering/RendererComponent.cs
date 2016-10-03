using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering
{
	/// <summary>
	/// A script that handles some aspect of rendering the game.
	/// </summary>
	public abstract class RendererComponent : Singleton<RendererComponent>
	{
		protected static UnityLogic.GameFSM GameFSM { get { return UnityLogic.GameFSM.Instance; } }

		protected static GameLogic.Map Map { get { return GameFSM.Map; } }
		protected static GameLogic.TileGrid Tiles { get { return Map.Tiles; } }

		protected static UnityLogic.GameFSM.WorldProgress WorldProgress { get { return GameFSM.Progress; } }


		public Transform MyTr { get; private set; }


		protected override void Awake()
		{
            base.Awake();
			MyTr = transform;
		}
		protected virtual void Start()
		{
            GameFSM.OnNewMap += StartMap;
            GameFSM.Map.OnMapCleared += EndMap;
            GameFSM.Map.Tiles.OnTileChanged += TileChanged;
            GameFSM.Map.Tiles.OnTileGridResized += TileGridResized;
            GameFSM.Map.Units.OnElementAdded += UnitAddedToMap;
            GameFSM.Map.Units.OnElementRemoved += UnitRemovedFromMap;
		}
        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameFSM.OnNewMap -= StartMap;
            GameFSM.Map.OnMapCleared -= EndMap;
            GameFSM.Map.Tiles.OnTileChanged -= TileChanged;
            GameFSM.Map.Tiles.OnTileGridResized -= TileGridResized;
            GameFSM.Map.Units.OnElementAdded -= UnitAddedToMap;
            GameFSM.Map.Units.OnElementRemoved -= UnitRemovedFromMap;
        }


        //TODO: Callbacks for specific types of unit.

        //Callbacks for when certain things happen.
        protected virtual void StartMap(GameLogic.Map map) { }
        protected virtual void EndMap(GameLogic.Map map) { }

        protected virtual void TileChanged(GameLogic.TileGrid tiles, Vector2i tilePos,
                                           GameLogic.TileTypes oldTile, GameLogic.TileTypes newTile) { }
        protected virtual void TileGridResized(GameLogic.TileGrid tiles, Vector2i oldSize, Vector2i newSize) { }

        protected virtual void UnitAddedToMap(LockedSet<GameLogic.Unit> collection,
                                              GameLogic.Unit unit) { }
        protected virtual void UnitRemovedFromMap(LockedSet<GameLogic.Unit> collection,
                                                  GameLogic.Unit unit) { }
	}
}