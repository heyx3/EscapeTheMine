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


		public Job_MoveToPos(Vector2i targetPos, bool isEmergency, PlayerChar owner = null)
			: base(isEmergency, owner)
		{
			TargetPos = new Stat<Vector2i, Job_MoveToPos>(this, targetPos);
		}


		public override IEnumerable TakeTurn()
		{
			//Use A* to find the best path to the destination from here.
			//TODO: Implement. Still need to define A* classes for the Map. Each Unit type will need its own heuristics.

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
