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

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

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

                Debug.Log($"Player {id_car} has completed a lap");
                return;
            }
            Debug.Log($"Player {id_car} has already passed through this gate");
        }



        public Vector3 NextGatePosition(int id)
        {
            if (id >= maxGates)
            {
                return gates[0].transform.position;
            }
            return gates[id].transform.position;
        }
    }

}