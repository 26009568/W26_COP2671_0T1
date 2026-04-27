using System.Collections;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    // makes sure this enemy always has the required components
    [RequireComponent(typeof(AnimationController), typeof(Collider2D), typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class GolemEnemyController : MonoBehaviour
    {
        // path the enemy walks on
        public PatrolPath path;

        // how fast it moves along the path
        public float patrolSpeedMultiplier = 0.5f;

        // sound played when enemy dies
        public AudioClip ouch;

        // delay before the enemy gets destroyed after dying
        public float destroyDelay = 1.2f;

        // how close player needs to be before enemy attacks
        public float attackRange = 3f;

        // how long enemy waits before attacking again
        public float attackCooldown = 1.0f;

        // where the attack is "centered" (like the fist)
        public Transform attackPoint;

        // how close player must be to actually get hit
        public float attackHitRange = 1f;

        private float lastAttackTime;
        private bool isAttacking = false;

        // reference to the player
        private Transform playerTarget;

        // starting position of attack point (used for flipping left/right)
        private Vector3 attackPointStartLocalPos;

        // internal references to components
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
            // grab all needed components on this enemy
            control = GetComponent<AnimationController>();
            enemyCollider = GetComponent<Collider2D>();
            audioSourceEnemy = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // track time so attack cooldown works
            lastAttackTime = Time.time;

            // store original attack point position
            if (attackPoint != null)
            {
                attackPointStartLocalPos = attackPoint.localPosition;
            }
        }

        void Start()
        {
            // find the player in the scene
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        void Update()
        {
            // if dead, stop everything
            if (isDead) return;

            // update attack direction (left/right)
            UpdateAttackPointDirection();

            // if currently attacking, don't move or do anything else
            if (isAttacking) return;

            // make sure we still have a player reference
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

            // distance from enemy to player
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            // if player is close enough and cooldown is ready → attack
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackMove();
                return;
            }

            // patrol logic (walk back and forth)
            if (path != null)
            {
                if (mover == null)
                {
                    mover = path.CreateMover(control.maxSpeed * patrolSpeedMultiplier);
                }

                // move toward next patrol point
                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1f, 1f);
            }
            else
            {
                // no path → stand still
                control.move.x = 0f;
            }
        }

        void UpdateAttackPointDirection()
        {
            // if missing references, do nothing
            if (attackPoint == null || spriteRenderer == null) return;

            // flip attack point based on which direction enemy is facing
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
            // start attack
            isAttacking = true;

            // reset cooldown timer
            lastAttackTime = Time.time;

            // stop movement while attacking
            control.move = Vector2.zero;

            // play attack animation
            animator.SetTrigger("Attack");
        }

        public void DealDamage()
        {
            // safety checks
            if (isDead) return;
            if (playerTarget == null) return;
            if (attackPoint == null) return;

            // check distance from attack point to player
            float distanceToPlayer = Vector2.Distance(attackPoint.position, playerTarget.position);

            // if close enough → kill player
            if (distanceToPlayer <= attackHitRange)
            {
                Schedule<PlayerDeath>();
            }
        }

        public void GolemEndAbility()
        {
            // called at end of animation → allow attacking again
            isAttacking = false;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                bool stompedFromAbove = false;

                // check if player hit from above
                if (collision.contacts.Length > 0)
                {
                    ContactPoint2D contact = collision.contacts[0];

                    if (contact.normal.y < -0.5f)
                    {
                        stompedFromAbove = true;
                    }
                }

                if (stompedFromAbove)
                {
                    // player jumped on enemy → enemy dies
                    Die();
                }
                else
                {
                    // player touched from side → player dies
                    Schedule<PlayerDeath>();
                }
            }
        }

        void Die()
        {
            if (isDead) return;

            isDead = true;

            // stop all movement
            control.move = Vector2.zero;
            control.enabled = false;
            enemyCollider.enabled = false;

            // freeze physics
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            // play death sound
            if (audioSourceEnemy != null && ouch != null)
            {
                audioSourceEnemy.PlayOneShot(ouch);
            }

            // play death animation
            animator.SetTrigger("Death");

            // wait, then destroy object
            StartCoroutine(DestroyAfterDeath());
        }

        IEnumerator DestroyAfterDeath()
        {
            // wait before removing enemy from scene
            yield return new WaitForSeconds(destroyDelay);

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            // draws attack range in editor for debugging
            if (attackPoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRange);
        }
    }
}