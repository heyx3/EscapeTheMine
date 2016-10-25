using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GameLogic.Units.Player_Char;


namespace GameLogic
{
	/// <summary>
	/// Holds all jobs that were designated for any PlayerChar to grab.
	/// </summary>
	public class GlobalJobs : IEnumerable<Job>, MyData.IReadWritable
	{
		/// <summary>
		/// Raised when a new job was added to this collection.
		/// </summary>
		public event Action<GlobalJobs, Job> OnNewJob;
		/// <summary>
		/// Raised when a job was taken from this collection by a PlayerChar.
		/// Note that the PlayerChar did not actually start the job yet,
		///     although it's presumably about to.
		/// </summary>
		public event Action<GlobalJobs, Job, Units.PlayerChar> OnJobTaken;
		/// <summary>
		/// Raised when a job was removed from this collection.
		/// </summary>
		public event Action<GlobalJobs, Job> OnJobCanceled;

		public Map TheMap { get; private set; }

		private HashSet<Job> normalJobs = new HashSet<Job>();
		private HashSet<Job> emergencyJobs = new HashSet<Job>();


		public GlobalJobs(Map theMap) { TheMap = theMap; }


		/// <summary>
		/// Adds the given job to this collection.
		/// Returns whether it already existed in this collection.
		/// </summary>
		public bool Add(Job j)
		{
			if ((j.IsEmergency && emergencyJobs.Add(j)) ||
				(!j.IsEmergency && normalJobs.Add(j)))
			{
				InitJob(j);

				if (OnNewJob != null)
					OnNewJob(this, j);
				return true;
			}
			else
			{
				return false;
			}
		}
		/// <summary>
		/// Removes the given job from this collection, if it exists.
		/// Returns whether it existed.
		/// </summary>
		public bool Cancel(Job j)
		{
			if ((j.IsEmergency && emergencyJobs.Remove(j)) ||
				(!j.IsEmergency && normalJobs.Remove(j)))
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
		/// Removes all jobs from this collection.
		/// </summary>
		public void Clear()
		{
			while (normalJobs.Count > 0)
				Cancel(normalJobs.First());
			while (emergencyJobs.Count > 0)
				Cancel(emergencyJobs.First());
		}

		/// <summary>
		/// Finds the most urgent job that the given PlayerChar can do,
		///     removes it from this collection, and returns it.
		/// Returns "null" if no applicable jobs exist.
		/// </summary>
		/// <param name="onlyEmergencies">
		/// Only looks at emergency jobs.
		/// </param>
		public Job Take(Units.PlayerChar pChar, bool onlyEmergencies)
		{
			//Find the most urgent job that's applicable to the PlayerChar.
			HashSet<Job> jobCollection = emergencyJobs;
			Job toTake = pChar.Career.ChooseApplicable(emergencyJobs);
			if (toTake == null && !onlyEmergencies)
			{
				jobCollection = normalJobs;
				toTake = pChar.Career.ChooseApplicable(normalJobs);
			}

			if (toTake != null)
			{
				//Remove the job from this collection.
				DeInitJob(toTake);
				jobCollection.Remove(toTake);
				if (OnJobTaken != null)
					OnJobTaken(this, toTake, pChar);

				toTake.OnJobFinished += Callback_TakenJobFinished;
			}

			return toTake;
		}

		private void Callback_TakenJobFinished(Job j, bool failed, string msg)
		{
			//If this job was a failure, add it back into this collection.

			j.OnJobFinished -= Callback_TakenJobFinished;

			if (failed)
				Add(j);
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


		//IEnumerable<> interface.
		public IEnumerator<Job> GetEnumerator() { return normalJobs.Concat(emergencyJobs).GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return normalJobs.Concat(emergencyJobs).GetEnumerator(); }

		//Serialization stuff.
		public void WriteData(MyData.Writer writer)
		{
			writer.Collection<Job, HashSet<Job>>(normalJobs, "normalJobs",
												 (wr, val, name) => Job.Write(wr, val, name));
			writer.Collection<Job, HashSet<Job>>(emergencyJobs, "emergencyJobs",
												 (wr, val, name) => Job.Write(wr, val, name));
		}
		public void ReadData(MyData.Reader reader)
		{
			Clear();

			normalJobs = reader.Collection("normalJobs",
										   (MyData.Reader rd, ref Job outVal, string name) =>
										       { outVal = Job.Read(rd, name, TheMap); },
										   (i) => new HashSet<Job>());
			emergencyJobs = reader.Collection("emergencyJobs",
										      (MyData.Reader rd, ref Job outVal, string name) =>
										          { outVal = Job.Read(rd, name, TheMap); },
										      (i) => new HashSet<Job>());

			foreach (Job j in normalJobs.Concat(emergencyJobs))
				InitJob(j);
		}
	}
}
