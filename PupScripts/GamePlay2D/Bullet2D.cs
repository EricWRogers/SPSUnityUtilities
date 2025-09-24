using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SuperPupSystems.Helper;

namespace SuperPupSystems.GamePlay2D
{
    [RequireComponent(typeof(Timer))]
    public class Bullet2D : MonoBehaviour
    {
        public int damage = 1;
        public float speed = 20f;
        public float lifeTime = 10f;
        public float gravity = 0.0f;
        public bool destroyOnImpact = true;
        public UnityEvent hitTarget;
        public LayerMask mask;
        public bool checkTags = true;
        public List<string> tags;
        private Vector3 m_lastPosition;

        [HideInInspector]
        public  RaycastHit2D hitInfo;
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
            transform.position += transform.right * speed * Time.fixedDeltaTime;
        }

        private void CollisionCheck()
        {
            hitInfo = Physics2D.Linecast(m_lastPosition, transform.position, mask);
            if (hitInfo)
            {
                if (tags.Contains(hitInfo.transform.tag))
                {
                    hitInfo.transform.GetComponent<Health>()?.Damage(damage);

                    hitTarget.Invoke();
                }

                if (destroyOnImpact)
                {
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