using Mirror;
using QuickStart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Offline_CarsManager : MonoBehaviour
{
    public List<Offline_CarInfos> cars;
    [SerializeField] Offline_LapsManager lapsManager;
    //public Transform carInfoUi;
    //public TextMeshProUGUI playerNameText;

    //public string PlayerNameUI { set => playerNameText.text = value; }
    public Offline_LapsManager LapsManager { get { return lapsManager; } set { lapsManager = value; } }

    private void Awake()
    {
        cars = new List<Offline_CarInfos>();
    }

    public void AddCar(OfflineCar macchina, string name)
    {
        CarDescriptor toAdd = new CarDescriptor();
        toAdd.Id = name;
        cars.Add(new Offline_CarInfos(toAdd, macchina));
    }

    public void RemoveCar(string name)
    {
        var disconnected = cars.Find(x => x.Player.playerName == name);
        cars.Remove(disconnected);

    }
    private void Update()
    {
        UpdateUi();
    }

    void UpdateUi()
    {
        //Destroy Ui of all disconnected cars
        var disconnected = cars.FindAll(x => x.Player == null);
        foreach (var toDelete in disconnected)
        {
            cars.Remove(toDelete);
        }

        List<Offline_CarInfos> sorted = cars;
        sorted.Sort();


        foreach (var item in cars)
        {
            //update car descriptor
            item.Car.Velocity = item.Player.Velocity;

            item.Car.DistanceNextGate = Vector3.Distance(lapsManager.NextGatePosition(item.Car.Gates),
                item.Player.transform.localPosition);


            var previousRank = item.Car.Rank;
            item.Car.Rank = cars.Count - sorted.FindIndex(x => x.Car.Id == item.Car.Id);

            item.Player.agent.changedRank = previousRank - item.Car.Rank;
        }



    }

    public void EndEpisodeForAll()
    {
        //stop the writing to file
        var writer = GetComponent<RecordWriter>();
        if (writer != null)
        {
            writer.newRace = true;
        }

        List<Offline_MLcar> agents = new List<Offline_MLcar>();
        foreach (var item in cars)
        {
            agents.Add(item.Player.agent);
            //reward all cars based on their rank
            RewardsBasedOnPosition(item);
        }
        foreach (var item in agents)
            item.EndEpisode();

    }

    public void RewardsBasedOnPosition(Offline_CarInfos item)
    {

        float posReward = cars.Count - 1 - item.Car.Rank;
        item.Player.agent.AddReward(posReward);


    }

}


public struct Offline_CarInfos : IComparable<Offline_CarInfos>
{
    public Offline_CarInfos(CarDescriptor x, OfflineCar z)
    {
        Car = x;
        Player = z;
    }

    public CarDescriptor Car { get; }
    public OfflineCar Player { get; }


    //Funziona usata in automatico ogni volta che uso Sort di Carta_Container
    public int CompareTo(Offline_CarInfos other)
    {

        return this.Car.CompareTo(other.Car);


    }
    public override string ToString()
    {
        return Player.name + ": " + Car.ToString();
    }

    public string[] AllInfos()
    {
        string time = ";" + Time.time.ToString();
        // Posizione assoluta, Velocità, rotation, angular rotaion, acc, Tile, Tile number, tile relative position, time


        string physic = Player.playerName + ";" + Player.Position + ";" + Player.VectorizedVelocity + ";" + Player.Rotation + ";" +
            Player.AngularVelString + ";" + Player.Acceleration + ";" + Player.ReturnInfoTile() + time;
        //string physic = Player.playerName + ";" + Player.VectorizedVelocity + ';' + Player.Acceleration;
        string rules = Player.playerName + ";" + Car.Laps + ";" + Car.Rank + time;
        int[] action = Player.LastAction;
        string commands = action[0] + ";" + action[1] + ";" + action[2];
        return new string[] { physic, rules, commands };
    }
    public float[] PhysicInfos()
    {
        return Player.PhysicInfos;

    }
};
