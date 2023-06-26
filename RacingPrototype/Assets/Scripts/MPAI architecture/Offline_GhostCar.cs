using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Offline_GhostCar : MonoBehaviour
{
    [SerializeField] GameObject toCopyCar;
    Rigidbody toCopyBody, mybody;
    public bool predicting = false;
    public float[] lastInfo;
    //Wheels
    public WheelCollider frontDriverW, frontPassengerW;
    public WheelCollider rearDriverW, rearPassengerW;
    public Transform frontDriverT, frontPassengerT;
    public Transform rearDriverT, rearPassengerT;
    public MeshRenderer bodyRenderer;
    public TrailRenderer trailRenderer;
    public Transform centerOfMass;

    private bool onlyPosition, onlyVelocity;

    public float maxSteerAngle = 30;

    public bool OnlyPosition
    {
        set
        {
            mybody.isKinematic = value;
            onlyPosition = value;
        }
    }

    public bool OnlyVelocity { set => onlyVelocity = value; }
    public bool Predicting { set { predicting = value; bodyRenderer.enabled = value; if (!value) trailRenderer.Clear(); trailRenderer.enabled = value; } }
    private void Start()
    {
        predicting = false;
        toCopyBody = toCopyCar.GetComponent<Rigidbody>();
        mybody = GetComponent<Rigidbody>();
        lastInfo = new float[5] { 0, 0, 0, 0, 0 };
        mybody.centerOfMass = centerOfMass.localPosition;

    }



    private void Update()
    {
        if (predicting) return;
        mybody.velocity = toCopyBody.velocity;
        mybody.angularVelocity = toCopyBody.angularVelocity;
        mybody.position = toCopyCar.transform.position;
        mybody.rotation = toCopyCar.transform.rotation;
        UpdateWheelPoses();

    }


    // public (float[], float[]) PhysicInfos()
    // {
    //     var v = mybody.velocity;
    //     var lp = transform.localPosition;
    //     var r = transform.rotation.eulerAngles;
    //     float[] info = new float[]  {
    //             lp.x,
    //             lp.z,
    //             v.x,
    //             v.z,
    //             r.y
    //         };
    //
    //     float[] delta = new float[5];
    //     for (int i = 2; i < 5; i++)
    //     {
    //         delta[i] = info[i] - lastInfo[i];
    //     }
    //     delta[0] = info[0];
    //     delta[1] = info[1];
    //
    //     //for (int i = 0; i < 5; i++)
    //     //{
    //     //    delta[i] = info[i] - lastInfo[i];
    //     //}
    //
    //     delta[4] = (delta[4] + 180) % 360 - 180;
    //
    //     return (delta, info);
    //
    // }

    public float[] PhysicInfos()
    {
        //VEL ANG_VEL TILE TILE_IND X_R Z_R
        var vel = mybody.velocity;
        var ang = mybody.angularVelocity.y;
        var tile = ReturnInfoTileFloat();
        float[] toRet = { vel.x, vel.z, ang, tile[0], tile[1], tile[2], tile[3] };
        return toRet;
    }



    public float[] ReturnInfoTileFloat()
    {
        var hits = Physics.RaycastAll(centerOfMass.position + Vector3.up, Vector3.down, 10);
        foreach (var item in hits)
        {
            var ind = TagManager.tags.FindIndex(t => t.Name.Equals(item.collider.tag));
            if (ind != -1)
            {
                return RetrieveInfoFromHitFloat(item.collider, ind);

            }
        }

        //se non trova nulla con i raycast potrebbe essere proprio nel mezzo tra due
        //quindi provo a spostare il raggio poco più avanti
        hits = Physics.RaycastAll(centerOfMass.position + Vector3.up + Vector3.forward * 0.1f, Vector3.down, 10);
        foreach (var item in hits)
        {
            var ind = TagManager.tags.FindIndex(t => t.Name.Equals(item.collider.tag));
            if (ind != -1)
            {
                return RetrieveInfoFromHitFloat(item.collider, ind);

            }
        }

        float[] zeros = { 0f, 0f, 0f, 0f };
        return zeros;

    }


    private float[] RetrieveInfoFromHitFloat(Collider c, int ind)
    {

        var tag = c.tag;
        var value = TagManager.tags[ind].Value;
        var sibling = c.transform.GetSiblingIndex();

        var pos_onBox = transform.position - c.transform.position;
        var x_b = c.bounds.size.x;
        var z_b = c.bounds.size.z;

        //se x_b e z_b sono diversi allora in base alla rotazione dell'oggetto bisogna invertirli
        var rot = c.transform.rotation.eulerAngles.y;


        if (rot == 90 || rot == 270 || rot == -90)
        {
            var t = x_b;
            x_b = z_b;
            z_b = t;
        }

        pos_onBox = new Vector3(pos_onBox.x / x_b, 0, pos_onBox.z / z_b);

        float[] toRet = { value, sibling, pos_onBox.x, pos_onBox.z };
        return toRet;
    }


    //public void UpdateBody(float[] infos, int movex)
    //{
    //
    //    float moveX = movex * maxSteerAngle;
    //    frontDriverW.steerAngle = moveX;
    //    frontPassengerW.steerAngle = moveX;
    //
    //    var x = mybody.rotation.eulerAngles.x;
    //    var z = mybody.rotation.eulerAngles.z;
    //    if (onlyPosition)
    //    {
    //        mybody.isKinematic = true;
    //        mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
    //        mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
    //        return;
    //    }
    //    else if (onlyVelocity)
    //    {
    //        mybody.velocity = new Vector3(infos[0], 0, infos[1]);
    //        mybody.rotation = Quaternion.Euler(new Vector3(x, infos[2], z));
    //        //mybody.velocity = new Vector3(infos[2], mybody.velocity.y, infos[3]);
    //        //mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
    //        //UpdateWheelPoses();
    //        return;
    //    }
    //
    //    mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
    //    mybody.velocity = new Vector3(infos[2], mybody.velocity.y, infos[3]);
    //    mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
    //    UpdateWheelPoses();
    //
    //}
    public void UpdateBody(float[] infos, int movex)
    {
        if (onlyPosition)
        {
            //mybody.isKinematic = true;
            //mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
            //mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
            //return;
        }
        else if (onlyVelocity)
        {
            mybody.velocity = new Vector3(infos[0], 0, infos[1]);
            mybody.angularVelocity = new Vector3(mybody.angularVelocity.x, infos[2], mybody.angularVelocity.z);
            return;
        }


        //mybody.velocity = new Vector3(infos[0], 0, infos[1]);
        //mybody.angularVelocity = new Vector3(0, infos[2], 0);
        //
        //
        //
        //
        //mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
        //mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
        //UpdateWheelPoses();

    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontDriverW, frontDriverT);
        UpdateWheelPose(frontPassengerW, frontPassengerT);
        UpdateWheelPose(rearDriverW, rearDriverT);
        UpdateWheelPose(rearPassengerW, rearPassengerT);
    }


}
