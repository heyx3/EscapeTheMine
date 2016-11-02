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
		public abstract class State
		{
			protected static GameFSM FSM { get { return GameFSM.Instance; } }
			
			public virtual void Start(State previousState) { }
			public virtual void End(State nextState) { }

			//"Update()" is a coroutine.
			public virtual IEnumerable Update() { yield break; }
		}

		public class WorldProgress : MyData.IReadWritable
		{
			/// <summary>
			/// The 0-based index of the current map.
			/// </summary>
			public int Level = 0;
			/// <summary>
			/// The units that have exited the current map and are ready to go to the next one.
			/// </summary>
			public HashSet<GameLogic.Unit> ExitedUnits = new HashSet<GameLogic.Unit>();

			public void WriteData(MyData.Writer writer)
			{
				writer.Int(Level, "level");
				writer.Collection(ExitedUnits, "exitedUnits",
								  (MyData.Writer wr, GameLogic.Unit u, string name) =>
									  GameLogic.Unit.Write(wr, name, u));
			}
			public void ReadData(MyData.Reader reader)
			{
				Level = reader.Int("level");
				ExitedUnits = reader.Collection("exitedUnits",
												(MyData.Reader rd, ref GameLogic.Unit u, string name) =>
													u = GameLogic.Unit.Read(rd, GameFSM.Instance.Map, name),
												i => new HashSet<GameLogic.Unit>());
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

		/// <summary>
		/// Called when the game is paused or unpaused.
		/// The parameter indicates whether the game is now paused.
		/// </summary>
		public event Action<bool> OnPauseToggle;


		public WorldSettings Settings = new WorldSettings();
		public int NThreads = 5;

		public GameLogic.Unit.Teams CurrentTurn = GameLogic.Unit.Teams.Player;


		public State CurrentState
		{
			get { return currState; }
			set
			{
				State oldState = currState;

				//Tell the current state that it's ending.
				if (currState != null)
					currState.End(value);

				currState = value;

				//Tell the new state that it's starting.
				if (currState != null)
					currState.Start(oldState);
			}
		}
		private State currState = null;

        /// <summary>
        /// Note that this Map is persistent; it never gets a new value assigned to it after creation.
        /// This means that Map callbacks never have to be re-added wheneve e.x. a new map is loaded.
        /// </summary>
		public GameLogic.Map Map { get; private set; }

		public WorldProgress Progress { get; private set; }
		

		public bool IsPaused
		{
			get { return isPaused; }
			set
			{
				if (isPaused == value)
					return;
				isPaused = value;

				if (OnPauseToggle != null)
					OnPauseToggle(isPaused);
			}
		}
		private bool isPaused = false;
		

		public void QuitWorld()
		{
			Map.Clear();

			Progress.ExitedUnits.Clear();
			Progress.Level = 0;

			Settings = new WorldSettings();

			CurrentState = null;

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
