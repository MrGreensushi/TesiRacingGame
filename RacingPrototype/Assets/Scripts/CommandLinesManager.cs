using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLinesManager : MonoBehaviour
{
    public static CommandLinesManager instance;
    public int bot = 0;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        bot = 0;

        string[] args = System.Environment.GetCommandLineArgs();
        int widthInput = -1;
        int heightInput = -1;
        bool startClient = false;
        bool startServer = false;
        int x = -1, y = -1;
        for (int i = 0; i < args.Length; i++)
        {
            //Debug.Log("ARG " + i + ": " + args[i]);
            if (args[i] == "-screen-width")
            {
                int.TryParse(args[i + 1], out widthInput);
            }
            else if (args[i] == "-screen-height")
            {
                int.TryParse(args[i + 1], out heightInput);
            }
            else if (args[i] == "-client")
            {
                startClient = true;
            }
            else if (args[i] == "-server")
            {
                startServer = true;
            }
            else if (args[i] == "-x")
            {
                int.TryParse(args[i + 1], out x);
            }
            else if (args[i] == "-y")
            {
                int.TryParse(args[i + 1], out y);
            }
            else if (args[i] == "-bot")
            {
                bot++;
            }
        }


        var manager = GetComponent<NetworkRoomManager>();

        if (startClient)
        {
            manager.StartClient();
        }
        else if (startServer)
            manager.StartServer();

        if (heightInput != -1 && widthInput != -1)
            Screen.SetResolution(widthInput, heightInput, false);


        var disp = Screen.mainWindowDisplayInfo;
        if (x != -1 && y != -1)
            Screen.MoveMainWindowTo(disp, new Vector2Int(x, y));

    }


}
