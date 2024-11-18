using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace QuickStart
{
    public class CarDescriptor : IComparable<CarDescriptor>
    {
        string _id;
        float _velocity;
        int _laps;
        int _gates;
        float _distanceNextGate;
        int _rank;

        public string Id { get => _id; set => _id = value; }
        public float Velocity { get => _velocity; set => _velocity = value; }
        public int Laps { get => _laps; set => _laps = value; }
        public int Gates { get => _gates; set => _gates = value; }
        public float DistanceNextGate { get => _distanceNextGate; set => _distanceNextGate = value; }
        public int Rank { get => _rank; set => _rank = value; }

        public CarDescriptor()
        {
            _id = "";
            _velocity = 0;
            _laps = 0;
            _gates = 0;
            _distanceNextGate = 0;
        }

        public CarDescriptor(string id, float velocity, int laps, int gates, float distanceNextGate)
        {
            _id = id;
            _velocity = velocity;
            _laps = laps;
            _gates = gates;
            _distanceNextGate = distanceNextGate;
        }

        public int CompareTo(CarDescriptor other)
        {
            if (this.Id.Equals(other.Id)) return 0;
            // compariamo il numero di giri
            if (this.Laps.CompareTo(other.Laps) != 0)
                return this.Laps.CompareTo(other.Laps);
            //se lo stesso controlliamo quanti gate hanno passato
            if (this.Gates.CompareTo(other.Gates) != 0)
                return this.Gates.CompareTo(other.Gates);
            //Se anche i gate sono uguali controlliamo la distanza dal next gate 
            //meno perch� minore � la distanza meglio �
            return -this.DistanceNextGate.CompareTo(other.DistanceNextGate);


        }

        public override string ToString()
        {
            return Id + ": " + Velocity + ", " + Laps + ", " + Rank;
        }
    }
}

