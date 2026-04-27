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
    public class PlayerController : KinematicObject
    {
        // sound effects for different actions
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // how fast the player moves left/right
        public float maxSpeed = 7;

        // how strong the jump is (vertical force)
        public float jumpTakeOffSpeed = 7;

        // tracks what phase of jumping we are in
        public JumpState jumpState = JumpState.Grounded;

        // used to stop jump early when button released
        private bool stopJump;

        // references to important components on the player
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;

        // allows disabling player movement (used on death/victory)
        public bool controlEnabled = true;

        // particle system for jump dust effect
        public ParticleSystem jumpEffect;

        // internal variables used for movement
        bool jump;
        Vector2 move;

        // visuals and animation
        SpriteRenderer spriteRenderer;
        internal Animator animator;

        // global game model (used for shared values like jump multiplier)
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        // input system actions
        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        // used for collision calculations
        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            // grab references to components attached to the player
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // get movement + jump input actions from Input System
            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");

            // enable those inputs so they actually work
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Update()
        {
            // only allow movement if control is enabled (not dead, not paused, etc.)
            if (controlEnabled)
            {
                // read horizontal movement (left/right)
                move.x = m_MoveAction.ReadValue<Vector2>().x;

                // if grounded and jump button pressed → prepare to jump
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;

                // if jump button released → shorten jump
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                // if controls disabled, stop horizontal movement
                move.x = 0;
            }

            // update jump logic
            UpdateJumpState();

            // run base movement physics
            base.Update();

            // if dead and player presses R → restart level
            if (isDead && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
                );
            }

            // if health reaches 0 → trigger death
            if (health != null && !health.IsAlive)
            {
                Die();
            }
        }

        void UpdateJumpState()
        {
            // reset jump trigger each frame
            jump = false;

            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    // we are starting a jump
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;

                    // play jump particle effect (dust)
                    if (jumpEffect != null)
                        jumpEffect.Play();

                    break;

                case JumpState.Jumping:
                    // once player leaves ground → switch to in-air state
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;

                case JumpState.InFlight:
                    // when player touches ground again → landed
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;

                case JumpState.Landed:
                    // after landing → reset to grounded state
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            // handle jumping force
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }

            // reduce jump height if player releases button early
            else if (stopJump)
            {
                stopJump = false;

                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            // flip sprite depending on direction
            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            // update animations
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // apply horizontal movement
            targetVelocity = move * maxSpeed;
        }

        // tracks if player has already died
        bool isDead = false;

        public void Die()
        {
            if (isDead) return;

            isDead = true;

            // disable control and stop movement
            controlEnabled = false;
            move.x = 0;
            velocity = Vector2.zero;
            targetVelocity = Vector2.zero;

            // show game over UI
            GameOverUI.instance.Show(transform.position);

            // freeze game
            Time.timeScale = 0f;
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