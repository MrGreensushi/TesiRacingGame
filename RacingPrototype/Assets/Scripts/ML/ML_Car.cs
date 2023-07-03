using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using QuickStart;
using Unity.MLAgents.Sensors;
using Unity.Barracuda;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Mirror;

public class ML_Car : Agent
{
    [SerializeField] PlayerScript _playerCar;
    [HideInInspector] public bool lap = false;
    [HideInInspector] public int checkpoint, wrongCheck;
    private bool startCollision = false, stillCollision = false;
    private bool carCollision = false, stillCarCollision = false;
    float startTime, additionalRew = 0;
    float collisionDuration = 0f;
    int lapsDone = 0, lapsEpisode = 2;

    public bool accelerationRew = false, directionRew = true, breakingRew = false;
    float directionSensor;
    [SerializeField] float checkpointReward, lapReward, directionReward, wrongCheckReward, wallCollisionReward, carCollisionReward;
    [SerializeField] bool completeRace = false;
    [HideInInspector] public int changedRank = 0;

    public int[] lastAction = new int[3];
    private async void Start()
    {
        var dr = gameObject.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 1;
        int passed;
        do
        {
            passed = CarsManager.instance.cars.FindIndex(x => x.Car.Id.Equals(_playerCar.playerName));
            await Task.Yield();

        } while (passed < 0);
    }
    public override void OnEpisodeBegin()
    {

        _playerCar.StopCar();
        checkpoint = 0;
        wrongCheck = 0;
        lap = false;
        transform.rotation = Quaternion.identity;
        startTime = Time.time;
        lapsDone = 0;

        Debug.LogWarning($"Additional rew: {additionalRew}");
        additionalRew = 0;
        //transform.localPosition = StartingPointsManager.instance.StartingPoint(transform);

    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveX = GetDiscrete(actions.DiscreteActions[0]); //Girare a sinisatra o destra
        int moveZ = GetDiscrete(actions.DiscreteActions[1]); //Accellerare o retromarcia
        int breaking = actions.DiscreteActions[2]; //frenare o meno
        lastAction = new int[3] { moveX, moveZ, breaking };
        _playerCar.UseInput(moveX, moveZ, breaking);
        Rewards(moveZ, breaking);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        directionSensor = LIDirectionGates();
        sensor.AddObservation(directionSensor);

    }

    private float LIDirectionGates()
    {
        //Direzione rispetto gate successivo
        Vector3 checkpointForward = LapsManager.instance.NextGateForward(_playerCar.playerName);
        //Direzione rispetto due gate successivi
        Vector3 nextcheckpointForward = LapsManager.instance.NextNextGateForward(_playerCar.playerName);
        //t = 1 - distanza in percentuale rispetto al prossimo gate
        float t = LapsManager.instance.GetPercentageDistance(_playerCar.playerName, transform.position);
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
                CarsManager.instance.EndEpisodeForAll();
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


    private void Rewards(int moveZ, int breaking)
    {
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
                CarsManager.instance.EndEpisodeForAll();
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
            float newDirectioSensor = LIDirectionGates();
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
                    CarsManager.instance.EndEpisodeForAll();
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

            lapsDone++;
            if (lapsDone >= lapsEpisode)
                EndEpisode();
        }




    }
}
