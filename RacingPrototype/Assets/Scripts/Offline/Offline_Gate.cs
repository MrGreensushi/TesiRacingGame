using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offline_Gate : MonoBehaviour
{
    public int id;

    [SerializeField] Offline_LapsManager _lapsManager;

    public Offline_LapsManager LapsManager { set => _lapsManager = value; }
    private void OnTriggerEnter(Collider other)
    {
        var car = other.transform.parent.GetComponent<OfflineCar>();
        if (car == null) return;
        string nome = car.playerName;
        _lapsManager.CarPassedThrough(id, nome);
        // Debug.Log($"Collision: {nome} has entered gate {id}");
    }
}
