using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour
{
    List<Dictionary<string, List<float>>> physics = new List<Dictionary<string, List<float>>>();
    List<Dictionary<string, List<float>>> rules = new List<Dictionary<string, List<float>>>();
    //List<Dictionary<string,List<float>>> commands = new List<Dictionary<string,List<float>>>();

    public List<Dictionary<string, List<float>>> PhysicInput { get => physics; }
    public List<Dictionary<string, List<float>>> RuleInput { get => rules; }
    void UpdateData()
    {
        physics.Clear();
        rules.Clear();

        //retrieve information for each car
        foreach (var player in CarsManager.instance.cars)
        {
            physics.Add(player.PhisicInfos());
            rules.Add(player.RuleInfos());

        }

    }
}
