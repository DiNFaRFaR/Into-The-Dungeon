using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Damage")]
    public int impactDamage = 25;

    [Header("Poison Effect")]
    public bool applyPoison = true;
    public int poisonDamagePerTick = 3;
    public float poisonDuration = 5f;
    public float poisonTickRate = 1f;     // Damage every 1 second

    [Header("Effects")]
    [SerializeField] private float lifeTime = 4f;
    [SerializeField] private GameObject impactEffect;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Direct impact damage
                player.Changehealth(-impactDamage);

                // Apply Poison
                if (applyPoison)
                {
                    ApplyPoison(player);
                }
            }
            TriggerImpact();
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            TriggerImpact();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Changehealth(-impactDamage);
                if (applyPoison) ApplyPoison(player);
            }
            TriggerImpact();
        }
    }

    private void ApplyPoison(PlayerController player)
    {
        // Try to get or add Poison component
        PoisonEffect poison = player.gameObject.GetComponent<PoisonEffect>();
        if (poison == null)
        {
            poison = player.gameObject.AddComponent<PoisonEffect>();
        }

        poison.ApplyPoison(poisonDamagePerTick, poisonDuration, poisonTickRate);
    }

    private void TriggerImpact()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}