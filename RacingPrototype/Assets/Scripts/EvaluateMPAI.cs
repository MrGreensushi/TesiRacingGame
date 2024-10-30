using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EvaluateMPAI : MonoBehaviour
{
    public string path;
    public string fileName;
    private string file;
    public bool evaluating = false;
    private bool first = false;
    public Rigidbody trueRB, ghostRB;
    private int tot,counter = 0;

    public void Writing(Rigidbody trueRB, Rigidbody ghostRB)
    {
        this.trueRB = trueRB;
        this.ghostRB = ghostRB;
        evaluating = true;
        if (!first)
            tot++;
        first = true;

    }

    public void StopWriting()
    {
        first = false;
        evaluating = false;
        counter = 0;
    }
    private void Start()
    {
        tot=0;
        counter = 0;
        evaluating = false;
        var cl = CommandLinesManager.instance;
        if (cl != null)
        {
            if (!string.IsNullOrEmpty(cl.path))
                path = cl.path;
            if (!string.IsNullOrEmpty(cl.fileName))
                fileName = cl.fileName;
            
        }

        file = path + '\\' + fileName;


        if (!File.Exists(file))
        {
            var myFile = File.Create(file);
            myFile.Close();
        }

        string toWrite = "POS;ROT;VEL;COUNTER;NUMBER\n";
        File.AppendAllText(file, toWrite);

    }
    private void FixedUpdate()
    {
        if (!evaluating) return;

        var posDiff = Mathf.Abs((trueRB.position - ghostRB.position).magnitude);
        var rotDiff = Mathf.Abs(trueRB.rotation.y - ghostRB.rotation.y);
        if (rotDiff > 180f) rotDiff -= 180f;
        var velDiff = Mathf.Abs((trueRB.velocity - ghostRB.velocity).magnitude);

        var toWrite = posDiff.ToString() + ';' + rotDiff.ToString() + ';' + velDiff.ToString() +';'+counter.ToString()+';'+tot.ToString() +'\n';

        File.AppendAllText(file, toWrite);
        counter++;

    }

}
