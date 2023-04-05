using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Barracuda;
using UnityEngine;

public class OfflineCar : MonoBehaviour
{

    public Renderer render;
    private Material lightsMaterialClone;
    public Transform centerOfMass;
    public string playerName = "Player";

    //Wheels
    public WheelCollider frontDriverW, frontPassengerW;
    public WheelCollider rearDriverW, rearPassengerW;
    public Transform frontDriverT, frontPassengerT;
    public Transform rearDriverT, rearPassengerT;

    //Driving
    public float maxSteerAngle = 30;
    public float motorForce = 50;
    public float breakForce = 30;
    public int initial_boost = 15;
    public float boost_duration = 0.1f;

    public bool giocatore;
    Rigidbody _rigidbody;

    //ML Agent
    public Offline_MLcar agent;
    public float Velocity { get => _rigidbody.velocity.magnitude; }
    public Vector3 VelocityNormal { get => _rigidbody.velocity.normalized; }
    public string VectorizedVelocity
    {
        get
        {
            var v = _rigidbody.velocity;
            return v.x.ToString() + ';' + v.z.ToString();
        }
    }
    public string Position { get { var v = transform.localPosition; return v.x.ToString() + ';' + v.z.ToString(); } }
    public string Rotation { get { var v = transform.localRotation.eulerAngles; return v.y.ToString(); } }
    public int[] LastAction { get => agent.lastAction; }
    // Start is called before the first frame update
    void Start()
    {
        //Set up camera
        if (giocatore)
        {
            Camera.main.transform.SetParent(transform);

            Camera.main.transform.localPosition = new Vector3(0, 4, -6);
            Camera.main.transform.localRotation = Quaternion.Euler(16, 0, 0);
            Camera.main.orthographic = false;
            Camera.main.fieldOfView = 60;

        }

        //check il nome sia unico
        string name;
        List<OfflineCar> allCars = new List<OfflineCar>();
        allCars.AddRange(FindObjectsOfType<OfflineCar>());
        do
        {
            name = "Player " + Random.Range(100, 999);

        }
        while (allCars.FindIndex(x => x.playerName == name) >= 0);// se il nome già esiste allora creane uno nuovo
        playerName = name;

        //Lower center of mass
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass.localPosition;

        //agent.ConfigCar();
        //agent.carsManager.AddCar(this, playerName);
    }

    public void UseInput(float movex, float movez, int breaking)
    {
        float moveX = movex * maxSteerAngle;
        frontDriverW.steerAngle = moveX;
        frontPassengerW.steerAngle = moveX;


        float moveZ = movez * motorForce;
        frontPassengerW.motorTorque = moveZ;
        frontDriverW.motorTorque = moveZ;

        //Bost per la velocità per aiutare la macchina ad accelerare più velocemente 
        if (Velocity > 0 && moveZ > 0)
            _rigidbody.AddForce(initial_boost * transform.forward * motorForce * Mathf.Exp(-Velocity * boost_duration));

        frontDriverW.brakeTorque = breakForce * breaking;
        frontPassengerW.brakeTorque = breakForce * breaking;

        Color _col = breaking == 1 ? Color.white : Color.clear;
        if (_col != render.materials[1].GetColor("_EmissionColor"))
        {
            lightsMaterialClone = new Material(render.materials[1]); //crea copia materiale altrimenti cambierebbe il materiale a tutti
            render.materials[1] = lightsMaterialClone;
            render.materials[1].SetColor("_EmissionColor", _col); //modifica copia materiale
        }

        //Update transform and rotation of the wheels (wheel collider is not attached to the mesh transform of the wheels)
        UpdateWheelPoses();
    }


    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontDriverW, frontDriverT);
        UpdateWheelPose(frontPassengerW, frontPassengerT);
        UpdateWheelPose(rearDriverW, rearDriverT);
        UpdateWheelPose(rearPassengerW, rearPassengerT);
    }

    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
        Vector3 _pos = _transform.position;
        Quaternion _quat = _transform.rotation;

        _collider.GetWorldPose(out _pos, out _quat);

        _transform.position = _pos;
        _transform.rotation = _quat;
    }

    private void Update()
    {
        frontDriverW.GetGroundHit(out WheelHit wheelData);
        var slipLat = wheelData.sidewaysSlip * 1000;
        var slipLong = wheelData.forwardSlip * 1000;
        var forceValue = wheelData.force - 1562.595;
        //Debug.Log((int)slipLong + "  " + (int)slipLat);

    }

    public void StopCar()
    {
        /*frontDriverW.brakeTorque = Mathf.Infinity;
        frontPassengerW.brakeTorque = Mathf.Infinity;
        rearDriverW.brakeTorque = Mathf.Infinity;
        rearDriverW.brakeTorque = Mathf.Infinity;
        */
        _rigidbody.velocity = Vector3.zero;
    }

}
