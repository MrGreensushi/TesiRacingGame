using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestingAgents : MonoBehaviour
{
    public int crashes;
    public List<float> recordedVelocity;
    public string fileName;

    private float startingTime = 0;
    public OfflineCar car;

    private void Awake()
    {
        recordedVelocity = new List<float>();
    }

    public void StartRace()
    {
        startingTime = Time.time;
        crashes = 0;
        recordedVelocity.Clear();
    }

    private void Update()
    {
        recordedVelocity.Add(car.Velocity);
    }

    public void EndRace(int wrongChecks, bool finished)
    {
        float time = Time.time - startingTime;
        float averageVel = 0f;
        foreach (var v in recordedVelocity)
        {
            averageVel += v;
        }
        averageVel /= recordedVelocity.Count;

        string toStore = time.ToString() + ';' +
            crashes.ToString() + ';' +
            averageVel.ToString() + ';' +
            finished.ToString() + ';' +
            wrongChecks.ToString() + ';'
            + '\n';
        ;

        File.AppendAllText(fileName, toStore);

    }

}
