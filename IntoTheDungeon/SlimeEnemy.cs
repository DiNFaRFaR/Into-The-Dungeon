using UnityEngine;
using System.Collections;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHP = 50f;
    private float currentHP;

    [Header("Combat Settings")]
    public int damageToPlayer = 1;

    [Header("AI Settings")]
    public Transform player;             // Assign player in Inspector
    public float detectionRadius = 3f;   // Circle around slime
    public float activationTime = 2f;    // Time player needs to stay inside
    public float moveSpeed = 3f;         // Dash speed
    public float dashCooldown = 1f;      // Delay between dashes

    private bool isHostile = false;
    private float playerInsideTimer = 0f;
    private float lastDashTime = 0f;

    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Player detection
        if (distance <= detectionRadius)
        {
            playerInsideTimer += Time.deltaTime;

            if (!isHostile && playerInsideTimer >= activationTime)
            {
                isHostile = true;
                Debug.Log("Slime is now hostile!");
                // Optional: trigger hostile animation
            }
        }
        else
        {
            playerInsideTimer = 0f;
        }

        // AI Behavior
        if (isHostile)
        {
            HostileBehavior();
        }
        else
        {
            IdleBehavior();
        }
    }

    void IdleBehavior()
    {
        // Simple bobbing effect
        float bounce = Mathf.Sin(Time.time * 2f) * 0.05f;
        transform.localScale = new Vector3(1f + bounce, 1f + bounce, 1f);
    }

    void HostileBehavior()
    {
        if (Time.time - lastDashTime > dashCooldown)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Move slime toward player instantly (like a dash)
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

            lastDashTime = Time.time;
        }
    }

    // Collision with player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Changehealth(-damageToPlayer);
        }
    }

    // Take damage from spells / AoE
    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // Draw detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}