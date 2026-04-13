using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision>
    {
        public PlayerController player;
        public TokenInstance token;

        public override void Execute()
        {
            if (token == null) return;
            if (!token.collected) return;

            if (token.tokenCollectAudio != null)
            {
                AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);
            }

            if (ScoreUI.instance != null)
            {
                ScoreUI.instance.AddPoint();
            }

            Object.Destroy(token.gameObject);
        }
    }
}