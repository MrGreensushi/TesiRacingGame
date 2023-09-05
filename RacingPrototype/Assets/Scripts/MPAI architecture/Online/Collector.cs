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

        [Tooltip("Testing: the prediction is applied every 10 seconds \n" +
            "Real-Case: The prediction is appliead only if it differenties too much")]
        public OperatingMode operatingMode;
        public float threshold;

        public void CollectPrediction(Player_Ghost pg)
        {
            //controllo se la predizione è simile al dato riscontrato
            var conn = pg.player.netIdentity.connectionToClient;
            if (ConfrontPrediction(pg))//real data è diverso dalla predizione quindi uso la predizione
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


        private bool ConfrontPrediction(Player_Ghost pg)
        {

            if (operatingMode == OperatingMode.Testing)
            {
                var time = NetworkTime.time;
                var div = (int)time % 11;
                if (div >= pg.min && div <= pg.max)
                {
                    return true;
                }
                return false;
            }
            else
            {
                //Controllo la predizione, se il MAE è maggiore del treshold allora sbaglia
                var diff = 0f;
                var toCompare = pg.player.CompareWithPrediction();
                var prediction = pg.prediction;
                for (int i = 0; i < prediction.Length; i++)
                {
                    diff += Mathf.Abs(toCompare[i] - prediction[i]);
                }
                diff /= (float)prediction.Length;
                //Debug.Log(diff.ToString());
                return diff > threshold;
            }





        }
    }
}
