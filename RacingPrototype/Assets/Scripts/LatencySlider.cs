using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LatencySlider : MonoBehaviour
{
    [SerializeField] LatencySimulation simulation;
    [SerializeField] TextMeshProUGUI text;


    public void OnChange(float value)
    {
        simulation.unreliableLatency = value;
        text.text = value.ToString();
    }
}
