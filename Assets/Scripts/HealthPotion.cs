using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public PlayerHealth player;

    public int healAmount = 25;

    // Cooldown i sekunder
    public float cooldown = 30f;

    private float cooldownTimer;

    void Update()
    {
        // Räknar ner cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Tryck Q för att använda abilityn
        if (Input.GetKeyDown(KeyCode.H) && cooldownTimer <= 0)
        {
            Use(player);

            // Starta cooldown
            cooldownTimer = cooldown;
        }
    }

    public void Use(PlayerHealth player)
    {
        player.Changehealth(healAmount);

        Debug.Log("Health potion used: +" + healAmount);
    }
}