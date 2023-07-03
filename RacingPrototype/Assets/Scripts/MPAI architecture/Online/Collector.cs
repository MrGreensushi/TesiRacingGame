using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuickStart
{
    public class Collector : MonoBehaviour
    {
        [SerializeField] Physic_Engine pe;
        public Ghost_Car car;
        public PlayerScript trueCar;

        public bool usePrediction = false;
        float[] prediction;

        public void CollectPrediction()
        {
            //controllo se la predizione � simile al dato riscontrato
            prediction = pe.prediction;

            if (ConfrontPrediction())//real data � diverso dalla predizione quindi uso la predizione
            {
                car.UpdateBody(prediction);

                Debug.Log("USATO PREDIZIONE");
            }
            else//uso i dati reali
            {

                car.CopyFromTrue();
            }

            pe.predictionDone = false;
        }


        private bool ConfrontPrediction()
        {

            //Controllo velocit� e rotazione, se la differenza � maggiore di 1 allora sbaglia
            var diff = 0f;
            var toCompare = car.InfoToCompare;
            for (int i = 0; i < prediction.Length; i++)
            {
                diff += Mathf.Abs(toCompare[i] - prediction[i]);
            }
            diff /= 3f;
            //Debug.Log(diff.ToString());
            return diff > 2f;
        }
    }
}
