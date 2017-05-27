using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MyData;


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
		public override bool BlocksStructures { get { return false; } }
		public override bool BlocksMovement{ get { return false; } }


		/// <summary>
		/// The jobs this player was specifically tasked to do.
		/// </summary>
		private List<Player_Char.Job> customJobs = new List<Player_Char.Job>();
		/// <summary>
		/// The job this player is currently performing.
		/// </summary>
		private Player_Char.Job currentlyDoing = null;


		public PlayerChar(Map theMap, ulong groupID, float food, float energy, float strength,
						  float adultMultiplier, string name, Player_Char.Personality.Genders gender)
			: base(theMap, groupID)
		{
			Food = new Stat<float, PlayerChar>(this, food);
			Strength = new Stat<float, PlayerChar>(this, strength);

			Energy = new Stat<float, PlayerChar>(this, energy);
			Energy.OnChanged += (thisChar, oldVal, newVal) =>
			{
				float max = Player_Char.Consts.MaxEnergy(Strength);
				if (newVal > max)
					Energy.Value = max;
			};

			Health = new Stat<float, PlayerChar>(this, Player_Char.Consts.Max_Health);
			Health.OnChanged += (thisChar, oldVal, newVal) =>
			{
				if (newVal > Player_Char.Consts.Max_Health)
					Health.Value = Player_Char.Consts.Max_Health;
			};

			AdultMultiplier = new Stat<float, PlayerChar>(this, adultMultiplier);

			LowFoodThreshold =
				new Stat<float, PlayerChar>(this, Player_Char.Consts.InitialLowFoodThreshold);

			Career = new Player_Char.JobQualifications(this);
			Personality = new Player_Char.Personality(this, name, gender);

			//When this Unit gets killed,
			//    make sure all callbacks for its remaining custom jobs get called.
			OnKilled += (_this, _theMap) => RemoveAllJobs();
		}
		public PlayerChar(Map theMap)
			: this(theMap, ulong.MaxValue, 0.0f, 0.0f, 0.0f, 1.0f,
				   "", Player_Char.Personality.Genders.Male) { }


		public override System.Collections.IEnumerable TakeTurn()
		{
			//Lose food over time.
			if (Food > 0.0f)
			{
				float foodLoss = Player_Char.Consts.FoodLossPerTurn(Strength);
				Food.Value = Mathf.Max(0.0f, Food - foodLoss);
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
				var job = TakeAJob(false);
				if (job != null)
					StartDoingJob(job, null);
			}
			//If we have a job but it isn't an emergency, see if there IS an emergency.
			else if (!currentlyDoing.IsEmergency)
			{
				var emergencyJob = TakeAJob(true);
				if (emergencyJob != null)
				{
					var msg = Localization.Get("INTERRUPT_JOB_EMERGENCY",
											   emergencyJob.ToString());
					StartDoingJob(emergencyJob, msg);
				}
			}

			//Perform the current job.
			if (currentlyDoing != null)
				foreach (object o in currentlyDoing.TakeTurn())
					yield return o;
		}

		/// <summary>
		/// Finds the most pressing job this PlayerChar should automatically do,
		///     such as "sleep", "heal", or "eat".
		/// Returns "null" if nothing needs doing.
		/// </summary>
		private Player_Char.Job FindNeedsJob(bool emergenciesOnly)
		{
			//TODO: If there are no beds, don't return a sleep job. Maybe have the map cache all available beds for quick queries.
			//TODO: Once sleeping on the floor is also a thing, consider doing that if beds are too far away or something.

			if (emergenciesOnly)
			{
				//Sleep, if health is dangerously low.
				if (Health < Career.SleepWhen_HealthBelow)
					return new Player_Char.Job_SleepBed(true, TheMap);

				//Grow up, if growing up is considered a priority.
				if (!IsAdult && Career.GrowingUpIsEmergency.Value)
					return new Player_Char.Job_GrowUp(true, TheMap);
			}
			else
			{
				//Try finding an emergency job first.
				var emergencyJob = FindNeedsJob(true);
				if (emergencyJob != null)
					return emergencyJob;


				//Otherwise, find a non-emergency job.

				//Sleep, if energy is low.
				if (Energy < Career.SleepWhen_EnergyBelow)
					return new Player_Char.Job_SleepBed(true, TheMap);

				//Grow up.
				if (!IsAdult)
					return new Player_Char.Job_GrowUp(false, TheMap);
			}

			return null;
		}
		/// <summary>
		/// Finds a job to do. Returns "null" if nothing was found.
		/// Note that if it was a global job, this PlayerChar is now responsible for it
		///     until the job ends one way or another.
		/// </summary>
		/// <param name="emergenciesOnly">Whether to only look for emergency jobs.</param>
		private Player_Char.Job TakeAJob(bool emergenciesOnly)
		{
			//There are three types of jobs:
			//  1. "Needs"-related jobs, like getting food/sleep or running from enemies.
			//  2. "Custom" jobs given specifically to this unit.
			//  3. "Global" jobs that any unit can take on.

			//Jobs are taken in this order:
			//  1. Emergency custom jobs.
			//  2. Emergency "needs" jobs.
			//  3. Emergency global jobs.
			//  4. Non-emergency "needs" jobs.
			//  5. Non-emergency custom jobs.
			//  6. Non-emergency global jobs.

			Player_Char.Job job = null;

			if (emergenciesOnly)
			{
				job = customJobs.FirstOrDefault(j => j.IsEmergency);

				if (job == null)
					job = FindNeedsJob(true);

				if (job == null)
					job = ((Groups.PlayerGroup)MyGroup).TakeJob(this, true);
			}
			else
			{
				//Try taking emergency jobs first.
				job = TakeAJob(true);

				if (job == null)
					job = FindNeedsJob(false);

				if (job == null && customJobs.Count > 0)
					job = customJobs[0];

				if (job == null)
					job = ((Groups.PlayerGroup)MyGroup).TakeJob(this, false);
			}

			return job;
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
		/// </param>
		public bool RemoveJob(Player_Char.Job job, string alert = "")
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
		/// Removes all jobs specifically given to this player.
		/// </summary>
		/// <param name="alert">
		/// If the PlayerChar was in the middle of doing this job,
		///     it will be ended with the given message.
		/// </param>
		public void RemoveAllJobs(string alert = "")
		{
			var jobsToDestroy = customJobs.ToList();
			if (currentlyDoing != null)
				jobsToDestroy.Add(currentlyDoing);

			foreach (var job in jobsToDestroy)
				RemoveJob(job, alert);
		}
		
		/// <summary>
		/// Stops this PlayerChar from doing its current job and starts the given one.
		/// Returns the job that was stopped (or "null" if it wasn't doing anything).
		/// </summary>
		/// <param name="alertOldJob">
		/// If a job was canceled to make way for this one,
		///     it will be ended with the given message.
		/// Pass "null" for no message.
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
		public Player_Char.Job StopDoingJob(string alert, bool succeeded = false)
		{
			if (currentlyDoing != null)
			{
				Player_Char.Job j = currentlyDoing;

				Callback_OnJobFinished(j, false, alert);
				j.EndJob(succeeded, alert);

				return j;
			}
			else
			{
				return null;
			}
		}

		#region Callbacks

		//These methods set up/take down the callbacks for the currently-active job.
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

		//These are the callbacks are for the currently-active job.
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
