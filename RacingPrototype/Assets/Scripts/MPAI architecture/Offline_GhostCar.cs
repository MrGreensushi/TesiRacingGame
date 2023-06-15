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


    public (float[], float[]) PhysicInfos()
    {
        var v = mybody.velocity;
        var lp = transform.localPosition;
        var r = transform.rotation.eulerAngles;
        float[] info = new float[]  {
                lp.x,
                lp.z,
                v.x,
                v.z,
                r.y
            };

        float[] delta = new float[5];
        for (int i = 2; i < 5; i++)
        {
            delta[i] = info[i] - lastInfo[i];
        }
        delta[0] = info[0];
        delta[1] = info[1];

        //for (int i = 0; i < 5; i++)
        //{
        //    delta[i] = info[i] - lastInfo[i];
        //}

        delta[4] = (delta[4] + 180) % 360 - 180;

        return (delta, info);

    }



    public void UpdateBody(float[] infos, int movex)
    {

        float moveX = movex * maxSteerAngle;
        frontDriverW.steerAngle = moveX;
        frontPassengerW.steerAngle = moveX;

        var x = mybody.rotation.eulerAngles.x;
        var z = mybody.rotation.eulerAngles.z;
        if (onlyPosition)
        {
            mybody.isKinematic = true;
            mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
            mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
            return;
        }
        else if (onlyVelocity)
        {
            mybody.velocity = new Vector3(infos[0], 0, infos[1]);
            mybody.rotation = Quaternion.Euler(new Vector3(x, infos[2], z));
            //mybody.velocity = new Vector3(infos[2], mybody.velocity.y, infos[3]);
            //mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
            //UpdateWheelPoses();
            return;
        }

        mybody.position = new Vector3(infos[0], transform.position.y, infos[1]);
        mybody.velocity = new Vector3(infos[2], mybody.velocity.y, infos[3]);
        mybody.rotation = Quaternion.Euler(new Vector3(x, infos[4], z));
        UpdateWheelPoses();

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
