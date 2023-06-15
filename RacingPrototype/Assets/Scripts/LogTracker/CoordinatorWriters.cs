using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CoordinatorWriters : MonoBehaviour
{
    public int startinIndex = 0;
    public int totWriters = 0;
    RecordWriter[] writers;
    public int awaitTime;
    float timescale;

    private void Awake()
    {
        Time.timeScale = 1f;
        writers = GameObject.FindObjectsOfType<RecordWriter>();
        totWriters = writers.Length;
        timescale = Time.timeScale;
        StartWriting();


    }

    private void FixedUpdate()
    {
        Time.timeScale = 0;
        StopTime();
    }


    async void StartWriting()
    {
        Task[] t = new Task[totWriters];
        for (int i = 0; i < totWriters; i++)
        {
            t[i] = writers[i].StartWriting(i + startinIndex);
        }
        await Task.WhenAll(t);

        StopTime();
    }

    async void StopTime()
    {
        Task[] t = new Task[totWriters];
        for (int i = 0; i < totWriters; i++)
        {
            t[i] = writers[i].WriteLogs();
        }
        await Task.WhenAll(t);
        Time.timeScale = timescale;

    }



}

