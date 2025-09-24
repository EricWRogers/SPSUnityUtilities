using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Helper
{
    public class CollisionEvent : MonoBehaviour
    {
        public CollisionEventType type = CollisionEventType.ENTER;
        public UnityEvent onTrigger;
        public string targetTag;
        public bool destroyAfter = false;
        private bool m_eventFired = false;
        public void OnTriggerEnter(Collider _collider)
        {
            if (type == CollisionEventType.ENTER)
            {
                Use(_collider);
            }
        }

        public void OnTriggerExit(Collider _collider)
        {
            if (type == CollisionEventType.EXIT)
            {
                Use(_collider);
            }
        }
        private void Use(Collider _collider)
        {
            if (destroyAfter && m_eventFired)
                return;

            if (_collider.gameObject.tag == targetTag)
            {
                m_eventFired = true;
                onTrigger.Invoke();

                if (destroyAfter)
                    Destroy(gameObject);
            }
        }
    }

    [System.Serializable]
    public enum CollisionEventType
    {
        ENTER,
        EXIT
    }
}