using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offline_StartingPoint : MonoBehaviour
{
    BoxCollider _collider;
    int overlap = 0;
    public bool isEntering = false;

    public bool IsFree { get => overlap == 0 && !isEntering; set => isEntering = !value; }

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {

        overlap++;
        if (isEntering)
            isEntering = false;
    }

    private void OnTriggerExit(Collider other)
    {
        overlap--;
        if (isEntering)
            isEntering = false;
    }


}
