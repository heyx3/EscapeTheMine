using System;
using UnityEngine;


public class BlinkComponent : MonoBehaviour
{
    public float BlinkInteval = 0.1f;
    public MonoBehaviour ToBlink;

    [NonSerialized]
    public float TimeToBlink = -1.0f;


    public bool IsEnabled { get { return ToBlink.enabled; } }


    public void Stop()
    {
        ToBlink.enabled = true;
        enabled = false;
    }

    void Start()
    {
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