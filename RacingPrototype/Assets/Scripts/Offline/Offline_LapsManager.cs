using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offline_LapsManager : MonoBehaviour
{
    public List<Offline_Gate> gates;
    public int maxGates; // id del gate 'start'
    [SerializeField] Offline_CarsManager carsManager;
    [SerializeField] bool initializeIds = false, findGates = false;
    [SerializeField] Transform _parentGates;
    [SerializeField] bool completeRace = false;

    public Offline_CarsManager CarsManager { set { carsManager = value; } get { return carsManager; } }
    public bool InitializeIds { set { initializeIds = value; } get { return initializeIds; } }
    public bool SetFindGates { set { findGates = value; } get { return findGates; } }
    public Transform ParentGates { set { _parentGates = value; } get { return _parentGates; } }

    private void Awake()
    {
        Inizialize();
    }
    private void Inizialize()
    {
        if (gates == null || gates.Count == 0)
            FindGates();

        gates.RemoveAll(x => x.gameObject.activeInHierarchy == false);
        maxGates = gates.Count;
        if (!initializeIds) return;
        for (int i = 0; i < gates.Count; i++)
        {
            gates[i].id = i;
        }
    }

    private void FindGates()
    {
        if (findGates)
        {
            gates = new List<Offline_Gate>();
            for (int i = 0; i < _parentGates.childCount; i++)
            {
                var child = _parentGates.GetChild(i);

                for (int j = 0; j < child.childCount; j++)
                {
                    var gate = child.GetChild(j);
                    if (gate.TryGetComponent(out Offline_Gate offline_Gate))
                    {
                        gates.Add(offline_Gate);
                        offline_Gate.LapsManager = this;

                    }
                }
            }

        }
    }
    public void CarPassedThrough(int id_gate, string id_car)
    {
        int passed = carsManager.cars.FindIndex(x => x.Car.Id.Equals(id_car));

        if (passed < 0)
        {
            Debug.LogWarning($"Car {id_car} is not in the server cars list");
            return;
        }
        CarDescriptor c = carsManager.cars[passed].Car;


        //Se sto allenando le macchine a vincere la gara allora quando il primo giocatore passa in un gate premia le macchine in base alla loro
        //posizione



        //Gate indica il prossimo Gate da passare, quindi controllo se il gate in cui è passato è lo stesso di quello che doveva passare
        if (c.Gates == id_gate)
        {
            c.Gates = id_gate + 1;

            //Premia l'agente per aver passato un checkpoint
            carsManager.cars[passed].Player.agent.checkpoint++;

            return;
        }
        //se il valore di gate è maggiore del numero di gate in campo vuol dire che è passato in tutti i gate
        //se è passato per tutti i gates e ora sta passando al gate iniziale ha fatto un giro
        if (c.Gates == maxGates && id_gate == 0)
        {

            c.Gates = 1;
            c.Laps++;

            //reset timer
            // Offline_CarsManager.instance.cars[passed].Ui.offsetTime = NetworkTime.time;

            //Premia l'agente per aver concluso un giro
            carsManager.cars[passed].Player.agent.lap = true;

            //Debug.Log($"Player {id_car} has completed a lap");
            return;
        }
        Debug.LogWarning($"Player {id_car} has passed through wrong gate{id_gate}, gate da passare: {c.Gates}");
        carsManager.cars[passed].Player.agent.wrongCheck++;

    }



    public Vector3 NextGatePosition(int id)
    {
        if (maxGates == 0)
            Inizialize();
        int id_gate = (id + maxGates) % maxGates;
        return gates[id_gate].transform.position;
    }

    public Vector3 NextGateForward(string id_car)
    {
        if (maxGates == 0)
            Inizialize();


        return gates[GetGateIdPlayer(id_car)].transform.forward;
    }

    public Vector3 NextNextGateForward(string id_car)
    {
        if (maxGates == 0)
            Inizialize();

        int id = (GetGateIdPlayer(id_car) + 1) % maxGates;
        return gates[id].transform.forward;
    }

    private int GetGateIdPlayer(string id_car)
    {
        int passed = carsManager.cars.FindIndex(x => x.Car.Id.Equals(id_car));

        if (passed < 0)
        {
            Debug.LogError($"Car {id_car} is not in the server cars list");
            //2 possibilità o c'è stato un errore oppure il codice per aggiungere la macchina nel carmanager non è stato ancora eseguito
            return 0;
        }
        CarDescriptor c = carsManager.cars[passed].Car;
        return ((c.Gates + maxGates) % maxGates);
    }

    public float DistanceBetweemTwoGates(int id1, int id2)
    {
        var pos1 = NextGatePosition(id1);
        var pos2 = NextGatePosition(id2);

        return Mathf.Abs(Vector3.Distance(pos1, pos2));
    }

    public float GetPercentageDistance(string playerName, Vector3 posPlayer)
    {
        if (maxGates == 0)
            Inizialize();

        //gate = prossimo gate
        int gate = GetGateIdPlayer(playerName);
        //previous= checkpoint precedente a gate
        int previous = (gate - 1 + maxGates) % maxGates;

        //distanza massima per arrivare al checkpoint da passare
        var distance = DistanceBetweemTwoGates(gate, previous);

        //distanza attuale tra giocatore e gate
        float actual = Mathf.Abs(Vector3.Distance(posPlayer, gates[gate].transform.position));

        return (1.0f - actual / distance);



    }


}

