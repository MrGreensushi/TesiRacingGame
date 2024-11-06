using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Barracuda;
using UnityEngine;

public class CommandLinesManager : MonoBehaviour
{
    public static CommandLinesManager instance;
    public int bot = 0;
    public LatencyLevel level;
    public bool doNotMPAI;
    public string filePath,filePathPredictionsTime;
    public string playerName,path,fileName;
    public WorkerFactory.Type workerType = WorkerFactory.Type.CSharpBurst;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        bot = 0;
        level = LatencyLevel.None;
        doNotMPAI = false;

        string[] args = System.Environment.GetCommandLineArgs();
        int widthInput = -1;
        int heightInput = -1;
        bool startClient = false;
        bool startServer = false;
        int x = -1, y = -1;
        bool fullscreen = false;
        playerName = "";
         var durationParse = false;
        var frequencyParse = false;

        int mpaiDuration=0;
        int mpaiFrequency=0;
        
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
            else if (args[i] == "-full")
            {
                fullscreen = true;
            }
            else if (args[i] == "-name")
            {
                playerName= args[i + 1];
            }
            else if (args[i] == "-evalName")
            {
                fileName = args[i + 1];
            }
            else if (args[i] == "-evalPath")
            {
                path = args[i + 1];
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
            else if (args[i] == "-l1")
            {
                level = LatencyLevel.L1;
            }
            else if (args[i] == "-l2")
            {
                level = LatencyLevel.L2;
            }
            else if (args[i] == "-l3")
            {
                level = LatencyLevel.L3;
            }
            else if (args[i] == "-dont")
            {
                doNotMPAI = true;
            }
            else if ( args[i].StartsWith("-path="))
            {
                filePath= args[i][6..];
            }
            else if (args[i]=="-duration")
            {
                durationParse=int.TryParse(args[i + 1], out mpaiDuration);
            }
            else if (args[i]=="-frequency")
            {
                frequencyParse=int.TryParse(args[i + 1], out mpaiFrequency);
            } else if ( args[i].StartsWith("-pathPredictions="))
            {
                filePathPredictionsTime= args[i]["-pathPredictions=".Length..];
            }
            else if (args[i].StartsWith("-workerType="))
            {
                var type=args[i]["-workerType=".Length..];
                workerType = type switch
                {
                    "Compute" => WorkerFactory.Type.Compute,
                    "ComputePrecompiled" => WorkerFactory.Type.ComputePrecompiled,
                    "ComputeRef" => WorkerFactory.Type.ComputeRef,
                    "CSharp" => WorkerFactory.Type.CSharp,
                    "CSharpRef" => WorkerFactory.Type.CSharpRef,
                    "CSharpBurst" => WorkerFactory.Type.CSharpBurst,
                    _ => workerType
                };
            }

        } 

    if (durationParse)
    {
        LatencyLevels.L1Duration = mpaiDuration;
    }
        
    if (frequencyParse)
    {
        LatencyLevels.L1Frequency = mpaiFrequency;
    }

        var manager = GetComponent<NetworkRoomManager>();

        if (manager != null)
        {

            if (startClient)
            {
                manager.StartClient();
            }
            else if (startServer)
                manager.StartServer();
        }

        if (heightInput != -1 && widthInput != -1)
            Screen.SetResolution(widthInput, heightInput, fullscreen);


        var disp = Screen.mainWindowDisplayInfo;
        if (x != -1 && y != -1)
            Screen.MoveMainWindowTo(disp, new Vector2Int(x, y));
    }


}
