using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatencyLevels : MonoBehaviour
{

    public static int L1Duration, L2Duration, L3Duration;

    public static int L1Frequency, L2Frequency, L3Frequency;
    [Tooltip("Total duration in milliseconds of the latency spike")]
    public int durationL1, durationL2, durationL3;
    [Tooltip("Time between latency spikes in milliseconds")]
    public int frequencyL1, frequencyL2, frequencyL3;


    public void Awake()
    {
        // L1Duration = durationL1;
        L2Duration = durationL2;
        L3Duration = durationL3;
        // L1Frequency = frequencyL1;
        L2Frequency = frequencyL2;
        L3Frequency = frequencyL3;
    }
}



public readonly struct Latency
{
    public Latency(LatencyLevel l)
    {
        int x = 0, y = 0;

        switch (l)
        {
            case LatencyLevel.L1:
                x = LatencyLevels.L1Frequency;
                y = LatencyLevels.L1Duration;
                break;
            case LatencyLevel.L2:
                x = LatencyLevels.L2Frequency;
                y = LatencyLevels.L2Duration;
                break;
            case LatencyLevel.L3:
                x = LatencyLevels.L3Frequency;
                y = LatencyLevels.L3Duration;
                break;
        }


        Frequency = x;
        Duration = y;
    }

    public int Frequency { get; }
    public int Duration { get; }

}


public enum LatencyLevel
{
    None,
    L1,
    L2,
    L3,
}