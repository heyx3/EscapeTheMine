using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace GameLogic.Units.Player_Char
{
	/// <summary>
	/// A thing that should be done by a PlayerChar.
	/// </summary>
	public abstract class Job : MyData.IReadWritable
	{
		public static Consts PlayerConsts {  get { return Player_Char.Consts.Instance; } }

		/// <summary>
		/// The PlayerChar that is actively doing this job.
		/// Is "null" if no PlayerChar is actively doing this job.
		/// </summary>
		public Stat<PlayerChar, Job> Owner;
		/// <summary>
		/// The map this job belongs to.
		/// </summary>
		public Stat<Map, Job> TheMap;

		/// <summary>
		/// If true, this job has a much higher priority.
		/// </summary>
		public Stat<bool, Job> IsEmergency;

		/// <summary>
		/// If not null, then a successful completion of this job will use the given message.
		/// This allows particularly important jobs to notify the player when they're done.
		/// </summary>
		public Stat<string, Job> SuccessMessage;

		
		/// <summary>
		/// Raised when a PlayerChar stops doing this job.
		/// The second parameter represents whether the job was successful.
		/// The third parameter will contain a message about the job finishing,
		///     or "null" if it is not important.
		/// </summary>
		public event Action<Job, bool, string> OnJobFinished;

		
		public Job(bool isEmergency, Map theMap, string successMessage = null)
		{
			Owner = new Stat<PlayerChar, Job>(this, null);
			IsEmergency = new Stat<bool, Job>(this, IsEmergency);
			SuccessMessage = new Stat<string, Job>(this, successMessage);
			TheMap = new Stat<Map, Job>(this, theMap);
		}


		/// <summary>
		/// Makes the PlayerChar that owns this job take a turn working on this job.
		/// Acts as a coroutine.
		/// </summary>
		public abstract System.Collections.IEnumerable TakeTurn();

		/// <summary>
		/// Raises the "OnJobFinished" event.
		/// Called by the player if a job was interrupted, or the job itself if it's done.
		/// </summary>
		/// <param name="wasFailure">
		/// If false, and "message" is null, then the "SuccessMessage" will be used in the event.
		/// </param>
		/// <param name="message">
		/// If the job's end is notable, this should contain a message about it.
		/// Otherwise, pass "null".
		/// </param>
		/// <remarks>
		/// Unfortunately, C# doesn't let sub-classes raise base class events manually.
		/// </remarks>
		public void EndJob(bool wasSuccess, string message = null)
		{
			if (wasSuccess && message == null)
				message = SuccessMessage.Value;

			if (OnJobFinished != null)
				OnJobFinished(this, wasSuccess, message);
		}


		#region Serialization

		public enum Types
		{
			MoveToPos = 0,
		}
		public abstract Types ThisType { get; }

		public static void Write(MyData.Writer writer, Job job, string name)
		{
			writer.Int((int)job.ThisType, name + "_Type");
			writer.Structure(job, name + "_Value");
		}
		public static Job Read(MyData.Reader reader, string name, Map theMap)
		{
			Types jType = (Types)reader.Int(name + "_Type");

			Job j = null;
			switch (jType)
			{
				case Types.MoveToPos: j = new Job_MoveToPos(Vector2i.Zero, false, theMap); break;
				default: throw new NotImplementedException(jType.ToString());
			}

			reader.Structure(j, name + "_Value");
			return j;
		}
		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Bool(IsEmergency, "isEmergency");
			writer.String(SuccessMessage, "successMessage");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			IsEmergency.Value = reader.Bool("isEmergency");
			SuccessMessage.Value = reader.String("successMessage");
		}

		#endregion
	}
}
