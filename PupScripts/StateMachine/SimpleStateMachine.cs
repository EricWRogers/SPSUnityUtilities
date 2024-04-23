using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.StateMachine
{
    public class SimpleStateMachine : MonoBehaviour
    {
        [Header("States")]
        protected SimpleState m_state = null;
        [HideInInspector]
        public List<SimpleState> states;
        public string stateName;

        /// <summary>
        /// Sets new current state
        /// </summary>
        /// <param name="s">the state class that we will switch to</param>
        private void SetState(SimpleState _s)
        {
            if (_s == null)
                return;
            if (m_state != null)
            {
                m_state.stateExited.Invoke();
                m_state.OnExit();
            }

            m_state = _s;
            m_state.stateMachine = this;
            m_state.stateStart.Invoke();
            m_state.OnStart();
            stateName = m_state.GetType().ToString();
        }

        /// <summary>
        /// Try to change the current state
        /// </summary>
        /// <param name="_stateName">SimpleState.GetType().ToString().ToLower()</param>
        public void ChangeState(string _stateName)
        {
            foreach (SimpleState s in states)
            {
                if (_stateName.ToLower() == s.GetType().ToString().ToLower())
                {
                    SetState(s);
                    return;
                }
            }

            Debug.LogWarning("State Not found: " + _stateName);
        }

        /// <summary>
        /// Calls update on current state
        /// </summary>
        public void FixedUpdate()
        {
            if (m_state == null)
                return;

            m_state.UpdateState(Time.fixedDeltaTime);
        }
    }
}