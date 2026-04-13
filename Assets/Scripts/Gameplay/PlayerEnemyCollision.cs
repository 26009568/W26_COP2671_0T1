using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    public class PlayerEnemyCollision : Simulation.Event<PlayerEnemyCollision>
    {
        public EnemyController enemy;
        public PlayerController player;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            if (player == null)
                return;

            if (enemy == null)
            {
                Die();
                return;
            }

            bool willHurtEnemy = player.Bounds.center.y >= enemy.Bounds.max.y;

            if (willHurtEnemy)
            {
                Health enemyHealth = enemy.GetComponent<Health>();

                if (enemyHealth != null)
                {
                    enemyHealth.Decrement();

                    if (!enemyHealth.IsAlive)
                    {
                        Schedule<EnemyDeath>().enemy = enemy;
                        player.Bounce(2);
                    }
                    else
                    {
                        player.Bounce(7);
                    }
                }
                else
                {
                    Schedule<EnemyDeath>().enemy = enemy;
                    player.Bounce(2);
                }
            }
            else
            {
                Die();
            }
        }

        void Die()
        {
            if (ScoreUI.instance != null)
            {
                ScoreUI.instance.ResetPoints();
            }

            GameOverUI gameOverUI = Object.FindFirstObjectByType<GameOverUI>();
            if (gameOverUI != null && player != null)
            {
                Vector3 popupPosition = player.transform.position + new Vector3(0f, 1.5f, 0f);
                gameOverUI.Show(popupPosition);
            }

            Schedule<PlayerDeath>();
        }
    }
}