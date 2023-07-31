using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Player_Ghost
{
    public PlayerScript player;
    public Ghost_Car ghost;
    public Queue<float[]> matrix;
    public MPAI_Info info;
    public float[] prediction;
    public IWorker worker;
    public bool canPredict;
    public int min, max;


    public Player_Ghost(PlayerScript x, Ghost_Car y, MPAI_Info z, NNModel nn, bool canPredict)
    {
        this.ghost = y;
        this.player = x;
        this.info = z;
        this.matrix = new Queue<float[]>();
        var model = ModelLoader.Load(nn);
        this.worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model);
        this.prediction = new float[3];
        this.canPredict = true;
        this.min = Random.Range(0, 9);
        this.max = min + 2;
        //this.job = new JobHandle();
    }

    public void Prediction(int timesteps, int featuresNumber, float[] lastInfo)
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


            //int stepsPerFrame = 5;
            //var enumerator = worker.StartManualSchedule(input);
            //int step = 0;
            //while (enumerator.MoveNext())
            //{
            //    if (++step % stepsPerFrame == 0) yield return null;
            //}
            var output = worker.Execute(input).PeekOutput();
            input.Dispose();
            var pr = output.AsFloats();

            //DELTA
            for (int i = 0; i < 3; i++)
            {
                prediction[i] = pr[i] + lastInfo[2 + i];
            }

            output.Dispose();

        }

    }

}
