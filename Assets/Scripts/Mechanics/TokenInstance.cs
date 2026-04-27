using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    // makes sure this object always has a Collider2D (needed for trigger detection)
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        // sound that plays when the token is collected
        public AudioClip tokenCollectAudio;

        // if true, each token starts its animation at a random frame (makes them look less identical)
        public bool randomAnimationStartTime = false;

        // sprites for idle animation (spinning coin, etc.)
        public Sprite[] idleAnimation;

        // sprites for collected animation (disappearing effect)
        public Sprite[] collectedAnimation;

        // current sprite animation being used (idle or collected)
        internal Sprite[] sprites = new Sprite[0];

        // reference to the sprite renderer on this object
        internal SpriteRenderer _renderer;

        // index of this token (used by controller system if needed)
        internal int tokenIndex = -1;

        // reference to the token controller (if managing multiple tokens)
        internal TokenController controller;

        // current frame of the animation
        internal int frame = 0;

        // tracks if this token has already been collected
        internal bool collected = false;

        void Awake()
        {
            // grab the sprite renderer component
            _renderer = GetComponent<SpriteRenderer>();

            // start with idle animation
            sprites = idleAnimation;

            // optionally randomize starting frame so tokens don't all animate the same
            if (randomAnimationStartTime)
                frame = Random.Range(0, sprites.Length);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // check if the thing entering the trigger is the player
            PlayerController player = other.GetComponentInParent<PlayerController>();

            // if it is the player → handle collection
            if (player != null)
            {
                OnPlayerEnter(player);
            }
        }

        void OnPlayerEnter(PlayerController player)
        {
            // if already collected, do nothing (prevents double collecting)
            if (collected) return;

            // reset animation frame
            frame = 0;

            // switch to collected animation (like disappearing)
            sprites = collectedAnimation;

            // mark this token as collected
            collected = true;

            // trigger the event that handles what happens when player collects a token
            var ev = Schedule<PlayerTokenCollision>();

            ev.token = this;
            ev.player = player;
        }
    }
}