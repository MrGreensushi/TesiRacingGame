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

        private void Awake()
        {
            _lapsManager = GameObject.FindObjectOfType<LapsManager>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.parent.GetComponent<PlayerScript>() == null) return;
            string nome = other.transform.parent.GetComponent<PlayerScript>().playerName;
            _lapsManager.CarPassedThrough(id, nome);
            Debug.Log($"Collision: {nome} has entered gate {id}");
        }
    }
}

