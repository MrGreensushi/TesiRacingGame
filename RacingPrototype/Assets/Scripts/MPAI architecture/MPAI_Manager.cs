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

    private void Start()
    {
        predicting = false;
        first = true;
        counter = 0;
    }

    private void FixedUpdate()
    {
        counter++;
        if (counter == 2 && predicting)
            StartCoroutine(Prediction());
        if (counter == 3)
            Routine();


    }
    private void Routine()
    {
        counter = 0;

        ds.Routine();
        //cl.CollectPrediction();
        if (predicting)
            first = false;
    }


    private IEnumerator Prediction()
    {
        Time.timeScale = 0;
        if (!first)
        {
            //aspetta che finisce la predizione
            while (!pe.predictionDone)
            {
                yield return null;
            }

            cl.CollectPrediction();
        }
        Time.timeScale = 1f;

    }


    public void PredictionCoroutine(bool value)
    {
        predicting = value;
        if (value) PredictionStart();
        else PredictionStop();

    }

    public void PredictionStop()
    {
        pe.usePrediction = false;
        car.Predicting = false;
        cl.usePrediction = false;
        first = true;
    }

    public void PredictionStart()
    {
        Debug.Break();
        cl.usePrediction = true;
        pe.usePrediction = true;
        pe.predictionDone = false;
        car.Predicting = true;




    }


}
