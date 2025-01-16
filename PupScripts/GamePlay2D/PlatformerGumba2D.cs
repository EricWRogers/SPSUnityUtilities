using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperPupSystems.GamePlay2D
{
    public class PlatformerGumba2D : CharacterController2D
    {
        public float speed = 10.0f;
        public float collisionTestOffset;
        public Rigidbody2D rb2d;
        public Vector2 direction = Vector2.right;
        public int damage = 2;
        public List<string> tagsToDamage;
        
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        
        void Update()
        {
            Vector2 motion = rb2d.linearVelocity;
            isTouchingGround = IsTouchingGround();

            if (isTouchingGround)
            {
                // Attack
                List<RaycastHit2D> rightHits = new List<RaycastHit2D>();
                List<RaycastHit2D> leftHits = new List<RaycastHit2D>();
                if (!TestMove(Vector2.right, collisionTestOffset, tagsToDamage, rightHits) ||
                !TestMove(Vector2.left, collisionTestOffset, tagsToDamage, leftHits))
                {
                    List<RaycastHit2D> results = rightHits.Concat(leftHits).ToList();

                    foreach(RaycastHit2D hit in results)
                    {
                        SuperPupSystems.Helper.Health hitHealth =
                            hit.collider.gameObject.GetComponent<SuperPupSystems.Helper.Health>();
                        
                        if (hitHealth)
                        {
                            hitHealth.Damage(damage);
                        }
                    }
                }

                // Should I change direction
                if (!TestMove(direction, collisionTestOffset))
                {
                    direction *= -1;
                }

                // Check that I will walk off the edge
                RaycastHit2D rayHit = EdgeHit(transform.position, direction, 0.1f);
                bool edgeFound = false;

                if (rayHit)
                {
                    Vector2 characterBase = ((Vector2)transform.position)+col.offset;
                    characterBase.y -= col.size.y/2;

                    if (rayHit.point.y < characterBase.y-0.1f)
                    {
                        Debug.Log("rpy : " + rayHit.point.y + " cby : " + characterBase.y);
                        edgeFound = true;
                    }
                }
                else
                {
                    Debug.Log("Dropoff");
                    edgeFound = true;
                }

                if (edgeFound)
                {
                    direction *= -1;
                }

                // Movement
                rb2d.linearVelocity = direction * speed;
            }
        }
    }
}