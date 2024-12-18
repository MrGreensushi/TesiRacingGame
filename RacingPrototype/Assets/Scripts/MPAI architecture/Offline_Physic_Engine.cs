using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class Offline_Physic_Engine : MonoBehaviour
{
    [SerializeField] NNModel nnModel;
    Model model;
    IWorker worker;
    public float[] prediction;
    Queue<float[]> matrix;

    public int Count { get => matrix.Count; }
    private void Start()
    {
        model = ModelLoader.Load(nnModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
        matrix = new Queue<float[]>();

    }



    //DELTA
    public IEnumerator MakePrediction(int timesteps, int featuresNumber, float[] lastInfo)
    {
        if (matrix.Count == timesteps)
        {
            Tensor input = new Tensor(1, 1, featuresNumber, timesteps);
            var array = matrix.ToArray();
            for (int i = 0; i < timesteps; i++)
            {
                for (int j = 0; j < featuresNumber; j++)
                {
                    input[0, 0, j, i] = array[i][j];
                }

            }


            var output = worker.Execute(input).PeekOutput();
            yield return new WaitForCompletion(output);
            input.Dispose();
            var pr = output.AsFloats();
            output.Dispose();

            //DELTA
            for (int i = 0; i < 3; i++)
            {
                prediction[i] = pr[i] + lastInfo[2 + i];
            }

        }

    }

    public void UpdateMatrix(float[] input, int timesteps, int featuresNumber, float[] lastInfo)
    {

        if (matrix.Count == timesteps)
            matrix.Dequeue();
        matrix.Enqueue(input);

        if (matrix.Count == timesteps)
            StartCoroutine(MakePrediction(timesteps, featuresNumber, lastInfo));


    }

    //REAL
    //public IEnumerator MakePrediction(int timesteps, int featuresNumber)
    //{
    //    if (matrix.Count == timesteps)
    //    {
    //        //Tensor input = new Tensor(1, 1, featuresNumber, timesteps, matrix.ToArray());
    //        Tensor input = new Tensor(1, 1, featuresNumber, timesteps);
    //        var array = matrix.ToArray();

    //        for (int i = 0; i < timesteps; i++)
    //        {
    //            for (int j = 0; j < featuresNumber; j++)
    //            {
    //                input[0, 0, j, i] = array[i][j];
    //            }

    //        }

    //        //foreach (var t in input.AsFloats())
    //        //{
    //        //    //var s = t[0] + ";" + t[1] + ";" + t[2] + ";" + t[3] + ";" + t[4] + ";" + t[5] + ";" + t[6];
    //        //    Debug.Log(t);
    //        //}
    //        //Debug.Log("TOSTRING: "+input.ToString());


    //        Tensor output = worker.Execute(input).PeekOutput();

    //        //Wait  for the prediction completion
    //        yield return new WaitForCompletion(output);

    //        input.Dispose();
    //        prediction = output.AsFloats();
    //        prediction[2] = prediction[2] * 360f;
    //        output.Dispose();

    //    }

    //    predictionDone = true;


    //}

    //public void UpdateMatrix(float[] input, int timesteps, int featuresNumber)
    //{

    //    if (matrix.Count == timesteps)
    //        matrix.Dequeue();
    //    matrix.Enqueue(input);
    //    if (!usePrediction) return;
    //    StartCoroutine(MakePrediction(timesteps, featuresNumber));


    //}


}
