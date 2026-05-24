using UnityEngine;

public class InvisibilityPotion : MonoBehaviour
{
    public float duration = 5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.StartCoroutine(player.Invisibility(duration));
            }

            Destroy(gameObject);
        }
    }
}