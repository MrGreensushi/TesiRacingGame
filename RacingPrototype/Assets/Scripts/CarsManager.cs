using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using QuickStart;
using TMPro;
using System.Numerics;
using Unity.VisualScripting;

namespace QuickStart
{
    public class CarsManager : NetworkBehaviour
    {
        public static CarsManager instance;
        public List<CarInfos> cars;
        public Transform carInfoUi;
        public TextMeshProUGUI playerNameText;

        public string PlayerNameUI { set => playerNameText.text = value; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                cars = new List<CarInfos>();
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void AddCar(PlayerScript macchina, UI_Velocity ui, string name, Color _col)
        {
            CarDescriptor toAdd = new CarDescriptor();
            toAdd.Id = name;
            ui.Name = name;
            ui.nameText.color = _col;
            cars.Add(new CarInfos(toAdd, ui, macchina));


        }

        public void RemoveCar(string name)
        {
            var disconnected = cars.Find(x => x.Player.playerName == name);
            cars.Remove(disconnected);
            Destroy(disconnected.Ui.gameObject);

        }


        private void Update()
        {
            if (isServer) UpdateUi();
        }

        [Server]
        void UpdateUi()
        {
            //Destroy Ui of all disconnected cars
            var disconnected = cars.FindAll(x => x.Player == null);
            foreach (var toDelete in disconnected)
            {
                cars.Remove(toDelete);
                Destroy(toDelete.Ui.gameObject);
            }

            List<CarInfos> sorted = cars;
            sorted.Sort();


            foreach (var item in cars)
            {
                //update car descriptor
                item.Car.Velocity = item.Player.Velocity;

                item.Car.DistanceNextGate = UnityEngine.Vector3.Distance(LapsManager.instance.NextGatePosition(item.Car.Gates),
                    item.Player.transform.position);

                item.Car.Rank = cars.Count - sorted.FindIndex(x => x.Car.Id == item.Car.Id);

                item.Ui.Udpate(item.Car);

                //Update UI on every client
                RpcUdateUi(item.Car.Velocity, item.Car.Rank, item.Car.Laps, item.Ui.Time, item.Car.Id);
            }


        }


        [ClientRpc]
        void RpcUdateUi(float velocity, int rank, int laps, string time, string id)
        {
            int index = cars.FindIndex(x => x.Car.Id == id);
            if (index < 0)
            {
                Debug.LogWarning($"Car {id} not present in host");
                return;
            }

            cars[index].Ui.Udpate(velocity, laps, rank, time);
        }
    }
}


public struct CarInfos : IComparable<CarInfos>
{
    public CarInfos(CarDescriptor x, UI_Velocity y, PlayerScript z)
    {
        Car = x;
        Ui = y;
        Player = z;
    }

    public CarDescriptor Car { get; }
    public UI_Velocity Ui { get; }
    public PlayerScript Player { get; }


    //Funziona usata in automatico ogni volta che uso Sort di Carta_Container
    public int CompareTo(CarInfos other)
    {

        return this.Car.CompareTo(other.Car);


    }
    public override string ToString()
    {
        return Player.name + ": " + Car.ToString();
    }

    public Dictionary<string, List<float>> PhisicInfos()
    {
        List<float> infos = new List<float>();
        infos.Add(Player.VectorizedVelocity.x);
        infos.Add(Player.VectorizedVelocity.y);
        infos.Add(Player.VectorizedVelocity.z);

        infos.Add(Player.transform.position.x);
        infos.Add(Player.transform.position.y);
        infos.Add(Player.transform.position.z);

        infos.Add(Player.transform.rotation.x);
        infos.Add(Player.transform.rotation.y);
        infos.Add(Player.transform.rotation.z);
        infos.Add(Player.transform.rotation.w);

        Dictionary<string, List<float>> result = new Dictionary<string, List<float>>();
        result.Add(Player.playerName, infos);
        return result;
    }
    public Dictionary<string, List<float>> RuleInfos()
    {
        List<float> infos = new List<float>();
        infos.Add(Car.Laps);
        infos.Add(Car.Rank);
        Dictionary<string, List<float>> result = new Dictionary<string, List<float>>();
        result.Add(Player.playerName, infos);
        return result;
    }


    public Dictionary<string, List<float>> CommandInfos()
    {
        List<float> infos = new List<float>();
        infos.AddRange(Player.LastAction);
        Dictionary<string, List<float>> result = new Dictionary<string, List<float>>();

        result.Add(Player.playerName, infos);
        return result;
    }



};