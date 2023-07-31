using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using Unity.Barracuda.ONNX;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;


public struct PredictionJob : IJob
{
    IWorker model;
    public NativeArray<float> prediction;
    int timesteps, featuresNumber;
    NativeArray<float> lastInfo, matrix;


    public PredictionJob(NativeArray<float> prediction, int timesteps, int featuresNumber, 
        NativeArray<float> lastInfo, NativeArray<float> matrix, IWorker model)
    {
        this.prediction = prediction;
        this.timesteps = timesteps;
        this.featuresNumber = featuresNumber;
        this.lastInfo = lastInfo;
        this.matrix = matrix;
        this.model = model;

    }





    public void Execute()
    {
        if (matrix.Length == timesteps * featuresNumber)
        {
            Tensor input = new Tensor(1, 1, featuresNumber, timesteps);

            for (int i = 0; i < matrix.Length; i++)
            {
                int j = Mathf.FloorToInt(i / (float)timesteps);
                input[0, 0, j, i % timesteps] = matrix[i];
            }

           
            Tensor output = model.Execute(input).PeekOutput();
            input.Dispose();
            var pr = output.AsFloats();

            //DELTA
            for (int i = 0; i < 3; i++)
            {
                prediction[i] = pr[i] + lastInfo[2 + i];
            }

            output.Dispose();
            lastInfo.Dispose();
            matrix.Dispose();

        }
    }

   



}
