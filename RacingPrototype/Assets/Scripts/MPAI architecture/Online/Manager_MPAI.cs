using Mirror;
using QuickStart;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.Jobs;
using UnityEngine;
using static Messages;

namespace QuickStart
{
    public class Manager_MPAI : MonoBehaviour
    {
        static public Manager_MPAI instance;
        private int counter;
        [SerializeField] Dispatcher ds;
        [SerializeField] Collector cl;
        [SerializeField] Physic_Engine pe;

        [SerializeField] GameObject ghostObject, infoObject;
        [SerializeField] Transform UI_transform;
        [SerializeField] NNModel nnModel;

        public int discard;

        [Tooltip("Testing: The ghost car is visible on the server but it does not change the player's car\n" +
        "Real-Case: Ghost car remains invisible but the prediction is applied to the player's car")]
        public OperatingMode ghost_OpMode;

        public List<Player_Ghost> ghosts = new List<Player_Ghost>();
        public int bot;

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

            counter = 0;

            bot = CommandLinesManager.instance.bot;
            NetworkServer.RegisterHandler<InputMessage>(OnInputMessage);
        }

        private void FixedUpdate()
        {
            if (ghosts.Count == 0) return;


            //controllo se la latenza del giocatore è elevata
            //double rtt = NetworkTime.rtt;
            //Debug.Log(rtt);


            counter++;
            if (counter == discard)
                Prediction();
            if (counter == discard + 1)
                Routine();


        }
        private void Routine()
        {
            counter = 0;

            foreach (var item in ghosts)
            {
                if (!item.canPredict) continue;

                if (item.doNotMPAI) continue;

                item.predicting = cl.ConfrontPrediction(item);
                ds.Routine(item);

            }

            //if (predicting)
            //    first = false;
        }


        private void Prediction()
        {

            foreach (var item in ghosts)
            {
                if (!item.canPredict) continue;

                if (item.doNotMPAI) continue;

                if (item.matrix.Count == ds.timesteps)
                {
                    //while (!pe.predictionDone)
                    //{
                    //    yield return null;
                    //}

                    cl.CollectPrediction(item);
                }
            }



            //Time.timeScale = 0;
            //if (!first)
            //{
            //aspetta che finisce la predizione

            //}
            //Time.timeScale = 1f;

        }


        public void AddCar(PlayerScript c_t, bool doNotMPAI, LatencyLevel level)
        {
            var ghost = Instantiate(ghostObject);
            if (!ghost.TryGetComponent(out Ghost_Car g_c))
            {
                Debug.LogError("Non ha componente ghost!");
                return;
            }
            g_c.toCopyCar = c_t.gameObject;
            g_c.operatingMode = ghost_OpMode;
            //g_c.Heuristic = firstPlayerHeuristic;
            var ui_o = Instantiate(infoObject, UI_transform);
            var ui = ui_o.GetComponent<MPAI_Info>();
            ui.Nome(c_t.playerName, c_t.playerColor);



            ghosts.Add(new Player_Ghost(c_t, g_c, ui, nnModel, doNotMPAI, level, nnModel.name));


        }

        public void OnInputMessage(NetworkConnectionToClient conn, InputMessage msg)
        {
            //check if the input is valid:
            var moveX = msg.moveX <= 1 && msg.moveX >= -1;
            var moveY = msg.moveY <= 1 && msg.moveY >= -1;
            var breaking = msg.breaking == 1 || msg.breaking == 0;


            if (moveX && moveY && breaking) return;
            Debug.Log("WRONG INPUT");

            //one of the three is wrong!

        }
    }
}


public enum OperatingMode : ushort
{
    Testing = 0,
    RealCase = 1
}