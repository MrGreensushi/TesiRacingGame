using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Barracuda;
using UnityEngine;
using System.IO;

public class Player_Ghost
{
    public PlayerScript player;
    public Ghost_Car ghost;
    public Queue<float[]> matrix;
    public MPAI_Info info;
    public float[] prediction;
    public IWorker worker;
    public bool canPredict, doNotMPAI, predicting;
    public int min, max;

    public LatencyLevel level;
    private Latency lat;
    private double lastActivation;
    private int randomAddedLatency;
    private bool newActivation;
    private float initialTime = -1;

    public Player_Ghost(PlayerScript x, Ghost_Car y, MPAI_Info z, NNModel nn, bool doNotMPAI, LatencyLevel level, string name)
    {
        this.ghost = y;
        this.player = x;
        this.info = z;
        this.matrix = new Queue<float[]>();

        this.prediction = new float[3];
        this.canPredict = true;
        this.min = Random.Range(0, 9);
        this.max = min + 2;
        this.predicting = false;

        this.doNotMPAI = doNotMPAI;
        this.level = level;
        if (level == LatencyLevel.None)
        {
            this.lat = new Latency(LatencyLevel.L1);
        }
        else
        {
            this.lat = new Latency(level);
        }
        this.lastActivation = 0;
        this.randomAddedLatency = 0;
        this.newActivation = true;

        if (doNotMPAI) return;

        var model = ModelLoader.Load(nn);
        var workerType = CommandLinesManager.instance?.workerType ?? WorkerFactory.Type.CSharpBurst;
        this.worker = WorkerFactory.CreateWorker(workerType, model);
        UpdateRandomLatency();
        //this.job = new JobHandle();

        initialTime = PredictionLogger.Instance.InitialTime;
        //WriteOnFile(name);
    }

    private void UpdateRandomLatency()
    {
        randomAddedLatency = Random.Range(0, 4) * 1000 + Random.Range(0, 1001);
    }

    public void Prediction(int timesteps, int featuresNumber, float[] lastInfo, MonoBehaviour mb)
    {
        if (matrix.Count != timesteps) return;

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
        mb.StartCoroutine(PredictionRoutine(input, lastInfo));
        //PredictionRoutine(input, lastInfo);
        //var output = worker.Execute(input).PeekOutput();
        //input.Dispose();
        //var pr = output.AsFloats();
        //
        ////DELTA
        //for (int i = 0; i < 3; i++)
        //{
        //    prediction[i] = pr[i] + lastInfo[2 + i];
        //}
        //
        //output.Dispose();



    }

    IEnumerator PredictionRoutine(Tensor input, float[] lastInfo)
    {
        var starTimePrediction = Time.time;
        var output = worker.Execute(input).PeekOutput();
        yield return new WaitForCompletion(output);
        input.Dispose();
        var pr = output.AsFloats();
        output.Dispose();

        //var output = ExecuteInParts(worker, input);
        //yield return new WaitForCompletion(output);
        //input.Dispose();
        //var pr = output.AsFloats();
        //output.Dispose();

        //DELTA
        for (int i = 0; i < 3; i++)
        {
            prediction[i] = pr[i] + lastInfo[2 + i];
        }

        var timeNeededForPrediction = Time.time - starTimePrediction;
        //WriteOnFile(timeNeededForPrediction.ToString());
        PredictionLogger.Instance.AddPredictionLog(player.playerName,starTimePrediction.ToString(CultureInfo.InvariantCulture),Time.time.ToString(CultureInfo.InvariantCulture));
    }



    void WriteOnFile(string message)
    {
        var file = "C:/Users/dansp/OneDrive/Desktop/TempoPrevisioni.txt";

        if (!File.Exists(file))
        {
            var myFile = File.Create(file);
            myFile.Close();
        }

        File.AppendAllText(file, message + "\n");
    }


    Tensor ExecuteInParts(IWorker worker, Tensor I, int syncEveryNthLayer = 5)
    {

        syncEveryNthLayer = 52 / 4;
        var executor = worker.ExecuteAsync(I);
        var it = 0;
        bool hasMoreWork;

        do
        {
            hasMoreWork = executor.MoveNext();
            if (++it % syncEveryNthLayer == 0)
                worker.WaitForCompletion();

        } while (hasMoreWork);

        return worker.CopyOutput();
    }


    public bool CheckLatencyActivation()
    {
        // var time = NetworkTime.time;
        //time in second lo trasformo in millisecodi
        //var sub = (time - lastActivation) * 1000;
        var time = Time.time;
        var sub = (time -initialTime) * 1000;

        // if (sub < lat.Duration || sub > lat.Frequency + lat.Duration )
        // {
        //     if (newActivation)
        //     {
        //         UpdateRandomLatency(); //aggiungo da 0 a 4 secondi
        //         lastActivation = time;
        //     }
        //     newActivation = false;
        //     return true;
        // }
        // newActivation = true;
        // return false;

        return sub > lat.Duration;

    }
}
