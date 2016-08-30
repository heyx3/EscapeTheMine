using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	/// <summary>
	/// Runs the Finite State Machine controlling the flow of the game.
	/// </summary>
	public class GameFSM : Singleton<GameFSM>
	{
		public abstract class State
		{
			protected GameFSM FSM { get { return GameFSM.Instance; } }

			/// <summary>
			/// Called when this state starts up.
			/// If everything is normal, the state should return null.
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
			/// If everything is normal, the state should return itself.
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


		/// <summary>
		/// Called when a new map is being created.
		/// Rendering components can subscribe to this action to add hooks into the new map.
		/// </summary>
		public event Action OnMapCreated;
		/// <summary>
		/// Called when the map is being destroyed, generally to start a new one.
		/// Rendering components can subscribe to this action to destroy themselves in preparation.
		/// </summary>
		public event Action OnMapDestroyed;

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

		public GameLogic.Map Map { get; private set; }
		public WorldProgress Progress { get; private set; }


		public void GenerateWorld()
		{
			Progress = new WorldProgress();
			//TODO: Set CurrentState to a state that generates a new map with new units.
		}
		public void GenerateNextMap()
		{
			Progress.Level += 1;
			//TODO: Set CurrentState to a state that generates a new map using the ExitedUnits. Make sure to empty out ExitedUnits afterwards.
		}

		public void LoadWorld(string filePath)
		{
			//Clean up the current world.
			Map.Units.Clear();
			if (OnMapDestroyed != null)
				OnMapDestroyed();

			try
			{
				MyData.JSONReader reader = new MyData.JSONReader(filePath);
				reader.Structure(Progress, "progress");
				reader.Structure(Map, "map");
			}
			catch (MyData.Reader.ReadException e)
			{
				Debug.LogError("Unable to load " + filePath + ": " + e.Message);
			}

			if (OnMapCreated != null)
				OnMapCreated();
		}
		public void SaveWorld(string filePath)
		{
			using (MyData.JSONWriter writer = new MyData.JSONWriter(filePath))
			{
				writer.Structure(Progress, "progress");
				writer.Structure(Map, "map");
			}
		}

		void Update()
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
