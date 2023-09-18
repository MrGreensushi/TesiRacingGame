using Mirror;
using Mirror.Experimental;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.MLAgents.Policies;
using UnityEngine;

public class Ghost_Car : MonoBehaviour
{
    public GameObject toCopyCar;
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


    [Tooltip("Testing: The ghost car is visible on the server but it does not change the player's car\n" +
        "Real-Case: Ghost car remains invisible but the prediction is applied to the player's car")]
    public OperatingMode operatingMode;
    private PlayerScript player;
    private bool copyCar = true;
    private bool teleportCar = false;
    public bool Heuristic
    {
        set
        {
            if (!toCopyCar.TryGetComponent(out BehaviorParameters bp))
            {
                Debug.LogError("Non ha componente Behavior Parameters!");
                return;
            }
            if (value)
                bp.BehaviorType = BehaviorType.HeuristicOnly;
            else
                bp.BehaviorType = BehaviorType.Default;

        }
    }

    public float maxSteerAngle = 30;

    public Rigidbody RigidbodyCar { get { return mybody; } }
    public Vector3 InfoToCompare { get => new Vector3(toCopyBody.velocity.x, toCopyBody.velocity.z, toCopyBody.rotation.eulerAngles.y); }
    public bool Predicting { set { predicting = value; bodyRenderer.enabled = value; if (!value) trailRenderer.Clear(); trailRenderer.enabled = value; } }
    private void Start()
    {
        predicting = false;
        toCopyBody = toCopyCar.GetComponent<Rigidbody>();
        player = toCopyCar.GetComponent<PlayerScript>();
        mybody = GetComponent<Rigidbody>();
        lastInfo = new float[5] { 0, 0, 0, 0, 0 };
        mybody.centerOfMass = centerOfMass.localPosition;

        bodyRenderer.enabled = false;
        trailRenderer.Clear();
        trailRenderer.enabled = false;
    }

    private void FixedUpdate()
    {
        //if the car is being controlled by the prediction do not copy the player car otherwise yes
        if (!copyCar) return;
        if (toCopyBody == null) return;
        mybody.velocity = toCopyBody.velocity;
        mybody.angularVelocity = toCopyBody.angularVelocity;
        mybody.position = toCopyCar.transform.position;
        mybody.rotation = toCopyCar.transform.rotation;

    }

    public (float[], float[]) CommPhyInfos()
    {
        //VEL ANG_VEL TILE TILE_IND X_R Z_R


        var vel = mybody.velocity;
        var ang = mybody.rotation.eulerAngles.y;
        var lp = transform.localPosition;
        float[] info = new float[]  {
            lp.x,
            lp.z,
            vel.x,
            vel.z,
            ang
        };
        var tile = ReturnInfoTileFloat();
        float[] delta = new float[5];
        for (int i = 0; i < 3; i++)
        {
            delta[i] = info[i + 2] - lastInfo[i + 2];
        }
        delta[2] = (delta[2] + 180) % 360 - 180;
        var commands = player.LastAction;
        float[] toRet = { delta[0], delta[1], delta[2], tile[0], tile[1], tile[2], tile[3], commands[0], commands[1], commands[2] };
        return (toRet, info);
    }

    public (float[], float[]) PhysicInfos()
    {
        //VEL ANG_VEL TILE TILE_IND X_R Z_R


        var vel = mybody.velocity;
        var ang = mybody.rotation.eulerAngles.y;
        var lp = transform.localPosition;
        float[] info = new float[]  {
            lp.x,
            lp.z,
            vel.x,
            vel.z,
            ang
        };
        var tile = ReturnInfoTileFloat();
        float[] delta = new float[5];
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


    public void UpdateBody(float[] infos)
    {
        //The ghost is driven by the prediction
        mybody.velocity = new Vector3(infos[0], 0, infos[1]);
        mybody.rotation = Quaternion.Euler(new Vector3(0, infos[2], 0));
        //StartCoroutine(InterpolateRotation(infos[2], 4));

        if (operatingMode == OperatingMode.Testing)
        {
            UpdateTestingMode();
            copyCar = false;
            return;
        }
        copyCar = false;
        //Takes authority over the player's car both on the client and server
        //player.clientAuthority = false;
        player.ChangeAuthority(false);

        //Updates the real car attributes with the predicted ones
        player.UpdateRealCar(new Vector3(infos[0], 0, infos[1]), infos[2]);
    }

    private void UpdateTestingMode()
    {
        //Car is made visible on the server
        bodyRenderer.enabled = true;
        trailRenderer.enabled = true;

        //if copycar is true it means it is the first time it is called only then the trail must be cleared
        if (copyCar)
            trailRenderer.Clear();

        //No changes are made on the player's car
    }

    public void CopyFromTrue()
    {
        if (!copyCar)// se è appena finito SPG allora copycar è falso, per assicurarsi che il client posizioni la macchina rispetto al server tardo la consegna dell'autorità di un frame
            teleportCar = true;



        copyCar = true;
        bodyRenderer.enabled = false;
        trailRenderer.Clear();
        trailRenderer.enabled = false;

    }

    private void LateUpdate()
    {
        if (teleportCar)
        {
            if (operatingMode == OperatingMode.RealCase)
                player.TargetTeleportCar(
                    player.netIdentity.connectionToClient,
                    mybody.position,
                    mybody.rotation,
                    mybody.velocity
                    );


            player.ChangeAuthority(true); //cambio autorità sul selrver
        }
        teleportCar = false;
    }


    public IEnumerator InterpolateRotation(float rot, int iterations)
    {

        var diffRot = rot - mybody.rotation.eulerAngles.y;
        diffRot = (diffRot + 180) % 360 - 180;
        float angVel = diffRot / (iterations * 0.02f);

        Vector3 EulerAngelVel = new Vector3(0, angVel, 0);

        if (diffRot < 0f)
            EulerAngelVel *= -1;
        var deltaRot = Quaternion.Euler(EulerAngelVel * Time.fixedDeltaTime);

        int counter = 0;



        while (counter < iterations)
        {

            mybody.MoveRotation(mybody.rotation * deltaRot);
            counter++;
            yield return new WaitForFixedUpdate();
        }
        mybody.rotation = Quaternion.Euler(0, rot, 0);
    }

}


