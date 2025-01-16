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
        public Animator animator;

        private Rigidbody2D _rb2d;
        
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            float xInput = Input.GetAxis("Horizontal");
            isTouchingGround = IsTouchingGround();
            Vector2 motion = _rb2d.linearVelocity;

            if (xInput != 0.0f)
            {
                
                if (!TestMove(Vector2.right, collisionTestOffset) && xInput > 0.0f)
                {
                    motion.x = -xInput * (speed*0.01f);
                }
                else if (!TestMove(Vector2.left, collisionTestOffset) && xInput < 0.0f)
                {
                    motion.x = -xInput * (speed*0.01f);
                }
                else
                {
                    motion.x = xInput * speed;
                }
            }

            if (Input.GetAxis("Jump") > 0 && isTouchingGround)
            {
                motion.y = speed+2.5f;
            }

            if (animator != null)
            {
                animator.SetFloat("SpeedX", Mathf.Abs(motion.x));
                animator.SetFloat("SpeedY", motion.y);
                animator.SetBool("Grounded", isTouchingGround);
            }

            if (spriteRenderer != null && xInput != 0.0f)
            {
                if (xInput > 0.0f)
                    spriteRenderer.flipX = false;
                else
                    spriteRenderer.flipX = true;
            }

            _rb2d.linearVelocity = motion;
        }
    }
}