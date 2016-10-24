using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyData;
using UnityEngine;


namespace GameLogic.Units
{
	/// <summary>
	/// The player's characters.
	/// Does two kinds of jobs: those that were specifically given to it,
	///     and those that were designated for anybody to do.
	/// </summary>
	public class PlayerChar : Unit
	{
		/// <summary>
		/// Calculates edge length/heuristics for PlayerChar's A* pathfinding.
		/// </summary>
		public static void AStarEdgeCalc(Pathfinding.Goal<Vector2i> goal,
										 Pathfinding.Edge<Vector2i> edge,
										 out float edgeLength, out float heuristic)
		{
			Graph.AStarEdgeCalc(goal, edge, out edgeLength, out heuristic);

			//TODO: Add enemy distances (squared) to heuristic.
		}


		public Stat<float, PlayerChar> Food, Energy, Health, Strength;


		/// <summary>
		/// Raised when a new job is given to this player specifically.
		/// </summary>
		public event Action<PlayerChar, Player_Char.Job> OnNewCustomJob;


		/// <summary>
		/// The jobs this player was specifically tasked to do.
		/// </summary>
		private List<Player_Char.Job> customJobs = new List<Player_Char.Job>();
		/// <summary>
		/// The job this player is currently performing.
		/// </summary>
		private Player_Char.Job currentlyDoing = null;


		public PlayerChar(Map newOwner, float food, float energy, float health, float strength)
			: base(newOwner, Teams.Player)
		{
			Food = new Stat<float, PlayerChar>(this, food);
			Energy = new Stat<float, PlayerChar>(this, energy);
			Health = new Stat<float, PlayerChar>(this, health);
			Strength = new Stat<float, PlayerChar>(this, strength);
		}
		public PlayerChar(Map newOwner) : this(newOwner, 0.0f, 0.0f, 0.0f, 0.0f) { }

		protected PlayerChar(Map newOwner, PlayerChar copyFrom)
			: base(newOwner, copyFrom)
		{
			Food = new Stat<float, PlayerChar>(this, copyFrom.Food);
			Energy = new Stat<float, PlayerChar>(this, copyFrom.Energy);
			Health = new Stat<float, PlayerChar>(this, copyFrom.Health);
			Strength = new Stat<float, PlayerChar>(this, copyFrom.Strength);
		}
		public override Unit Clone(Map newOwner)
		{
			return new PlayerChar(newOwner, this);
		}


		public override System.Collections.IEnumerable TakeTurn()
		{
			//Lose food over time.
			if (Food > 0.0f)
			{
				float newFood = Food - (Player_Char.Consts.BaseLossPerTurn_Food / Strength.Value);
				Food.Value = Mathf.Max(0.0f, newFood);
			}
			//If no food is left, lose health over time (i.e. starvation).
			else
			{
				float newHealth = Health - Player_Char.Consts.LossPerTurn_Health;
				if (newHealth <= 0.0f)
				{
					Health.Value = 0.0f;
					Owner.Units.Remove(this);
					yield break;
				}
				else
				{
					Health.Value = newHealth;
				}
			}


			//Grab the most pressing job and do it.

			//If no current job exists, find one.
			if (currentlyDoing == null)
			{
				if (customJobs.Count > 0)
				{
					//Prioritize "emergency" jobs.
					Player_Char.Job nextJob = customJobs.FirstOrDefault(j => j.IsEmergency);
					if (nextJob == null)
						nextJob = customJobs[0];

					StartDoingJob(nextJob);
				}
				else
				{
					//TODO: Grab a job from a global queue (held by the map, probably).
				}
			}
			//If we have a job but it isn't an emergency, see if there IS an emergency.
			else if (!currentlyDoing.IsEmergency)
			{
				Player_Char.Job emergencyJob = customJobs.FirstOrDefault(j => j.IsEmergency);
				if (emergencyJob != null)
					StartDoingJob(emergencyJob);
			}

			//Finally, perform the current job.
			if (currentlyDoing != null)
				foreach (object o in currentlyDoing.TakeTurn())
					yield return o;
		}
		private void StopDoingJob()
		{
			if (currentlyDoing != null)
			{
				currentlyDoing.OnJobFinished -= Callback_OnJobFinished;
				currentlyDoing.Owner.Value = null;
				currentlyDoing = null;
			}
		}
		private void StartDoingJob(Player_Char.Job job)
		{
			StopDoingJob();
			currentlyDoing = job;
			currentlyDoing.Owner.Value = this;
			currentlyDoing.OnJobFinished += Callback_OnJobFinished;
		}
		private void Callback_OnJobFinished(Player_Char.Job job)
		{
			UnityEngine.Assertions.Assert.IsTrue(customJobs.Contains(job), job.ToString());
			customJobs.Remove(job);

			StopDoingJob();
		}

		/// <summary>
		/// Adds a new job for this specific PlayerChar to do.
		/// </summary>
		public void AddJob(Player_Char.Job job)
		{
			customJobs.Add(job);
		}
		/// <summary>
		/// Removes a job that was specifically given to this player.
		/// Returns whether the player actually had this job in the first place.
		/// </summary>
		public bool RemoveJob(Player_Char.Job job)
		{
			if (currentlyDoing == job)
				StopDoingJob();

			return customJobs.Remove(job);
		}


		//Serialization.
		public override Types MyType { get { return Types.PlayerChar; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);
			writer.Float(Food, "food");
			writer.Float(Energy, "energy");
			writer.Float(Health, "health");
			writer.Float(Strength, "strength");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);
			Food.Value = reader.Float("food");
			Energy.Value = reader.Float("energy");
			Health.Value = reader.Float("health");
			Strength.Value = reader.Float("strength");
		}
	}
}
