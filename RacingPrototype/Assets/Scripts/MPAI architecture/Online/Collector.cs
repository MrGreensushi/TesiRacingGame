using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            //controllo se la predizione è simile al dato riscontrato
            prediction = pe.prediction;
            var player_ = Manager_MPAI.instance.ghosts.Find(x => x.Ghost == car);
            if (ConfrontPrediction())//real data è diverso dalla predizione quindi uso la predizione
            {

                player_.Info.Info(true);
                car.UpdateBody(prediction);

                Debug.Log("USATO PREDIZIONE");
            }
            else//uso i dati reali
            {
                player_.Info.Info(false);
                car.CopyFromTrue();
            }

            pe.predictionDone = false;
        }


        private bool ConfrontPrediction()
        {

            ////Controllo velocità e rotazione, se la differenza è maggiore di 1 allora sbaglia
            //var diff = 0f;
            //var toCompare = car.InfoToCompare;
            //for (int i = 0; i < prediction.Length; i++)
            //{
            //    diff += Mathf.Abs(toCompare[i] - prediction[i]);
            //}
            //diff /= 3f;
            ////Debug.Log(diff.ToString());
            //return diff > 2f;

            var time = NetworkTime.time;
            if ((int)time % 10 >= 6f)
            {
                return true;
            }
            return false;




        }
    }
}
