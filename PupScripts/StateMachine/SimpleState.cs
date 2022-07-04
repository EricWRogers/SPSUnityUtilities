using UnityEngine;

namespace SuperPupSystems.StateMachine
{
    [System.Serializable]
    public class SimpleState
    {
        private bool HasBeenInitialized = false;
        
        [Header("State Events")]
        [SerializeField]
        public OnStateStart StateStart;
        [SerializeField]
        public OnStateUpdate StateUpdated;
        [SerializeField]
        public OnStateExit StateExited;

        [HideInInspector]
        public SimpleStateMachine StateMachine;

        /// <summary>
        /// Called first when changing to this state
        /// </summary>
        public virtual void OnStart()
        {
            StateStart.Invoke();
            HasBeenInitialized = true;
        }

        /// <summary>
        /// Called on FixedUpdate while this state is active
        /// </summary>
        /// <param name="dt">amount of time in seconds since the last frame</param>
        public virtual void UpdateState(float dt)
        {
            if (!HasBeenInitialized)
                return;
            StateUpdated.Invoke();
        }

        /// <summary>
        /// Called before the next states start state
        /// </summary>
        public virtual void OnExit()
        {
            if (!HasBeenInitialized)
                return;
            StateExited.Invoke();
            HasBeenInitialized = false;
        }
    }

    [System.Serializable]
    public class OnStateStart : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class OnStateUpdate : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class OnStateExit : UnityEngine.Events.UnityEvent { }
}