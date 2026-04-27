using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    // This event runs when the player dies
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        // gets the shared game data (player, camera, etc.)
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            // get reference to the player
            var player = model.player;

            // only run if player is still alive (prevents double death)
            if (player.health.IsAlive)
            {
                // set player health to dead
                player.health.Die();

                // stop camera from following player (freeze view)
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;

                // disable player movement so they can't control anymore
                player.controlEnabled = false;

                // play "ouch" sound if it exists
                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);

                // trigger hurt animation
                player.animator.SetTrigger("hurt");

                // set animation to dead state
                player.animator.SetBool("dead", true);

                // this used to respawn player after 2 seconds
                // Simulation.Schedule<PlayerSpawn>(2);
            }
        }
    }
}