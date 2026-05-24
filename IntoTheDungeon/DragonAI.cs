using UnityEngine;
using System.Collections;

public class DragonAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float roamSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float roamDistance = 6f;

    [Header("Detection & Fireball Attack")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackCooldown = 2.2f;
    [SerializeField] private float chargeDuration = 0.55f;

    [Header("Fireball Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float fireballSpeed = 9f;
    [SerializeField] private int fireballDamage = 25;

    [Header("Health")]
    [SerializeField] private int maxHealth = 120;
    private int currentHealth;

    [Header("Rewards")]
    [SerializeField] private int xpGiven = 80;

    [Header("Visuals")]
    [SerializeField] private float damageFlashTime = 0.15f;
    [SerializeField] private float deathDelay = 0.8f;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool facingRight = true;

    private Vector2 patrolStartPos;
    private float attackTimer;
    private bool isChargingAttack = false;
    private bool isDead = false;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player")?.transform;

        currentHealth = maxHealth;
        patrolStartPos = transform.position;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // === CHARGING PHASE ===
        if (isChargingAttack)
        {
            rb.linearVelocity *= 0.75f;     // Slow down but keep some movement
        }
        else if (distanceToPlayer <= attackRange && attackTimer <= 0f)
        {
            StartChargeAttack();
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
        UpdateAnimation();          // ← Always update animation (important fix)
    }

    private void StartChargeAttack()
    {
        isChargingAttack = true;
        rb.linearVelocity *= 0.5f;
        Invoke(nameof(ShootFireball), chargeDuration);
        attackTimer = attackCooldown;
    }

    private void ShootFireball()
    {
        if (fireballPrefab == null || fireballSpawnPoint == null) return;

        Vector2 direction = (player.position - fireballSpawnPoint.position).normalized;

        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

        Rigidbody2D rbFire = fireball.GetComponent<Rigidbody2D>();
        if (rbFire != null)
            rbFire.linearVelocity = direction * fireballSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        fireball.transform.rotation = Quaternion.Euler(0, 0, angle);

        Collider2D dragonCol = GetComponent<Collider2D>();
        Collider2D fireCol = fireball.GetComponent<Collider2D>();
        if (dragonCol && fireCol) Physics2D.IgnoreCollision(dragonCol, fireCol, true);

        isChargingAttack = false;
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

        if (Vector2.Distance(transform.position, targetPos) < 0.3f)
        {
            Flip();
            patrolStartPos = transform.position;
        }
    }

    private void HandleFlip(float distanceToPlayer)
    {
        if (isChargingAttack) return;

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

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    // ====================== ANIMATION ======================
    private void UpdateAnimation()
    {
        if (animator == null) return;

        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);

        // If your animator uses a bool instead:
        // animator.SetBool("IsMoving", speed > 0.1f);
    }

    // ================= DAMAGE & DEATH =================
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) Die();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}