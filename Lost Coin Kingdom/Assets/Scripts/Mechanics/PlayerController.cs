using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Main player controller script.
    /// Handles movement, jumping, double jumping, and wall jumping.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // player movement speed
        public float maxSpeed = 7;

        // upward force for normal jump
        public float jumpTakeOffSpeed = 7;

        // how many jumps the player is allowed
        // 2 = normal jump + double jump
        public int maxJumps = 2;
        private int jumpCount = 0;

        // wall jump settings
        public float wallJumpHorizontalSpeed = 10f;
        public float wallJumpTakeOffSpeed = 7f;

        // layer that contains the level walls
        public LayerMask wallLayer;

        // tracks if player is touching a wall
        private bool isTouchingWall;

        // tells which side the wall is on
        // -1 = wall on left, 1 = wall on right
        private int wallDirection;

        public JumpState jumpState = JumpState.Grounded;

        private bool stopJump;

        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;

        public bool controlEnabled = true;

        bool jump;
        Vector2 move;

        SpriteRenderer spriteRenderer;
        internal Animator animator;

        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // get player movement inputs
            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");

            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                // read left / right movement input
                move.x = m_MoveAction.ReadValue<Vector2>().x;

                // check if player is touching a wall
                CheckWall();

                // when player presses jump
                if (m_JumpAction.WasPressedThisFrame())
                {
                    // if player is in air and touching wall → wall jump
                    if (!IsGrounded && isTouchingWall)
                    {
                        velocity.y = wallJumpTakeOffSpeed * model.jumpModifier;

                        // push player away from the wall
                        velocity.x = -wallDirection * wallJumpHorizontalSpeed;

                        animator.SetTrigger("WallJump");

                        jumpState = JumpState.InFlight;
                        jumpCount = 1;
                    }

                    // otherwise do normal jump or double jump
                    else if (jumpCount < maxJumps)
                    {
                        jumpCount++;
                        jumpState = JumpState.PrepareToJump;

                        // play double jump animation on second jump
                        if (jumpCount == 2)
                        {
                            animator.SetTrigger("doubleJump");
                        }
                    }
                }
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();
            base.Update();
        }

        // controls jump state transitions
        void UpdateJumpState()
        {
            jump = false;

            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;

                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;

                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;

                case JumpState.Landed:
                    // reset jumps when player lands
                    jumpCount = 0;
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        // checks if player is touching a wall on left or right
        void CheckWall()
        {
            isTouchingWall = false;
            wallDirection = 0;

            // shoot ray to the left
            RaycastHit2D hitLeft =
                Physics2D.Raycast(transform.position, Vector2.left, 0.8f, wallLayer);

            // shoot ray to the right
            RaycastHit2D hitRight =
                Physics2D.Raycast(transform.position, Vector2.right, 0.8f, wallLayer);

            if (hitLeft.collider != null)
            {
                isTouchingWall = true;
                wallDirection = -1;
            }
            else if (hitRight.collider != null)
            {
                isTouchingWall = true;
                wallDirection = 1;
            }
        }

        protected override void ComputeVelocity()
        {
            // apply upward force when jump starts
            if (jump)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;

                // shorten jump if player releases button early
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            // flip sprite when moving left / right
            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            // update animator values
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // apply horizontal movement
            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}