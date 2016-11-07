using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace GameLogic
{
    /// <summary>
    /// Represents a close-knit group of units who share
    ///     allies, enemies, and (usually) a common goal.
    /// </summary>
    public abstract class Group : MyData.IReadWritable
    {
        /// <summary>
        /// The higher the number, the earlier this group gets to take its turn.
        /// </summary>
        public Stat<int, Group> TurnPriority;

        public ulong ID { get; private set; }
        public Map TheMap { get; private set; }

        public Unit.UnitSet Units { get; private set; }
        public UlongSet AlliesByID { get; private set; }
        public UlongSet EnemiesByID { get; private set; }


        public Group(Map theMap, int turnPriority)
        {
            TheMap = theMap;
            TurnPriority = new Stat<int, Group>(this, turnPriority);
            Units = new Unit.UnitSet(this);
            AlliesByID = new UlongSet();
            EnemiesByID = new UlongSet();

            //When another Group becomes an ally, remove them from the enemies list
            //    and vice-versa.
            AlliesByID.OnElementAdded += (allies, element) =>
            {
                EnemiesByID.Remove(element);
            };
            EnemiesByID.OnElementAdded += (enemies, element) =>
            {
                AlliesByID.Remove(element);
            };
        }


        /// <summary>
        /// A Unity coroutine that runs the turn of every unit in this group.
        /// Default behavior: For every unit, takes its turn. Adds a separation of
        /// "UnityLogic.Options.UnitTurnInterval" seconds between each turn.
        /// </summary>
        public virtual IEnumerable TakeTurn()
        {
            //Keep careful track of the units we're going to update.
            List<Unit> unitsToUpdate = new List<Unit>(Units);
            int currentUnit = 0;
            Action<LockedSet<Unit>, Unit> onUnitAdded = (units, unit) =>
                unitsToUpdate.Add(unit);
            Action<LockedSet<Unit>, Unit> onUnitRemoved = (units, unit) =>
            {
                int indexOf = unitsToUpdate.IndexOf(unit);

                if (indexOf <= currentUnit)
                    currentUnit -= 1;

                unitsToUpdate.RemoveAt(indexOf);
            };
            Units.OnElementAdded += onUnitAdded;
            Units.OnElementRemoved += onUnitRemoved;

            while (currentUnit < unitsToUpdate.Count)
            {
                //Take the unit's turn.
                foreach (object o in unitsToUpdate[currentUnit].TakeTurn())
                    yield return o;
                currentUnit += 1;

                //Wait for the next turn.
                yield return UnityLogic.Options.UnitTurnIntervalFlag;
            }

            Units.OnElementRemoved -= onUnitAdded;
            Units.OnElementRemoved -= onUnitRemoved;
        }

        public bool IsAllyTo(Group g)
        {
            return AlliesByID.Contains(g.ID);
        }
        public bool IsEnemyTo(Group g)
        {
            return EnemiesByID.Contains(g.ID);
        }

        #region Serialization stuff

        public enum Types
        {
            PlayerChars = 0,
        }
        public abstract Types MyType { get; }

        public virtual void WriteData(MyData.Writer writer)
        {
            writer.UInt64(ID, "id");

            writer.Int(TurnPriority, "turnPriority");

            writer.Collection(Units, "units",
                              (MyData.Writer w, Unit outVal, string name) =>
                                  Unit.Write(w, name, outVal));

            writer.Collection(AlliesByID, "allies",
                              (MyData.Writer w, ulong outVal, string name) =>
                                  w.UInt64(outVal, name));
            writer.Collection(EnemiesByID, "enemies",
                              (MyData.Writer w, ulong outVal, string name) =>
                                  w.UInt64(outVal, name));
        }
        public virtual void ReadData(MyData.Reader reader)
        {
            ID = reader.UInt64("id");

            TurnPriority.Value = reader.Int("turnPriority");

            Units.Clear();
            reader.Collection("units",
                              (MyData.Reader r, ref Unit outUnit, string name) =>
                                  { outUnit = Unit.Read(r, this, name); },
                              (size) => Units);

            AlliesByID.Clear();
            reader.Collection("allies",
                              (MyData.Reader r, ref ulong outID, string name) =>
                                  { outID = r.UInt64(name); },
                              (size) => AlliesByID);

            EnemiesByID.Clear();
            reader.Collection("enemies",
                              (MyData.Reader r, ref ulong outID, string name) =>
                                  { outID = r.UInt64(name); },
                              (size) => EnemiesByID);
        }

        public static void Write(Group g, string name, MyData.Writer writer)
        {
            writer.UInt((uint)g.MyType, name + "_type");
            writer.Structure(g, name + "_value");
        }
        public static Group Read(Map theMap, string name, MyData.Reader reader)
        {
            Types type = (Types)reader.UInt(name + "_type");

            Group g = null;
            switch (type)
            {
                case Types.PlayerChars:
					g = new Groups.PlayerGroup(theMap);
                    break;

                default: throw new NotImplementedException(type.ToString());
            }

            reader.Structure(g, name + "_value");
            return g;
        }

        #endregion

        #region GroupSet class

        public class GroupSet : IDCollection<Group>
        {
            public Map TheMap { get; private set; }

            public GroupSet(Map theMap) { TheMap = theMap; }

            protected override void SetID(ref Group owner, ulong id) { owner.ID = id; }
            protected override ulong GetID(Group owner) { return owner.ID; }
            protected override void Write(MyData.Writer writer, Group value, string name)
            {
                Group.Write(value, name, writer);
            }
            protected override Group Read(MyData.Reader reader, string name)
            {
                return Group.Read(TheMap, name, reader);
            }
        }

        #endregion
    }
}
