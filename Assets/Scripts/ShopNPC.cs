using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [Header("UI Referens")]
    [SerializeField] private GameObject shopMenuUI; // Dra in din Shop-meny här i Inspector

    [Header("Inställningar")]
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Knappen för att öppna

    private bool isPlayerInRange = false;

    void Update()
    {
        // Om spelaren är i närheten och trycker på interact-knappen
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        // Starta/stäng shop-menyn beroende på om den redan är öppen
        bool isActive = !shopMenuUI.activeSelf;
        shopMenuUI.SetActive(isActive);

        // Valfritt: Pausa spelet när shopen är öppen, eller frigör muspeparen
        if (isActive)
        {
            Time.timeScale = 0f; // Pausar spelet (valfritt)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f; // Startar spelet igen
            // Återställ musen om det behövs för ditt spel
        }
    }

    // Känner av när spelaren går in i NPC:ns område
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Här kan du också aktivera en "Tryck E för att handla"-text i UI:t
        }
    }

    // Känner av när spelaren lämnar området
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            shopMenuUI.SetActive(false); // Stäng shopen automatiskt om de går iväg
            Time.timeScale = 1f;
        }
    }
}