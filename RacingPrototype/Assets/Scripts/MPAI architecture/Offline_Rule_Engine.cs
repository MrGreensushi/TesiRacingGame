using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class Offline_Rule_Engine : MonoBehaviour
{
    [SerializeField] NNModel nnModel;
    Model model;
    IWorker worker;
    float[] prediction;
    public bool predictionDone;
    private void Start()
    {
        model = ModelLoader.Load(nnModel);
        worker = WorkerFactory.CreateReferenceCPUWorker(model);

    }

    IEnumerator MakePrediction(float[][] matrix, int timesteps, int featuresNumber)
    {
        predictionDone = false;
        Tensor input = new Tensor(timesteps, featuresNumber, matrix);

        Tensor output = worker.Execute(input).PeekOutput();

        //Wait  for the prediction completion
        yield return new WaitForCompletion(output);

        input.Dispose();
        prediction = output.AsFloats();
        output.Dispose();
        predictionDone = true;

    }
}
