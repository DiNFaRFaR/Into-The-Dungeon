using UnityEngine;
using System.Collections;

public class GoblinAI : MonoBehaviour
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

    private Rigidbody2D rb;
    private Transform player;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    private Vector2 patrolStartPos;
    private float attackTimer;
    private float attackEndTime;
    private bool isPerformingAttack;
    private bool isDead = false;

    public AudioClip hitSound;
    public float volume = 1f;

    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        player = GameObject.FindWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        patrolStartPos = transform.position;
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        // 👻 INVIIBILITY CHECK (viktig)
        if (playerHealth != null && playerHealth.isInvisible)
        {
            rb.linearVelocity = Vector2.zero;
            Patrol(); // eller return om du vill att den ska stå still
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

        HandleFlip(distanceToPlayer);
    }

    private void HandleFlip(float distanceToPlayer)
    {
        if (isPerformingAttack) return;

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
        float targetX = patrolStartPos.x + (facingRight ? roamDistance : -roamDistance);
        Vector2 targetPos = new Vector2(targetX, patrolStartPos.y);

        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * roamSpeed;

        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            Flip();
            patrolStartPos = transform.position;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // ================= DAMAGE =================

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);


        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
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

        // === GIVE XP TO PLAYER ===
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.AddXP(35);        // Change 35 to desired XP value
                Debug.Log($"✅ Goblin died - Gave 35 XP to player!");
            }
            else
            {
                Debug.LogError("❌ PlayerController not found on Player!");
            }
        }
        else
        {
            Debug.LogError("❌ Player Transform not found!");
        }

        StartCoroutine(DestroyAfterDelay(deathDelay));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRadius);
        }
    }
}