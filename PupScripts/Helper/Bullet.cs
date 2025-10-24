using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperPupSystems.Helper
{
    [RequireComponent(typeof(Timer))]
    public class Bullet : MonoBehaviour
    {
        public int damage = 1;
        public float speed = 20f;
        public float lifeTime = 10f;
        public float gravity = 0.0f;
        public bool destroyOnImpact = true;
        public UnityEvent hitTarget;
        public LayerMask mask;
        public List<string> tags;
        private Vector3 m_lastPosition;

        [HideInInspector]
        public  RaycastHit hitInfo;
        private Timer m_timer;

        private void Awake()
        {
            if (hitTarget == null)
            {
                hitTarget = new UnityEvent();
            }
        }

        private void Start()
        {
            m_timer = GetComponent<Timer>();
            m_timer.timeout.AddListener(DestroyBullet);

            m_timer.StartTimer(lifeTime);

            // set init position
            m_lastPosition = transform.position;
        }

        private void FixedUpdate()
        {
            Move();

            CollisionCheck();

            m_lastPosition = transform.position;
        }

        private void Move()
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
            transform.position += Vector3.up * gravity * Time.fixedDeltaTime; 
        }

        private void CollisionCheck()
        {
            if (Physics.Linecast(m_lastPosition, transform.position, out hitInfo, mask))
            {
                if (tags.Contains(hitInfo.transform.tag))
                {
                    hitInfo.transform.GetComponent<Health>()?.Damage(damage);

                    if (destroyOnImpact == false)
                    {
                        hitTarget.Invoke();
                    }
                }

                

                if (destroyOnImpact)
                {
                    hitTarget.Invoke();
                    DestroyBullet();
                }
            }
        }

        private void DestroyBullet()
        {
            Destroy(gameObject);
        }
    }
}