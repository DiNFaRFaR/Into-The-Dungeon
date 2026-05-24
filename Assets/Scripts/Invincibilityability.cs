using UnityEngine;
using System.Collections;

public class InvincibilityAbility : MonoBehaviour
{
    public PlayerController player;

    public float duration = 5f;
    public float cooldown = 30f;

    private float cooldownTimer;
    private bool isActive;

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer <= 0 && !isActive)
        {
            StartCoroutine(Activate());
        }
    }

    private IEnumerator Activate()
    {
        isActive = true;
        player.isInvincible = true;

        yield return new WaitForSeconds(duration);

        player.isInvincible = false;
        isActive = false;

        cooldownTimer = cooldown;
    }
}