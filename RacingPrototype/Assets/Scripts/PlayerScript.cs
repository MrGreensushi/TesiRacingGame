using Google.Protobuf.WellKnownTypes;
using Mirror;
using Mirror.Experimental;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMeshPro playerNameText;
        public GameObject floatingInfo;
        public Renderer render;
        private Material playerMaterialClone;
        private Material lightsMaterialClone;
        public GameObject uiPrefab;
        public Transform centerOfMass;

        //Trail only for server
        [SerializeField] TrailRenderer trilRenderer;

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
        //ML Agent
        public ML_Car agent;
        private BehaviorParameters agent_type;

        Rigidbody _rigidbody;

        //Network
        private NetworkTransformChild[] net_TransformChilds;
        private NetworkTransform net_Transform;
        NetworkRigidbody net_Rigidbody;
        NetworkIdentity net_Identity;


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


        public float Velocity
        {
            get
            {
                if (_rigidbody != null)
                    return _rigidbody.velocity.magnitude;
                else return 0;
            }
        }
        public Vector3 VelocityNormal { get => _rigidbody.velocity.normalized; }
        public Vector3 VectorizedVelocity { get => _rigidbody.velocity; }

        public int[] LastAction { get => agent.lastAction; }

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        [SyncVar(hook = nameof(OnLightsChanged))]
        public Color ligthsColor = Color.clear;

        [SyncVar(hook = nameof(OnBotChanged))]
        public bool bot = false;

        public void OnBotChanged(bool _Old, bool _New)
        {
            bot = _New;
            if (bot)
            {
                agent_type.BehaviorType = BehaviorType.InferenceOnly;
            }
            else
            {
                agent_type.BehaviorType = BehaviorType.HeuristicOnly;
            }

        }



        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = playerName;
        }
        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;//Setta il nuovo colore
            playerMaterialClone = new Material(render.material); //Recupera materiale attuale
            playerMaterialClone.color = _New; //Cambia colore del materiale con quello nuovo
            render.material = playerMaterialClone; //Aggiorna materiale
        }
        void OnLightsChanged(Color _Old, Color _New)
        {
            lightsMaterialClone = new Material(render.materials[1]); //crea copia materiale altrimenti cambierebbe il materiale a tutti
            render.materials[1] = lightsMaterialClone;
            render.materials[1].SetColor("_EmissionColor", _New); //modifica copia materiale
        }


        [SyncVar(hook = nameof(OnAuthorityChanged))]
        public bool clientAuthority = true;

        void OnAuthorityChanged(bool _Old, bool _New)
        {
            ChangeAuthority(_New);

        }

        public void ChangeAuthority(bool value)
        {
            if (net_Transform == null || net_Rigidbody == null) return;
            net_Transform.clientAuthority = value;
            net_Rigidbody.clientAuthority = value;
            foreach (var t in net_TransformChilds)
                t.clientAuthority = value;
        }


        private void Awake()
        {
            agent_type = GetComponent<BehaviorParameters>();
        }

        public override void OnStartLocalPlayer()
        {
            //Set up camera
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 4, -6);
            Camera.main.transform.localRotation = Quaternion.Euler(16, 0, 0);
            Camera.main.orthographic = false;
            Camera.main.fieldOfView = 60;


            //Setup player infos
            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            //Lower center of mass
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.centerOfMass = centerOfMass.localPosition;

            //check il nome sia unico
            string name;
            List<PlayerScript> allCars = new List<PlayerScript>();
            allCars.AddRange(FindObjectsOfType<PlayerScript>());
            do
            {
                name = "Player " + Random.Range(100, 999);

            } while (allCars.FindIndex(x => x.playerName == name) >= 0);

            Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            //Aggiorna nome player in basso a sinistra
            CarsManager.instance.PlayerNameUI = name;
            CarsManager.instance.playerNameText.color = color;

            //ritrova le informazioni del network
            net_Identity = GetComponent<NetworkIdentity>();
            net_Transform = GetComponent<NetworkTransform>();
            net_Rigidbody = GetComponent<NetworkRigidbody>();
            net_TransformChilds = GetComponents<NetworkTransformChild>();

            //send infos to the server
            CmdSetupPlayer(name, color);


        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            //Set up camera
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 50, 0);
            Camera.main.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Camera.main.orthographicSize = 55;


            //Player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            OnColorChanged(Color.black, _col);

            _rigidbody = GetComponent<Rigidbody>();

            //colore del giocatore corrisponde anche al colore della scia
            trilRenderer.enabled = true;
            trilRenderer.startColor = _col;
            trilRenderer.endColor = _col;

            NetworkConnectionToClient conn = GetComponent<NetworkIdentity>().connectionToClient;
            //recupera tutte le altre ui

            //recupera id di tutte le macchine
            List<string> names = new List<string>();
            List<Color> colors = new List<Color>();
            foreach (var item in CarsManager.instance.cars)
            {
                names.Add(item.Car.Id);
                colors.Add(item.Player.playerColor);
            }


            //Aggiungo UI nel Manager del server
            GameObject ui = Instantiate(uiPrefab, CarsManager.instance.carInfoUi);
            CarsManager.instance.AddCar(this, ui.GetComponent<UI_Velocity>(), _name, _col);

            //chiedo ad ogni client di stampare la UI della nuova macchina
            InstantiateUI(_name, _col);

            //star position
            var starting = StartingPointsManager.instance.StartingPoint(transform);

            //aggiungi macchina al sistema MPAI
            Manager_MPAI.instance.AddCar(this);

            //lastInfo seto to zero
            lastInfo = new float[5] { 0, 0, 0, 0, 0 };

            //ritrova le informazioni del network
            net_Identity = GetComponent<NetworkIdentity>();
            net_Transform = GetComponent<NetworkTransform>();
            net_Rigidbody = GetComponent<NetworkRigidbody>();
            net_TransformChilds = GetComponents<NetworkTransformChild>();

            clientAuthority = true;

            TargetInstantiatePreviousUis(conn, starting, names.ToArray(), colors.ToArray());
        }

        [TargetRpc]
        public void TargetInstantiatePreviousUis(NetworkConnection target, Vector3 start, string[] names, Color[] colors)
        {

            transform.localPosition = start;

            for (int i = 0; i < names.Length - 1; i++)
            {
                GameObject ui = Instantiate(uiPrefab, CarsManager.instance.carInfoUi);
                CarsManager.instance.AddCar(this, ui.GetComponent<UI_Velocity>(), names[i], colors[i]);
            }
            agent.enabled = true;
        }

        [ClientRpc]
        void InstantiateUI(string name, Color _col)
        {

            //instanzio la UI e aggoingo la macchina al manager
            GameObject ui = Instantiate(uiPrefab, CarsManager.instance.carInfoUi);
            CarsManager.instance.AddCar(this, ui.GetComponent<UI_Velocity>(), name, _col);
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

        /*  private void FixedUpdate()
          {
              if (!isLocalPlayer)
                  return;
              float moveX = Input.GetAxis("Horizontal") * maxSteerAngle;
              frontDriverW.steerAngle = moveX;
              frontPassengerW.steerAngle = moveX;

              float moveZ = Input.GetAxis("Vertical") * motorForce;
              frontPassengerW.motorTorque = moveZ;
              frontDriverW.motorTorque = moveZ;

              int breaking = Input.GetButton("Fire2") ? 1 : 0;
              frontDriverW.brakeTorque = breakForce * breaking;
              frontPassengerW.brakeTorque = breakForce * breaking;

              Color _col = breaking == 1 ? Color.white : Color.clear;
              if (_col != render.materials[1].GetColor("_EmissionColor"))
              {
                  CmdBrakeLights(_col);
              }

              //Update transform and rotation of the wheels (wheel collider is not attached to the mesh transform of the wheels)
              UpdateWheelPoses();


          }*/

        public void UseInput(float movex, float movez, int breaking)
        {
            if (!clientAuthority) return;

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

        [Command]
        public void CmdUseInput(float movex, float movez, int breaking)
        {
            UseInput(movex, movez, breaking);
        }

        [Command]
        public void CmdBrakeLights(Color _col)
        {
            ligthsColor = _col;
        }


        void Update()
        {
            if (!isLocalPlayer)
            {
                //non-local player run this
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }
        }

        public override void OnStopLocalPlayer()
        {
            agent.enabled = false;
            ResetCamera();
            CarsManager.instance.RemoveCar(playerName);
            base.OnStopLocalPlayer();
        }

        public void ResetCamera()
        {
            Camera.main.transform.SetParent(null);
            Camera.main.transform.position = new Vector3(197, 369, 91);
            Camera.main.transform.rotation = Quaternion.Euler(90, 90, 0);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 321;
        }

        public void StopCar()
        {
            /*frontDriverW.brakeTorque = Mathf.Infinity;
            frontPassengerW.brakeTorque = Mathf.Infinity;
            rearDriverW.brakeTorque = Mathf.Infinity;
            rearDriverW.brakeTorque = Mathf.Infinity;
            */
            if (_rigidbody != null)
                _rigidbody.velocity = Vector3.zero;
        }


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

        [Server]
        public void UpdateRealCar(Vector3 vel, float rot)
        {
            _rigidbody.velocity = vel;
            var v_rot = _rigidbody.rotation.eulerAngles;
            //_rigidbody.rotation = Quaternion.Euler(v_rot.x, rot, v_rot.z);
            _rigidbody.rotation = Quaternion.Euler(0, rot, 0);




            //var diff = Vector3.Angle(vel.normalized, transform.rotation.eulerAngles.normalized);
            //float moveX = 0f;
            //
            //if (Mathf.Abs(diff) > 1f)
            //    moveX = 1f;
            //else if (Mathf.Abs(diff) < 1f)
            //    moveX = -1f;
            //
            //
            //moveX = moveX * maxSteerAngle;
            //frontDriverW.steerAngle = moveX;
            //frontPassengerW.steerAngle = moveX;
            //
            //UpdateWheelPoses();
        }

        //private void FixedUpdate()
        //{
        //    var ang = transform.rotation.eulerAngles;
        //    transform.rotation = Quaternion.Euler(0, ang.y, 0);
        //}

    }
}