using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SuperPupSystems.Helper
{
    public class CountDownText : MonoBehaviour
    {
        public Timer timer;
        public float time { get { return timer.TimeLeft; } }
        public float startTime = 30.0f;
        public TMP_Text text;
        void Start()
        {
            StartCountDownTimer();
        }

        // Update is called once per frame
        void Update()
        {
            text.text = "Timeleft : " + (int)time;
        }

        public void StartCountDownTimer()
        {
            timer.StartTimer(startTime, false);
        }
    }
}