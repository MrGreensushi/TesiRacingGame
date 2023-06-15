using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using UnityEditor.UIElements;
using UnityEngine;

public class Offline_Physic_Engine : MonoBehaviour
{
    [SerializeField] NNModel nnModel;
    Model model;
    IWorker worker;
    public float[] prediction;
    public int movex;
    public bool predictionDone;
    Queue<float[]> matrix;
    public bool usePrediction = false;

    private void Start()
    {
        model = ModelLoader.Load(nnModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
        predictionDone = false;
        matrix = new Queue<float[]>();

    }



    public IEnumerator MakePrediction(int timesteps, int featuresNumber, float[] lastInfo)
    {
        if (matrix.Count == timesteps)
        {
            Tensor input = new Tensor(1, 1, featuresNumber, timesteps, matrix.ToArray());

            Tensor output = worker.Execute(input).PeekOutput();

            //Wait  for the prediction completion
            yield return new WaitForCompletion(output);

            input.Dispose();
            prediction = output.AsFloats();
            if (prediction[2] < -1.4f) movex = 1;
            else if (prediction[2] > 1.4f) movex = -1;
            else movex = 0;

            for (int i = 0; i < 3; i++)
            {
                prediction[i] += lastInfo[2 + i];
            }
            //for (int i = 0; i < 5; i++)
            //{
            //    prediction[i] += lastInfo[i];
            //}
            output.Dispose();

        }

        predictionDone = true;


    }

    public void UpdateMatrix(float[] input, int timesteps, int featuresNumber, float[] lastInfo)
    {

        if (matrix.Count == timesteps)
            matrix.Dequeue();
        matrix.Enqueue(input);
        if (!usePrediction) return;
        StartCoroutine(MakePrediction(timesteps, featuresNumber, lastInfo));


    }


}
