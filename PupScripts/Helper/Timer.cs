using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Helper
{
    public class Timer : MonoBehaviour
    {
        public UnityEvent TimeOut;
        [Tooltip("When AutoStart is set to true the timer starts when the GameObject Start method is called by Unity.")]
        public bool AutoStart = false;
        [Tooltip("When AutoRestart is set to true the timer with start again as soon as it runs out.")]
        public bool AutoRestart = false;
        [Tooltip("CountDownTime is the amount of time the timer will be set to but not the variable that will be counting down.")]
        public float CountDownTime = 1.0f;
        [Tooltip("Time scale and be used to speed up or slow down to timer")]
        public float timeScale = 1.0f;

        public float TimeLeft { get { return timeLeft; } }

        // timeLeft is used as the countDown variable
        private float timeLeft = 0.0f;

        /// <summary>
        /// Start is called in the frame when a script is enable just before any
        /// update methods are called the first time.
        /// </summary>
        void Start()
        {
            if (TimeOut == null)
                TimeOut = new UnityEvent();

            if (AutoStart)
                StartTimer(CountDownTime, AutoRestart);
        }

        /// <summary>
        /// Update is called on every frame
        /// </summary>
        void Update()
        {
            if (timeLeft > 0.0f)
            {
                timeLeft -= (Time.deltaTime * timeScale);

                if (timeLeft <= 0.0f)
                {
                    TimeOut.Invoke();
                    if (AutoRestart)
                        StartTimer(CountDownTime, AutoRestart);
                }
            }
        }

        /// <summary>
        /// Start timer will start the timer with the values passed in or
        /// the public class variables are null.
        /// </summary>
        /// <param name="countDown">The amount of time in seconds the timer will run.</param>
        /// <param name="autoRestart">If true the timer will restart when finish.</param>
        public void StartTimer(float? countDown = null, bool autoRestart = false)
        {
            if (countDown != null && countDown > 0.0f)
                CountDownTime = (float)countDown;

            AutoRestart = autoRestart;

            timeLeft = CountDownTime;
        }

        /// <summary>
        /// Stop timer will end the timer without invoking the TimeOut Event
        /// </summary>
        public void StopTimer()
        {
            timeLeft = 0.0f;
        }

        /// <summary>
        /// Adds extra time to the timer
        /// </summary>
        public void AddTime(float time)
        {
            timeLeft += time;
        }
    }
}
