using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BlinkSprite : Blinker<SpriteRenderer>
{
    public override bool IsOn { get { return ToBlink.enabled; } }

    public override void Start()
    {
        base.Start();
        ToBlink.enabled = true;
    }
    public override void Toggle()
    {
        ToBlink.enabled = !ToBlink.enabled;
    }
}