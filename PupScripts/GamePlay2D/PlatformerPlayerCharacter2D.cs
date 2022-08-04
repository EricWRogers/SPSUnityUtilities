using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.GamePlay2D
{
    public class PlatformerPlayerCharacter2D : CharacterController2D
    {
        public float speed = 10.0f;
        public float collisionTestOffset;

        public SpriteRenderer spriteRenderer;

        private Rigidbody2D rb2d;
        private Vector2 motion = new Vector2();
        private float jumpInputLastFrame = 0.0f;
        
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            float xInput = Input.GetAxis("Horizontal");
            isTouchingGround = IsTouchingGround();
            Vector2 motion = rb2d.velocity;

            if (xInput != 0.0f)
            {
                
                if (!TestMove(Vector2.right, collisionTestOffset) && xInput > 0.0f)
                {
                    Debug.Log("Hit Right");
                    motion.x = -xInput * (speed*0.01f);
                }
                else if (!TestMove(Vector2.left, collisionTestOffset) && xInput < 0.0f)
                {
                    Debug.Log("Hit Left");
                    motion.x = -xInput * (speed*0.01f);
                }
                else
                {
                    motion.x = xInput * speed;;
                }
            }

            if (Input.GetAxis("Jump") > 0 && isTouchingGround)
            {
                motion.y = speed+2.5f;
            }

            rb2d.velocity = motion;
        }
    }
}