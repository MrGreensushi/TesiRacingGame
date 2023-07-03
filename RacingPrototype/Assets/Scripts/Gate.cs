using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class Gate : NetworkBehaviour
    {
        public int id;

        LapsManager _lapsManager;

        private void Start()
        {
            _lapsManager = GameObject.FindObjectOfType<LapsManager>();
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            var car = other.transform.parent.GetComponent<PlayerScript>();
            if (car == null) return;
            string nome = car.playerName;
            _lapsManager.CarPassedThrough(id, nome);
            //Debug.Log($"Collision: {nome} has entered gate {id}");
        }
    }
}

