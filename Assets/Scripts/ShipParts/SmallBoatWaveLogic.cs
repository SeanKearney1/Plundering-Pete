using System;
using UnityEngine;

public class SmallBoatWaveLogic : MonoBehaviour
{
    private float wavesize = 0;
    private float wavefrequency = 0;
    private float OG_Y_Level;

    void Start()
    {
        OG_Y_Level = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x,CurWaveHeight(),transform.position.z);
    }

    public void SetWaveStats(float new_wave_size, float new_wave_frequency)
    {
        wavesize = new_wave_size;
        wavefrequency = new_wave_frequency;
    }



    private float CurWaveHeight()
    {
        if (wavesize != 0 && wavefrequency != 0)
        {
            return (float)(wavesize * Math.Sin(wavefrequency*Time.time)) + OG_Y_Level;
        }
        return OG_Y_Level;
    }
}
