using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyData;
using UnityEngine;


namespace GameLogic.Units.Player_Char
{
	public class Job_MoveToPos : Job
	{
		public Stat<Vector2i, Job_MoveToPos> TargetPos;


		public Job_MoveToPos(Vector2i targetPos, bool isEmergency, Map theMap)
			: base(isEmergency, theMap)
		{
			TargetPos = new Stat<Vector2i, Job_MoveToPos>(this, targetPos);
		}


		public override IEnumerable TakeTurn()
		{
			TryMoveToPos_Status moveStatus = new TryMoveToPos_Status();
			foreach (object o in TryMoveToPos(new Pathfinding.Goal<Vector2i>(TargetPos), moveStatus))
				yield return o;

			switch (moveStatus.CurrentState)
			{
				case TryMoveToPos_States.EnRoute:
					//Nothing to do; still moving.
					break;

				case TryMoveToPos_States.NoPath:
					//Cancel the job.
					EndJob(false, Localization.Get("NO_PATH_JOB"));
					yield break;

				case TryMoveToPos_States.Finished:
					//End the job successfully.
					EndJob(true);
					yield break;

				default: throw new NotImplementedException(moveStatus.CurrentState.ToString());
			}
		}

		//Serialization:
		public override Types ThisType { get { return Types.MoveToPos; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);
			writer.Vec2i(TargetPos.Value, "targetPos");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);
			TargetPos.Value = reader.Vec2i("targetPos");
		}

		//Give each type of Job a unique hash code.
		public override int GetHashCode()
		{
			return 11231;
		}
	}
}
