using System.Collections;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    [RequireComponent(typeof(AnimationController), typeof(Collider2D), typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class SpikedSlimeController : MonoBehaviour
    {
        // patrol settings
        public PatrolPath path;
        public float patrolSpeedMultiplier = 0.5f;

        // sound and death
        public AudioClip ouch;
        public float destroyDelay = 1.2f;

        // attack settings
        public float attackRange = 2.5f;
        public float attackCooldown = 1.0f;

        // hit area for the slime attack
        public Transform attackPoint;
        public float attackHitRange = 1f;

        private float lastAttackTime;
        private bool isAttacking = false;
        private Transform playerTarget;
        private Vector3 attackPointStartLocalPos;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D enemyCollider;
        internal AudioSource audioSourceEnemy;
        internal SpriteRenderer spriteRenderer;
        internal Rigidbody2D rb;
        internal Animator animator;

        bool isDead = false;

        public Bounds Bounds => enemyCollider.bounds;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            enemyCollider = GetComponent<Collider2D>();
            audioSourceEnemy = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            lastAttackTime = Time.time;

            if (attackPoint != null)
            {
                attackPointStartLocalPos = attackPoint.localPosition;
            }
        }

        void Start()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        void Update()
        {
            if (isDead) return;

            UpdateAttackPointDirection();

            if (isAttacking) return;

            if (playerTarget == null)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();

                if (player != null)
                {
                    playerTarget = player.transform;
                }
                else
                {
                    return;
                }
            }

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            // if player is close enough, attack
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackMove();
                return;
            }

            // otherwise patrol
            if (path != null)
            {
                if (mover == null)
                {
                    mover = path.CreateMover(control.maxSpeed * patrolSpeedMultiplier);
                }

                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1f, 1f);
            }
            else
            {
                control.move.x = 0f;
            }
        }

        void UpdateAttackPointDirection()
        {
            if (attackPoint == null || spriteRenderer == null) return;

            if (spriteRenderer.flipX)
            {
                attackPoint.localPosition = new Vector3(
                    -Mathf.Abs(attackPointStartLocalPos.x),
                    attackPointStartLocalPos.y,
                    attackPointStartLocalPos.z
                );
            }
            else
            {
                attackPoint.localPosition = new Vector3(
                    Mathf.Abs(attackPointStartLocalPos.x),
                    attackPointStartLocalPos.y,
                    attackPointStartLocalPos.z
                );
            }
        }

        void AttackMove()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // stop moving while attacking
            control.move = Vector2.zero;

            animator.SetTrigger("Attack");

            // backup reset in case the animation event does not fire
            CancelInvoke(nameof(ForceEndAttack));
            Invoke(nameof(ForceEndAttack), attackCooldown);
        }

        // this gets called at the hit frame of the attack animation
        public void DealDamage()
        {
            if (isDead) return;
            if (playerTarget == null) return;
            if (attackPoint == null) return;

            float distanceToPlayer = Vector2.Distance(attackPoint.position, playerTarget.position);

            if (distanceToPlayer <= attackHitRange)
            {
                Schedule<PlayerDeath>();
            }
        }

        // this should be called at the end of the attack animation
        public void SpikedSlimeEndAbility()
        {
            isAttacking = false;
        }

        // backup reset so the slime does not get stuck after one attack
        void ForceEndAttack()
        {
            isAttacking = false;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                bool stompedFromAbove = false;

                if (collision.contacts.Length > 0)
                {
                    ContactPoint2D contact = collision.contacts[0];

                    if (contact.normal.y < -0.5f)
                    {
                        stompedFromAbove = true;
                    }
                }

                // if player lands on top, slime dies
                if (stompedFromAbove)
                {
                    Die();
                }
                else
                {
                    // touching the slime body still kills the player
                    Schedule<PlayerDeath>();
                }
            }
        }

        void Die()
        {
            if (isDead) return;
            isDead = true;

            control.move = Vector2.zero;
            control.enabled = false;
            enemyCollider.enabled = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            if (audioSourceEnemy != null && ouch != null)
            {
                audioSourceEnemy.PlayOneShot(ouch);
            }

            animator.SetTrigger("Death");
            StartCoroutine(DestroyAfterDeath());
        }

        IEnumerator DestroyAfterDeath()
        {
            yield return new WaitForSeconds(destroyDelay);
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRange);
        }
    }
}