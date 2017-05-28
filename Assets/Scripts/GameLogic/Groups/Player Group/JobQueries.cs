using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyData;
using GameLogic.Units.Player_Char;

using PlayerChar = GameLogic.Units.PlayerChar;


namespace GameLogic.Groups.Player_Group
{
	/// <summary>
	/// Provides high-level information about jobs.
	/// </summary>
	public class JobQueries
	{
		public PlayerGroup Owner { get; private set; }

		/// <summary>
		/// All jobs that haven't been finished yet,
		///     whether given to the group or to a specific unit.
		/// </summary>
		public IEnumerable<Job> AllJobs { get { return outstandingJobs; } }
		/// <summary>
		/// All positions that the given job will affect.
		/// </summary>
		public IEnumerable<Vector2i> GetAffectedPoses(Job j)
		{
			if (affectedPoses.ContainsKey(j))
				if (affectedPoses[j].HasValue)
					yield return affectedPoses[j].Value;
				else
					yield break;
			else
				foreach (Vector2i p in affectedMultiPoses[j])
					yield return p;
		}
		/// <summary>
		/// All jobs that affect the given position.
		/// </summary>
		public IEnumerable<Job> GetAffectingJobs(Vector2i worldPos)
		{
			return AllJobs.Where(job =>
			{
				if (affectedPoses.ContainsKey(job))
					return affectedPoses[job].HasValue && affectedPoses[job] == worldPos;
				else
					return affectedMultiPoses[job].Contains(worldPos);
			});
		}

		/// <summary>
		/// Gets whether the given job affects at least one tile.
		/// </summary>
		public bool AffectsAnyPoses(Job job)
		{
			return affectedMultiPoses.ContainsKey(job) ||
				   (affectedPoses.ContainsKey(job) && affectedPoses[job].HasValue);
		}

		
		/// <summary>
		/// Raised when any job is created, whether a global job or a PlayerChar-specific one.
		/// </summary>
		public event Action<PlayerGroup, Job> OnJobCreated;
		/// <summary>
		/// Raised when any job is destroyed, whether global or PlayerChar-specific,
		///     whether it was completed or not.
		/// </summary>
		public event Action<PlayerGroup, Job> OnJobDestroyed;
		/// <summary>
		/// Raised when a PlayerChar started actively doing a job.
		/// </summary>
		public event Action<PlayerGroup, Job> OnJobStarted;
		/// <summary>
		/// Raised when a PlayerChar stopped actively doing the job.
		/// This may be because of success, failure, or whatever.
		/// NOTE: This sometimes won't get raised until AFTER "OnJobDestroyed" gets raised!
		/// </summary>
		public event Action<PlayerGroup, Job> OnJobStopped;


		private HashSet<Job> outstandingJobs = new HashSet<Job>();

		private Dictionary<Job, Vector2i?> affectedPoses = new Dictionary<Job, Vector2i?>();
		private Dictionary<Job, IEnumerable<Vector2i>> affectedMultiPoses = new Dictionary<Job, IEnumerable<Vector2i>>();


