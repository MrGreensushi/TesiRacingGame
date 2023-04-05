using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToStart : MonoBehaviour
{
    [SerializeField] Vector3 start;
    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.parent.localPosition = start;
    }
}
