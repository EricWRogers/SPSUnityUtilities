using UnityEngine;
using UnityEngine.Events;
using SuperPupSystems.Helper;

namespace SuperPupSystems.GamePlay2D
{
    public class CollisionEvent2D : MonoBehaviour
    {
        public CollisionEventType type = CollisionEventType.ENTER;
        public UnityEvent onTrigger;
        public string targetTag;
        public bool destroyAfter = false;
        private bool m_eventFired = false;
        public void OnTriggerEnter2D(Collider2D _collider)
        {
            if (type == CollisionEventType.ENTER)
            {
                Use(_collider);
            }
        }

        public void OnTriggerExit2D(Collider2D _collider)
        {
            if (type == CollisionEventType.EXIT)
            {
                Use(_collider);
            }
        }
        private void Use(Collider2D _collider)
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
}