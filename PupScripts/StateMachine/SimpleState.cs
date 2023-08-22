using UnityEngine;

namespace SuperPupSystems.StateMachine
{
    [System.Serializable]
    public class SimpleState
    {
        private bool m_hasBeenInitialized = false;
        
        [Header("State Events")]
        [SerializeField]
        public OnStateStart stateStart;
        [SerializeField]
        public OnStateUpdate stateUpdated;
        [SerializeField]
        public OnStateExit stateExited;

        [HideInInspector]
        public SimpleStateMachine stateMachine;

        /// <summary>
        /// Called first when changing to this state
        /// </summary>
        public virtual void OnStart()
        {
            stateStart.Invoke();
            m_hasBeenInitialized = true;
        }

        /// <summary>
        /// Called on FixedUpdate while this state is active
        /// </summary>
        /// <param name="_dt">amount of time in seconds since the last frame</param>
        public virtual void UpdateState(float _dt)
        {
            if (!m_hasBeenInitialized)
                return;
            stateUpdated.Invoke();
        }

        /// <summary>
        /// Called before the next states start state
        /// </summary>
        public virtual void OnExit()
        {
            if (!m_hasBeenInitialized)
                return;
            stateExited.Invoke();
            m_hasBeenInitialized = false;
        }
    }

    [System.Serializable]
    public class OnStateStart : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class OnStateUpdate : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class OnStateExit : UnityEngine.Events.UnityEvent { }
}