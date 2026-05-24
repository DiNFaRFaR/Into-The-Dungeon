using UnityEngine;

public class Monster : MonoBehaviour
{
    [Header("XP Reward")]
    public int xpGiven = 35;   // Change this per monster type

    private bool isDead = false;

    // Call this when the monster dies (from Health script, collision, animation event, etc.)
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerController player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            player.AddXP(xpGiven);
            Debug.Log($"✅ Monster gave {xpGiven} XP to player. New XP: {player.currentXP}");
        }
        else
        {
            Debug.LogError("❌ PlayerController NOT FOUND when monster died!");
        }

        // Optional death effects
        Destroy(gameObject, 0.4f);
    }
}