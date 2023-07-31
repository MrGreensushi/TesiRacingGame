using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace QuickStart
{
    public class Physic_Engine : MonoBehaviour
    {
        static public Physic_Engine instance;
        [SerializeField] NNModel nnModel;
        Model model;
        public IWorker worker;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            //model = ModelLoader.Load(nnModel);
            //worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model);
            //predictionDone = false;
            //var bytes = nnModel.modelData.Value;
            //nnmodel=new NativeArray<byte>(bytes.Length,Allocator.Persistent);
            //for (int i = 0; i < bytes.Length; i++)
            //{
            //    nnmodel[i] = bytes[i];
            //}
        }



        //DELTA
        //public IEnumerator MakePrediction(int timesteps, int featuresNumber, float[] lastInfo, Queue<float[]> matrix)
        //{
        //    if (matrix.Count == timesteps)
        //    {
        //        Tensor input = new Tensor(1, 1, featuresNumber, timesteps);
        //        var array = matrix.ToArray();

        //        for (int i = 0; i < timesteps; i++)
        //        {
        //            for (int j = 0; j < featuresNumber; j++)
        //            {
        //                input[0, 0, j, i] = array[i][j];
        //            }

        //        }


        //        Tensor output = worker.Execute(input).PeekOutput();

        //        //Wait  for the prediction completion
        //        yield return new WaitForCompletion(output);

        //        input.Dispose();
        //        prediction = output.AsFloats();

        //        //DELTA
        //        for (int i = 0; i < 3; i++)
        //        {
        //            prediction[i] += lastInfo[2 + i];
        //        }

        //        output.Dispose();

        //    }

        //    predictionDone = true;


        //}
        public void UpdateMatrix(float[] input, int timesteps, int featuresNumber, float[] lastInfo, Player_Ghost pg)
        {

            if (pg.matrix.Count == timesteps)
                pg.matrix.Dequeue();
            pg.matrix.Enqueue(input);

            if (pg.matrix.Count == timesteps)
            {
                pg.Prediction(timesteps, featuresNumber, lastInfo);
            }


        }
    }
}

