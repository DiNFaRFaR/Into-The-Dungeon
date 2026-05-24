using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public string displayMessage = "Tryck [E] för att gå vidare";
    public string lockedMessage = "Kill more enemies to unlock the stairs";
    public string targetScene = "Map 2";

    private bool playerInside = false;
    private int totalEnemies = 0;
    private GameObject enemiesContainer;

    void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        enemiesContainer = GameObject.Find("Enemies");

        if (enemiesContainer != null)
            totalEnemies = enemiesContainer.transform.childCount;
        else
            Debug.LogWarning("Kunde inte hitta GameObject 'Enemies'!");
    }

    bool IsUnlocked()
    {
        if (enemiesContainer == null) return true;

        int remaining = enemiesContainer.transform.childCount;
        float killed = totalEnemies - remaining;

        return totalEnemies == 0 || (killed / (float)totalEnemies) >= 0.8f;
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E) && IsUnlocked())
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (promptText != null)
            {
                promptText.text = IsUnlocked() ? displayMessage : lockedMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && promptText != null)
        {
            promptText.text = IsUnlocked() ? displayMessage : lockedMessage;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}