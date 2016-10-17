using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// As soon as this script is active, it starts blinking on and off.
/// The actual "blinking" behavior is defined by child classes.
/// </summary>
public abstract class Blinker<BlinkType> : MonoBehaviour
{
    public float BlinkInterval = 0.1f;
    public BlinkType ToBlink;

    [NonSerialized]
    public float TimeToBlink = -1.0f;


    /// <summary>
    /// Is the think being blinked currently on or off?
    /// </summary>
    public abstract bool IsOn { get; }


    public virtual void Start()
    {
        TimeToBlink = BlinkInterval;
        enabled = true;
    }
    public virtual void Stop()
    {
        enabled = false;
    }
    protected virtual void Update()
    {
        TimeToBlink -= Time.deltaTime;
        if (TimeToBlink <= 0.0f)
        {
            TimeToBlink += BlinkInterval;
            Toggle();
        }
    }

    /// <summary>
    /// Does the "blink" behavior.
    /// </summary>
    public abstract void Toggle();
}