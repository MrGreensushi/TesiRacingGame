using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Vector3 cameraPos,cameraRot;
    public bool orthographic;
    public float size,velocity;
    public GameObject toFollow,holder;
    private bool following=false;
    public Camera cam;

    private Vector3 initial;
    private Quaternion ini_rot;
    private float ini_size;

    private void Start()
    {
         //previous values
        initial=transform.position;
        ini_rot=transform.rotation;
        ini_size = cam.orthographicSize;
    }

    public void StartFollowing(GameObject g)
    {
        toFollow = g;
       
       

        transform.localRotation = Quaternion.Euler(cameraRot.x, cameraRot.y, cameraRot.z);
        transform.localPosition = cameraPos;
        cam.orthographic = orthographic;
        
        if(orthographic)
            cam.orthographicSize = size;
        else
            cam.fieldOfView = size;

        holder.transform.position=toFollow.transform.position;
        following = true;

    }

    public void SwitchView(GameObject g)
    {
        if (following)
            StopFollowing();
        else
            StartFollowing(g);
    }


    void LateUpdate()
    {
        if (!following) return;
        var pos= toFollow.transform.position;
        var final= new Vector3(pos.x, 0, pos.z);
        var rot=toFollow.transform.rotation.eulerAngles.y;


        holder.transform.position = final;
        holder.transform.rotation = Quaternion.Euler(0, rot, 0);
    }

    public void StopFollowing()
    {
        holder.transform.position = Vector3.zero;
        following = false;
        transform.position = initial;
        transform.rotation = ini_rot;
        cam.orthographic = true;
        cam.orthographicSize = ini_size;

       
    }
}
