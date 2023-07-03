using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class ServerCommunicator : NetworkBehaviour
    {
        public static ServerCommunicator instance;


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

      
        public Vector3 CMDNextGateForward(string id_car)
        {
            var toRet = LapsManager.instance.NextGateForward(id_car);
            return toRet;

        }



        public Vector3 NextGateForward(string id_car)
        {
            return CMDNextGateForward(id_car);
        }

       
        public Vector3 CMDNextNextGateForward(string id_car)
        {
            var toRet = LapsManager.instance.NextNextGateForward(id_car);
            return toRet;

        }

        public Vector3 NextNextGateForward(string id_car)
        {
            return CMDNextNextGateForward(id_car);
        }

       
        public float CMDGetPercentageDistance(string playerName, Vector3 posPlayer)
        {
            var toRet = LapsManager.instance.GetPercentageDistance(playerName, posPlayer);
            return toRet;

        }

        public float GetPercentageDistance(string playerName, Vector3 posPlayer)
        {
            return CMDGetPercentageDistance(playerName, posPlayer);
        }

    }
}

