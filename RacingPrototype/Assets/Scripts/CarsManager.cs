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
        [SerializeField] GameObject predictingUI;
        bool predictUI = false;

        public void UIPrediction(NetworkConnection target, bool value)
        {
            //if (predictUI!=value) 
            TargetActivePrediction(target, value);
            //predictUI = value;
        }

        [TargetRpc]
        public void TargetActivePrediction(NetworkConnection target, bool value)
        {
            predictingUI.SetActive(value);
        }

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

        public void EndEpisodeForAll()
        {
            //stop the writing to file
            var writer = GetComponent<RecordWriter>();
            if (writer != null)
            {
                writer.newRace = true;
            }

            List<ML_Car> agents = new List<ML_Car>();
            foreach (var item in cars)
            {
                agents.Add(item.Player.agent);
                //reward all cars based on their rank
                RewardsBasedOnPosition(item);
            }
            foreach (var item in agents)
                item.EndEpisode();

        }

        public void RewardsBasedOnPosition(CarInfos item)
        {

            float posReward = cars.Count - 1 - item.Car.Rank;
            item.Player.agent.AddReward(posReward);


        }


        public bool CheckCarIsPresent(string name)
        {
            var pos=cars.FindIndex(element => element.Car.Id.Equals(name));

            return pos >= 0;
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


};