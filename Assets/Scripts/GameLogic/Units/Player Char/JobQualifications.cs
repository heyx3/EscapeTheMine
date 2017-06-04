using System;
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
		/// Whether this PlayerChar will accept global mining jobs.
		/// </summary>
        public Stat<bool, JobQualifications> AcceptJob_Mining { get; private set; }
		/// <summary>
		/// Whether this PlayerChar will accept global building jobs.
		/// </summary>
		public Stat<bool, JobQualifications> AcceptJob_Build { get; private set; }

		/// <summary>
		/// The largest "MoveToPos" job this PlayerChar will accept,
		///     in terms of the number of moves needed.
		/// Zero is considered to be "never".
		/// "int.MaxValue" is considered to be "always".
		/// </summary>
		public Stat<int, JobQualifications> MoveToPos_MaxDist { get; private set; }

		public Stat<float, JobQualifications> SleepWhen_EnergyBelow { get; private set; }
		public Stat<float, JobQualifications> SleepWhen_HealthBelow { get; private set; }

        public Stat<bool, JobQualifications> GrowingUpIsEmergency { get; private set; }
		public Stat<bool, JobQualifications> AvoidEnemiesWhenPathing { get; private set; }

		//TODO: Stat for how to respond to enemies (attack if within distance, run if within distance, ignore), plus Stat<bool> for whether enemies are considered an emergency.


		public JobQualifications(PlayerChar owner)
		{
			UnityEngine.Assertions.Assert.IsTrue(owner != null);
			Owner = owner;

			MoveToPos_MaxDist = new Stat<int, JobQualifications>(this, int.MaxValue);

            AcceptJob_Mining = new Stat<bool, JobQualifications>(this, true);
			AcceptJob_Build = new Stat<bool, JobQualifications>(this, true);

			SleepWhen_EnergyBelow =
				new Stat<float, JobQualifications>(this, Consts.DefaultSeekBedEnergy);
			SleepWhen_HealthBelow =
				new Stat<float, JobQualifications>(this, Consts.DefaultSeekBedHealth);

            GrowingUpIsEmergency = new Stat<bool, JobQualifications>(this, false);

			AvoidEnemiesWhenPathing = new Stat<bool, JobQualifications>(this, true);
		}
		public JobQualifications(PlayerChar owner, JobQualifications copyFrom)
			: this(owner)
		{
			MoveToPos_MaxDist.Value = copyFrom.MoveToPos_MaxDist;

            AcceptJob_Mining.Value = copyFrom.AcceptJob_Mining;
			AcceptJob_Build.Value = copyFrom.AcceptJob_Build;

			SleepWhen_EnergyBelow.Value = copyFrom.SleepWhen_EnergyBelow;
			SleepWhen_HealthBelow.Value = copyFrom.SleepWhen_HealthBelow;

            GrowingUpIsEmergency.Value = copyFrom.GrowingUpIsEmergency;

			AvoidEnemiesWhenPathing.Value = copyFrom.AvoidEnemiesWhenPathing;
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
							var path = Owner.FindPath(goal, AvoidEnemiesWhenPathing);
							if (path != null && path.Count < maxDist)
								return j;
						}
					} break;

					case Job.Types.Mine:
						if (AcceptJob_Mining)
							return j;
					break;

					case Job.Types.BuildBed:
						if (AcceptJob_Build)
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
			writer.Bool(AcceptJob_Build, "acceptJob_Build");

			writer.Float(SleepWhen_EnergyBelow, "sleepWhen_EnergyBelow");
			writer.Float(SleepWhen_HealthBelow, "sleepWhen_HealthBelow");

			writer.Bool(GrowingUpIsEmergency, "growingUpIsEmergency");
		}
		public void ReadData(MyData.Reader reader)
		{
			MoveToPos_MaxDist.Value = reader.Int("moveToPos_MaxDist");

			AcceptJob_Mining.Value = reader.Bool("acceptJob_Mining");
			AcceptJob_Build.Value = reader.Bool("acceptJob_Build");

			SleepWhen_EnergyBelow.Value = reader.Float("sleepWhen_EnergyBelow");
			SleepWhen_HealthBelow.Value = reader.Float("sleepWhen_HealthBelow");

			GrowingUpIsEmergency.Value = reader.Bool("growingUpIsEmergency");
		}
	}
}
