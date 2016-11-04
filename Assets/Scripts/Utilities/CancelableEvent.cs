using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// An event with a clearly-ordered list of delegates to call when raised.
/// If any of the delegates returns "true", all older delegates are not notified of the event.
/// </summary>
public class CancelableEvent<EventDataType>
{
    private List<Func<EventDataType, bool>> toRaise = new List<Func<EventDataType, bool>>();


    /// <summary>
    /// Adds a new responder to the event that gets executed before all current responders.
    /// If it returns "true", it will prevent the event from reaching those other responders.
    /// </summary>
    public void Add(Func<EventDataType, bool> newResponder)
    {
        toRaise.Add(newResponder);
    }
    public void Remove(Func<EventDataType, bool> responder)
    {
        toRaise.Remove(responder);
    }

    /// <summary>
    /// Sends the event to responders until one of them cancels it.
    /// Returns whether a responder canceled the event.
    /// </summary>
    public bool Raise(EventDataType data)
    {
        for (int i = toRaise.Count - 1; i >= 0; --i)
            if (toRaise[i](data))
                return true;
        return false;
    }
}