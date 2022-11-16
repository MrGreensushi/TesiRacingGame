using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuickStart
{
    public class UI_Velocity : NetworkBehaviour
    {
        public TextMeshProUGUI velocityText;
        public TextMeshProUGUI lapsText;
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI timeText;

        public double offsetTime;
        private void Start()
        {
            offsetTime = NetworkTime.time;
        }
        public string Velocity { set { velocityText.text = value; } }
        public string Laps { set { lapsText.text = value; } }
        public string Rank { set { rankText.text = value; } }
        public string Name { set { nameText.text = value; } }
        public string Time { get => timeText.text; set { timeText.text = value; } }

        public void Udpate(CarDescriptor x)
        {
            Velocity = Mathf.RoundToInt(x.Velocity).ToString();
            Laps = x.Laps.ToString();
            Rank = x.Rank.ToString();
            Time = TimeFormat();
        }

        public void Udpate(float velocity, int laps, int rank, string time)
        {
            Velocity = Mathf.RoundToInt(velocity).ToString();
            Laps = laps.ToString();
            Rank = rank.ToString();
            Time = time;

        }

        string TimeFormat()
        {
            var now = NetworkTime.time - offsetTime;

            var minutes = Mathf.FloorToInt((float)now / 60.0f);
            var seconds = (int)now - minutes * 60;
            var millsec = (now - (int)now) * 1000;
            return minutes + ":" + string.Format("{0:00}", seconds) + ":"
                + string.Format("{0:000}", millsec);

        }


    }

}
