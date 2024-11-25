using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Mirror;
using QuickStart;
using UnityEngine;
using Random = UnityEngine.Random;

public class RTTClientLogger : MonoBehaviour
{
    private string clientName = "";
    private string logs = "TIME,RTT,RTTVariance\n";

    private void Start()
    {
        var id = GetComponent<NetworkIdentity>();
        if (!id.isServer)
        {
            return;
        }
        
        Debug.LogError("Destroying RTT Logger");
        Destroy(this);
    }

    private void Update()
    {
        logs += $"{Time.time.ToString(CultureInfo.InvariantCulture)},{NetworkTime.rtt.ToString(CultureInfo.InvariantCulture)},{NetworkTime.rttVariance.ToString(CultureInfo.InvariantCulture)}\n";
    }

    private void OnDestroy()
    {
        var path = CommandLinesManager.instance.rttClientPath;
        if(string.IsNullOrEmpty(path))
            return;
        path += $"\\{DateTime.Now:yy_MM_dd_hh_mm}";
        Directory.CreateDirectory(path);

        clientName = Random.Range(0, 10000).ToString();
        path += $"\\{clientName}.txt";
        Debug.LogError("RTT Path: "+path);
        File.WriteAllText(path,logs);
    }
}
 