using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MyData;
using System.Collections;


namespace GameLogic.Units.Player_Char
{
	public class Job_BuildBed : Job
	{
		/// <summary>
		/// The tile the bed should be built on.
		/// </summary>
		public Vector2i Tile { get; private set; }

		/// <summary>
		/// The number of turns left until the PlayerChar finishes this job.
		/// </summary>
		public int TurnsLeft { get; private set; }

		/// <summary>
		/// If true, the PlayerChar currently doing this job
		///     is already at the spot and building the bed.
		/// </summary>
		public bool IsCurrentlyBuilding { get; private set; }

		/// <summary>
		/// The Group that the new bed will belong to.
		/// </summary>
		public ulong OwnerGroupId { get; private set; }


		public Job_BuildBed(Vector2i tile, ulong ownerGroupID, bool isEmergency, Map theMap)
			: base(isEmergency, theMap)
		{
			Tile = tile;
			OwnerGroupId = ownerGroupID;

			IsCurrentlyBuilding = false;

			//When somebody takes or gives up this job, reset it.
			Owner.OnChanged += (thisJob, oldVal, newVal) =>
			{
				IsCurrentlyBuilding = false;
			};
		}


		public override IEnumerable TakeTurn()
		{
			//If the tile no longer supports building a bed, stop the job.
			if (!TheMap.Value.Tiles[Tile].IsBuildableOn() ||
				TheMap.Value.AnyUnitsAt(Tile, u => u.BlocksStructures))
			{
				EndJob(false, Localization.Get("CANT_BUILD_ON_TILE"));
				yield break;
			}

			//If we're not currently building, path to the tile to build from.
			if (!IsCurrentlyBuilding)
			{
				var movementStatus = new TryMoveToPos_Status();
				var goal = new Pathfinding.Goal<Vector2i>(Tile);
				foreach (object o in TryMoveToPos(goal, movementStatus))
					yield return o;

				switch (movementStatus.CurrentState)
				{
					case TryMoveToPos_States.Finished:
						//Start building.
						IsCurrentlyBuilding = true;
						TurnsLeft = Consts.TurnsToBuildStructure(Owner.Value.Strength,
																 Owner.Value.AdultMultiplier);
						break;

					case TryMoveToPos_States.EnRoute:
						//End the turn.
						yield break;

					case TryMoveToPos_States.NoPath:
						//Cancel the job.
						EndJob(false, Localization.Get("NO_PATH_JOB"));
						yield break;
				}
			}

			//Build.
			if (TurnsLeft > 0)
			{
				TurnsLeft -= 1;
				yield break;
			}

			//Create the bed.
			var bed = new Bed(TheMap, OwnerGroupId, Tile);
			TheMap.Value.AddUnit(bed);

			EndJob(true);
		}


		//Serialization:
		public override Types ThisType { get { return Types.BuildBed; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);

			writer.Vec2i(Tile, "tileToBuildOn");
			writer.Int(TurnsLeft, "turnsLeft");
			writer.UInt64(OwnerGroupId, "ownerGroupId");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);

			Tile = reader.Vec2i("tileToBuildOn");
			TurnsLeft = reader.Int("turnsLeft");
			OwnerGroupId = reader.UInt64("ownerGroupId");

			IsCurrentlyBuilding = false;
		}

		//Give each type of job a unique hash code.
		public override int GetHashCode()
		{
			return 1337654141;
		}
	}
}
