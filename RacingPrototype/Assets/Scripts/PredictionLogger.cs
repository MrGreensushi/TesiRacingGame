using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class PredictionLogger : MonoBehaviour
{
    public static PredictionLogger Instance;

    public string filePath="";
    private string logs = "Player ID,Start Prediction, End Prediction\n";
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
        }
        
        filePath = CommandLinesManager.instance.filePathPredictionsTime;
        Assert.IsFalse(filePath is null or "");

        var lat = new Latency(LatencyLevel.L1);
        WriteOnFile("L1 Duration L1 Frequency",lat.Duration.ToString(CultureInfo.InvariantCulture),lat.Frequency.ToString(CultureInfo.InvariantCulture) );
    }

    public void WriteOnFile(string playerId, string startTimePrediction, string endTimePrediction)
    {
        if (!File.Exists(filePath))
        {
            var myFile = File.Create(filePath);
            myFile.Close();
        }
        
        // Creiamo una stringa con i valori separati da ','
        var contenuto = $"{playerId},{startTimePrediction},{endTimePrediction}\n";
        logs += contenuto;
        // try
        // {
        //     // Scriviamo la stringa sul file specificato
        //     File.AppendAllText(filePath, contenuto);
        //     Debug.Log("Dati scritti con successo.");
        // }
        // catch (Exception ex)
        // {
        //     // Gestione errori in caso di problemi con la scrittura del file
        //     Debug.LogError($"Errore durante la scrittura del file: {ex.Message}");
        // }
    }

    private void OnDestroy()
    {
        try
        {
            // Scriviamo la stringa sul file specificato
            File.AppendAllText(filePath, logs);
            Debug.Log("Dati scritti con successo.");
        }
        catch (Exception ex)
        {
            // Gestione errori in caso di problemi con la scrittura del file
            Debug.LogError($"Errore durante la scrittura del file: {ex.Message}");
        }
    }
}
