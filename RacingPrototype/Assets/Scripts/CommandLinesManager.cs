using System;
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
    public string filePath,filePathPredictionsTime,processName,rttClientPath;
    public string playerName,path,fileName,pythonDirectory,pythonScriptPath;
    public WorkerFactory.Type workerType = WorkerFactory.Type.CSharpBurst;
    public int percentageSPGPlayers=-1, percentageActiveSPG=-1;
    public int experimentDuration=120;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        
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
        var networkAddress = "";
        
        for (int i = 0; i < args.Length; i++)
        {
            // Debug.LogError($"ARG {i}: {args[i]}");
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
            }
            else if (args[i].StartsWith("-networkAddress="))
            {
                networkAddress=args[i]["-networkAddress=".Length..];
            }
            else if ( args[i].StartsWith("-logOutput="))
            {
                filePathPredictionsTime= args[i]["-logOutput=".Length..];
            }
            else if ( args[i].StartsWith("-logRTTOutput="))
            {
                rttClientPath= args[i]["-logRTTOutput=".Length..];
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
            else if ( args[i].StartsWith("-pythonDirectory="))
            {
                pythonDirectory= args[i]["-pythonDirectory=".Length..];
            }
            else if ( args[i].StartsWith("-pythonScriptPath="))
            {
                pythonScriptPath= args[i]["-pythonScriptPath=".Length..];
            }
            else if ( args[i].StartsWith("-processName="))
            {
                processName= args[i]["-processName=".Length..];
            }
            else if ( args[i].StartsWith("-percentageSPGPlayers"))
            {
                int.TryParse(args[i + 1], out percentageSPGPlayers);
            }
            else if ( args[i].StartsWith("-percentageActiveSPG"))
            {
                int.TryParse(args[i + 1], out percentageActiveSPG);
            }
            else if ( args[i].StartsWith("-experimentDuration"))
            {
                int.TryParse(args[i + 1], out experimentDuration);
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
            if (!string.IsNullOrEmpty(networkAddress))
                manager.networkAddress = networkAddress;
            
            if (startClient)
                manager.StartClient();
            
            else if (startServer)
                manager.StartServer();
        }

        if (heightInput != -1 && widthInput != -1)
            Screen.SetResolution(widthInput, heightInput, fullscreen);


        var disp = Screen.mainWindowDisplayInfo;
        if (x != -1 && y != -1)
            Screen.MoveMainWindowTo(disp, new Vector2Int(x, y));
    }


    private void Start()
    {
        StartCoroutine("QuitApp");
    }

    IEnumerator QuitApp()
    {
        yield return new WaitForSecondsRealtime(experimentDuration);
        Application.Quit();
    }

}
