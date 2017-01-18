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
		private void AStarEdgeCalc(Pathfinding.Goal<Vector2i> goal,
								   Pathfinding.Edge<Vector2i> edge,
								   out float edgeLength, out float heuristic)
		{
			Graph.AStarEdgeCalc(goal, edge, out edgeLength, out heuristic);

			//Subtract enemy distances squared from the heuristic.
			foreach (Unit enemy in _temp_enemies)
			{
				float distSqr = enemy.Pos.Value.DistanceSqr(Pos);
				float distT = distSqr * Player_Char.Consts.One_Over_MaxEnemyDistSqr;
				distT = Math.Max(0.0f, Mathf.Min(1.0f, distT));

				heuristic += (Player_Char.Consts.EnemyDistHeuristicMax * distT);
			}
		}
		/// <summary>
		/// Re-use a collection of a PlayerChar's enemies to reduce garbage.
		/// </summary>
		private static HashSet<Unit> _temp_enemies = new HashSet<Unit>();


		/// <summary>
		/// Raised when a new job is given to this PlayerChar specifically.
		/// </summary>
		public event Action<PlayerChar, Player_Char.Job> OnAddCustomJob;
		/// <summary>
		/// Raised when a job given to this specific PlayerChar was taken away.
		/// </summary>
		public event Action<PlayerChar, Player_Char.Job> OnRemoveCustomJob;
		
		
		public Stat<float, PlayerChar> Food { get; private set; }
		public Stat<float, PlayerChar> Energy { get; private set; }
		public Stat<float, PlayerChar> Health { get; private set; }
		public Stat<float, PlayerChar> Strength { get; private set; }

		public Stat<float, PlayerChar> AdultMultiplier { get; private set; }
		public bool IsAdult { get { return AdultMultiplier.Value >= 1.0f; } }

		public Stat<float, PlayerChar> LowFoodThreshold { get; private set; }

		public Player_Char.JobQualifications Career { get; private set; }
		public Player_Char.Personality Personality { get; private set; }

		/// <summary>
		/// All the jobs specifically given to this PlayerChar.
		/// Does NOT include the job he's currently doing.
		/// </summary>
		public IEnumerable<Player_Char.Job> CustomJobs { get { return customJobs; } }

		public Player_Char.Job CurrentJob { get { return currentlyDoing; } }

		public override string DisplayName { get { return Personality.Name; } }


		/// <summary>
		/// The jobs this player was specifically tasked to do.
		/// </summary>
		private List<Player_Char.Job> customJobs = new List<Player_Char.Job>();
		/// <summary>
		/// The job this player is currently performing.
		/// </summary>
		private Player_Char.Job currentlyDoing = null;


		public PlayerChar(Map theMap, ulong groupID, float food, float energy, float strength,
						  float adultMultiplier,
						  string name, Player_Char.Personality.Genders gender, int appearanceIndex)
			: base(theMap, groupID)
		{
			Food = new Stat<float, PlayerChar>(this, food);
			Energy = new Stat<float, PlayerChar>(this, energy);
			Strength = new Stat<float, PlayerChar>(this, strength);

			AdultMultiplier = new Stat<float, PlayerChar>(this, adultMultiplier);

			Health = new Stat<float, PlayerChar>(this, Player_Char.Consts.Max_Health);

			LowFoodThreshold =
				new Stat<float, PlayerChar>(this, Player_Char.Consts.InitialLowFoodThreshold);

			Career = new Player_Char.JobQualifications(this);
			Personality = new Player_Char.Personality(this, name, gender, appearanceIndex);
		}
		public PlayerChar(Map theMap)
			: this(theMap, ulong.MaxValue, 0.0f, 0.0f, 0.0f, 1.0f,
				   "", Player_Char.Personality.Genders.Male, 0) { }


		public override System.Collections.IEnumerable TakeTurn()
		{
			//Lose food over time.
			if (Food > 0.0f)
			{
				float newFood = Food - Player_Char.Consts.FoodLossPerTurn(Strength);
				Food.Value = Mathf.Max(0.0f, newFood);
			}
			//If no food is left, lose health over time (i.e. starvation).
			else
			{
				float newHealth = Health - Player_Char.Consts.StarvationDamagePerTurn;
				if (newHealth <= 0.0f)
				{
					Health.Value = 0.0f;
					TheMap.RemoveUnit(this);
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
                //If we need to grow up ASAP, do that.
                if (!IsAdult && Career.GrowingUpIsEmergency.Value)
                {
                    StartDoingJob(new Player_Char.Job_GrowUp(true, TheMap), null);
                }
				//Otherwise, look at any jobs specifically given to this unit.
				else if (customJobs.Count > 0)
				{
					//Prioritize "emergency" jobs.
					Player_Char.Job nextJob = customJobs.FirstOrDefault(j => j.IsEmergency);
					if (nextJob == null)
						nextJob = customJobs[0];

					StartDoingJob(nextJob, null);
				}
                //Otherwise, see if this unit needs to grow up as a non-emergency job.
                else if (!IsAdult)
                {
                    StartDoingJob(new Player_Char.Job_GrowUp(false, TheMap), null);
                }
				//Otherwise, grab from the global job collection.
				else
				{
					Player_Char.Job nextJob = ((Groups.PlayerGroup)MyGroup).TakeJob(this, false);
					if (nextJob != null)
						StartDoingJob(nextJob, null);
				}
			}
			//If we have a job but it isn't an emergency, see if there IS an emergency.
			else if (!currentlyDoing.IsEmergency)
			{
				//Grab from jobs specific to this instance if they exist.
				//Otherwise, grab from the global job collection.
				Player_Char.Job emergencyJob = customJobs.FirstOrDefault(j => j.IsEmergency);
                if (emergencyJob == null && !IsAdult && Career.GrowingUpIsEmergency.Value)
                    emergencyJob = new Player_Char.Job_GrowUp(true, TheMap);
                if (emergencyJob == null)
					emergencyJob = ((Groups.PlayerGroup)MyGroup).TakeJob(this, true);

				if (emergencyJob != null)
				{
                    StartDoingJob(emergencyJob,
                                  Localization.Get("INTERRUPT_JOB_EMERGENCY", emergencyJob.ToString()));
				}
			}

			//Perform the current job.
			if (currentlyDoing != null)
				foreach (object o in currentlyDoing.TakeTurn())
					yield return o;
		}

		/// <summary>
		/// Outputs the shortest path from this PlayerChar to the given goal
		///     into the "outPath" list.
		/// Does not include this PlayerChar's own position in the list.
		/// Returns whether a path was actually found.
		/// </summary>
		public bool FindPath(Pathfinding.Goal<Vector2i> goal, List<Vector2i> outPath)
		{
			//Collect all of this PlayerChar's current enemies for the A* heuristic.
			_temp_enemies.Clear();
			foreach (ulong enemyGroupID in TheMap.Groups.Get(MyGroupID).EnemiesByID)
				foreach (ulong enemyUnitID in TheMap.Groups.Get(enemyGroupID).UnitsByID)
					_temp_enemies.Add(TheMap.GetUnit(enemyUnitID));

			return TheMap.FindPath(Pos, goal, outPath, AStarEdgeCalc);
		}
		/// <summary>
		/// Finds the shortest path from this PlayerChar to the given goal.
		/// Does not include this PlayerChar's own position in the list.
		/// Returns "null" if a path wasn't found.
		/// IMPORTANT: The returned list is reused for other calls to this method,
		///     so treat it as a temp variable!
		/// </summary>
		public List<Vector2i> FindPath(Pathfinding.Goal<Vector2i> goal)
		{
			//Collect all of this PlayerChar's current enemies for the A* heuristic.
			_temp_enemies.Clear();
			foreach (ulong enemyGroupID in TheMap.Groups.Get(MyGroupID).EnemiesByID)
				foreach (ulong enemyUnitID in TheMap.Groups.Get(enemyGroupID).UnitsByID)
					_temp_enemies.Add(TheMap.GetUnit(enemyUnitID));

			return TheMap.FindPath(Pos, goal, AStarEdgeCalc);
		}

		/// <summary>
		/// Adds a new job for this specific PlayerChar to do.
		/// </summary>
		public void AddJob(Player_Char.Job job)
		{
			customJobs.Add(job);

			if (OnAddCustomJob != null)
				OnAddCustomJob(this, job);
		}
		/// <summary>
		/// Removes a job that was specifically given to this player.
		/// Returns whether the player actually had this job in the first place.
		/// </summary>
		/// <param name="alert">
		/// If the PlayerChar was in the middle of doing this job,
		///     it will be ended with the given message.
		/// If the interruption isn't important enough to notice, pass the empty string.
		/// </param>
		public bool RemoveJob(Player_Char.Job job, string alert)
		{
			bool existed;

			if (currentlyDoing == job)
			{
				existed = true;
				StopDoingJob(alert);
			}
			else
			{
				existed = customJobs.Remove(job);
			}

			if (existed && OnRemoveCustomJob != null)
				OnRemoveCustomJob(this, job);
			return existed;
		}
		
		/// <summary>
		/// Stops this PlayerChar from doing its current job and starts the given one.
		/// Returns the job that was stopped (or "null" if it wasn't doing anything).
		/// </summary>
		/// <param name="alertOldJob">
		/// If a job was canceled to make way for this one,
		///     it will be ended with the given message.
		/// </param>
		public Player_Char.Job StartDoingJob(Player_Char.Job job, string alertOldJob)
		{
			Player_Char.Job oldJob = StopDoingJob(alertOldJob);

			currentlyDoing = job;
			currentlyDoing.Owner.Value = this;

			InitCallbacks(job);

			customJobs.Remove(job);

			return oldJob;
		}
		/// <summary>
		/// Stops this PlayerChar from doing its current job.
		/// Returns the job it was doing (or "null" if it wasn't doing anything).
		/// </summary>
		/// <param name="alert">
		/// The job's "OnJobFinished" event will be raised with this message.
		/// May be "null" if the interruption isn't important.
		/// </param>
		public Player_Char.Job StopDoingJob(string alert)
		{
			if (currentlyDoing != null)
			{
				Player_Char.Job j = currentlyDoing;

				Callback_OnJobFinished(j, false, alert);
				j.EndJob(false, alert);

				return j;
			}
			else
			{
				return null;
			}
		}

		#region Callbacks

		private void InitCallbacks(Player_Char.Job activeJob)
		{
			activeJob.OnJobFinished += Callback_OnJobFinished;
			activeJob.Owner.OnChanged += Callback_JobOwnerChanged;
		}
		private void DeInitCallbacks(Player_Char.Job activeJob)
		{
			activeJob.OnJobFinished -= Callback_OnJobFinished;
			activeJob.Owner.OnChanged -= Callback_JobOwnerChanged;
		}

		//These callbacks are for the job this instance is currently performing.
		private void Callback_JobOwnerChanged(Player_Char.Job job, PlayerChar oldP, PlayerChar newP)
		{
			UnityEngine.Assertions.Assert.IsTrue(currentlyDoing == job);
			StopDoingJob(Localization.Get("INTERRUPT_JOB_UNKNOWN", job.ToString()));
		}
		private void Callback_OnJobFinished(Player_Char.Job job, bool succeeded, string message)
		{
			UnityEngine.Assertions.Assert.IsTrue(currentlyDoing == job);

			DeInitCallbacks(job);
			currentlyDoing.Owner.Value = null;
			currentlyDoing = null;
		}

		#endregion

		#region Serialization

		public override Types MyType { get { return Types.PlayerChar; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);

			writer.Float(Food, "food");
			writer.Float(Energy, "energy");
			writer.Float(Health, "health");
			writer.Float(Strength, "strength");

			writer.Float(AdultMultiplier, "adultMultiplier");

			writer.Float(LowFoodThreshold, "lowFoodThreshold");

			writer.Structure(Career, "career");
			writer.Structure(Personality, "personality");

			writer.Collection<Player_Char.Job, List<Player_Char.Job>>(
				customJobs, "customJobs",
				(w, valToWrite, name) =>
					Player_Char.Job.Write(w, valToWrite, name));
			
			if (currentlyDoing != null)
			{
				writer.Bool(true, "hasJob");
				Player_Char.Job.Write(writer, currentlyDoing, "currentJob");
			}
		}
		public override void ReadData(Reader reader)
		{
			StopDoingJob(null);

			base.ReadData(reader);

			Food.Value = reader.Float("food");
			Energy.Value = reader.Float("energy");
			Health.Value = reader.Float("health");
			Strength.Value = reader.Float("strength");

			AdultMultiplier.Value = reader.Float("adultMultiplier");

			LowFoodThreshold.Value = reader.Float("lowFoodThreshold");

			reader.Structure(Career, "career");
			reader.Structure(Personality, "personality");

			customJobs.Clear();
			reader.Collection<Player_Char.Job, List<Player_Char.Job>>(
				"customJobs",
				(MyData.Reader r, ref Player_Char.Job outVal, string name) =>
					outVal = Player_Char.Job.Read(r, name, TheMap),
				(size) => customJobs);
			
			if (reader.Bool("hasJob"))
				StartDoingJob(Player_Char.Job.Read(reader, "currentJob", TheMap), null);
		}

		#endregion
	}
}
