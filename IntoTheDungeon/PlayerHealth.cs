using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public bool isInvisible;
    public int currentHealth;
    public int maxHealth;
    public Slider healthSlider;
    public TMP_Text healthText;           // assign your TMP_Text in Inspector
    public float changeDuration = 0.5f;   // how long the health change animation takes

    [Header("Damage Flash")]
    public SpriteRenderer playerSprite;   // ? DRAG YOUR PLAYER'S SPRITERENDERER HERE
    public Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);  // bright red tint
    public float flashDuration = 0.18f;   // total flash time (feels great at 0.15–0.22)

    private Coroutine healthRoutine;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        healthText.text = currentHealth + " / " + maxHealth;

        // Auto-assign SpriteRenderer if you forgot to drag it
        if (playerSprite == null)
        {
            playerSprite = GetComponent<SpriteRenderer>();
            if (playerSprite == null)
                Debug.LogWarning("PlayerHealth: No SpriteRenderer assigned! Drag it in the Inspector for damage flash to work.", this);
        }
    }

    public void Changehealth(int amount)
    {
        int oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // === DAMAGE FLASH (only when actually losing health) ===
        if (amount < 0 && currentHealth < oldHealth)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRed());
        }

        // === HEALTH BAR ANIMATION ===
        if (healthRoutine != null)
            StopCoroutine(healthRoutine);

        healthRoutine = StartCoroutine(AnimateHealth(oldHealth, currentHealth));

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateHealth(int from, int to)
    {
        float elapsed = 0f;
        while (elapsed < changeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / changeDuration;

            healthSlider.value = Mathf.Lerp(from, to, t);

            int displayHealth = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            healthText.text = displayHealth + " / " + maxHealth;

            yield return null;
        }

        // snap to final value
        healthSlider.value = to;
        healthText.text = to + " / " + maxHealth;
    }

    private IEnumerator FlashRed()
    {
        if (playerSprite == null) yield break;

        Color originalColor = playerSprite.color;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;

            // Ping-pong effect: red ? normal ? red ? normal (very smooth)
            float pingPong = Mathf.PingPong(t * 2.2f, 1f);   // 2.2x makes it feel snappier
            playerSprite.color = Color.Lerp(originalColor, flashColor, pingPong);

            yield return null;
        }

        // guarantee we return to normal color
        playerSprite.color = originalColor;
    }
    public IEnumerator Invisibility(float duration)
    {
        isInvisible = true;

        // visuellt (valfritt men snyggt)
        if (playerSprite != null)
            playerSprite.color = new Color(1f, 1f, 1f, 0.4f);

        yield return new WaitForSeconds(duration);

        isInvisible = false;

        if (playerSprite != null)
            playerSprite.color = Color.white;
    }
}