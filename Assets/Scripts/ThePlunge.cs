using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ThePlunge : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public string displayMessage = "Take the plunge? [E]";
    public string targetScene = "Map 2";

    private bool playerInside = false;

    void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
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
                promptText.text = displayMessage;
                promptText.gameObject.SetActive(true);
            }
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