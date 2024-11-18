using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using Mirror;
using QuickStart;

public class Offline_Collector : MonoBehaviour
{
    [SerializeField] Offline_Physic_Engine pe;
    [SerializeField] Offline_Rule_Engine re;
    [SerializeField] Offline_GhostCar car;
    [SerializeField] OfflineCar trueCar;
    public float threshold;

    public OperatingMode operatingMode;
    public EvaluateMPAI evaluator;

    public bool usePrediction = false;

    public void CollectPrediction()
    {
        if (ConfrontPrediction())//real data � diverso dalla predizione quindi uso la predizione
        {
            //Scrivi su file esterno la differenza tra ghost cat e player car
            if (evaluator != null)
                evaluator.Writing(trueCar.RigidbodyCar, car.RigidbodyCar);

            car.UpdateBody(pe.prediction);

            //pg.prediction.Dispose();
            //Debug.Log("USATO PREDIZIONE");
        }
        else//uso i dati reali
        {
            if (evaluator != null)
                evaluator.StopWriting();


            car.CopyFromTrue();
            // pg.prediction.Dispose();
        }

    }


    private bool ConfrontPrediction()
    {

        if (operatingMode == OperatingMode.Testing)
        {
            var time = NetworkTime.time;
            var div = (int)time % 7;
            if (div >= 0 && div <= 2)
            {
                return true;
            }
            return false;
        }
        else
        {
            //Controllo la predizione, se il MAE � maggiore del treshold allora sbaglia
            var diff = 0f;
            var toCompare = trueCar.CompareWithPrediction();
            var prediction = pe.prediction;
            for (int i = 0; i < prediction.Length; i++)
            {
                diff += Mathf.Abs(toCompare[i] - prediction[i]);
            }
            diff /= (float)prediction.Length;
            //Debug.Log(diff.ToString());
            return diff > threshold;
        }





    }
}
