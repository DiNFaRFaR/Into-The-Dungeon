using UnityEngine;

public class ThrownSword : MonoBehaviour
{
    [Header("Damage")]
    public int impactDamage = 35;

    [Header("Effects")]
    [SerializeField] private float lifeTime = 2.5f;
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
                player.Changehealth(-impactDamage);
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
            }
            TriggerImpact();
        }
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