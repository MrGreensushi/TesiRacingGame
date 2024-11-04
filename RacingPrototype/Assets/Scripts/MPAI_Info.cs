using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MPAI_Info : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nome, info;
    private GameObject carObject;
    private FollowCamera camera;
    private Button button;

    private void Awake()
    {
        camera = FindObjectOfType<FollowCamera>();
        button = GetComponentInChildren<Button>();
        Assert.IsNotNull(button);
        
        button.onClick.AddListener(ChangeServerCamera);

    }

    public void Nome(string x, Color y)
    {
        var names=x.Split(' ');
        nome.text = names[1];
        nome.color = y;
    }

    public void Info(bool x)
    {
        if (x)
            info.text = "Yes";
        else
            info.text = "No";
    }

    public void SetCameraCarFocus(GameObject car)
    {
        carObject = car;
    }

    private void ChangeServerCamera()
    {
        camera.SwitchView(carObject);
    }
}
