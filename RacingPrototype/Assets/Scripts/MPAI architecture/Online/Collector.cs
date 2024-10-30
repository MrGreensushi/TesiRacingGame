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

        public LatencyLevel level;
        private Latency lat;
        private double lastActivation;
        private int randomAddedLatency;
        private bool newActivation;

        public float threshold;

        public EvaluateMPAI evaluator;


        public void Start()
        {
            newActivation = true;
            if (CommandLinesManager.instance != null)
            {
                if (CommandLinesManager.instance.level != LatencyLevel.None)
                    level = CommandLinesManager.instance.level;
            }
            lat = new Latency(level);
            lastActivation = 0;
            randomAddedLatency = 0;
        }

        public void CollectPrediction(Player_Ghost pg)
        {
            //controllo se la predizione è simile al dato riscontrato
            var conn = pg.player.netIdentity.connectionToClient;
            //if (ConfrontPrediction(pg))//real data è diverso dalla predizione quindi uso la predizione

            if (pg.predicting)//Se stavo predicendo
            {
                //Scrivi su file esterno la differenza tra ghost cat e player car
                if (evaluator != null)
                    evaluator.Writing(pg.player.RigidbodyCar, pg.ghost.RigidbodyCar);

                //Visibile sullo schermo del giocatore solo in fase di testing
                if (operatingMode == OperatingMode.Testing)
                    CarsManager.instance.UIPrediction(conn, true);


                pg.info.Info(true);
                pg.ghost.UpdateBody(pg.prediction);

                //pg.prediction.Dispose();
                //Debug.Log("USATO PREDIZIONE");
            }
            else//uso i dati reali
            {
                if (evaluator != null)
                    evaluator.StopWriting();

                CarsManager.instance.UIPrediction(conn, false);
                pg.info.Info(false);
                pg.ghost.CopyFromTrue();
                // pg.prediction.Dispose();
            }

            //pe.predictionDone = false;
        }


        public bool ConfrontPrediction(Player_Ghost pg)
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
                ////Controllo la predizione, se il MAE è maggiore del treshold allora sbaglia
                //var diff = 0f;
                //var toCompare = pg.player.CompareWithPrediction();
                //var prediction = pg.prediction;
                //for (int i = 0; i < prediction.Length; i++)
                //{
                //    diff += Mathf.Abs(toCompare[i] - prediction[i]);
                //}
                //diff /= (float)prediction.Length;
                ////Debug.Log(diff.ToString());
                //return diff > threshold;

                return pg.CheckLatencyActivation();

            }





        }
    }
}

