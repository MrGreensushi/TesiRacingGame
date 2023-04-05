using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Offline_StartingPoint_Manager : MonoBehaviour
{
    [SerializeField] List<GameObject> tracks;
    List<List<Offline_StartingPoint>> _startingPointsTracks = new List<List<Offline_StartingPoint>>();

    private void Initialize()
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            _startingPointsTracks.Add(new List<Offline_StartingPoint>());
            _startingPointsTracks[i].AddRange(tracks[i].gameObject.GetComponentsInChildren<Offline_StartingPoint>());
        }

    }


    public Vector3 StartingPoint(float config_num, Transform g)
    {
        if (_startingPointsTracks.Count == 0)
        {
            Initialize();
        }

        List<Offline_StartingPoint> _startingPoints = _startingPointsTracks[Mathf.FloorToInt(config_num)];

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

        /* foreach (var item in _startingPoints)
         {
             if (item.IsFree)
             {
                 item.IsFree = false;
                 return item.transform.localPosition;
             }

         }*/

        Debug.LogError("No available starting points");

        return Vector3.zero;
    }


    private void SetParent(Offline_StartingPoint par, Transform g)
    {
        var genitore = par.transform.parent.parent.GetComponentInChildren<CarHolder>();
        g.SetParent(genitore.transform);

    }

}
