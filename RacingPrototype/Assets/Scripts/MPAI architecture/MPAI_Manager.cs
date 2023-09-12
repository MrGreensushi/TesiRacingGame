using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPAI_Manager : MonoBehaviour
{
    private int counter;
    [SerializeField] Offline_Dispatcher ds;
    [SerializeField] Offline_Collector cl;
    [SerializeField] Offline_GhostCar car;
    [SerializeField] Offline_Physic_Engine pe;
    private bool first, predicting;
    public int discard;

    private void Start()
    {
        predicting = false;
        first = true;
        counter = 0;
    }

    private void FixedUpdate()
    {
        counter++;
        if (counter == discard)
            Prediction();
        if (counter == discard + 1)
            Routine();


    }
    private void Routine()
    {
        counter = 0;
        ds.Routine();

    }


    private void Prediction()
    {
        if (pe.Count == ds.timesteps)
        {
            cl.CollectPrediction();
        }

    }


}
