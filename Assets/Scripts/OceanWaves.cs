using System;
using UnityEngine;

public class OceanWaves : MonoBehaviour
{

    private float wavesize;
    private float wavefrequency;
    private float OG_Y_level;
    private GameObject sea_1;
    private Level_Info Poseidon;

    void Start()
    {
        sea_1 = transform.Find("Sea_1").gameObject;
        Poseidon = GameObject.Find("Poseidon").GetComponent<Level_Info>();
        OG_Y_level = sea_1.transform.position.y;
        wavesize = Poseidon.WaveSize;
        wavefrequency = Poseidon.WaveFrequency;
    }

    void Update()
    {
        sea_1.transform.position = new Vector3(sea_1.transform.position.x,CurWaveHeight(),sea_1.transform.position.z);
    }


    private float CurWaveHeight()
    {
        if (wavesize != 0 && wavefrequency != 0)
        {
            return (float)(wavesize * Math.Sin(wavefrequency*Time.time)) + OG_Y_level;
        }
        return OG_Y_level;
    }
}
