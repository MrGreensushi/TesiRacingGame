using Mirror;
using QuickStart;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CheckPresence : MonoBehaviour
{
    private string file;
    private PlayerScript[] cars;
    private bool canWrite=false;
    // Start is called before the first frame update
    async void Start()
    {
        canWrite = false;
        cars=FindObjectsOfType<PlayerScript>();
       while (cars.Length <= 1)
       {
            cars = FindObjectsOfType<PlayerScript>();
            await Task.Yield();
        }

        foreach (PlayerScript p in cars)
        {

            while (string.IsNullOrEmpty(p.playerName))
                await Task.Yield();

        }

        //elimina se stesso
        var self = GetComponent<PlayerScript>();

        cars = cars.Where(val => val.playerName != self.playerName).ToArray();

        if (CommandLinesManager.instance == null)
            Debug.LogError("File path not specified");

        file = CommandLinesManager.instance.filePath;

        
        string toWrite = "TIME";
        foreach (PlayerScript p in cars)
        {
           toWrite += ';';

            toWrite += p.playerName +" VISIBLE;";
            toWrite += p.playerName + " MPAI";

        }
        toWrite += '\n';

        if (!File.Exists(file))
        {
            var myFile = File.Create(file);
            myFile.Close();
        }

        File.AppendAllText(file, toWrite);

        canWrite = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!canWrite) return;

        double now = NetworkTime.time;

        string toWrite = now.ToString();
        foreach (PlayerScript player in cars)
        {
            toWrite += ';';

            if (checkIfVisible(player))
                toWrite += '1';
            
            else
                toWrite += '0';

            if (player.mpaiActive)
                toWrite += ";1";
            else
                toWrite += ";0";


        }
        toWrite += '\n';
        File.AppendAllText(file, toWrite);


    }


    bool checkIfVisible(PlayerScript p)
    {
        return p.render.isVisible;
         
    }
}
