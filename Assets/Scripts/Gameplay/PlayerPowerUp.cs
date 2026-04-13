using System.Collections;
using UnityEngine;
using Platformer.Mechanics;

public class PlayerPowerUp : MonoBehaviour
{
    public bool isInvincible = false;

    private PlayerController player;
    private float originalJump;

    private Coroutine jumpRoutine;
    private Coroutine invincibleRoutine;

    void Start()
    {
        player = GetComponent<PlayerController>();
        originalJump = player.jumpTakeOffSpeed;
    }

    public void StartJumpBoost(float time, float extraJump)
    {
        if (jumpRoutine != null)
        {
            StopCoroutine(jumpRoutine);
        }

        jumpRoutine = StartCoroutine(JumpBoostRoutine(time, extraJump));
    }

    private IEnumerator JumpBoostRoutine(float time, float extraJump)
    {
        player.jumpTakeOffSpeed = originalJump + extraJump;

        yield return new WaitForSeconds(time);

        player.jumpTakeOffSpeed = originalJump;
        jumpRoutine = null;
    }

    public void StartInvincible(float time)
    {
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
        }

        invincibleRoutine = StartCoroutine(InvincibleRoutine(time));
    }

    private IEnumerator InvincibleRoutine(float time)
    {
        isInvincible = true;

        yield return new WaitForSeconds(time);

        isInvincible = false;
        invincibleRoutine = null;
    }
}