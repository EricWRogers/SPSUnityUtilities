using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.GamePlay2D
{
    public class PlatformerPlayerCharacter2D : CharacterController2D
    {
        public float speed = 10.0f;

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

            if (isTouchingGround)
            {
                rb2d.velocity = new Vector2(
                    xInput * speed,
                    rb2d.velocity.y
                );
            }

            if (Input.GetAxis("Jump") > 0 && isTouchingGround)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, speed+2.5f);
            }
        }
    }
}