using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Helper
{
    public class Timer : MonoBehaviour
    {
        // m_timeLeft is used as the countDown variable
        private float m_timeLeft = 0.0f;

        public UnityEvent timeout;
        [Tooltip("When autoStart is set to true the timer starts when the GameObject Start method is called by Unity.")]
        public bool autoStart = false;
        [Tooltip("When autoRestart is set to true the timer with start again as soon as it runs out.")]
        public bool autoRestart = false;
        [Tooltip("countDownTime is the amount of time the timer will be set to but not the variable that will be counting down.")]
        public float countDownTime = 1.0f;
        [Tooltip("Time scale and be used to speed up or slow down to timer")]
        public float timeScale = 1.0f;

        public float timeLeft { get { return m_timeLeft; } }

        /// <summary>
        /// Start is called in the frame when a script is enable just before any
        /// update methods are called the first time.
        /// </summary>
        void Start()
        {
            if (timeout == null)
                timeout = new UnityEvent();

            if (autoStart)
                StartTimer(countDownTime, autoRestart);
        }

        /// <summary>
        /// Update is called on every frame
        /// </summary>
        void Update()
        {
            if (m_timeLeft > 0.0f)
            {
                m_timeLeft -= (Time.deltaTime * timeScale);

                if (m_timeLeft <= 0.0f)
                {
                    timeout.Invoke();
                    if (autoRestart)
                        StartTimer(countDownTime, autoRestart);
                }
            }
        }

        public void StartTimerFromEvent()
        {
            StartTimer();
        }

        /// <summary>
        /// Start timer will start the timer with the values passed in or
        /// the public class variables are null.
        /// </summary>
        /// <param name="_countDown">The amount of time in seconds the timer will run.</param>
        /// <param name="_autoRestart">If true the timer will restart when finish.</param>
        public void StartTimer(float? _countDown = null, bool _autoRestart = false)
        {
            if (_countDown != null && _countDown > 0.0f)
                countDownTime = (float)_countDown;

            autoRestart = _autoRestart;

            m_timeLeft = countDownTime;
        }

        /// <summary>
        /// Stop timer will end the timer without invoking the timeout Event
        /// </summary>
        public void StopTimer()
        {
            m_timeLeft = 0.0f;
        }

        /// <summary>
        /// Adds extra time to the timer
        /// </summary>
        public void AddTime(float _time)
        {
            m_timeLeft += _time;
        }
    }
}
