using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using PlayerConsts = GameLogic.Units.Player_Char.Consts;


namespace GameLogic.Units.Player_Char
{
	/// <summary>
	/// A thing that should be done by a PlayerChar.
	/// </summary>
	public abstract class Job : MyData.IReadWritable
	{
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
			IsEmergency = new Stat<bool, Job>(this, isEmergency);
			SuccessMessage = new Stat<string, Job>(this, successMessage);
			TheMap = new Stat<Map, Job>(this, theMap);
		}


		/// <summary>
		/// Makes the PlayerChar that owns this job take a turn working on this job.
		/// Acts as a coroutine.
		/// </summary>
		public abstract System.Collections.IEnumerable TakeTurn();
		
		protected enum TryMoveToPos_States
		{
			EnRoute,
			Finished,
			NoPath,
		}
		protected class TryMoveToPos_Status
		{
			public int NActualMoves;
			public TryMoveToPos_States CurrentState;
		}
		/// <summary>
		/// A coroutine that attempts to make the owning PlayerChar move to the given position.
		/// </summary>
		/// <param name="nMoves">
		/// The maximum number of moves to take,
		/// or -1 to take as many as he is allowed in one turn.
		/// </param>
		/// <param name="nActualMoves">
		/// The number of actual moves the PlayerChar took by the end of this.
		/// He will stop early if he reaches the goal.
		/// </param>
		/// <param name="endState">
		/// Whether the PlayerChar made it to the goal, is still en route,
		///     or couldn't even find a path.
		/// </param>
		protected System.Collections.IEnumerable TryMoveToPos(Pathfinding.Goal<Vector2i> goal,
															  TryMoveToPos_Status outStatus,
															  int nMoves = -1)
		{
			outStatus.NActualMoves = 0;
			outStatus.CurrentState = TryMoveToPos_States.EnRoute;

			//Try to find the best path.
			List<Vector2i> path = Owner.Value.FindPath(goal);
			if (path == null)
			{
				outStatus.CurrentState = TryMoveToPos_States.NoPath;
				yield break;
			}

			//Move along the path.
			if (nMoves == -1)
				nMoves = PlayerConsts.MovesPerTurn;
			nMoves = Math.Min(nMoves, path.Count);
			for (int i = 0; i < nMoves; ++i)
			{
				Owner.Value.Pos.Value = path[i];
				outStatus.NActualMoves += 1;

				//If we're at the end of the path, exit.
				if (goal.IsValidEnd(Owner.Value.Pos))
				{
					outStatus.CurrentState = TryMoveToPos_States.Finished;
					yield break;
				}

				yield return null;
			}

			//We didn't make it to the end of the path.
			outStatus.CurrentState = TryMoveToPos_States.EnRoute;
		}

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
			Mine,
			Sleep,
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
				case Types.Mine: j = new Job_Mine(new HashSet<Vector2i>(), false, theMap); break;
				case Types.Sleep: new Job_Sleep(false, theMap); break;
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
