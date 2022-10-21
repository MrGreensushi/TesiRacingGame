using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using QuickStart;
using TMPro;

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

        private void Update()
        {
            if (isServer) UpdateUi();
        }

        [Server]
        void UpdateUi()
        {
            List<CarInfos> sorted = cars;
            sorted.Sort();
            foreach (var item in cars)
            {
                //update car descriptor
                item.Car.Velocity = item.Player.Velocity;

                item.Car.DistanceNextGate = Vector3.Distance(LapsManager.instance.NextGatePosition(item.Car.Gates),
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
};