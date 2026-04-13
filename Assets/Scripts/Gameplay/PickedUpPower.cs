using UnityEngine;
using Platformer.Mechanics;

public class PickedUpPower : MonoBehaviour
{
    public float jumpBoostAmount = 5f;
    public float powerTime = 4f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            PlayerPowerUp power = player.GetComponent<PlayerPowerUp>();

            if (power != null)
            {
                power.StartJumpBoost(powerTime, jumpBoostAmount);
            }

            Destroy(gameObject);
        }
    }
}