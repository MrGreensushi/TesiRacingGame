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
    float maxSpeed = 40;

    public bool giocatore;
    Rigidbody _rigidbody;

    //acceleration
    private Vector3 lastVelocity = new Vector3();
    private Vector3 acceleration = new Vector3();

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

    public string AngularVelString { get => _rigidbody.angularVelocity.y.ToString(); }
    public string Acceleration { get { return acceleration.x.ToString() + ';' + acceleration.z.ToString(); } }
    public int[] LastAction { get => agent.lastAction; }

    public float[] lastInfo;
    public float[] PhysicInfos
    {
        get
        {
            var v = _rigidbody.velocity;
            var lp = transform.localPosition;
            var r = transform.rotation.eulerAngles;
            float[] info = new float[]  {
                lp.x,
                lp.z,
                v.x,
                v.z,
                r.y
            };


            return info;
        }
    }

    //DELTA

    //public (float[], float[]) DispatcherInfos()
    //{
    //    //var info = PhysicInfos;
    //    //float[] delta = new float[5];
    //    //for (int i = 2; i < 5; i++)
    //    //{
    //    //    delta[i] = info[i] - lastInfo[i];
    //    //}
    //    //delta[0] = info[0];
    //    //delta[1] = info[1];
    //    //
    //    //// for (int i = 0; i < 5; i++)
    //    //// {
    //    ////     delta[i] = info[i] - lastInfo[i];
    //    //// }
    //    //delta[4] = (delta[4] + 180) % 360 - 180;
    //    //return (delta, info);
    //}

    //REAL
    //public float[] DispatcherInfos()
    //{
    //    var vel = _rigidbody.velocity;
    //    var ang = _rigidbody.rotation.eulerAngles.y / 360f; //Normalizzo la rotazione
    //    var tile = ReturnInfoTileFloat();
    //    float[] toRet = { vel.x, vel.z, ang, tile[0], tile[1], tile[2], tile[3] };
    //    return toRet;
    //}

    //DELTA W/ TILE
    public (float[], float[]) DispatcherInfos()
    {
        //VEL ANG_VEL TILE TILE_IND X_R Z_R
        var info = PhysicInfos;
        var tile = ReturnInfoTileFloat();
        float[] delta = new float[3];
        for (int i = 0; i < 3; i++)
        {
            delta[i] = info[i + 2] - lastInfo[i + 2];
        }
        delta[2] = (delta[2] + 180) % 360 - 180;
        float[] toRet = { delta[0], delta[1], delta[2], tile[0], tile[1], tile[2], tile[3] };

        return (toRet, info);
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

    public string ReturnInfoTile()
    {
        var hits = Physics.RaycastAll(centerOfMass.position + Vector3.up, Vector3.down, 10);
        foreach (var item in hits)
        {
            var ind = TagManager.tags.FindIndex(t => t.Name.Equals(item.collider.tag));
            if (ind != -1)
            {
                return RetrieveInfoFromHit(item.collider, ind);

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
                return RetrieveInfoFromHit(item.collider, ind);

            }
        }

        return ";;;";

    }


    private string RetrieveInfoFromHit(Collider c, int ind)
    {

        var tag = c.tag;
        var value = TagManager.tags[ind].Value;
        var sibling = c.transform.GetSiblingIndex();
        Debug.Log(sibling);

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
        Debug.Log(pos_onBox);


        string toRet = value.ToString() + ';' + sibling.ToString() + ';' + pos_onBox.x.ToString()
            + ';' + pos_onBox.z.ToString();
        return toRet;
    }



    void Start()
    {
        lastInfo = new float[5] { 0, 0, 0, 0, 0 };
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

        if (_rigidbody.velocity.magnitude > maxSpeed)
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
        }
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
    private void FixedUpdate()
    {
        acceleration = (_rigidbody.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = _rigidbody.velocity;


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
