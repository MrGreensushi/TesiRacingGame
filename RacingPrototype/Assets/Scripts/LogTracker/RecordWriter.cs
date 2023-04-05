using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordWriter : MonoBehaviour
{
    public bool write;
    private Offline_CarsManager manager;
    [ShowInInspector] private string baseName_ph = "Physic", baseName_cmd = "Commands", baseName_rl = "Rules";
    private string physicsFilename, rulesFilename, commandsFilename;
    public string physicsFolder, rulesFolder, commandsFolder;
    public int name_index;
    public float waitTime;
    public bool newRace = false;

    private void CreateNames()
    {
        if (name_index > 0)
        {
            baseName_ph += " - Copia";
            baseName_cmd += " - Copia";
            baseName_rl += " - Copia";
        }
        if (name_index > 1)
        {
            baseName_ph += " (" + name_index + ")";
            baseName_cmd += " (" + name_index + ")";
            baseName_rl += " (" + name_index + ")";
        }
        baseName_ph += ".txt";
        baseName_cmd += ".txt";
        baseName_rl += ".txt";

        physicsFilename = physicsFolder + baseName_ph;
        rulesFilename = rulesFolder + baseName_rl;
        commandsFilename = commandsFolder + baseName_cmd;
    }

    public void StartWriting(int index)
    {
        name_index = index;
        manager = GetComponent<Offline_CarsManager>();
        //Clear content of previous file
        //File.WriteAllText(physicsFilename, string.Empty);
        //File.WriteAllText(rulesFilename, string.Empty);
        //File.WriteAllText(commandsFilename, string.Empty);
        //File.OpenWrite(rulesFilename);
        CreateNames();

        File.Create(physicsFilename);
        File.Create(rulesFilename);
        File.Create(commandsFilename);

        StartCoroutine(WriteLogs());
    }
    IEnumerator WriteLogs()
    {
        // int i = 0;
        while (write)
        {
            yield return new WaitForSeconds(waitTime);
            if (newRace)
            {
                newRace = false;
                File.AppendAllText(physicsFilename, "&&");
                File.AppendAllText(rulesFilename, "&&");
                File.AppendAllText(commandsFilename, "&&");
            }
            string physics = "";
            string rules = "";
            string commands = "";
            // for each car: car velocity, car position & direction, car rank, car laps 
            foreach (var item in manager.cars)
            {
                string[] both = item.AllInfos();
                string physich = both[0];
                string rule = both[1];
                string command = both[2];

                commands += command + "||";
                physics += physich + "||";
                rules += rule + "||";
            }
            commands += '\n';
            physics += '\n';
            rules += '\n';
            File.AppendAllText(physicsFilename, physics);
            File.AppendAllText(rulesFilename, rules);
            File.AppendAllText(commandsFilename, commands);
        }

    }

    void ReadFile()
    {



    }
}
