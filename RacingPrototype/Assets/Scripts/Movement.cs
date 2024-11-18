using Mirror;
using Mirror.Experimental;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    Rigidbody _rigidbody;
    public Transform centerOfMass;
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

    public Renderer render;
    private Material playerMaterialClone;
    private Material lightsMaterialClone;


    public float Velocity
    {
        get
        {
            if (_rigidbody != null)
                return _rigidbody.velocity.magnitude;
            else return 0;
        }
    }

    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;
    void OnColorChanged(Color _Old, Color _New)
    {
        playerMaterialClone = new Material(render.material); //Recupera materiale attuale
        playerMaterialClone.color = _New; //Cambia colore del materiale con quello nuovo
        render.material = playerMaterialClone; //Aggiorna materiale
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }


    public override void OnStartLocalPlayer()
    {
        maxSpeed = 40;

        //Set up camera
        var cam = FindObjectOfType<FollowCamera>();
        cam.StartFollowing(gameObject);

        _rigidbody.centerOfMass = centerOfMass.localPosition;

        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        CmdSetupPlayer(color);
    }
    [Command]
    public void CmdSetupPlayer(Color _col)
    {
        playerColor = _col;
        OnColorChanged(Color.black, _col);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        var moveX = ReadAxis("Horizontal");
        var MoveZ = ReadAxis("Vertical");
        var breaking = ReadAxis("Fire2");

        UseInput(moveX, 1, breaking);
        
    }

    private int ReadAxis(string name)
    {
        float x = Input.GetAxis(name);
        int moveX = 0;
        if (x > 0) moveX = 1;
        else if (x < 0) moveX = -1;
        return moveX;
    }

#if Wheels
    public void UseInput(int movex,int movez,int breaking)
    {
        float moveX = movex * maxSteerAngle;
        frontDriverW.steerAngle = moveX;
        frontPassengerW.steerAngle = moveX;


        float moveZ = movez * motorForce;
        frontPassengerW.motorTorque = moveZ;
        frontDriverW.motorTorque = moveZ;

        //Bost per la velocit� per aiutare la macchina ad accelerare pi� velocemente 
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

        if (_rigidbody.velocity.magnitude > maxSpeed)
        {
            _rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
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
#else
    public void UseInput(int movex, int movez, int breaking)
    {

        transform.Translate(movez*transform.forward*motorForce/10*Time.deltaTime);

        
    }
#endif
}
