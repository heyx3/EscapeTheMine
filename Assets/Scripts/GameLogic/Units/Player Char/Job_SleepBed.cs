using System;
using System.Collections.Generic;
using System.Linq;

using MyData;
using System.Collections;

namespace GameLogic.Units.Player_Char
{
	public class Job_SleepBed : Job
	{
		public static Bed FirstFriendlyBedAtPos(Vector2i tilePos, PlayerChar pChar)
		{
			foreach (Unit u in pChar.TheMap.GetUnits(tilePos))
			{
				if (u.MyType == Unit.Types.Bed &&
					(u.MyGroupID == pChar.MyGroupID || u.MyGroup.IsAllyTo(pChar.MyGroup)))
				{
					return (Bed)u;
				}
			}

			return null;
		}


		/// <summary>
		/// The ID of the bed currently being slept in,
		///     or nothing if the PlayerChar isn't sleeping yet.
		/// </summary>
		public ulong? BedID { get; private set; }

		/// <summary>
		/// If it exists, this is the specific bed the PlayerChar must sleep in.
		/// Otherwise, he will just find the closest bed.
		/// </summary>
		public Stat<ulong?, Job_SleepBed> TargetBedID { get; private set; }
		

		public Job_SleepBed(bool isEmergency, Map theMap, Bed targetBed = null)
			: base(isEmergency, theMap)
		{
			BedID = null;

			TargetBedID = new Stat<ulong?, Job_SleepBed>(this, null);
			if (targetBed != null)
				TargetBedID.Value = targetBed.ID;

			//When the job ends, make sure the PlayerChar is no longer sleeping.
			OnJobFinished += (thisJob, wasSuccessful, msg) =>
			{
				if (BedID.HasValue)
					StopSleeping();
			};
		}


		private HashSet<Vector2i> friendlyBedPoses = new HashSet<Vector2i>();
		public override IEnumerable TakeTurn()
		{
			//If the unit's energy and health is high enough, end the job.
			if (Owner.Value.Health.Value >= Consts.Max_Health &&
				Owner.Value.Energy.Value >= Consts.MaxEnergy(Owner.Value.Strength))
			{
				EndJob(true);
			}
			//Otherwise, if we're not sleeping yet, path to a bed.
			else if (!BedID.HasValue)
			{
				//Get all friendly beds.
				friendlyBedPoses.Clear();
				foreach (Bed b in TheMap.Value.GetUnits(Unit.Types.Bed).Cast<Bed>())
					if (b.MyGroup == Owner.Value.MyGroup || b.MyGroup.IsAllyTo(Owner.Value.MyGroup))
						friendlyBedPoses.Add(b.Pos);

				//If there are no friendly beds, cancel the job.
				if (friendlyBedPoses.Count == 0)
				{
					EndJob(false, Localization.Get("NO_BED"));
					yield break;
				}

				//Either move towards a specific bed, or towards any friendly bed.
				var goal = new Pathfinding.Goal<Vector2i>();
				if (TargetBedID.Value.HasValue)
					goal.SpecificGoal = TheMap.Value.GetUnit(TargetBedID.Value.Value).Pos.Value;
				else
					goal.GeneralGoal = friendlyBedPoses.Contains;

				//Move to the goal.
				TryMoveToPos_Status moveStatus = new TryMoveToPos_Status();
				foreach (object o in TryMoveToPos(goal, moveStatus))
					yield return o;
				switch (moveStatus.CurrentState)
				{
					case TryMoveToPos_States.Finished:
						//Start sleeping.
						StartSleeping((Bed)TheMap.Value.FirstUnitAt(Owner.Value.Pos, u => u is Bed));
						break;

					case TryMoveToPos_States.EnRoute:
						//End the turn.
						yield break;

					case TryMoveToPos_States.NoPath:
						//Cancel the job.
						EndJob(false, Localization.Get("NO_PATH_JOB"));
						break;

					default: throw new NotImplementedException(moveStatus.CurrentState.ToString());
				}
			}
		}

		private void StartSleeping(Bed b)
		{
			b.SleepingUnitsByID.Add(Owner.Value.ID);
			BedID = b.ID;

			b.OnKilled += Callback_BedDestroyed;
		}
		private void StopSleeping()
		{
			Bed b = (Bed)TheMap.Value.GetUnit(BedID.Value);
			b.SleepingUnitsByID.Remove(Owner.Value.ID);
			BedID = null;

			b.OnKilled -= Callback_BedDestroyed;
		}

		private void Callback_BedDestroyed(Unit bed, Map theMap)
		{
			BedID = null;
		}

		//Serialization:
		public override Types ThisType { get { return Types.SleepBed; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);

			writer.Bool(BedID.HasValue, "isSleeping");
			if (BedID.HasValue)
				writer.UInt64(BedID.Value, "bedID");

			writer.Bool(TargetBedID.Value.HasValue, "hasTargetBed");
			if (TargetBedID.Value.HasValue)
				writer.UInt64(TargetBedID.Value.Value, "targetBedID");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);

			if (reader.Bool("isSleeping"))
				BedID = reader.UInt64("bedID");
			else
				BedID = null;

			if (reader.Bool("hasTargetBed"))
				TargetBedID.Value = reader.UInt64("targetBedID");
			else
				TargetBedID.Value = null;
		}

        //Give each type of job a unique hash code.
        public override int GetHashCode()
        {
            return 46587;
        }
    }
}
