using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class StartingPointsManager : NetworkBehaviour
    {
        List<StartingPoint> _startingPoints = new List<StartingPoint>();
        public static StartingPointsManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                _startingPoints.AddRange(FindObjectsOfType<StartingPoint>());

            }
            else
                Destroy(this);
        }

        private void SetParent(StartingPoint par, Transform g)
        {
            var genitore = par.transform.parent.parent.GetComponentInChildren<CarHolder>();
            g.SetParent(genitore.transform);

        }

        public Vector3 StartingPoint(Transform g)
        {

            //scegli starting poin in maniera casuale
            int i = Random.Range(0, _startingPoints.Count);

            for (int j = 0; j < _startingPoints.Count; j++)
            {
                if (_startingPoints[i].IsFree)
                {
                    _startingPoints[i].IsFree = false;
                    SetParent(_startingPoints[i], g);
                    return _startingPoints[i].transform.localPosition;
                }
                i++;
                if (i >= _startingPoints.Count)
                    i = 0;
            }


            Debug.LogError("No available starting points");

            return Vector3.zero;
        }

       
    }
}