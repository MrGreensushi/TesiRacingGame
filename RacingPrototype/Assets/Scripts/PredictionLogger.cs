using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class PredictionLogger : MonoBehaviour
{
    public static PredictionLogger Instance;

    public float InitialTime { get; private set; }
    private string folderInfoPath = "";
    private string scenarioInfoPath=""; 
    private string predictionInfoPath=""; 
    private string systemInfoPath=""; 
    private string fpsInfoPath=""; 
    private string logs = "Player ID, Unscaled Start Prediction, Unscaled End Prediction\n";
    private SC_FPSCounter fps;
    private int counter = 0;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
        }

        InitialTime = Time.time;
        fps = FindObjectOfType<SC_FPSCounter>();
        Assert.IsNotNull(fps);
        
        folderInfoPath = CommandLinesManager.instance.filePathPredictionsTime;
        Assert.IsFalse(string.IsNullOrEmpty(folderInfoPath));

        folderInfoPath +=
            $"\\{DateTime.Now:yyyy-M-d_hh_mm_ss}_{CommandLinesManager.instance.workerType}_{NetworkManager.singleton.numPlayers}";
        var dirInfo=Directory.CreateDirectory(folderInfoPath);
        Assert.IsNotNull(dirInfo);
        
        predictionInfoPath = $"{folderInfoPath}\\predictionPerformance.txt";
        Assert.IsFalse(string.IsNullOrEmpty(predictionInfoPath));
        
        fpsInfoPath=$"{folderInfoPath}\\fpsInfo.txt";
        Assert.IsFalse(string.IsNullOrEmpty(fpsInfoPath));
        
        scenarioInfoPath = $"{folderInfoPath}\\scenarioInfo.txt";
        Assert.IsFalse(string.IsNullOrEmpty(predictionInfoPath));
        WriteScenarioInfo();
        
        systemInfoPath = $"{folderInfoPath}\\systemPerformance.csv";
        Assert.IsFalse(string.IsNullOrEmpty(predictionInfoPath));
        File.Create(systemInfoPath);
        StartPythonScript();

    }

    public void AddPredictionLog(string playerId, string startTimePrediction, string endTimePrediction)
    {
        // Creiamo una stringa con i valori separati da ','
        var contenuto = $"{playerId},{startTimePrediction},{endTimePrediction}\n";
        logs += contenuto;
    }
    
    private void WriteScenarioInfo()
    {
        var lat = new Latency(LatencyLevel.L1);
        var infos =
            $"Duration:{lat.Duration}\nFrequency:{LatencyLevels.L1Frequency}\nWorkerType:{CommandLinesManager.instance.workerType}\nPercentage SPG Players:{CommandLinesManager.instance.percentageSPGPlayers}\nPercentage Active SPG Players:{CommandLinesManager.instance.percentageActiveSPG}";
        File.WriteAllText(scenarioInfoPath, infos);
    }
    private void OnDestroy()
    {
        try
        {
            // Scriviamo la stringa sul file specificato
            File.WriteAllText(predictionInfoPath, logs);
            File.WriteAllText(fpsInfoPath, fps.fpsData);
            Debug.Log("Dati scritti con successo.");
        }
        catch (Exception ex)
        {
            // Gestione errori in caso di problemi con la scrittura del file
            Debug.LogError($"Errore durante la scrittura del file: {ex.Message}");
        }
    }

    private void StartPythonScript()
    {
        var runner=FindObjectOfType<RunPythonAnalysisScript>();
        Assert.IsNotNull(runner);
        runner.RunScript(CommandLinesManager.instance.pythonDirectory, CommandLinesManager.instance.pythonScriptPath,
            systemInfoPath, CommandLinesManager.instance.processName);
    }

    private void LateUpdate()
    {
        // counter++;
        // if (counter < 5) return;
        // fpsData += $"{Time.time.ToString(CultureInfo.InvariantCulture)},{fps.fps.ToString("F2", CultureInfo.InvariantCulture)}\n";
        // counter = 0;
    }
}
