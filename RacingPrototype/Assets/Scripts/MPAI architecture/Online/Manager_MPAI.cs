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
        private List<Player_Ghost> ghostSPG = new List<Player_Ghost>();
        private List<Player_Ghost> ghostActiveSPG = new List<Player_Ghost>();
        public int bot;

        public bool SPGactivated { get; protected set; }
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
            Debug.LogError("Frequency: "+LatencyLevels.L1Frequency);
            Debug.LogError("Duration: "+LatencyLevels.L1Duration);
            
            Invoke("SelectSPGPlayers",1);
            Invoke("ActivateSPG",LatencyLevels.L1Duration/1000);
        }

        private void FixedUpdate()
        {
            if (ghosts.Count == 0) return;


            //controllo se la latenza del giocatore � elevata
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

            foreach (var item in ghostSPG)
            {
                if(item.player==null) continue;
                
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

            foreach (var item in ghostActiveSPG)
            {
                if(item.player==null) continue;
                
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
            ui.SetCameraCarFocus(c_t.gameObject);


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

        private void SelectSPGPlayers()
        {
            var percentage = CommandLinesManager.instance.percentageSPGPlayers;
            var numberOfPlayers = ghosts.Count * percentage / 100;

            var playerIndexes = RandomNumbers.Get(numberOfPlayers, 0, ghosts.Count);
            Debug.LogError("Players: "+ playerIndexes.Count);
            foreach (int index in playerIndexes)
            {
                ghostSPG.Add(ghosts[index]);
            }

        }

        private void SelecteActiveSPGPlayers()
        {
            var percentage = ghostSPG.Count * CommandLinesManager.instance.percentageActiveSPG / 100;
            var indexes = RandomNumbers.Get(percentage, 0,  ghostSPG.Count);
            ghostActiveSPG.Clear();
            Debug.LogError("Selected Players: "+ indexes.Count);
            foreach (var index in indexes)
            {
                ghostActiveSPG.Add(ghostSPG[index]);
            }
        }


        private void ActivateSPG()
        {
            Debug.LogError(Time.time+" Activate SPG");
            SPGactivated = true;
            SelecteActiveSPGPlayers();
            Invoke("DeactivateSPG", LatencyLevels.L1Duration/1000);
        }

        private void DeactivateSPG()
        {
            Debug.LogError(Time.time+" DeActivate SPG");
            SPGactivated = false;
            var time = LatencyLevels.L1Frequency - LatencyLevels.L1Duration;
            Invoke("ActivateSPG",time/1000);
        }

        public bool IsGhostContained(Player_Ghost pg)
        {
            return ghostActiveSPG.Contains(pg);
        }
    }
}


public enum OperatingMode : ushort
{
    Testing = 0,
    RealCase = 1
}