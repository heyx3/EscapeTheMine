using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameLogic;


namespace MyUI
{
	//TODO: Add Window_Tile buttons for bringing up a "Job" window for each job that touches this tile (using "JobOverview").

	/// <summary>
	/// A window for doing something with a tile.
	/// </summary>
	public class Window_Tile : Window<Vector2i>
	{
		public Dict.GameObjectsByTileType UIObjectsByTileType = new Dict.GameObjectsByTileType(true);
		public Localizer Label_Title;


		public GameLogic.TileTypes TargetType
		{
			get
			{
				return UnityLogic.EtMGame.Instance.Map.Tiles[Target];
			}
		}


		protected override void Awake()
		{
			base.Awake();

			UnityLogic.EtMGame.Instance.Map.Tiles.OnTileChanged += Callback_TileTypeChanged;
		}
		private void Start()
		{
			Callback_TileTypeChanged(UnityLogic.EtMGame.Instance.Map.Tiles, Target,
									 GameLogic.TileTypes.Empty,
									 TargetType);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			UnityLogic.EtMGame.Instance.Map.Tiles.OnTileChanged -= Callback_TileTypeChanged;
		}

		public void Callback_Mine(bool isEmergency)
		{
			var data = new Window_SelectTiles.TilesSelectionData(
				(tilePoses) =>
				{
					if (tilePoses != null)
					{
						var playerGroup = Game.Map.FindGroup<GameLogic.Groups.PlayerGroup>();
						playerGroup.AddJob(
							new GameLogic.Units.Player_Char.Job_Mine(tilePoses, isEmergency, Game.Map));
					}
				},
				(tilePos) => Game.Map.Tiles[tilePos].IsMinable(),
				true, "WINDOW_MINEPOSES_TITLE", "WINDOW_MINEPOSES_MESSAGE");
			var wnd = (Window_SelectTiles)ContentUI.Instance.CreateWindow(
										      ContentUI.Instance.Window_SelectTiles, data);
			wnd.Callback_WorldTileClicked(Target);
		}
		public void Callback_BuildBed(bool isEmergency)
		{
			var playerGroup = Game.Map.FindGroup<GameLogic.Groups.PlayerGroup>();
			playerGroup.AddJob(
				new GameLogic.Units.Player_Char.Job_BuildBed(Target, playerGroup.ID,
															 isEmergency, Game.Map));
		}

		private void Callback_TileTypeChanged(GameLogic.TileGrid tiles,
											  Vector2i tilePos,
											  GameLogic.TileTypes oldType,
											  GameLogic.TileTypes newType)
		{
			foreach (var kvp in UIObjectsByTileType)
				kvp.Value.SetActive(kvp.Key == newType);

			switch (newType)
			{
				case GameLogic.TileTypes.Empty:
					Label_Title.Key = "WINDOW_TILE_EMPTY";
					break;
				case GameLogic.TileTypes.Wall:
					Label_Title.Key = "WINDOW_TILE_WALL";
					break;
				case GameLogic.TileTypes.Deposit:
					Label_Title.Key = "WINDOW_TILE_DEPOSIT";
					break;
				case GameLogic.TileTypes.Bedrock:
					Label_Title.Key = "WINDOW_TILE_BEDROCK";
					break;
			}
		}
	}
}
