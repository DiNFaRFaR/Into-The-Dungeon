using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public bool isPossessed = false;

    [Header("Health Settings")]
    public int currentHealth;
    public int maxHealth = 100;
    public Slider healthSlider;
    public TMP_Text healthText;
    public float changeDuration = 0.5f;

    [Header("Mana Settings")]
    public float currentMana;
    public float maxMana = 100f;
    public float regenRate = 10f;
    public Slider manaSlider;
    public TMP_Text manaText;
    public float manaChangeDuration = 0.3f;

    [Header("Abilities")]

    // Heal Ability
    public int healAmount = 25;
    public float healCooldown = 30f;
    private float healCooldownTimer;

    // Invincibility Ability
    public bool isInvincible = false;
    public float invincibleDuration = 5f;
    public float invincibleCooldown = 30f;

    private float invincibleCooldownTimer;
    private bool invincibleActive;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject _Spell1prefab;
    [SerializeField] private Transform _Spell1Offset;
    [SerializeField] private float _Spell1Speed = 15f;
    [SerializeField] private float _timeBetweenShots = 0.3f;
    [SerializeField] private float _lifeTime = 2.0f;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private float _shootVolume = 1f;

    [Header("Damage Flash")]
    public SpriteRenderer playerSprite;
    public Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
    public float flashDuration = 0.18f;

    [Header("=== Player Stats & Leveling ===")]
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] _footstepSounds;
    [SerializeField] private float _footstepInterval = 0.4f;
    [SerializeField] private float _footstepVolume = 2f;

    private float _nextFootstepTime;

    // Base Stats
    public int power = 10;
    public int defense = 10;
    public int vitality = 10;
    public int agility = 10;
    public int wisdom = 10;
    public int swagger = 10;
    public int luck = 10;

    public int statPoints = 0;

    public event System.Action OnStatsChanged;
    public event System.Action OnLevelUp;

    // ====================== EFFECTIVE STAT VALUES ======================
    public float GetDamageMultiplier() => 1f + (power - 10) * 0.10f;
    public float GetDamageReduction() => 1f - (defense - 10) * 0.10f;
    public float GetSpeedMultiplier() => 1f + (agility - 10) * 0.10f;
    public float GetMaxHealthMultiplier() => 1f + (vitality - 10) * 0.10f;
    public float GetMaxManaMultiplier() => 1f + (wisdom - 10) * 0.10f;

    public int GetMaxHealth() => Mathf.RoundToInt(maxHealth * GetMaxHealthMultiplier());
    public int GetMaxMana() => Mathf.RoundToInt(maxMana * GetMaxManaMultiplier());

    public void AddPointToStat(string statName)
    {
        if (statPoints <= 0) return;

        switch (statName.ToLower())
        {
            case "power": power++; break;
            case "defense": defense++; break;
            case "vitality": vitality++; break;
            case "agility": agility++; break;
            case "wisdom": wisdom++; break;
            case "swagger": swagger++; break;
            case "luck": luck++; break;
        }

        statPoints--;

        if (statName.ToLower() == "vitality")
        {
            maxHealth = GetMaxHealth();
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        if (statName.ToLower() == "wisdom")
        {
            maxMana = GetMaxMana();
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        OnStatsChanged?.Invoke();
    }

    // Private
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 input;
    private Vector2 velocity;
    private Vector2 lastMoveDir = Vector2.down;
    private float nextDirectionChange = 0f;
    private Coroutine healthRoutine;
    private Coroutine flashRoutine;
    private Coroutine manaRoutine;
    private float _lastShotTime;
    private bool _isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;

        maxHealth = GetMaxHealth();
        maxMana = GetMaxMana();

        currentHealth = maxHealth;
        currentMana = maxMana;

        InitializeUI();
        OnStatsChanged?.Invoke();
    }

    void InitializeUI()
    {
        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;

        UpdateHealthUI();
        UpdateManaUI();

        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (_isDead) return;

        if (isPossessed)
        {
            if (Time.time >= nextDirectionChange)
            {
                input = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                nextDirectionChange = Time.time + Random.Range(0.08f, 0.25f);
            }
        }
        else
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            input = input.normalized;
        }

        // ====================== HEAL ABILITY ======================

        if (healCooldownTimer > 0)
            healCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && healCooldownTimer <= 0)
        {
            Changehealth(healAmount);

            Debug.Log("Heal Ability Used!");

            healCooldownTimer = healCooldown;
        }

        // ====================== INVINCIBILITY ABILITY ======================

        if (invincibleCooldownTimer > 0)
            invincibleCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E) && invincibleCooldownTimer <= 0 && !invincibleActive)
        {
            StartCoroutine(InvincibilityRoutine());
        }

        RegenerateMana();
        HandleShooting();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        float currentSpeed = speed * GetSpeedMultiplier();

        velocity = Vector2.Lerp(rb.linearVelocity, input * currentSpeed, 0.2f);
        rb.linearVelocity = velocity;

        bool isMoving = rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                lastMoveDir = new Vector2(Mathf.Sign(input.x), 0);
            else
                lastMoveDir = new Vector2(0, Mathf.Sign(input.y));
        }

        animator.SetFloat("MoveX", lastMoveDir.x);
        animator.SetFloat("MoveY", lastMoveDir.y);
    }

    // ====================== XP & LEVELING ======================

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel && level < 99)
        {
            LevelUp();
        }

        OnStatsChanged?.Invoke();
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;

        level++;
        statPoints += 1;

        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.28f);

        OnLevelUp?.Invoke();
        OnStatsChanged?.Invoke();

        Debug.Log($"Level Up! Now Level {level}");
    }

    // ====================== HEALTH ======================

    public void Changehealth(int amount)
    {
        // Block damage if invincible
        if (isInvincible && amount < 0)
            return;

        int oldHealth = currentHealth;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (amount < 0 && currentHealth < oldHealth)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRed());
        }

        if (healthRoutine != null)
            StopCoroutine(healthRoutine);

        healthRoutine = StartCoroutine(AnimateHealth(oldHealth, currentHealth));

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator AnimateHealth(int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < changeDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / changeDuration;
            float currentVal = Mathf.Lerp(from, to, t);

            if (healthSlider != null)
                healthSlider.value = currentVal;

            if (healthText != null)
                healthText.text = Mathf.RoundToInt(currentVal) + " / " + maxHealth;

            yield return null;
        }

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    private IEnumerator FlashRed()
    {
        if (playerSprite == null)
            yield break;

        Color originalColor = playerSprite.color;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / flashDuration;
            float pingPong = Mathf.PingPong(t * 2.2f, 1f);

            playerSprite.color = Color.Lerp(originalColor, flashColor, pingPong);

            yield return null;
        }

        playerSprite.color = originalColor;
    }

    // ====================== INVINCIBILITY ======================

    private IEnumerator InvincibilityRoutine()
    {
        invincibleActive = true;
        isInvincible = true;

        Debug.Log("Invincibility Activated!");

        yield return new WaitForSeconds(invincibleDuration);

        isInvincible = false;
        invincibleActive = false;

        Debug.Log("Invincibility Ended!");

        invincibleCooldownTimer = invincibleCooldown;
    }

    // ====================== MANA ======================

    void RegenerateMana()
    {
        if (currentMana < maxMana)
            ChangeMana(regenRate * Time.deltaTime);
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            ChangeMana(-amount);
            return true;
        }

        return false;
    }

    public void ChangeMana(float amount)
    {
        float oldMana = currentMana;

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        if (manaRoutine != null)
            StopCoroutine(manaRoutine);

        manaRoutine = StartCoroutine(AnimateMana(oldMana, currentMana));
    }

    private IEnumerator AnimateMana(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < manaChangeDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / manaChangeDuration;
            float val = Mathf.Lerp(from, to, t);

            if (manaSlider != null)
                manaSlider.value = val;

            if (manaText != null)
                manaText.text = Mathf.RoundToInt(val) + " / " + maxMana;

            yield return null;
        }

        UpdateManaUI();
    }

    private void UpdateManaUI()
    {
        if (manaSlider != null)
            manaSlider.value = currentMana;

        if (manaText != null)
            manaText.text = Mathf.RoundToInt(currentMana) + " / " + maxMana;
    }

    // ====================== SHOOTING ======================

    private void HandleShooting()
    {
        if (isPossessed || _Spell1Offset == null)
            return;

        RotateTowardsMouse();

        if (Input.GetMouseButton(0))
        {
            if (Time.time >= _lastShotTime + _timeBetweenShots)
            {
                // NEW: Only shoot if the player has at least 2 mana
                if (currentMana >= 5f)
                {
                    ShootSpell1();

                    // NEW: Deduct 2 mana and update the UI
                    currentMana -= 5f;
                    currentMana = Mathf.Max(currentMana, 0f);
                    UpdateManaUI();

                    _lastShotTime = Time.time;
                }
                else
                {
                    Debug.Log("Out of mana!");
                }
            }
        }
    }

    private void RotateTowardsMouse()
    {
        if (_Spell1Offset == null)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = new Vector2(
            mousePos.x - _Spell1Offset.position.x,
            mousePos.y - _Spell1Offset.position.y
        );

        _Spell1Offset.right = direction;
    }

    private void ShootSpell1()
    {
        if (_Spell1prefab == null || _Spell1Offset == null)
            return;

        GameObject spell1 = Instantiate(
            _Spell1prefab,
            _Spell1Offset.position,
            _Spell1Offset.rotation
        );

        Rigidbody2D rbSpell = spell1.GetComponent<Rigidbody2D>();

        if (rbSpell != null)
            rbSpell.linearVelocity = _Spell1Offset.right * _Spell1Speed;

        if (_shootSound != null)
            AudioSource.PlayClipAtPoint(
                _shootSound,
                _Spell1Offset.position,
                _shootVolume
            );

        Destroy(spell1, _lifeTime);
    }

    // ====================== FOOTSTEPS ======================

    private void HandleFootsteps()
    {
        bool isMoving = input != Vector2.zero;

        if (isMoving && Time.time >= _nextFootstepTime && _footstepSounds.Length > 0)
        {
            AudioClip clip = _footstepSounds[
                Random.Range(0, _footstepSounds.Length)
            ];

            AudioSource.PlayClipAtPoint(
                clip,
                transform.position,
                _footstepVolume
            );

            _nextFootstepTime = Time.time + _footstepInterval;
        }
    }

    // ====================== DEATH ======================

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        rb.linearVelocity = Vector2.zero;
        input = Vector2.zero;
        animator.SetBool("IsMoving", false);

        if (DeathScreenManager.Instance != null)
            DeathScreenManager.Instance.ShowDeathScreen();
        else
            Debug.LogWarning("DeathScreenManager not found in scene!");
    }
}