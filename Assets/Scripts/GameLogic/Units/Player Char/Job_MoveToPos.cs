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
			//Find the best path to the destination from here.
			//Start the search from scratch in case anything changed since the last turn.
			List<Vector2i> path =
				TheMap.Value.FindPath(Owner.Value.Pos,
									  new Pathfinding.Goal<Vector2i>(TargetPos),
									  PlayerChar.AStarEdgeCalc);
			
			//If there is no valid path, give up.
			if (path == null)
			{
				EndJob(false, "Couldn't find path"); //TODO: Localize.
				yield break;
			}

			//Move some number of spaces along the path.
			int nMoves = Math.Min(Consts.MovesPerTurn, path.Count);
			for (int i = 0; i < nMoves; ++i)
			{
				Owner.Value.Pos.Value = path[i];
				yield return null;
			}

			//If we made it to the destination, quit.
			if (Owner.Value.Pos == TargetPos.Value)
				EndJob(true);

			yield break;
		}

		//Serialization:
		public override Types ThisType {  get { return Types.MoveToPos; } }
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
