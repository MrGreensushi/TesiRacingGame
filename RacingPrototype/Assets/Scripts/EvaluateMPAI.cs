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

    public void Writing(Rigidbody trueRB, Rigidbody ghostRB)
    {
        this.trueRB = trueRB;
        this.ghostRB = ghostRB;
        evaluating = true;
        first = true;
    }

    public void StopWriting()
    {
        evaluating = false;
    }
    private void Awake()
    {
        file = path + '\\' + fileName;


        if (!File.Exists(file))
        {
            var myFile = File.Create(file);
            myFile.Close();
        }

    }
    private void FixedUpdate()
    {
        if (!evaluating) return;

        var posDiff = Mathf.Abs((trueRB.position - ghostRB.position).magnitude);
        var rotDiff = Mathf.Abs(trueRB.rotation.y - ghostRB.rotation.y);
        if (rotDiff > 180f) rotDiff -= 180f;
        var velDiff = Mathf.Abs((trueRB.velocity - ghostRB.velocity).magnitude);

        var toWrite = posDiff.ToString() + ';' + rotDiff.ToString() + ';' + velDiff.ToString() + '\n';

        File.AppendAllText(file, toWrite);

    }

}
