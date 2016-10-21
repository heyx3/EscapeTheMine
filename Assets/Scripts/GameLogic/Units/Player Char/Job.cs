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
		/// <summary>
		/// Set to null if no unit has taken this job yet.
		/// </summary>
		public Stat<PlayerChar, Job> Owner;

		/// <summary>
		/// If true, this job has a much higher priority.
		/// </summary>
		public Stat<bool, Job> IsEmergency;

		
		/// <summary>
		/// Raised when a PlayerChar successfully finishes this job.
		/// </summary>
		public event Action<Job> OnJobFinished;


		public Job(bool isEmergency, PlayerChar owner = null)
		{
			Owner = new Stat<PlayerChar, Job>(this, owner);
			IsEmergency = new Stat<bool, Job>(this, IsEmergency);
		}


		/// <summary>
		/// Makes the PlayerChar that owns this job take a turn working on this job.
		/// Acts as a coroutine.
		/// </summary>
		public abstract System.Collections.IEnumerable TakeTurn();

		/// <summary>
		/// Raises the "OnJobFinished" event.
		/// Unfortunately, C# doesn't let sub-classes raise base class events manually.
		/// </summary>
		protected void FinishJob()
		{
			if (OnJobFinished != null)
				OnJobFinished(this);
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
		public static Job Read(MyData.Reader reader, string name)
		{
			Types jType = (Types)reader.Int(name + "_Type");

			Job j = null;
			switch (jType)
			{
				case Types.MoveToPos: j = new Job_MoveToPos(Vector2i.Zero, false); break;
				default: throw new NotImplementedException(jType.ToString());
			}

			reader.Structure(j, name + "_Value");
			return j;
		}
		public virtual void WriteData(MyData.Writer writer)
		{
			writer.Bool(IsEmergency, "isEmergency");
		}
		public virtual void ReadData(MyData.Reader reader)
		{
			IsEmergency.Value = reader.Bool("isEmergency");
		}

		#endregion
	}
}
