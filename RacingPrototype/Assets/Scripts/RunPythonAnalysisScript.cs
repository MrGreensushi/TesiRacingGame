using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RunPythonAnalysisScript : MonoBehaviour
{
    // private string pythonPath = "C:\\Users\\Daniele\\AppData\\Local\\Programs\\Python\\Python312\\python.exe"; // Path to Python executable
    // private string scriptPath = "C:\\Users\\Daniele\\Downloads\\system_info_logger.py"; // Path to Python script
    // private string logFilePath ="C:\\Users\\Daniele\\Desktop\\Predictions\\system_usage_log.csv"; // Desired output log file
    // private string processName = "system-performance-log.exe";

    private Process systemInfoProcess;

    // private void Start()
    // {
    //     RunScript(pythonPath,scriptPath,logFilePath,processName);
    // }

    public void RunScript(string pythonPath, string scriptPath, string logFilePath, string processName)
    {
        // Construct the base arguments for the Python script
        string arguments = $"\"{scriptPath}\" --output \"{logFilePath}\"";

        // Add the --process_name argument if it's not null or empty
        if (!string.IsNullOrEmpty(processName))
        {
            arguments += $" --process_name \"{processName}\"";
        }

        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = arguments,
            UseShellExecute = true, // Allows the script to run independently
            CreateNoWindow = true
        };

        systemInfoProcess = new Process { StartInfo = processInfo };
        try
        {
            systemInfoProcess.Start();
            Debug.Log("Python script launched as a detached process.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to start Python script: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        if (systemInfoProcess != null) 
        {
            systemInfoProcess.Kill();
        }
    }
}