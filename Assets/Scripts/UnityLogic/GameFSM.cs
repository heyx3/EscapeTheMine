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
	public class GameFSM : MonoBehaviour
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


		public static GameFSM Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<GameFSM>();
					if (_instance == null)
					{
						GameObject go = new GameObject("Game FSM");
						DontDestroyOnLoad(go);
						_instance = go.AddComponent<GameFSM>();
					}
				}

				return _instance;
			}
		}
		private static GameFSM _instance = null;

		public static bool InstanceExists { get { return _instance != null; } }


        public event Action<GameLogic.Map> OnNewMap;


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
		

		//TODO: Refactor all this to use the new, refactored GameLogic interface.

		public void QuitWorld()
		{
			Map.Clear();

			Progress.ExitedUnitIDs.Clear();
			Progress.Level = 0;

			Settings = new WorldSettings();

			MenuController.Instance.Activate(MenuController.Instance.Menu_Main);
		}

		public void GenerateWorld()
		{
			Map.Clear();
			Progress = new WorldProgress();
			CurrentState = new State_GenMap(true, Settings.Seed.GetHashCode(), NThreads);

            if (OnNewMap != null)
                OnNewMap(Map);
		}
		public void GenerateNextMap()
		{
			Map.Clear();
			Progress.Level += 1;
			CurrentState = new State_GenMap(false, Settings.Seed.GetHashCode(), NThreads);

            if (OnNewMap != null)
                OnNewMap(Map);
        }
		
		public void LoadWorld(string name)
		{
			string filePath = MenuConsts.Instance.GetSaveFilePath(name);
			try
			{
				MyData.JSONReader reader = new MyData.JSONReader(filePath);

				reader.Structure(Progress, "progress");
				reader.Structure(Map, "map");
				reader.Structure(Settings, "worldSettings");

				CurrentTurn = (GameLogic.Unit.Teams)reader.Int("currentTurn");
			}
			catch (MyData.Reader.ReadException e)
			{
				Debug.LogError("Unable to load " + filePath + ": " + e.Message);
            }
			
			CurrentState = new State_Turn();

            if (OnNewMap != null)
                OnNewMap(Map);
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
					writer.Int((int)CurrentTurn, "currentTurn");
				}
				catch (MyData.Writer.WriteException e)
				{
					Debug.LogError("Unable to save " + filePath + ": " + e.Message);
				}
			}
		}


		private void Awake()
		{
			Map = new GameLogic.Map();
			Progress = new WorldProgress();
		}
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape) && CurrentState is State_Turn)
			{
				MyUI.ContentUI.Instance.CreateGlobalWindow(MyUI.ContentUI.Instance.Window_Options);
			}
		}
		private void Start()
		{
			StartCoroutine(StateMachineCoroutine());
		}

		private System.Collections.IEnumerator StateMachineCoroutine()
		{
			while (true)
			{
				if (CurrentState != null)
				{
					foreach (object o in CurrentState.Update())
						yield return o;
				}
				yield return null;
			}
		}
	}
}
