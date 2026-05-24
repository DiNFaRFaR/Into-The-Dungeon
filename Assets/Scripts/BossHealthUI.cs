using UnityEngine;
using UnityEngine.UI;
using TMPro; // Include this if you are using TextMeshPro for the name

public class BossHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText; // Optional: For the boss name

    private SkeletonKnightBoss bossReference;

    // Called by the boss script when it wakes up
    public void Initialize(SkeletonKnightBoss boss, string name)
    {
        bossReference = boss;
        gameObject.SetActive(true); // Show the health bar

        if (bossNameText != null)
        {
            bossNameText.text = name;
        }

        UpdateUI();
    }

    // Updates the slider value based on current health percentage
    public void UpdateUI()
    {
        if (bossReference == null) return;

        // Calculate health as a percentage value between 0f and 1f
        float healthPercent = (float)bossReference.currentHealth / bossReference.maxHealth;

        // Assign to the slider
        healthSlider.value = healthPercent;
    }
}