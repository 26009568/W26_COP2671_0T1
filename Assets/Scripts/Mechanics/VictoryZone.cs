using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class VictoryZone : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D collider)
        {
            var p = collider.gameObject.GetComponent<PlayerController>();

            if (p != null)
            {
                p.controlEnabled = false;

                if (LevelCompletedUI.instance != null)
                    LevelCompletedUI.instance.Show();
            }
        }
    }
}