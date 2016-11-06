using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyData;

namespace GameLogic.Groups
{
    /// <summary>
    /// Contains a 
    /// </summary>
    public class PlayerGroup : Group
    {
        //TODO: Job system.

        public override IEnumerable TakeTurn()
        {
            foreach (object o in base.TakeTurn())
                yield return o;
        }


        public PlayerGroup(Map theMap) : base(theMap, Consts.TurnPriority_Player) { }


        #region Serialization

        public override Types MyType { get { return Types.PlayerChars; } }

        public override void WriteData(Writer writer)
        {
            base.WriteData(writer);
        }
        public override void ReadData(Reader reader)
        {
            base.ReadData(reader);
        }

        #endregion
    }
}