		public JobQueries(PlayerGroup owner)
		{
			Owner = owner;

			Owner.TheMap.Groups.OnElementRemoved += Callback_GroupRemoved;

			Owner.OnNewJob += Callback_NewGroupJob;
			Owner.OnJobCanceled += Callback_CancelGroupJob;

			Owner.UnitsByID.OnElementAdded += Callback_NewUnit;
			Owner.UnitsByID.OnElementRemoved += Callback_RemoveUnit;

			//Call the "unit created" callback for all currently-existing units.
			foreach (ulong unitID in Owner.UnitsByID)
				Callback_NewUnit(Owner.UnitsByID, unitID);
		}

		
		/// <summary>
		/// Starts tracking the given job, if it wasn't already being tracked.
		/// </summary>
		private void AddJob(Job job)
		{
			//If this job alredy existed in the set, don't initialize it again!
			if (!outstandingJobs.Add(job))
				return;

			//Set up callbacks.
			job.OnJobFinished += Callback_JobFinished;
			job.Owner.OnChanged += (_job, oldOwner, newOwner) =>
			{
				if (oldOwner != newOwner)
				{
					if (newOwner == null)
					{
						if (OnJobStopped != null)
							OnJobStopped(Owner, _job);
					}
					else
					{
						if (OnJobStarted != null)
							OnJobStarted(Owner, _job);
					}
				}
			};

			//Find any positions the job will affect.
			if (job is Job_Mine)
			{
				var job_Mine = (Job_Mine)job;
				affectedMultiPoses.Add(job, job_Mine.TilesToMine);
			}
			else if (job is Job_MoveToPos)
			{
				var job_moveToPos = (Job_MoveToPos)job;
				affectedPoses.Add(job, job_moveToPos.TargetPos);
				job_moveToPos.TargetPos.OnChanged += (_j, oldPos, newPos) =>
				{
					affectedPoses[_j] = newPos;
				};
			}
			else if (job is Job_BuildBed)
			{
				var job_buildBed = (Job_BuildBed)job;
				affectedPoses.Add(job, job_buildBed.Tile);
			}
			//Any jobs that don't affect tiles go here.
			else if (job is Job_GrowUp || job is Job_SleepBed)
			{
				affectedPoses.Add(job, null);
			}
			else
			{
				throw new NotImplementedException("Unknown job type: " + job.GetType().Name);
			}

			if (OnJobCreated != null)
				OnJobCreated(Owner, job);
		}
		private void RemoveJob(Job job)
		{
			outstandingJobs.Remove(job);

			//Note that one of these dictionaries will just silently fail
			//    when it doesn't find the job.
			affectedPoses.Remove(job);
			affectedMultiPoses.Remove(job);

			if (OnJobDestroyed != null)
				OnJobDestroyed(Owner, job);
		}

		#region Callbacks

		//Note: if you're confused about the web of callbacks here,
		//    there is a flowchart in Billy's Dropbox called "Job Life Cycle.bmp".

		private void Callback_GroupRemoved(LockedSet<Group> groups, Group theGroup)
		{
			if (theGroup == Owner)
			{
				//Remove all outstanding jobs.
				while (outstandingJobs.Count > 0)
					RemoveJob(outstandingJobs.First());
				
				Owner.TheMap.Groups.OnElementRemoved -= Callback_GroupRemoved;
			}
		}

		private void Callback_NewGroupJob(PlayerGroup _owner, Job job)
		{
			AddJob(job);
		}
		private void Callback_CancelGroupJob(PlayerGroup _owner, Job job)
		{
			RemoveJob(job);
		}

		private void Callback_NewUnit(LockedSet<ulong> ownerUnitIDs, ulong newUnitID)
		{
			Unit unit = Owner.TheMap.GetUnit(newUnitID);
			if (unit is PlayerChar)
			{
				PlayerChar playerChar = (PlayerChar)unit;

				playerChar.OnAddCustomJob += Callback_NewUnitJob;
				playerChar.OnRemoveCustomJob += Callback_DestroyUnitJob;

				//Call the "Job created" callbacks for every job this PlayerChar already has.
				foreach (Job job in playerChar.CustomJobs)
					Callback_NewUnitJob(playerChar, job);
				if (playerChar.CurrentJob != null)
					Callback_NewUnitJob(playerChar, playerChar.CurrentJob);
			}
		}
		private void Callback_RemoveUnit(LockedSet<ulong> ownerUnitIDs, ulong removedUnitID)
		{
			Unit unit = Owner.TheMap.GetUnit(removedUnitID);
			if (unit is PlayerChar)
			{
				PlayerChar playerChar = (PlayerChar)unit;

				//Raise "Callback_DestroyUnitJob" for all outstanding jobs,
				//    then remove all callbacks.
				foreach (Job job in playerChar.CustomJobs)
					Callback_DestroyUnitJob(playerChar, job);
				if (playerChar.CurrentJob != null)
					Callback_DestroyUnitJob(playerChar, playerChar.CurrentJob);

				playerChar.OnAddCustomJob -= Callback_NewUnitJob;
				playerChar.OnRemoveCustomJob -= Callback_DestroyUnitJob;
			}
		}

		private void Callback_NewUnitJob(PlayerChar owner, Job job)
		{
			if (!outstandingJobs.Contains(job))
				AddJob(job);
		}
		private void Callback_DestroyUnitJob(PlayerChar owner, Job job)
		{
			if (outstandingJobs.Contains(job))
				RemoveJob(job);
		}

		private void Callback_JobFinished(Job job, bool success, string alertMsg)
		{
			if (outstandingJobs.Contains(job))
				RemoveJob(job);
		}

		#endregion
	}
}
