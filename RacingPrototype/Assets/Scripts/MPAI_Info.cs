using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MPAI_Info : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nome, info;

    public void Nome(string x, Color y)
    {
        nome.text = x;
        nome.color = y;
    }

    public void Info(bool x)
    {
        if (x)
            info.text = "Yes";
        else
            info.text = "No";
    }
}
