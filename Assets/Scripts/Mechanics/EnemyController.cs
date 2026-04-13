using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    [RequireComponent(typeof(AnimationController), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        public PatrolPath path;
        public AudioClip ouch;
        public bool canMove = false;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal AudioSource _audio;
        SpriteRenderer spriteRenderer;

        bool isDead = false;

        public Bounds Bounds => _collider.bounds;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                PlayerPowerUp power = player.GetComponent<PlayerPowerUp>();

                if (power != null && power.isInvincible)
                {
                    Debug.Log("Player is invincible");
                    return;
                }

                if (collision.contacts.Length > 0 && collision.contacts[0].normal.y < -0.5f)
                {
                    Die();
                }
                else
                {
                    var ev = Schedule<PlayerEnemyCollision>();
                    ev.player = player;
                    ev.enemy = this;
                }
            }
        }

        void Update()
        {
            if (isDead) return;

            if (!canMove)
            {
                control.move.x = 0;
                return;
            }

            if (path != null)
            {
                if (mover == null)
                    mover = path.CreateMover(control.maxSpeed * 0.5f);

                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
            }
        }

        void Die()
        {
            if (isDead) return;
            isDead = true;

            control.move = Vector2.zero;
            control.enabled = false;
            _collider.enabled = false;

            if (_audio != null && ouch != null)
            {
                _audio.PlayOneShot(ouch);
            }

            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Death");
            }

            Destroy(gameObject, 0.8f);
        }
    }
}