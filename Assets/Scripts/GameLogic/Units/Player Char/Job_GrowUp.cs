using System;
using System.Collections;
using System.Collections.Generic;


namespace GameLogic.Units.Player_Char
{
    public class Job_GrowUp : Job
    {
        public Job_GrowUp(bool isEmergency, Map theMap)
            : base(isEmergency, theMap) { }


        public override IEnumerable TakeTurn()
        {
            yield return null;

            Owner.Value.AdultMultiplier.Value += Consts.MaturityIncreasePerTurn;

            if (Owner.Value.IsAdult)
            {
                Owner.Value.AdultMultiplier.Value = 1.0f;
                EndJob(true);
            }
        }

        //Serialization:
        public override Types ThisType { get { return Types.GrowUp; } }

        //Give each type of job a unique hash code.
        public override int GetHashCode()
        {
            return 23165347;
        }
    }
}
