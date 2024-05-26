// Copyright (C) 2024 Nicholas Maltbie, Sam Scherer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using UnityEngine.Events;

namespace ProjectCoda.Player
{
    /// <summary>
    /// Reference - https://github.com/Brackeys/2D-Character-Controller/tree/master
    /// </summary>
    public class CharacterController2D : MonoBehaviour
    {
        /// <summary>
        /// Amount of force added when the player jumps.
        /// </summary>
        [SerializeField]
        private float m_JumpForce = 400f;

        /// <summary>
        /// Amount of maxSpeed applied to crouching movement. 1 = 100%
        /// </summary>
        [Range(0, 1)]
        [SerializeField]
        private float m_CrouchSpeed = .36f;

        /// <summary>
        /// How much to smooth out the movement
        /// </summary>
        [Range(0, .3f)]
        [SerializeField]
        private float m_MovementSmoothing = .05f;

        /// <summary>
        /// Whether or not a player can steer while jumping;
        /// </summary>
        [SerializeField]
        private bool m_AirControl = true;

        /// <summary>
        /// A mask determining what is ground to the character
        /// </summary>
        [SerializeField]
        private LayerMask m_WhatIsGround;

        /// <summary>
        /// A position marking where to check if the player is grounded.
        /// </summary>
        [SerializeField]
        private Transform m_GroundCheck;

        /// <summary>
        /// A position marking where to check for ceilings
        /// </summary>
        [SerializeField]
        private Transform m_CeilingCheck;

        /// <summary>
        /// A collider that will be disabled when crouching
        /// </summary>
        [SerializeField]
        private Collider2D m_CrouchDisableCollider;

        /// <summary>
        /// Radius of the overlap circle to determine if grounded
        /// </summary>
        private const float k_GroundedRadius = .2f;

        /// <summary>
        /// Whether or not the player is grounded.
        /// </summary>
        private bool m_Grounded;

        /// <summary>
        /// Radius of the overlap circle to determine if the player can stand up
        /// </summary>
        private const float k_CeilingRadius = .2f;

        private Rigidbody2D m_Rigidbody2D;

        /// <summary>
        /// For determining which way the player is currently facing.
        /// </summary>
        private bool m_FacingRight = true;

        private Vector3 m_Velocity = Vector3.zero;

        [Header("Events")]
        [Space]

        public UnityEvent OnLandEvent;

        [System.Serializable]
        public class BoolEvent : UnityEvent<bool> { }

        public BoolEvent OnCrouchEvent;
        private bool m_wasCrouching = false;

        /// <summary>
        /// For determining which way the player is currently facing.
        /// </summary>
        public bool FacingRight => m_FacingRight;

        public void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();

            if (OnLandEvent == null)
            {
                OnLandEvent = new UnityEvent();
            }

            if (OnCrouchEvent == null)
            {
                OnCrouchEvent = new BoolEvent();
            }
        }

        public void FixedUpdate()
        {
            bool wasGrounded = m_Grounded;
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    m_Grounded = true;
                    if (!wasGrounded)
                    {
                        OnLandEvent.Invoke();
                    }
                }
            }
        }

        public void Move(float move, bool crouch, bool jump)
        {
            // If crouching, check to see if the character can stand up
            if (!crouch)
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {

                // If crouching
                if (crouch)
                {
                    if (!m_wasCrouching)
                    {
                        m_wasCrouching = true;
                        OnCrouchEvent.Invoke(true);
                    }

                    // Reduce the speed by the crouchSpeed multiplier
                    move *= m_CrouchSpeed;

                    // Disable one of the colliders when crouching
                    if (m_CrouchDisableCollider != null)
                    {
                        m_CrouchDisableCollider.enabled = false;
                    }
                }
                else
                {
                    // Enable the collider when not crouching
                    if (m_CrouchDisableCollider != null)
                    {
                        m_CrouchDisableCollider.enabled = true;
                    }

                    if (m_wasCrouching)
                    {
                        m_wasCrouching = false;
                        OnCrouchEvent.Invoke(false);
                    }
                }

                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                // And then smoothing it out and applying it to the character
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump)
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            }
        }

        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;
        }
    }
}
