using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookUI : MonoBehaviour
{
    [Header("Book Panel")]
    public GameObject bookPanel;

    [Header("Player Reference")]
    public PlayerController player;

    [Header("Level & XP")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public TextMeshProUGUI xpText;

    [Header("Stats Texts")]
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI agilityText;
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI swaggerText;
    public TextMeshProUGUI luckText;

    [Header("Stat Points")]
    public TextMeshProUGUI statPointsText;

    private void Start()
    {
        if (bookPanel != null)
            bookPanel.SetActive(false);

        // Find player automatically if not assigned
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            player.OnStatsChanged += UpdateUI;   // Subscribe to event
            Debug.Log("BookUI successfully subscribed to Player events");
        }
        else
        {
            Debug.LogError("PlayerController not found! BookUI cannot update.");
        }

        UpdateUI(); // Initial update
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnStatsChanged -= UpdateUI;   // Unsubscribe to prevent memory leaks
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (bookPanel != null)
            {
                bool isOpen = !bookPanel.activeSelf;
                bookPanel.SetActive(isOpen);

                if (isOpen)
                    UpdateUI();        // Refresh when opening
            }
        }
    }

    public void UpdateUI()
    {
        if (player == null) return;

        // Level
        if (levelText != null)
            levelText.text = $"Level {player.level}";

        // XP Bar & Text
        if (xpSlider != null)
        {
            xpSlider.maxValue = player.xpToNextLevel;
            xpSlider.value = player.currentXP;
        }
        if (xpText != null)
            xpText.text = $"{player.currentXP} / {player.xpToNextLevel}";

        // Stats with bonuses
        if (powerText != null)
            powerText.text = $"{player.power} Power";

        if (defenseText != null)
            defenseText.text = $"{player.defense} Defense";

        if (vitalityText != null)
            vitalityText.text = $"{player.vitality} Vitality";

        if (agilityText != null)
            agilityText.text = $"{player.agility} Agility";

        if (wisdomText != null)
            wisdomText.text = $"{player.wisdom} Wisdom";

        if (swaggerText != null) swaggerText.text = $"{player.swagger} Swagger";
        if (luckText != null) luckText.text = $"{player.luck} Luck";

        // Stat Points
        if (statPointsText != null)
            statPointsText.text = $"Stat Points: <color=#00FF00>{player.statPoints}</color>";
    }

    // Button methods
    public void AddToPower() => player?.AddPointToStat("power");
    public void AddToDefense() => player?.AddPointToStat("defense");
    public void AddToVitality() => player?.AddPointToStat("vitality");
    public void AddToAgility() => player?.AddPointToStat("agility");
    public void AddToWisdom() => player?.AddPointToStat("wisdom");
    public void AddToSwagger() => player?.AddPointToStat("swagger");
    public void AddToLuck() => player?.AddPointToStat("luck");
}