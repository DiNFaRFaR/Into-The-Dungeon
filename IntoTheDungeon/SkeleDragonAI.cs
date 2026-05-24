using UnityEngine;
using System.Collections;

public class SkeleDragonAI : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 2.8f;
    [SerializeField] private float chargeDuration = 0.65f;

    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float spreadAngle = 30f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 90;
    private int currentHealth;

    [Header("Rewards")]
    [SerializeField] private int xpGiven = 60;

    [Header("Visuals")]
    [SerializeField] private float damageFlashTime = 0.15f;
    [SerializeField] private float deathDelay = 0.7f;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float attackTimer;
    private bool isChargingAttack = false;
    private bool isDead = false;
    private Color originalColor;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player")?.transform;

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        HandleFlip();

        if (isChargingAttack)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            StartChargeAttack();
        }

        // No need to control animation parameters since there's only Idle
    }

    private void StartChargeAttack()
    {
        isChargingAttack = true;
        rb.linearVelocity = Vector2.zero;
        Invoke(nameof(ShootTripleFireball), chargeDuration);
        attackTimer = attackCooldown;
    }

    private void ShootTripleFireball()
    {
        if (fireballPrefab == null || fireballSpawnPoint == null || player == null)
        {
            isChargingAttack = false;
            return;
        }

        Vector2 mainDirection = (player.position - fireballSpawnPoint.position).normalized;
        float[] angles = { -spreadAngle, 0f, spreadAngle };

        for (int i = 0; i < 3; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angles[i]);
            Vector2 direction = rotation * mainDirection;

            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

            Rigidbody2D rbFire = fireball.GetComponent<Rigidbody2D>();
            if (rbFire != null)
                rbFire.linearVelocity = direction * fireballSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            fireball.transform.rotation = Quaternion.Euler(0, 0, angle);

            Collider2D dragonCol = GetComponent<Collider2D>();
            Collider2D fireCol = fireball.GetComponent<Collider2D>();
            if (dragonCol && fireCol)
                Physics2D.IgnoreCollision(dragonCol, fireCol, true);
        }

        isChargingAttack = false;
    }

    private void HandleFlip()
    {
        if (player.position.x > transform.position.x != facingRight)
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        Color flashColor = new Color(1f, 0.4f, 0.3f, originalColor.a);
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(damageFlashTime);
        if (!isDead) spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();

        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            pc?.AddXP(xpGiven);
        }

        StartCoroutine(DestroyAfterDelay(deathDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}