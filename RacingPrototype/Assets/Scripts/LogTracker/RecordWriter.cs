using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class RecordWriter : MonoBehaviour
{
    public bool write;
    private Offline_CarsManager manager;
    [ShowInInspector] private string baseName_ph = "Physic", baseName_cmd = "Commands", baseName_rl = "Rules", baseName_cp = "CommPhy";
    private string physicsFilename, rulesFilename, commandsFilename, commPhyFileName;
    public string physicsFolder, rulesFolder, commandsFolder, commPhyFolder;
    public int name_index;
    //public float waitTime;
    public bool newRace = false;
    List<Offline_CarInfos> cars;
    private void CreateNames()
    {
        if (name_index > 0)
        {
            baseName_ph += " - Copia";
            baseName_cmd += " - Copia";
            baseName_rl += " - Copia";
            baseName_cp += " - Copia";
        }
        if (name_index > 1)
        {
            baseName_ph += " (" + name_index + ")";
            baseName_cmd += " (" + name_index + ")";
            baseName_rl += " (" + name_index + ")";
            baseName_cp += " (" + name_index + ")";
        }
        baseName_ph += ".txt";
        baseName_cmd += ".txt";
        baseName_rl += ".txt";
        baseName_cp += ".txt";

        physicsFilename = physicsFolder + baseName_ph;
        rulesFilename = rulesFolder + baseName_rl;
        commandsFilename = commandsFolder + baseName_cmd;
        commPhyFileName = commPhyFolder + baseName_cp;
    }

    public async Task StartWriting(int index)
    {
        name_index = index;
        manager = GetComponent<Offline_CarsManager>();
        //Clear content of previous file
        //File.WriteAllText(physicsFilename, string.Empty);
        //File.WriteAllText(rulesFilename, string.Empty);
        //File.WriteAllText(commandsFilename, string.Empty);
        //File.OpenWrite(rulesFilename);
        CreateNames();
        CreateFile(physicsFilename);
        CreateFile(rulesFilename);
        CreateFile(commandsFilename);
        CreateFile(commPhyFileName);
        while (manager.cars == null)
            await Task.Yield();
        cars = manager.cars;
        cars.Sort((x, y) => x.Player.playerName.CompareTo(y.Player.playerName));



    }


    void CreateFile(string myPath)
    {
        var myFile = File.Create(myPath);
        myFile.Close();
    }
    public Task WriteLogs()
    {


        if (!write) return Task.CompletedTask;
        // int i = 0;
        if (newRace)
        {
            newRace = false;
            File.AppendAllText(physicsFilename, "_");
            File.AppendAllText(rulesFilename, "_");
            File.AppendAllText(commandsFilename, "_");
        }
        string physics = "";
        string rules = "";
        string commands = "";
        // for each car: car velocity, car position & direction, car rank, car laps 
        //sort by name so that the order is always the same

        foreach (var item in cars)
        {
            string[] both = item.AllInfos();
            string physich = both[0];
            string rule = both[1];
            string command = both[2];

            commands += command + ";";
            physics += physich + ";";
            rules += rule + ";";
        }
        string commPhy = physics + commands;
        //remove last ;
        if (commands.Length > 0)
            commands = commands.Remove(commands.Length - 1, 1);
        if (physics.Length > 0)
            physics = physics.Remove(physics.Length - 1, 1);
        if (rules.Length > 0)
            rules = rules.Remove(rules.Length - 1, 1);
        if (commPhy.Length > 0)
            commPhy = commPhy.Remove(commPhy.Length - 1, 1);

        commands += '\n';
        physics += '\n';
        rules += '\n';
        commPhy += "\n";
        File.AppendAllText(physicsFilename, physics);
        File.AppendAllText(rulesFilename, rules);
        File.AppendAllText(commandsFilename, commands);
        File.AppendAllText(commPhyFileName, commPhy);

        return Task.CompletedTask;

    }
}
