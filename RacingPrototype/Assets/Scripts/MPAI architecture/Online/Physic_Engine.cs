using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace QuickStart
{
    public class Physic_Engine : MonoBehaviour
    {
        [SerializeField] NNModel nnModel;
        Model model;
        IWorker worker;
        public float[] prediction;
        public int movex;
        public bool predictionDone;
        Queue<float[]> matrix;
        public bool usePrediction = false;


        public int Count { get => matrix.Count; }
        private void Start()
        {
            model = ModelLoader.Load(nnModel);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, model);
            predictionDone = false;
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


                Tensor output = worker.Execute(input).PeekOutput();

                //Wait  for the prediction completion
                yield return new WaitForCompletion(output);

                input.Dispose();
                prediction = output.AsFloats();

                //DELTA
                for (int i = 0; i < 3; i++)
                {
                    prediction[i] += lastInfo[2 + i];
                }

                output.Dispose();

            }

            predictionDone = true;


        }

        public void UpdateMatrix(float[] input, int timesteps, int featuresNumber, float[] lastInfo)
        {

            if (matrix.Count == timesteps)
                matrix.Dequeue();
            matrix.Enqueue(input);

            StartCoroutine(MakePrediction(timesteps, featuresNumber, lastInfo));


        }
    }
}

