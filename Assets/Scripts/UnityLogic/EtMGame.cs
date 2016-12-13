using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	/// <summary>
	/// Runs the Finite State Machine controlling the flow of the game.
	/// </summary>
	public class EtMGame : MonoBehaviour
	{
		public class WorldProgress : MyData.IReadWritable
		{
			/// <summary>
			/// The 0-based index of the current map.
			/// </summary>
			public int Level = 0;
			/// <summary>
			/// The units that have exited the current map and are ready to go to the next one.
			/// </summary>
			public UlongSet ExitedUnitIDs = new UlongSet();

			public void Clear()
			{
				ExitedUnitIDs.Clear();
				Level = 0;
			}

			public void WriteData(MyData.Writer writer)
			{
				writer.Int(Level, "level");
				writer.Collection(ExitedUnitIDs, "exitedUnitIDs",
								  (MyData.Writer wr, ulong val, string name) =>
									  wr.UInt64(val, name));
			}
			public void ReadData(MyData.Reader reader)
			{
				Level = reader.Int("level");

				ExitedUnitIDs.Clear();
				reader.Collection("exitedUnitIDs",
								  (MyData.Reader rd, ref ulong id, string name) =>
									  { id = rd.UInt64(name); },
								  i => ExitedUnitIDs);
			}
		}

		[Serializable]
		public class WorldSettings : MyData.IReadWritable
		{
			public string Name = "My World";
			public int Size = 250;
			public string Seed = "abc123";

			public MapGen.BiomeGenSettings Biome = new MapGen.BiomeGenSettings();
			public MapGen.DepositGenSettings Deposits = new MapGen.DepositGenSettings();
			public MapGen.RoomGenSettings Rooms = new MapGen.RoomGenSettings();
			public MapGen.CAGenSettings CA = new MapGen.CAGenSettings();
			public MapGen.PlayerCharGenSettings PlayerChars = new MapGen.PlayerCharGenSettings();

			public void ReadData(MyData.Reader reader)
			{
				Name = reader.String("name");
				Size = reader.Int("size");
				Seed = reader.String("seed");

				reader.Structure(Biome, "biome");
				reader.Structure(Deposits, "deposits");
				reader.Structure(Rooms, "rooms");
				reader.Structure(CA, "ca");
				reader.Structure(PlayerChars, "playerChars");
			}
			public void WriteData(MyData.Writer writer)
			{
				writer.String(Name, "name");
				writer.Int(Size, "size");
				writer.String(Seed, "seed");

				writer.Structure(Biome, "biome");
				writer.Structure(Deposits, "deposits");
				writer.Structure(Rooms, "rooms");
				writer.Structure(CA, "ca");
				writer.Structure(PlayerChars, "playerChars");
			}
		}


		public static EtMGame Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<EtMGame>();
					if (_instance == null)
					{
						GameObject go = new GameObject("EtM Game");
						DontDestroyOnLoad(go);
						_instance = go.AddComponent<EtMGame>();
					}
				}

				return _instance;
			}
		}
		private static EtMGame _instance = null;

		public static bool InstanceExists { get { return _instance != null; } }


		/// <summary>
		/// "OnStart" is called when a new level is being started/reloaded.
		/// "OnEnd" is called when a level is being ended,
		///     e.x. if the player moved to the next level or quit the game.
		/// </summary>
		public event Action OnStart, OnEnd;


		public WorldSettings Settings = new WorldSettings();
		public int NThreads = 5;


        /// <summary>
        /// Note that this Map is persistent; it never gets a new value assigned to it after creation.
        /// This means that Map callbacks never have to be re-added wheneve e.x. a new map is loaded.
        /// </summary>
		public GameLogic.Map Map { get; private set; }
		/// <summary>
		/// The world's current state.
		/// </summary>
		public WorldProgress Progress { get; private set; }

		public bool IsInGame { get { return mapGameCoroutine != null; } }

		private Coroutine mapGameCoroutine = null;
		

		/// <summary>
		/// Quits the current world without saving and goes back to the main menu.
		/// </summary>
		public void QuitWorld()
		{
			if (IsInGame)
			{
				StopCoroutine(mapGameCoroutine);
				mapGameCoroutine = null;
			}

			if (OnEnd != null)
				OnEnd();

			Map.Clear();

			Progress.Clear();
			Settings = new WorldSettings();

			MenuController.Instance.Activate(MenuController.Instance.Menu_Main);
		}

		/// <summary>
		/// Generates a new world using the current settings,
		///     then starts the first level.
		/// </summary>
		public void GenerateWorld()
		{
			if (IsInGame)
				StopCoroutine(mapGameCoroutine);
			mapGameCoroutine = null;

			Map.Clear();
			Progress.Clear();

			GenerateMap(true);

			if (OnStart != null)
				OnStart();
		}
		/// <summary>
		/// Generates and starts the next level in the world
		///     using the current settings and progress.
		/// </summary>
		public void GenerateNextMap()
		{
			if (IsInGame)
				StopCoroutine(mapGameCoroutine);
			mapGameCoroutine = null;

			if (OnEnd != null)
				OnEnd();

			//Remove all units except for the ones that are continuing on to the next map.
			HashSet<ulong> toRemove = new HashSet<ulong>();
			foreach (GameLogic.Group g in Map.Groups)
				foreach (ulong id in g.UnitsByID)
					if (!Progress.ExitedUnitIDs.Contains(id))
						toRemove.Add(id);
			foreach (ulong id in toRemove)
				Map.RemoveUnit(Map.GetUnit(id));

			Progress.Level += 1;
			GenerateMap(false);
			Progress.ExitedUnitIDs.Clear();

			if (OnStart != null)
				OnStart();
        }
		
		public void LoadWorld(string name)
		{
			if (mapGameCoroutine != null)
				StopCoroutine(mapGameCoroutine);
			mapGameCoroutine = null;

			string filePath = MenuConsts.Instance.GetSaveFilePath(name);
			try
			{
				MyData.JSONReader reader = new MyData.JSONReader(filePath);

				reader.Structure(Progress, "progress");
				reader.Structure(Map, "map");
				reader.Structure(Settings, "worldSettings");
			}
			catch (MyData.Reader.ReadException e)
			{
				Debug.LogError("Unable to load " + filePath + ": " + e.Message);
            }

			mapGameCoroutine = StartCoroutine(Map.RunGameCoroutine());

			if (OnStart != null)
				OnStart();
        }
		public void SaveWorld()
		{
			string filePath = MenuConsts.Instance.GetSaveFilePath(Settings.Name);
			using (MyData.JSONWriter writer = new MyData.JSONWriter(filePath))
			{
				try
				{
					writer.Structure(Progress, "progress");
					writer.Structure(Map, "map");
					writer.Structure(Settings, "worldSettings");
				}
				catch (MyData.Writer.WriteException e)
				{
					Debug.LogError("Unable to save " + filePath + ": " + e.Message);
				}
			}
		}

		private void GenerateMap(bool fromScratch)
		{
			//Run the various generators.
			int seed = Settings.Seed.GetHashCode();
			var biomes = Settings.Biome.Generate(Settings.Size, Settings.Size,
												 NThreads, unchecked(seed * 462315));
			var deposits = Settings.Deposits.Generate(Settings.Size, Settings.Size,
													  NThreads, unchecked(seed * 123));
			var rooms = Settings.Rooms.Generate(biomes, NThreads, seed);
			var finalWalls = Settings.CA.Generate(biomes, rooms, NThreads, unchecked(seed * 3468));

			//Convert the generated data to actual game tiles.
			GameLogic.TileTypes[,] tiles = new GameLogic.TileTypes[Settings.Size, Settings.Size];
			for (int y = 0; y < tiles.GetLength(1); ++y)
			{
				for (int x = 0; x < tiles.GetLength(0); ++x)
				{
					if (x == 0 || x == Settings.Size - 1 || y == 0 || y == Settings.Size - 1)
						tiles[x, y] = GameLogic.TileTypes.Bedrock;
					else if (finalWalls[x, y])
						tiles[x, y] = (deposits[x, y] ? GameLogic.TileTypes.Deposit : GameLogic.TileTypes.Wall);
					else
						tiles[x, y] = GameLogic.TileTypes.Empty;
				}
			}
			Map.Tiles.Reset(tiles);

			//Generate units.
			var newUnits = Settings.PlayerChars.Generate(
							   Map, Settings, rooms, NThreads, unchecked(seed * 135789),
							   (fromScratch ? null : Progress.ExitedUnitIDs));
			Progress.ExitedUnitIDs.Clear();

			mapGameCoroutine = StartCoroutine(Map.RunGameCoroutine());

			//Make the camera focus in on the units.
			switch (Options.ViewMode)
			{
				case ViewModes.TwoD:
					Rendering.TwoD.Content2D.Instance.ZoomToSee(newUnits);
					break;
				case ViewModes.ThreeD: throw new NotImplementedException();

				default: throw new NotImplementedException(Options.ViewMode.ToString());
			}
			
			SaveWorld();
		}

		private void Awake()
		{
			Map = new GameLogic.Map();
			Progress = new WorldProgress();
		}
		private void Update()
		{
			if (IsInGame)
			{
				if (Input.GetKeyDown(KeyCode.Escape))
					MyUI.ContentUI.Instance.CreateGlobalWindow(MyUI.ContentUI.Instance.Window_Options);

				if (Input.GetKeyDown(KeyCode.Space))
					Map.IsPaused.Value = !Map.IsPaused;
			}
		}
	}
}
