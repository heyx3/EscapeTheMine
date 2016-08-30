using System;
using UnityEngine;


public class BlinkRenderer: MonoBehaviour
{
    public float BlinkInteval = 0.05f;
    public Renderer ToBlink;

    [NonSerialized]
    public float TimeToBlink = -1.0f;


    public bool IsVisible { get { return ToBlink.enabled; } }


    public void Stop()
    {
        ToBlink.enabled = true;
        enabled = false;
    }

    public void Start()
    {
		enabled = true;
        TimeToBlink = BlinkInteval;
    }
    void Update()
    {
        TimeToBlink -= Time.deltaTime;
        if (TimeToBlink <= 0.0f)
        {
            ToBlink.enabled = !ToBlink.enabled;
            TimeToBlink += BlinkInteval;
        }
    }
}