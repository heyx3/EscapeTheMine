using System;
using System.Collections.Generic;
using MyData;


/// <summary>
/// A collection of objects, each with a unique ID.
/// </summary>
public abstract class IDCollection<OwnerType> : LockedSet<OwnerType>
{
    /// <summary>
    /// The ID that will be assigned to the next object added to this collection.
    /// </summary>
    public ulong NextID = 0;

    private Dictionary<ulong, OwnerType> idToObj = new Dictionary<ulong, OwnerType>();


    public override void Add(OwnerType t)
    {
        if (!Contains(t))
        {
            ulong id = NextID;
            SetID(ref t, id);

            NextID = unchecked(NextID + 1);

            idToObj.Add(id, t);
        }

        base.Add(t);
    }
    public override bool Remove(OwnerType t)
    {
        if (Contains(t))
            idToObj.Remove(GetID(t));

        return base.Remove(t);
    }

    public OwnerType Get(ulong id)
    {
        return idToObj[id];
    }
    public bool TryGet(ulong id, ref OwnerType outOwner)
    {
        if (idToObj.ContainsKey(id))
        {
            outOwner = idToObj[id];
            return true;
        }
        return false;
    }

    protected abstract void SetID(ref OwnerType owner, ulong id);
    protected abstract ulong GetID(OwnerType owner);

    #region Serialization stuff

    public override void WriteData(Writer writer)
    {
        base.WriteData(writer);
        writer.UInt64(NextID, "nextID");
    }
    public override void ReadData(Reader reader)
    {
        base.ReadData(reader);
        NextID = reader.UInt64("nextID");

        idToObj.Clear();
        foreach (OwnerType owner in this)
            idToObj.Add(GetID(owner), owner);
    }

    #endregion
}