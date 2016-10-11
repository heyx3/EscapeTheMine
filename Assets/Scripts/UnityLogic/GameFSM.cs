using System;
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
			protected GameFSM FSM { get { return GameFSM.Instance; } }

			/// <summary>
			/// Called when this state starts up.
			/// If everything is normal, the method should return null.
			/// However, if this state should be replaced by a different one,
			///     that different state should be returned instead.
			/// Default behavior: returns null.
			/// </summary>
			public virtual State Start(State previousState) { return null; }
			/// <summary>
			/// Called every time Unity does an update.
			/// If everything is normal, the method should return null.
			/// However, if this state should be replaced by a different one,
			///     that different state should be returned instead.
			/// Default behavior: returns null.
			/// </summary>
			public virtual State Update() { return null; }
			/// <summary>
			/// Called when this state is about to be replaced by a new one.
			/// If everything is normal, the method should return null.
			/// However, if the new state should be replaced by a different one,
			///     that different state should be returned instead.
			/// Default behavior: returns null.
			/// </summary>
			public virtual State End(State nextState) { return null; }
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
			public MapGen.RoomGenSettings Rooms = new MapGen.RoomGenSettings();
			public MapGen.CAGenSettings CA = new MapGen.CAGenSettings();

			public void ReadData(MyData.Reader reader)
			{
				Name = reader.String("name");
				Size = reader.Int("size");
				Seed = reader.String("seed");

				reader.Structure(Biome, "biome");
				reader.Structure(Rooms, "rooms");
				reader.Structure(CA, "ca");
			}
			public void WriteData(MyData.Writer writer)
			{
				writer.String(Name, "name");
				writer.Int(Size, "size");
				writer.String(Seed, "seed");

				writer.Structure(Biome, "biome");
				writer.Structure(Rooms, "rooms");
				writer.Structure(CA, "ca");
			}
		}


        public enum ViewModes
        {
            ThreeD,
            TwoD,
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


		public State CurrentState
		{
			get { return currState; }
			set
			{
				//Tell the current state that it's ending.
				State oldState = currState;
				if (currState != null)
				{
					//The current state can tell us to use a different state in place of the new one.
					State replacement = currState.End(value);
					if (replacement != null)
						value = replacement;
				}

				currState = value;

				//Tell the new state that it's starting.
				if (currState != null)
				{
					//The new state can tell us to use a different state in place of itself.
					State replacement = currState.Start(oldState);
					while (replacement != null)
					{
						currState = replacement;
						replacement = currState.Start(oldState);
					}
				}
			}
		}
		private State currState = null;

        /// <summary>
        /// Note that this Map is persistent; it never gets a new value assigned to it after creation.
        /// This means that Map callbacks never have to be re-added wheneve e.x. a new map is loaded.
        /// </summary>
		public GameLogic.Map Map { get; private set; }

		public WorldProgress Progress { get; private set; }

        public ViewModes ViewMode
		{
			get { return viewMode; }
			set
			{
				if (viewMode == value)
					return;

				viewMode = value;
				Rendering.RendererController.Instance.UseRenderer(viewMode);

				PlayerPrefs.SetInt("viewMode", (int)viewMode);
			}
		}
		private ViewModes viewMode;
        

		public void GenerateWorld()
		{
			Map.Clear();
			Progress = new WorldProgress();
			CurrentState = new State_GenMap(true, Settings.Seed.GetHashCode(), NThreads,
											Settings.Biome, Settings.Rooms, Settings.CA);

            if (OnNewMap != null)
                OnNewMap(Map);
		}
		public void GenerateNextMap()
		{
			Map.Clear();
			Progress.Level += 1;
			CurrentState = new State_GenMap(false, Settings.Seed.GetHashCode(), NThreads,
											Settings.Biome, Settings.Rooms, Settings.CA);

            if (OnNewMap != null)
                OnNewMap(Map);
        }
		
		public void LoadWorld(string filePath)
		{
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
		}
		private void Start()
		{
			ViewMode = (ViewModes)PlayerPrefs.GetInt("viewMode", (int)ViewModes.TwoD);
		}
		private void Update()
		{
			if (CurrentState != null)
			{
				State replacement = CurrentState.Update();
				if (replacement != null)
					CurrentState = replacement;
			}
		}
	}
}
