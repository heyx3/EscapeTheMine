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

		private Pathfinding.PathFinder<Vector2i> pathFinder;
		private List<Vector2i> path = new List<Vector2i>();


		public Job_MoveToPos(Vector2i targetPos, bool isEmergency, PlayerChar owner = null)
			: base(isEmergency, owner)
		{
			TargetPos = new Stat<Vector2i, Job_MoveToPos>(this, targetPos);
			pathFinder = new Pathfinding.PathFinder<Vector2i>(null, PlayerChar.AStarEdgeCalc);
		}


		public override IEnumerable TakeTurn()
		{
			//If we made it to the destination, quit.
			if (Owner.Value.Pos == TargetPos.Value)
				FinishJob();

			//Use A* to find the best path to the destination from here.
			pathFinder.Graph = TheMap.PathingGraph;
			bool foundEnd = pathFinder.FindPath(Owner.Value.Pos,
												new Pathfinding.Goal<Vector2i>(TargetPos),
												float.PositiveInfinity, false, path);

			//If there is no valid path, give up.
			if (!foundEnd)
			{
				FinishJob();
				//TODO: Make announcement that a path couldn't be found.
				yield break;
			}
			else
			{
				UnityEngine.Assertions.Assert.IsTrue(path.Count > 0);
			}

			//Move to the next spot in the path.
			Owner.Value.Pos.Value = path[0];

			//If we made it to the destination, quit.
			if (Owner.Value.Pos == TargetPos.Value)
				FinishJob();

			yield break;
		}

		//Serialization:
		public override Types ThisType {  get { return Types.MoveToPos; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);
			writer.Int(TargetPos.Value.x, "targetPos_X");
			writer.Int(TargetPos.Value.y, "targetPos_Y");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);
			TargetPos.Value = new Vector2i(reader.Int("targetPos_X"),
										   reader.Int("targetPos_Y"));
		}
	}
}
