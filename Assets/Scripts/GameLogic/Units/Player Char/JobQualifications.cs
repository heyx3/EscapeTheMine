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

        //TODO: Stat<bool> for whether to avoid enemies when pathing normally. Also move enemy avoidance into Unit base class heuristic, and provide an abstract "ShouldAvoidEnemies" property.
		//TODO: Stat<bool> for whether running from enemies should be an automatic job (and another Stat<bool> for whether it's an emergency).

        public Stat<bool, JobQualifications> AcceptJob_Mining, AcceptJob_BuildBed;

		public Stat<float, JobQualifications> SleepWhen_EnergyBelow, SleepWhen_HealthBelow;

        public Stat<bool, JobQualifications> GrowingUpIsEmergency;


		public JobQualifications(PlayerChar owner)
		{
			UnityEngine.Assertions.Assert.IsTrue(owner != null);
			Owner = owner;
			
			MoveToPos_MaxDist = new Stat<int, JobQualifications>(this, int.MaxValue);

            AcceptJob_Mining = new Stat<bool, JobQualifications>(this, true);
			AcceptJob_BuildBed = new Stat<bool, JobQualifications>(this, true);

			SleepWhen_EnergyBelow =
				new Stat<float, JobQualifications>(this, Consts.DefaultSeekBedEnergy);
			SleepWhen_HealthBelow =
				new Stat<float, JobQualifications>(this, Consts.DefaultSeekBedHealth);

            GrowingUpIsEmergency =
                new Stat<bool, JobQualifications>(this, false);
		}
		public JobQualifications(PlayerChar owner, JobQualifications copyFrom)
			: this(owner)
		{
			MoveToPos_MaxDist.Value = copyFrom.MoveToPos_MaxDist;

            AcceptJob_Mining.Value = copyFrom.AcceptJob_Mining;
			AcceptJob_BuildBed.Value = copyFrom.AcceptJob_BuildBed;

			SleepWhen_EnergyBelow.Value = copyFrom.SleepWhen_EnergyBelow;
			SleepWhen_HealthBelow.Value = copyFrom.SleepWhen_HealthBelow;

            GrowingUpIsEmergency.Value = copyFrom.GrowingUpIsEmergency;
		}

		
		/// <summary>
		/// Gets the first applicable job from the given set.
		/// Returns null if none were applicable.
		/// </summary>
		public Job ChooseApplicable(HashSet<Job> jobs)
		{
			foreach (Job j in jobs)
			{
				switch (j.ThisType)
				{
					case Job.Types.MoveToPos: {
						var jMove = (Job_MoveToPos)j;
						
						//Edge-case: the PlayerChar is already there, and doesn't have to do anything.
						if (Owner.Pos.Value == jMove.TargetPos.Value)
							return j;

						//This job is doable if the PlayerChar is not too far away
						//    and can actually path to the object.
						int maxDist = MoveToPos_MaxDist;
						if (maxDist != 0)
						{
							var goal = new Pathfinding.Goal<Vector2i>(jMove.TargetPos);
							var path = Owner.FindPath(goal);
							if (path != null && path.Count < maxDist)
								return j;
						}
					} break;

					case Job.Types.Mine:
						if (AcceptJob_Mining)
							return j;
					break;
						
					case Job.Types.BuildBed:
						if (AcceptJob_BuildBed)
							return j;
					break;

					default: throw new NotImplementedException(j.ThisType.ToString());
				}
			}

			return null;
		}

		public void WriteData(MyData.Writer writer)
		{
			writer.Int(MoveToPos_MaxDist, "moveToPos_MaxDist");

			writer.Bool(AcceptJob_Mining, "acceptJob_Mining");
			writer.Bool(AcceptJob_BuildBed, "acceptJob_BuildBed");

			writer.Float(SleepWhen_EnergyBelow, "sleepWhen_EnergyBelow");
			writer.Float(SleepWhen_HealthBelow, "sleepWhen_HealthBelow");

			writer.Bool(GrowingUpIsEmergency, "growingUpIsEmergency");
		}
		public void ReadData(MyData.Reader reader)
		{
			MoveToPos_MaxDist.Value = reader.Int("moveToPos_MaxDist");

			AcceptJob_Mining.Value = reader.Bool("acceptJob_Mining");
			AcceptJob_BuildBed.Value = reader.Bool("acceptJob_BuildBed");

			SleepWhen_EnergyBelow.Value = reader.Float("sleepWhen_EnergyBelow");
			SleepWhen_HealthBelow.Value = reader.Float("sleepWhen_HealthBelow");

			GrowingUpIsEmergency.Value = reader.Bool("growingUpIsEmergency");
		}
	}
}
