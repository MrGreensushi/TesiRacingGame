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
    private Image background;

    private void Awake()
    {
        camera = FindObjectOfType<FollowCamera>();
        button = GetComponentInChildren<Button>();
        Assert.IsNotNull(button);
        
        button.onClick.AddListener(ChangeServerCamera);

        background = GetComponent<Image>();
        Assert.IsNotNull(background,"MPAI-INFO background should not be null");
        background.color=Color.clear;
    }

    public void Nome(string x, Color y)
    {
        var names=x.Split(' ');
        nome.text = names[1];
        nome.color = y;
    }

    public void Info(bool x)
    {
        info.text = x ? "Yes" : "No";
        background.color=x?Color.yellow:Color.clear;
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
