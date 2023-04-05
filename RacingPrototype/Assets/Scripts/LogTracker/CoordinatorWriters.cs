using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatorWriters : MonoBehaviour
{
    public int startinIndex = 0;
    public int totWriters = 0;

    private void Awake()
    {
        var writers = GameObject.FindObjectsOfType<RecordWriter>();
        totWriters = writers.Length;
        for (int i = 0; i < writers.Length; i++)
        {
            writers[i].StartWriting(startinIndex + i);
        }
    }
}
