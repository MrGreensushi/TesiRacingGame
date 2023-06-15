using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

public class Offline_Collector : MonoBehaviour
{
    [SerializeField] Offline_Physic_Engine pe;
    [SerializeField] Offline_Rule_Engine re;
    [SerializeField] string filename;
    [SerializeField] string path;
    [SerializeField] bool printFile;
    [SerializeField] Offline_GhostCar car;
    [SerializeField] OfflineCar trueCar;

    public bool usePrediction = false;
    float[] prediction;

    public void CollectPrediction()
    {
        if (usePrediction)
        {
            car.UpdateBody(pe.prediction,pe.movex);

            if (printFile)
                PrintToFile(prediction);

            pe.predictionDone = false;

        }

    }


    private void PrintToFile(float[] prediction)
    {
        string toWrite = "";
        string file = path + filename;
        foreach (var item in prediction)
        {
            toWrite += item.ToString() + ';';
        }
        foreach (var item in trueCar.PhysicInfos)
        {
            toWrite += item.ToString() + ';';
        }

        string result = toWrite.Remove(toWrite.Length - 1); //delete last ';'
        File.AppendAllText(file, result);
    }
}
