using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour
{
    [Header("Healing Chest Sprites")]
    public Sprite healingClosed;
    public Sprite healingOpening;
    public Sprite healingOpened;

    [Header("Empty Chest Sprites")]
    public Sprite emptyClosed;
    public Sprite emptyOpening;
    public Sprite emptyOpened;

    [Header("Hurtful Chest Sprites")]
    public Sprite hurtfulClosed;
    public Sprite hurtfulOpening;
    public Sprite hurtfulOpened;

    [Header("Settings")]
    public float openingDuration = 0.5f;
    public int damageAmount = 20;

    private enum ChestType { Healing, Empty, Hurtful }
    private ChestType chestType;

    private Sprite closedSprite;
    private Sprite openingSprite;
    private Sprite openedSprite;

    private SpriteRenderer sr;
    private bool isOpen = false;
    private bool playerInRange = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Roll the chest type once on spawn
        float roll = Random.value;
        if (roll < 0.5f)
            chestType = ChestType.Healing;
        else if (roll < 0.75f)
            chestType = ChestType.Empty;
        else
            chestType = ChestType.Hurtful;

        // Assign the correct sprite set
        switch (chestType)
        {
            case ChestType.Healing:
                closedSprite = healingClosed;
                openingSprite = healingOpening;
                openedSprite = healingOpened;
                break;
            case ChestType.Empty:
                closedSprite = emptyClosed;
                openingSprite = emptyOpening;
                openedSprite = emptyOpened;
                break;
            case ChestType.Hurtful:
                closedSprite = hurtfulClosed;
                openingSprite = hurtfulOpening;
                openedSprite = hurtfulOpened;
                break;
        }

        sr.sprite = closedSprite;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            StartCoroutine(OpenChest());
        }
    }

    IEnumerator OpenChest()
    {
        isOpen = true;

        sr.sprite = openingSprite;
        yield return new WaitForSeconds(openingDuration);
        sr.sprite = openedSprite;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) yield break;

        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player == null) yield break;

        switch (chestType)
        {
            case ChestType.Healing:
                int healAmount = Mathf.RoundToInt(player.maxHealth * 0.25f);
                player.Changehealth(healAmount);
                Debug.Log($"Healing chest! Restored {healAmount} HP.");
                break;

            case ChestType.Empty:
                Debug.Log("Empty chest. Nothing happened.");
                break;

            case ChestType.Hurtful:
                player.Changehealth(-damageAmount);
                Debug.Log($"Trap chest! Dealt {damageAmount} damage.");
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}