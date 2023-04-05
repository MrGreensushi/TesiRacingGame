using TMPro;
using UnityEngine;

public class Offline_Velocity : MonoBehaviour
{
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI timeText;
    public Rigidbody x;
    bool cinq = false, cent = false;

    public double offsetTime;
    private void Start()
    {
        offsetTime = Time.timeAsDouble;
    }
    public string Velocity { set { velocityText.text = value; } }
    public string UTime { get => timeText.text; set { timeText.text = value; } }

    private void Update()
    {
        //La velocità è in metri/secondo, la trasformo in km/h
        var vel = Kmh(x.velocity.magnitude);

        if (vel >= 50 && !cinq)
        {
            Debug.LogWarning("50: " + TimeFormat());
            cinq = true;
        }
        if (vel >= 100 && !cent)
        {
            Debug.LogWarning("100:" + TimeFormat());
            cent = true;
        }

        Velocity = vel.ToString();
        UTime = TimeFormat();
    }


    string TimeFormat()
    {
        var now = Time.timeAsDouble - offsetTime;

        var minutes = Mathf.FloorToInt((float)now / 60.0f);
        var seconds = (int)now - minutes * 60;
        var millsec = (now - (int)now) * 1000;


        return minutes + ":" + string.Format("{0:00}", seconds) + ":"
            + string.Format("{0:000}", millsec);

    }

    int Kmh(float x)
    {
        return Mathf.FloorToInt(x * 60 * 60 / 1000);
    }
}
