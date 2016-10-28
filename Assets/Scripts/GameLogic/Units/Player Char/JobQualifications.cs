﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GameLogic.Units.Player_Char
{
	/// <summary>
	/// Information about which global jobs a PlayerChar is willing to take.
	/// </summary>
	public class JobQualifications : MyData.IReadWritable
	{
		public PlayerChar Owner { get; private set; }

		
		/// <summary>
		/// The largest "MoveToPos" job this PlayerChar will accept,
		///     in terms of the number of moves needed.
		/// Zero is considered to be "never".
		/// "int.MaxValue" is considered to be "always".
		/// </summary>
		public Stat<int, JobQualifications> MoveToPos_MaxDist;


		public JobQualifications(PlayerChar owner)
		{
			UnityEngine.Assertions.Assert.IsTrue(owner != null);
			Owner = owner;
			
			MoveToPos_MaxDist = new Stat<int, JobQualifications>(this, int.MaxValue);
		}
		public JobQualifications(PlayerChar owner, JobQualifications copyFrom)
			: this(owner)
		{
			MoveToPos_MaxDist.Value = copyFrom.MoveToPos_MaxDist;
		}

		
		/// <summary>
		/// Gets the first applicable job from the given set.
		/// Returns null if none were applicable.
		/// </summary>
		public Job ChooseApplicable(HashSet<Job> jobs)
		{
			foreach (Job j in jobs)
			{
				if (j is Job_MoveToPos)
				{
					Job_MoveToPos jMove = (Job_MoveToPos)j;

					//Edge-case: the PlayerChar is already there.
					if (Owner.Pos.Value == jMove.TargetPos.Value)
						return j;

					//This job is doable if the PlayerChar is not too far away
					//    and can actually path to the object.
					int maxDist = MoveToPos_MaxDist;
					if (maxDist != 0)
					{
						var goal = new Pathfinding.Goal<Vector2i>(jMove.TargetPos);
						List<Vector2i> path = Owner.TheMap.FindPath(Owner.Pos, goal, Graph.AStarEdgeCalc);
						if (path != null && path.Count < maxDist)
							return j;
					}
				}
				else
				{
					throw new NotImplementedException(j.GetType().ToString());
				}
			}

			return null;
		}

		public void WriteData(MyData.Writer writer)
		{
			writer.Int(MoveToPos_MaxDist, "moveToPos_MaxDist");
		}
		public void ReadData(MyData.Reader reader)
		{
			MoveToPos_MaxDist.Value = reader.Int("moveToPos_MaxDist");
		}
	}
}