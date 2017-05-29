using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyData;
using Job = GameLogic.Units.Player_Char.Job;


namespace GameLogic.Groups
{
    /// <summary>
    /// The player's Group.
	/// Contains a set of global jobs that PlayerChars will take from as needed.
    /// </summary>
    public class PlayerGroup : Group
    {
		/// <summary>
		/// Raised when a new job was added.
		/// </summary>
		public event Action<PlayerGroup, Job> OnNewJob;
		/// <summary>
		/// Raised when a job was taken by a PlayerChar.
		/// Note that the PlayerChar did not actually start the job yet,
		///     although it's presumably about to.
		/// </summary>
		public event Action<PlayerGroup, Job, Units.PlayerChar> OnJobTaken;
		/// <summary>
		/// Raised when a job was removed.
		/// </summary>
		public event Action<PlayerGroup, Job> OnJobCanceled;

		public IEnumerable<Job> NormalJobs {  get { return normalJobs; } }
		public IEnumerable<Job> EmergencyJobs { get { return emergencyJobs; } }

		public Player_Group.JobQueries JobQueries { get; private set; }


		private HashSet<Job> normalJobs = new HashSet<Job>();
		private HashSet<Job> emergencyJobs = new HashSet<Job>();


		public PlayerGroup(Map theMap)
			: base(theMap, Consts.TurnPriority_Player, false)
		{
			JobQueries = new Player_Group.JobQueries(this);
			JobQueries.Init();
		}


        public override IEnumerable TakeTurn()
        {
            foreach (object o in base.TakeTurn())
                yield return o;
        }
		
		/// <summary>
		/// Adds the given job to this group.
		/// Returns whether it already existed in this collection.
		/// </summary>
		public bool AddJob(Job j)
		{
			HashSet<Job> toUse = (j.IsEmergency ? normalJobs : emergencyJobs);
			if (toUse.Add(j))
			{
				InitJob(j);

				if (OnNewJob != null)
					OnNewJob(this, j);

				return false;
			}
			else
			{
				return true;
			}
		}
		/// <summary>
		/// Removes the given job from this group, if it exists.
		/// Returns whether it existed.
		/// </summary>
		public bool Cancel(Job j)
		{
			HashSet<Job> toUse = (j.IsEmergency ? normalJobs : emergencyJobs);
			if (toUse.Remove(j))
			{
				DeInitJob(j);

				if (OnJobCanceled != null)
					OnJobCanceled(this, j);

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Finds the most urgent job that the given PlayerChar can do,
		///     removes it from this group, and returns it.
		/// Returns "null" if no applicable jobs exist.
		/// </summary>
		/// <param name="onlyEmergencies">If true, only emergency jobs will be looked at.</param>
		public Job TakeJob(Units.PlayerChar pChar, bool onlyEmergencies)
		{
			//First search emergency jobs, then (if applicable) non-emergencies.

			HashSet<Job> jobCollection = emergencyJobs;
			Job toTake = pChar.Career.ChooseApplicable(jobCollection);

			if (toTake == null && !onlyEmergencies)
			{
				jobCollection = normalJobs;
				toTake = pChar.Career.ChooseApplicable(jobCollection);
			}

			if (toTake != null)
			{
				//Remove the job from this collection.
				DeInitJob(toTake);
				jobCollection.Remove(toTake);
				if (OnJobTaken != null)
					OnJobTaken(this, toTake, pChar);
			}

			return toTake;
		}
		
		private void InitJob(Job j)
		{
			j.IsEmergency.OnChanged += Callback_JobEmergencyChanged;
		}
		private void DeInitJob(Job j)
		{
			j.IsEmergency.OnChanged -= Callback_JobEmergencyChanged;
		}
		private void Callback_JobEmergencyChanged(Job j, bool oldVal, bool newVal)
		{
			HashSet<Job> oldSet = (oldVal ? emergencyJobs : normalJobs),
						 newSet = (newVal ? emergencyJobs : normalJobs);
			oldSet.Remove(j);
			newSet.Add(j);
		}


        #region Serialization

        public override Types MyType { get { return Types.PlayerChars; } }

        public override void WriteData(Writer writer)
        {
            base.WriteData(writer);

			writer.Collection<Job, HashSet<Job>>(
				normalJobs, "normalJobs",
				(wr, val, name) => Job.Write(wr, val, name));
			writer.Collection<Job, HashSet<Job>>(
				emergencyJobs, "emergencyJobs",
				(wr, val, name) => Job.Write(wr, val, name));
        }
        public override void ReadData(Reader reader)
        {
			//Clear out any current jobs.
			while (normalJobs.Count > 0)
				Cancel(normalJobs.First());
			while (emergencyJobs.Count > 0)
				Cancel(emergencyJobs.First());

			//Stop JobQueries from doing stuff until after deserialization is done.
			JobQueries.Dispose();

            base.ReadData(reader);

			reader.Collection("normalJobs",
							  (MyData.Reader rd, ref Job outval, string name) =>
								  { outval = Job.Read(rd, name, TheMap); },
							  (i) => normalJobs);
			reader.Collection("emergencyJobs",
							  (MyData.Reader rd, ref Job outVal, string name) =>
								  { outVal = Job.Read(rd, name, TheMap); },
							  (i) => emergencyJobs);

			foreach (Job j in normalJobs.Concat(emergencyJobs))
				InitJob(j);
        }

		public override void FinishDeserialization()
		{
			base.FinishDeserialization();
			JobQueries.Init();
		}

		#endregion
	}
}
