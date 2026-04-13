using UnityEngine;
using Platformer.Mechanics;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

public class GearTrap : MonoBehaviour
{
    public PatrolPath path;
    public float speed = 3f;

    private PatrolPath.Mover mover;
    private Collider2D gearCollider;
    private SpriteRenderer gearSprite;
    private Rigidbody2D rb;

    public bool canMove = false;

    void Awake()
    {
        gearCollider = GetComponent<Collider2D>();
        gearSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        gearSprite.enabled = false;
        gearCollider.enabled = false;
    }

    void Update()
    {
        if (!canMove) return;

        if (path != null)
        {
            if (mover == null)
            {
                mover = path.CreateMover(speed);
            }

            Vector3 newPosition = transform.position;
            newPosition.x = mover.Position.x;
            transform.position = newPosition;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canMove) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            var ev = Schedule<PlayerEnemyCollision>();
            ev.player = player;
            ev.enemy = null;
        }
    }

    public void ShowGear()
    {
        Debug.Log("show gear");
        canMove = true;
        gearSprite.enabled = true;
        gearCollider.enabled = true;
    }

    public void HideGear()
    {
        Debug.Log("hide gear");
        canMove = false;
        gearSprite.enabled = false;
        gearCollider.enabled = false;
    }
}