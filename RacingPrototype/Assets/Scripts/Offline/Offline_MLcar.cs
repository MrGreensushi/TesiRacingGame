using QuickStart;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class Offline_MLcar : Agent
{
    [SerializeField] OfflineCar _offlineCar;
    [HideInInspector] public bool lap = false;
    [HideInInspector] public int checkpoint, wrongCheck;
    private bool startCollision = false, stillCollision = false;
    private bool carCollision = false, stillCarCollision = false;
    float startTime, additionalRew = 0;
    [SerializeField] Offline_StartingPoint_Manager spManager;
    Offline_LapsManager lapsManager;
    [HideInInspector] public Offline_CarsManager carsManager;
    float collisionDuration = 0f;
    int lapsDone = 0, lapsEpisode = 10;
    //Curriculum learning
    EnvironmentParameters m_ResetParams;
    float config_number;
    public bool accelerationRew = false, directionRew = true, breakingRew = false;
    float directionSensor;
    [SerializeField] float checkpointReward, lapReward, directionReward, wrongCheckReward, wallCollisionReward, carCollisionReward;
    [SerializeField] bool completeRace = false;
    [HideInInspector] public int changedRank = 0;

    public int[] lastAction = new int[3];
    public override void Initialize()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;

    }
    public override void OnEpisodeBegin()
    {

        _offlineCar.StopCar();
        ConfigCar();
        checkpoint = 0;
        wrongCheck = 0;
        lap = false;
        transform.rotation = Quaternion.identity;
        startTime = Time.time;
        lapsDone = 0;

        //Debug.LogWarning($"Additional rew: {additionalRew}");
        additionalRew = 0;
    }

    public void ConfigCar()
    {
        config_number = m_ResetParams.GetWithDefault("config_num", 0);



        if (carsManager != null)
            carsManager.RemoveCar(_offlineCar.playerName);

        transform.localPosition = spManager.StartingPoint(config_number, transform);
        lapsManager = transform.parent.parent.GetComponentInChildren<Offline_LapsManager>();
        carsManager = transform.parent.parent.GetComponentInChildren<Offline_CarsManager>();
        carsManager.AddCar(_offlineCar, _offlineCar.playerName);

        /*
        switch (config_number)
        {
            case 0.0f:
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Car"));
                break;
            case 1.0f:
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Car"), false);
                directionRew = true;
                break;
            case 2.0f:
                accelerationRew = true;
                break;
            case 3.0f:
                breakingRew = true;
                break;
            default:
                break;
        }
        */

    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveX = GetDiscrete(actions.DiscreteActions[0]); //Girare a sinisatra o destra
        int moveZ = GetDiscrete(actions.DiscreteActions[1]); //Accellerare o retromarcia
        int breaking = actions.DiscreteActions[2]; //frenare o meno
        lastAction = new int[3] { moveX, moveZ, breaking };
        _offlineCar.UseInput(moveX, moveZ, breaking);

        Rewards(moveZ, breaking, moveX);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        directionSensor = AverageDirectionGates();
        sensor.AddObservation(directionSensor);
    }


    private float AverageDirectionGates()
    {
        //Direzione rispetto gate successivo
        Vector3 checkpointForward = lapsManager.NextGateForward(_offlineCar.playerName);
        //Direzione rispetto due gate successivi
        Vector3 nextcheckpointForward = lapsManager.NextNextGateForward(_offlineCar.playerName);
        //t = 1 - distanza in percentuale rispetto al prossimo gate
        float t = lapsManager.GetPercentageDistance(_offlineCar.playerName, transform.position);
        if (t < 0) t = 0;

        Vector3 lerpForward = Vector3.Lerp(checkpointForward, nextcheckpointForward, t).normalized;
        float directionDot = Vector3.Dot(transform.forward, lerpForward);


        return directionDot;
    }

    private int GetDiscrete(int i)
    {
        if (i == 2)
            return -1;
        return i;
    }

    private int GetAxis(string axis)
    {
        var x = Input.GetAxis(axis);
        if (x > 0)
            return 1;
        else if (x == 0)
            return 0;
        return -1;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreateActions = actionsOut.DiscreteActions;

        discreateActions[0] = GetAxis("Horizontal");
        discreateActions[1] = GetAxis("Vertical");
        discreateActions[2] = Input.GetButton("Fire2") ? 1 : 0;
    }

    //Se sbatto conto un muro penalizzo l'agent
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Wall wall))
        {
            //ho colpito un muro
            startCollision = true;
            stillCollision = true;
            collisionDuration = Time.time;
        }
        if (collision.gameObject.TryGetComponent(out OfflineCar car))
        {
            //ho colpito un muro
            carCollision = true;
            stillCarCollision = true;
            collisionDuration = Time.time;
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        if (completeRace && collision.gameObject.TryGetComponent(out Wall wall))
        {
            //Se la collisione dura da più di 2 secondi allora fine episodio
            if ((Time.time - collisionDuration) >= 5f)
            {
                carsManager.EndEpisodeForAll();
                return;
            }

        }

        if (collision.gameObject.TryGetComponent(out Wall wall1) ||
        collision.gameObject.TryGetComponent(out OfflineCar car))
        {
            //Se la collisione dura da più di 2 secondi allora fine episodio
            if ((Time.time - collisionDuration) >= 1f)
            {
                EndEpisode();
            }

        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Wall wall))
        {
            //collisione contro il muro finita
            stillCollision = false;
        }
        if (collision.gameObject.TryGetComponent(out OfflineCar car))
        {
            //ho colpito un muro
            stillCarCollision = false;
        }
    }


    private void Rewards(int moveZ, int breaking, int moveX)
    {
        //to encorurage the agent to finisch the race quickly
        AddReward(-0.001f);

        if (checkpoint > 0)
        {
            AddReward(checkpoint * checkpointReward);
            Debug.Log($"Reward {checkpoint * checkpointReward} for checkpoint");
            checkpoint = 0;



        }


        if (wrongCheck > 0)
        {
            AddReward(-wrongCheckReward * wrongCheck);
            Debug.Log($"Reward {-wrongCheckReward * wrongCheck}for wrong checkpoint");
            wrongCheck = 0;

            if (!completeRace)
                EndEpisode();
            else
                carsManager.EndEpisodeForAll();

        }

        //Se rimango contro un muro penalizzo l'agent
        if (stillCollision)
        {
            AddReward(-0.005f);
            Debug.Log("Reward -0.005 for Staying on a wall");
        }

        //Se sbatto conto un muro penalizzo l'agent
        if (startCollision)
        {
            AddReward(-wallCollisionReward);
            startCollision = false;
            Debug.Log($"Reward {-wallCollisionReward} for starting a collision");
        }

        //Se rimango contro un muro penalizzo l'agent
        if (stillCarCollision)
        {
            AddReward(-0.005f);
            Debug.Log("Reward -0.005 for Staying on a car");
        }

        //Se sbatto conto un muro penalizzo l'agent
        if (carCollision)
        {
            AddReward(-carCollisionReward);
            carCollision = false;
            Debug.Log($"Reward {-carCollisionReward} for starting a collision with a car");
        }



        //Premio l'agente in base alla direzione della macchina rispetto al prossimo checkpoint
        if (directionRew && moveZ == 1)
        {
            //lerp tra i due prossimi checkpoint
            float newDirectioSensor = AverageDirectionGates();
            AddReward(directionReward * newDirectioSensor);

        }

        //Premio l'agente in base a quanto preme l'acceleratore
        if (accelerationRew)
            if (moveZ == 1 && breaking == 0)
            {
                AddReward(0.005f);
                additionalRew += 0.005f;
            }

        //penalizzo l'agente se frena 
        if (breakingRew)
        {
            AddReward(-breaking * 0.005f);
            additionalRew -= breaking * 0.005f;
        }


        if (completeRace)
        {
            //reward if the car surpass another one
            AddReward(changedRank);
            changedRank = 0;
            if (lap)
            {

                lap = false;
                lapsDone++;
                //EndEpisodForAll also reward each car based on thier rank
                if (lapsDone >= lapsEpisode)
                    carsManager.EndEpisodeForAll();
                else
                    AddReward(lapReward);
            }
            return;

        }

        if (lap)
        {
            lap = false;
            AddReward(lapReward);
            Debug.Log($"Reward {lapReward} for completing a lap");

            //reward in base al lapTime
            float totTime = Time.time - startTime;
            lapsDone++;
            if (lapsDone >= lapsEpisode)
                EndEpisode();
        }


    }
}
