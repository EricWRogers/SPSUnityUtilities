using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.StateMachine
{
    public class SimpleStateMachine : MonoBehaviour
    {
        [Header("States")]
        [HideInInspector]
        public List<SimpleState> States;
        public string StateName;
        protected SimpleState state = null;

        /// <summary>
        /// Sets new current state
        /// </summary>
        /// <param name="s">the state class that we will switch to</param>
        private void SetState(SimpleState s)
        {
            if (s == null)
                return;
            if (state != null)
            {
                state.OnExit();
            }

            state = s;
            state.OnStart();
        }

        /// <summary>
        /// Try to change the current state
        /// </summary>
        /// <param name="stateName">SimpleState.GetType().ToString().ToLower()</param>
        public void ChangeState(string stateName)
        {
            foreach (SimpleState _state in States)
            {
                if (stateName.ToLower() == _state.GetType().ToString().ToLower())
                {
                    SetState(_state);
                    Debug.Log("State Changed: " + nameof(_state));
                    StateName = stateName;
                    return;
                }
            }

            Debug.LogWarning("State Not found: " + stateName);
        }

        /// <summary>
        /// Calls update on current state
        /// </summary>
        public void FixedUpdate()
        {
            if (state == null)
                return;

            state.UpdateState(Time.deltaTime);
        }
    }
}