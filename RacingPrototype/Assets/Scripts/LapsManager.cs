using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using QuickStart;
using System;

namespace QuickStart
{
    public class LapsManager : NetworkBehaviour
    {
        public List<Gate> gates;
        public int maxGates; // id del gate 'start'
        public static LapsManager instance;

        [SerializeField] bool initializeIds = false, findGates = false;
        [SerializeField] Transform _parentGates;
        [SerializeField] CarsManager _carsManager;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Initialize();
                Debug.Log("MAX COUNTS: " + maxGates);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Initialize()
        {
            if (gates == null || gates.Count == 0)
                FindGates();

            //gates.RemoveAll(x => x.gameObject.activeInHierarchy == false);
            maxGates = gates.Count;
            if (!initializeIds) return;
            for (int i = 0; i < gates.Count; i++)
            {
                gates[i].id = i;
            }
        }

        private void FindGates()
        {
            if (findGates)
            {
                for (int i = 0; i < _parentGates.childCount; i++)
                {
                    var child = _parentGates.GetChild(i);

                    for (int j = 0; j < child.childCount; j++)
                    {
                        var gate = child.GetChild(j);


                        if (gate.TryGetComponent(out Gate item))
                        {
                            gates.Add(item);
                        }
                    }
                }

            }
        }

        [Server]
        public void CarPassedThrough(int id_gate, string id_car)
        {
            int passed = CarsManager.instance.cars.FindIndex(x => x.Car.Id.Equals(id_car));

            if (passed < 0)
            {
                Debug.LogWarning($"Car {id_car} is not in the server cars list");
                return;
            }
            CarDescriptor c = CarsManager.instance.cars[passed].Car;

            //Gate indica il prossimo Gate da passare, quindi controllo se il gate in cui è passato è lo stesso di quello che doveva passare
            if (c.Gates == id_gate)
            {
                c.Gates = id_gate + 1;

                //Premia l'agente per aver passato un checkpoint
                CarsManager.instance.cars[passed].Player.agent.checkpoint++;

                return;
            }
            //se il valore di gate è maggiore del numero di gate in campo vuol dire che è passato in tutti i gate
            //se è passato per tutti i gates e ora sta passando al gate iniziale ha fatto un giro
            if (c.Gates == maxGates && id_gate == 0)
            {

                c.Gates = 1;
                c.Laps++;

                //reset timer
                CarsManager.instance.cars[passed].Ui.offsetTime = NetworkTime.time;

                //Premia l'agente per aver concluso un giro
                CarsManager.instance.cars[passed].Player.agent.lap = true;

                Debug.Log($"Player {id_car} has completed a lap");
                return;
            }
            Debug.Log($"Player {id_car} has already passed through this gate");
        }

        private int GetGateIdPlayer(string id_car)
        {
            int passed = CarsManager.instance.cars.FindIndex(x => x.Car.Id.Equals(id_car));

            if (passed < 0)
            {
                Debug.LogWarning($"Car {id_car} is not in the server cars list");
                //2 possibilità o c'è stato un errore oppure il codice per aggiungere la macchina nel carmanager non è stato ancora eseguito
                return -1;
            }
            CarDescriptor c = CarsManager.instance.cars[passed].Car;
            return ((c.Gates + maxGates) % maxGates);
        }

        public Vector3 NextGatePosition(int id)
        {
            int id_gate = (id + maxGates) % maxGates;
            return gates[id_gate].transform.position;
        }

        public Vector3 NextGateForward(string id_car)
        {
            var id = GetGateIdPlayer(id_car);
            if (id < 0)
                return Vector3.zero;
            return gates[id].transform.forward;
        }

        public Vector3 NextNextGateForward(string id_car)
        {
            int id = (GetGateIdPlayer(id_car) + 1) % maxGates;
            return gates[id].transform.forward;
        }
        public float DistanceBetweemTwoGates(int id1, int id2)
        {
            var pos1 = NextGatePosition(id1);
            var pos2 = NextGatePosition(id2);

            return Mathf.Abs(Vector3.Distance(pos1, pos2));
        }

        public float GetPercentageDistance(string playerName, Vector3 posPlayer)
        {
            //gate = prossimo gate
            int gate = GetGateIdPlayer(playerName);

            //previous= checkpoint precedente a gate
            int previous = (gate - 1 + maxGates) % maxGates;

            //distanza massima per arrivare al checkpoint da passare
            var distance = DistanceBetweemTwoGates(gate, previous);

            //distanza attuale tra giocatore e gate
            float actual = Mathf.Abs(Vector3.Distance(posPlayer, gates[gate].transform.position));

            return (1.0f - actual / distance);
        }
    }

}