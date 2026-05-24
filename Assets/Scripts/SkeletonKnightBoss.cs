using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonKnightBoss : MonoBehaviour
{
    [Header("Boss Info")]
    public string bossName = "Skeleton Barbarian";
    [SerializeField] public int maxHealth = 1200;
    public int currentHealth;

    [Header("Phases")]
    [SerializeField] private float phase2Threshold = 0.66f;
    [SerializeField] private float phase3Threshold = 0.33f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float stopDistance = 2.5f;

    [Header("Damage Values")]
    [SerializeField] private int slashDamage = 40;
    [SerializeField] private int chargeDamage = 55;
    [SerializeField] private int swordThrowDamage = 35;

    [Header("Attack Ranges")]
    [SerializeField] private float meleeRange = 3f;

    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeed = 13f;
    [SerializeField] private float chargeDuration = 0.9f;

    [Header("Sword Throw")]
    [SerializeField] private GameObject thrownSwordPrefab;
    [SerializeField] private Transform swordSpawnPoint;
    [SerializeField] private float swordThrowSpeed = 15f;

    [Header("Dragon Summoning")]
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private int dragonsPerSummon = 3;
    [SerializeField] private float summonRadius = 4f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private BossHealthUI bossUI;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isCharging = false;
    private bool hasBeenTriggered = false;
    private int currentPhase = 1;
    private string lastAttack = "";

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player")?.transform;

        // Automatically find the UI if it wasn't dragged into the slot manually
        if (bossUI == null)
            bossUI = FindObjectOfType<BossHealthUI>();
    }

    void Start()
    {
        // We do NOT start the loop here anymore. The trigger will handle it.

        // Ensure the health bar slider draws at 100% on start frame
        if (bossUI != null)
            bossUI.Initialize(this, bossName);
    }

    void Update()
    {
        if (isDead || player == null || !hasBeenTriggered) return;

        UpdatePhase();

        if (!isCharging)
        {
            HandleFlip();

            float distance = Vector2.Distance(transform.position, player.position);
            if (distance > stopDistance)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void ActivateBoss()
    {
        if (hasBeenTriggered) return;

        hasBeenTriggered = true;

        // Double check UI initialization right at activation moment
        if (bossUI != null)
            bossUI.Initialize(this, bossName);

        StartCoroutine(BossLoop());
        Debug.Log("THE SKELETON BARBARIAN AWAKES!");
    }

    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f);
        while (!isDead)
        {
            PerformRandomAttack();
            yield return new WaitForSeconds(3f);
        }
    }

    private void UpdatePhase()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        if (healthPercent <= phase3Threshold && currentPhase != 3)
        {
            currentPhase = 3;
            Debug.Log("PHASE 3 ACTIVATED");
            AttackSummon(); // Force immediate phase transition summon!
        }
        else if (healthPercent <= phase2Threshold && currentPhase != 2)
        {
            currentPhase = 2;
            Debug.Log("PHASE 2 ACTIVATED");
        }
    }

    private void PerformRandomAttack()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        List<string> possibleAttacks = new List<string>();

        if (distance <= meleeRange)
        {
            possibleAttacks.Add("Slash");
        }
        else
        {
            possibleAttacks.Add("Charge");
            if (currentPhase >= 2) possibleAttacks.Add("Throw");
        }

        if (currentPhase == 3)
        {
            possibleAttacks.Add("Summon");
        }

        if (possibleAttacks.Count > 1 && possibleAttacks.Contains(lastAttack))
        {
            possibleAttacks.Remove(lastAttack);
        }

        string chosen = possibleAttacks[Random.Range(0, possibleAttacks.Count)];
        lastAttack = chosen;

        switch (chosen)
        {
            case "Slash": AttackSlash(); break;
            case "Charge": AttackCharge(); break;
            case "Summon": AttackSummon(); break;
            case "Throw": AttackThrow(); break;
        }
    }

    private void AttackSlash()
    {
        animator.SetTrigger("Slash");
        Debug.Log("Slash");
    }

    private void AttackCharge()
    {
        animator.SetTrigger("Charge");
        Debug.Log("Charge Started");
        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        Vector2 direction = (player.position - transform.position).normalized;
        float timer = chargeDuration;

        while (timer > 0f && !isDead)
        {
            rb.linearVelocity = direction * chargeSpeed;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isCharging = false;
        Debug.Log("Charge Finished");
    }

    private void AttackSummon()
    {
        animator.SetTrigger("Summon");
        Debug.Log("Summon");
        StartCoroutine(SummonRoutine());
    }

    private IEnumerator SummonRoutine()
    {
        for (int i = 0; i < dragonsPerSummon; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector2 pos = (Vector2)transform.position +
                new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * summonRadius;

            Instantiate(dragonPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void AttackThrow()
    {
        if (thrownSwordPrefab == null || swordSpawnPoint == null) return;

        animator.SetTrigger("ThrowSword");

        Vector2 direction = (player.position - swordSpawnPoint.position).normalized;
        GameObject sword = Instantiate(thrownSwordPrefab, swordSpawnPoint.position, Quaternion.identity);

        Rigidbody2D rbSword = sword.GetComponent<Rigidbody2D>();
        if (rbSword != null)
            rbSword.linearVelocity = direction * swordThrowSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sword.transform.rotation = Quaternion.Euler(0, 0, angle);

        Collider2D bossCol = GetComponent<Collider2D>();
        Collider2D swordCol = sword.GetComponent<Collider2D>();
        if (bossCol && swordCol)
            Physics2D.IgnoreCollision(bossCol, swordCol, true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && isCharging)
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
                pc.Changehealth(-chargeDamage);
        }
    }

    public void DealSlashDamage()
    {
        if (Vector2.Distance(transform.position, player.position) <= meleeRange)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.Changehealth(-slashDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage! Current Health: {currentHealth}");

        // Tell the UI to update visually immediately
        if (bossUI != null)
            bossUI.UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Die");

        if (bossUI != null)
            bossUI.gameObject.SetActive(false);
    }

    private void HandleFlip()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}