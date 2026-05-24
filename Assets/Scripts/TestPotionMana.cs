using UnityEngine;

public class TestPotionMana : MonoBehaviour
{
    public PlayerController playerController;

    public float manaPotionAmount = 25f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            UseManaPotion();
        }
    }

    void UseManaPotion()
    {
        playerController.ChangeMana(manaPotionAmount);
    }
}