using System.Collections;
using UnityEngine;
using Platformer.Mechanics;

public class PlateTrigger : MonoBehaviour
{
    public GearTrap gearTrap;
    public float activeTime = 4f;

    private bool isRunning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null && !isRunning)
        {
            StartCoroutine(ActivateTrap());
        }
    }

    private IEnumerator ActivateTrap()
    {
        isRunning = true;

        gearTrap.ShowGear();

        yield return new WaitForSeconds(activeTime);

        gearTrap.HideGear();

        isRunning = false;
    }
}