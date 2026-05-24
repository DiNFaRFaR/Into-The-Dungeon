using UnityEngine;
using System.Collections;

public class SpiderAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float roamSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float roamDistance = 4f;

    [Header("Detection & Attack")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int damage = 15;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackHitRadius = 0.7f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackDuration = 0.4f;

    [Header("Rewards")]
    [SerializeField] private int xpGiven = 35;

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;
    [SerializeField] private float damageFlashTime = 0.15f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 0.6f;

    [Header("Patrol")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckThreshold = 0.1f;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    private Vector2 patrolStartPos;
    private Vector2 patrolTarget;
    private float stuckTimer = 0f;
    private Vector2 lastPosition;

    private float attackTimer;
    private float attackEndTime;
    private bool isPerformingAttack;
    private bool isDead = false;

    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        spriteRenderer = GetComponent<SpriteRenderer>();

        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        patrolStartPos = transform.position;
        lastPosition = transform.position;
        attackTimer = 0f;

        patrolTarget = GetNewPatrolTarget();
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        if (playerHealth != null && playerHealth.isInvisible)
        {
            rb.linearVelocity = Vector2.zero;
            Patrol();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isPerformingAttack)
        {
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= attackEndTime)
            {
                isPerformingAttack = false;
                attackTimer = attackCooldown;
            }
        }
        else if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            StartAttack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        if (!isPerformingAttack)
        {
            Vector2 moveDir = rb.linearVelocity.normalized;

            if (moveDir != Vector2.zero)
            {
                bool shouldFaceRight = moveDir.x > 0;
                if (shouldFaceRight != facingRight) Flip();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                bool shouldFaceRight = player.position.x > transform.position.x;
                if (shouldFaceRight != facingRight) Flip();
            }
        }
    }

    private Vector2 GetNewPatrolTarget()
    {
        float randomX = patrolStartPos.x + Random.Range(-roamDistance, roamDistance);
        float randomY = patrolStartPos.y + Random.Range(-roamDistance, roamDistance);
        return new Vector2(randomX, randomY);
    }

    private void StartAttack()
    {
        isPerformingAttack = true;
        attackEndTime = Time.time + attackDuration;
        rb.linearVelocity = Vector2.zero;

        Invoke(nameof(PerformHit), attackDuration * 0.5f);
    }

    private void PerformHit()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackHitRadius,
            playerLayer
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerController>(out var health))
            {
                health.Changehealth(-damage);

                Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                hit.attachedRigidbody?.AddForce(knockDir * 5f, ForceMode2D.Impulse);
            }
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
    }

    private void Patrol()
    {
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * roamSpeed;

        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
            patrolTarget = GetNewPatrolTarget();

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            if (Vector2.Distance(transform.position, lastPosition) < stuckThreshold)
                patrolTarget = GetNewPatrolTarget();

            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
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

        Color flashColor = new Color(1f, 0.3f, 0.3f, originalColor.a);
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(damageFlashTime);

        if (!isDead)
            spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.AddXP(xpGiven);                    // Change 35 to whatever XP this goblin should give
            Debug.Log($"✅ Goblin died - Gave 35 XP to player!");
        }
        else
        {
            Debug.LogError("❌ PlayerController not found when Goblin died!");
        }

        StartCoroutine(DestroyAfterDelay(deathDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}