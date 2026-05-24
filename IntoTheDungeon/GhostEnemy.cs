using UnityEngine;
using System.Collections;

public class GhostEnemy : MonoBehaviour
{
    [Header("Movement & Possession")]
    public float moveSpeed = 4f;
    public float seeRadius = 15f; // NEW: The distance at which the ghost notices the player
    public float possessionRange = 1.5f;
    public float possessionDuration = 3f;
    public int damagePerSecond = 5;
    public float rePossessionGraceTime = 1.2f;

    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Rewards")]
    [SerializeField] private int xpGiven = 45;   // Change this in Inspector

    [Header("Visuals")]
    public float damageFlashTime = 0.15f;

    [Header("Death")]
    public string deathTrigger = "Die";
    public float deathAnimLength = 1.2f;

    // References
    private SpriteRenderer spriteRenderer;
    private Collider2D ghostCollider;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private PlayerController playerController;

    private bool isVisible = false;
    private bool isPossessing = false;
    private bool isDead = false;
    private Color originalColor;
    private Color visibleColor;
    private Transform originalParent;
    private float possessionCooldownEndTime = 0f;
    private int possessionCount = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ghostCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        originalParent = transform.parent;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            visibleColor = originalColor;
            visibleColor.a = 1f;

            Color c = originalColor;
            c.a = 0f;
            spriteRenderer.color = c;
        }

        // Find Player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerController = p.GetComponent<PlayerController>();

            if (playerController == null)
                Debug.LogError("GhostEnemy: PlayerController not found on Player!", this);
        }
        else
        {
            Debug.LogError("GhostEnemy: No Player with tag 'Player' found!");
        }
    }

    void Update()
    {
        if (isDead || player == null || playerController == null) return;

        if (!isPossessing)
        {
            // NEW: Calculate the current distance to the player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // NEW: Check if the player is within the ghost's see radius
            if (distanceToPlayer <= seeRadius)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                if (rb != null)
                    rb.linearVelocity = dir * moveSpeed;

                if (Time.time >= possessionCooldownEndTime && distanceToPlayer <= possessionRange)
                {
                    StartPossession();
                }
            }
            else
            {
                // NEW: Player is out of range, stop moving completely
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void HitByMagic()
    {
        if (isVisible || isDead) return;
        isVisible = true;
        if (spriteRenderer != null) spriteRenderer.color = visibleColor;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        StartCoroutine(DamageFlash());
        if (currentHealth <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        Color flash = new Color(1f, 0.3f, 0.3f, spriteRenderer.color.a);
        spriteRenderer.color = flash;
        yield return new WaitForSeconds(damageFlashTime);
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = isVisible ? visibleColor : originalColor;
    }

    private void StartPossession()
    {
        if (isPossessing || playerController == null || isDead) return;

        Debug.Log($"[Ghost] POSSESSION START #{++possessionCount} | {Time.time:F2}");
        isPossessing = true;

        if (player != null)
            player.localScale = Vector3.one;

        transform.SetParent(player, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (ghostCollider != null) ghostCollider.enabled = false;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0f;
            spriteRenderer.color = c;
        }

        playerController.isPossessed = true;
        StartCoroutine(DamageOverTime());
        StartCoroutine(PossessionTimer());
    }

    private IEnumerator PossessionTimer()
    {
        yield return new WaitForSeconds(possessionDuration);
        if (isPossessing && !isDead)
            EndPossession();
    }

    private IEnumerator DamageOverTime()
    {
        while (isPossessing && playerController != null)
        {
            yield return new WaitForSeconds(1f);
            if (playerController.currentHealth > 0 && player.gameObject.activeInHierarchy)
            {
                playerController.Changehealth(-damagePerSecond);
            }
            else
            {
                EndPossession();
                yield break;
            }
        }
    }

    private void EndPossession()
    {
        if (!isPossessing) return;

        Debug.Log($"[Ghost] POSSESSION END | {Time.time:F2}");
        isPossessing = false;

        if (playerController != null)
            playerController.isPossessed = false;

        Vector3 worldPos = transform.position;
        Quaternion worldRot = transform.rotation;
        Vector3 worldScale = transform.lossyScale;

        transform.SetParent(originalParent, false);
        transform.position = worldPos;
        transform.rotation = worldRot;
        transform.localScale = worldScale;

        if (ghostCollider != null) ghostCollider.enabled = true;

        if (!isDead && spriteRenderer != null)
        {
            isVisible = true;
            spriteRenderer.color = visibleColor;

            if (player != null)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * 3f;
                transform.position = player.position + (Vector3)offset;
            }
        }

        possessionCooldownEndTime = Time.time + rePossessionGraceTime;

        if (player != null)
            player.localScale = Vector3.one;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();

        if (isPossessing)
            EndPossession();

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (ghostCollider != null) ghostCollider.enabled = false;

        // Give XP
        if (playerController != null)
        {
            playerController.AddXP(xpGiven);
            Debug.Log($"✅ Ghost died - Gave {xpGiven} XP to player!");
        }
        else
        {
            Debug.LogError("❌ PlayerController not found when Ghost died!");
        }

        if (animator != null)
            animator.SetTrigger(deathTrigger);

        Destroy(gameObject, deathAnimLength);
    }

    void OnDestroy()
    {
        if (isPossessing && playerController != null)
            playerController.isPossessed = false;
    }

    // NEW: Draws the vision range circle inside Unity Editor Scene view when clicked
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, seeRadius);
    }
}