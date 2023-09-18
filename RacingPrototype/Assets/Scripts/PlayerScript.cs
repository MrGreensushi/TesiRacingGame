using Google.Protobuf.WellKnownTypes;
using Mirror;
using Mirror.Experimental;
using System.Collections;
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
        private TransformSync net_Transform;
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

        public Rigidbody RigidbodyCar { get { return _rigidbody; } }
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
        public bool bot = true;

        void OnBotChanged(bool _Old, bool _New)
        {
            bot = _New;
            if (bot)
            {
                agent_type.BehaviorType = BehaviorType.InferenceOnly;
                maxSpeed = 40;

            }
            else
            {
                agent_type.BehaviorType = BehaviorType.HeuristicOnly;
                maxSpeed -= 10;
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
            net_Transform.discardClientPosition = !value;
            net_Rigidbody.clientAuthority = value;
            foreach (var t in net_TransformChilds)
                t.clientAuthority = value;
        }


        private void Awake()
        {
            agent_type = GetComponent<BehaviorParameters>();
            bot = true;
        }

        public override void OnStartLocalPlayer()
        {
            //Set up camera
            //Camera.main.transform.SetParent(transform);
            //Camera.main.transform.localPosition = new Vector3(0, 4, -6);
            //Camera.main.transform.localRotation = Quaternion.Euler(16, 0, 0);
            //Camera.main.orthographic = false;
            //Camera.main.fieldOfView = 60;
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 50, 0);
            Camera.main.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Camera.main.orthographicSize = 55;



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
            net_Transform = GetComponent<TransformSync>();
            net_Rigidbody = GetComponent<NetworkRigidbody>();
            net_TransformChilds = GetComponents<NetworkTransformChild>();

            //vedo se il giocatore è un bot o meno
            var beBot = CommandLinesManager.instance.bot != 0;

            //ritrovo le informazioni riguardate la latenza fittizia
            var cl = CommandLinesManager.instance;
            var level = LatencyLevel.None;
            var doNotMPAI = false;

            if (cl != null)
            {
                level = cl.level;
                doNotMPAI = cl.doNotMPAI;
            }

            //send infos to the server
            CmdSetupPlayer(name, color, beBot, doNotMPAI, level);


        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col, bool _bot, bool doNotMPAI, LatencyLevel level)
        {
            ////Set up camera
            //Camera.main.transform.SetParent(transform);
            //Camera.main.transform.localPosition = new Vector3(0, 50, 0);
            //Camera.main.transform.localRotation = Quaternion.Euler(90, 0, 0);
            //Camera.main.orthographicSize = 55;


            //Player info sent to server, then server updates sync vars which handles it on all clients
            playerName = _name;
            playerColor = _col;
            OnColorChanged(Color.black, _col);

            bot = _bot; //cambio sul client
            OnBotChanged(true, _bot); //cambio sul server

            _rigidbody = GetComponent<Rigidbody>();



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
            //var starting = StartingPointsManager.instance.StartingPoint(transform);

            //aggiungi macchina al sistema MPAI
            Manager_MPAI.instance.AddCar(this, doNotMPAI, level);

            //lastInfo seto to zero
            lastInfo = new float[5] { 0, 0, 0, 0, 0 };

            //ritrova le informazioni del network
            net_Identity = GetComponent<NetworkIdentity>();
            net_Transform = GetComponent<TransformSync>();
            net_Rigidbody = GetComponent<NetworkRigidbody>();
            net_TransformChilds = GetComponents<NetworkTransformChild>();

            //colore del giocatore corrisponde anche al colore della scia
            trilRenderer.enabled = true;
            trilRenderer.startColor = _col;
            trilRenderer.endColor = _col;

            clientAuthority = true;
            OnAuthorityChanged(false, true);

            TargetInstantiatePreviousUis(conn, Vector3.zero, names.ToArray(), colors.ToArray());
        }

        [TargetRpc]
        public void TargetInstantiatePreviousUis(NetworkConnection target, Vector3 start, string[] names, Color[] colors)
        {

            //transform.localPosition = start;

            for (int i = 0; i < names.Length - 1; i++)
            {
                GameObject ui = Instantiate(uiPrefab, CarsManager.instance.carInfoUi);
                CarsManager.instance.AddCar(this, ui.GetComponent<UI_Velocity>(), names[i], colors[i]);
            }
            agent.enabled = true;
        }


        [TargetRpc]
        public void TargetTeleportCar(NetworkConnection target, Vector3 pos, Quaternion rot, Vector3 vel)
        {
            _rigidbody.velocity = vel;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.rotation = rot;
            _rigidbody.position = pos;


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

        private void FixedUpdate()
        {



        }
        [Command]
        private void SendPositionInformation()
        {

        }
        public void UseInput(int movex, int movez, int breaking)
        {
            if (!clientAuthority) return;

            //Messages.SendInput(movex, movez, breaking, playerName);

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

        public float[] CompareWithPrediction()
        {
            var (toRet, _) = DispatcherInfos();
            for (int i = 0; i < 3; i++)
            {
                toRet[i] += lastInfo[i + 2];
            }
            return toRet;
        }

        public (float[], float[]) CommPhyDispatcherInfos()
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
            float[] toRet = { delta[0],
                delta[1],
                delta[2],
                tile[0],
                tile[1],
                tile[2],
                tile[3],
                LastAction[0],
                LastAction[1],
                LastAction[2]
            };

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
            //reset wheel rotations
            frontDriverW.steerAngle = 0;
            frontPassengerW.steerAngle = 0;
            UpdateWheelPoses();

            _rigidbody.velocity = vel;
            _rigidbody.rotation = Quaternion.Euler(0, rot, 0);

            //StartCoroutine(InterpolateRotation(rot, 4));

        }


        [Server]


        public IEnumerator InterpolateRotation(float rot, int iterations)
        {

            var diffRot = rot - _rigidbody.rotation.eulerAngles.y;
            diffRot = (diffRot + 180) % 360 - 180;
            float angVel = diffRot / (iterations * 0.02f);

            Vector3 EulerAngelVel = new Vector3(0, angVel, 0);

            if (diffRot < 0f)
                EulerAngelVel *= -1;
            var deltaRot = Quaternion.Euler(EulerAngelVel * Time.fixedDeltaTime);

            int counter = 0;



            while (counter < iterations)
            {

                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRot);
                counter++;
                yield return new WaitForFixedUpdate();
            }
            _rigidbody.rotation = Quaternion.Euler(0, rot, 0);
        }

        //private void FixedUpdate()
        //{
        //    var ang = transform.rotation.eulerAngles;
        //    transform.rotation = Quaternion.Euler(0, ang.y, 0);
        //}

    }
}