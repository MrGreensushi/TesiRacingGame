using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class Manager_MPAI : MonoBehaviour
    {
        static public Manager_MPAI instance;
        private int counter;
        [SerializeField] Dispatcher ds;
        [SerializeField] Collector cl;
        [SerializeField] Ghost_Car car;
        [SerializeField] Physic_Engine pe;

        [SerializeField] GameObject ghostObject;
        private bool first, predicting;


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
        }
        private void Start()
        {
            predicting = false;
            first = true;
            counter = 0;
        }

        private void FixedUpdate()
        {
            if (car == null) return;


            //controllo se la latenza del giocatore è elevata
            //double rtt = NetworkTime.rtt;
            //Debug.Log(rtt);


            counter++;
            if (counter == 2)
                StartCoroutine(Prediction());
            if (counter == 3)
                Routine();


        }
        private void Routine()
        {
            counter = 0;

            ds.Routine();

            //if (predicting)
            //    first = false;
        }


        private IEnumerator Prediction()
        {
            if (pe.Count == ds.timesteps)
            {
                while (!pe.predictionDone)
                {
                    yield return null;
                }

                cl.CollectPrediction();
            }

            //Time.timeScale = 0;
            //if (!first)
            //{
            //aspetta che finisce la predizione

            //}
            //Time.timeScale = 1f;

        }


        public void PredictionCoroutine(bool value)
        {
            predicting = value;
            if (value) PredictionStart();
            else PredictionStop();

        }

        public void PredictionStop()
        {
            pe.usePrediction = false;
            car.Predicting = false;
            cl.usePrediction = false;
            first = true;
        }

        public void PredictionStart()
        {
            Debug.Break();
            cl.usePrediction = true;
            pe.usePrediction = true;
            pe.predictionDone = false;
            car.Predicting = true;




        }


        public void AddCar(PlayerScript c_t)
        {
            var ghost = Instantiate(ghostObject);
            if (!ghost.TryGetComponent(out Ghost_Car g_c))
            {
                Debug.LogError("Non ha componente ghost!");
                return;
            }
            g_c.toCopyCar = c_t.gameObject;
            ds.carTrue = c_t;
            ds.car = g_c;
            cl.trueCar = c_t;
            cl.car = g_c;
            car = g_c;
        }
    }
}
