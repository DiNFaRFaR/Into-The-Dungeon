using UnityEngine;

public class ManaPotion : MonoBehaviour
{
    public float manaAmount = 25f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMana mana = other.GetComponent<PlayerMana>();

            if (mana != null)
            {
                mana.ChangeMana(manaAmount);
            }

            Destroy(gameObject);
        }
    }
}