using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;


namespace QuickStart
{
    public class Collector : MonoBehaviour
    {
        [SerializeField] Physic_Engine pe;
        public bool usePrediction = false;


        public void CollectPrediction(Player_Ghost pg)
        {
            //controllo se la predizione è simile al dato riscontrato
            var conn = pg.player.netIdentity.connectionToClient;
            if (ConfrontPrediction(pg.min, pg.max))//real data è diverso dalla predizione quindi uso la predizione
            {

                CarsManager.instance.UIPrediction(conn, true);
                pg.info.Info(true);
                pg.ghost.UpdateBody(pg.prediction);

                //pg.prediction.Dispose();
                //Debug.Log("USATO PREDIZIONE");
            }
            else//uso i dati reali
            {
                CarsManager.instance.UIPrediction(conn, false);
                pg.info.Info(false);
                pg.ghost.CopyFromTrue();
                // pg.prediction.Dispose();
            }

            //pe.predictionDone = false;
        }


        private bool ConfrontPrediction(int min, int max)
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
            var div = (int)time % 11;
            if (div >= min && div <= max)
            {
                return true;
            }
            return false;




        }
    }
}
