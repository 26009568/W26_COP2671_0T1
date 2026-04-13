using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        public AudioClip tokenCollectAudio;
        public bool randomAnimationStartTime = false;
        public Sprite[] idleAnimation, collectedAnimation;

        internal Sprite[] sprites = new Sprite[0];
        internal SpriteRenderer _renderer;

        internal int tokenIndex = -1;
        internal TokenController controller;
        internal int frame = 0;
        internal bool collected = false;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            sprites = idleAnimation;

            if (randomAnimationStartTime)
                frame = Random.Range(0, sprites.Length);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                OnPlayerEnter(player);
            }
        }

        void OnPlayerEnter(PlayerController player)
        {
            if (collected) return;

            frame = 0;
            sprites = collectedAnimation;
            collected = true;

            var ev = Schedule<PlayerTokenCollision>();
            ev.token = this;
            ev.player = player;
        }
    }
}