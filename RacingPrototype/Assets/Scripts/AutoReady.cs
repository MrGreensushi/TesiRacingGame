using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AutoReady : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        var lm = CommandLinesManager.instance;

        if (lm.bot > 0)
        {
            DelayedReady();
        }
    }


    async void DelayedReady()
    {
        await Task.Delay(5000);
        var player = GetComponent<NetworkRoomPlayer>();
        player.CmdChangeReadyState(true);
    }
}
