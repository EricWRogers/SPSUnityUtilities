using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperPupSystems.GamePlay2D
{
    public class CharacterController2D : MonoBehaviour
    {
        public GameObject groundCheckPosition;
        public Vector3 groundCheckSize = new Vector3(0.75f, 0.2f, 1.0f);
        public List<string> jumpableTags = new List<string>();
        public ContactFilter2D contactFilter2D;
        public GameObject debugTarget;
        
        public bool isTouchingGround = false;

        void OnDrawGizmosSelected()
        {
            // Draw a semitransparent blue cube at the transforms position
            if (isTouchingGround)
                Gizmos.color = new Color(0, 1, 0, 0.5f);
            else
                Gizmos.color = new Color(1, 0, 0, 0.5f);
            
            Gizmos.DrawCube(groundCheckPosition.transform.position, groundCheckSize);
        }

        public bool IsTouchingGround()
        {
            List<RaycastHit2D> results = new List<RaycastHit2D>();

            GroundCheck(results);
            
            if (results.Count > 0)
            {
                foreach(RaycastHit2D hit in results)
                {
                    if (jumpableTags.Contains(hit.collider.gameObject.tag))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public void GroundCheck(List<RaycastHit2D> results)
        {
            Physics2D.BoxCast(
                new Vector2(groundCheckPosition.transform.position.x,groundCheckPosition.transform.position.y),
                new Vector2(groundCheckSize.x, groundCheckSize.y),
                0.0f,
                Vector2.right,
                contactFilter2D,
                results,
                0.1f);
        }
    }
}
