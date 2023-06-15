using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleCamera : MonoBehaviour
{
    [SerializeField] Transform toFollow;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(toFollow.position.x, transform.position.y, toFollow.position.z);
    }
}
