using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] private SkeletonKnightBoss boss; // Reference to your boss

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object stepping into the zone is the Player
        if (collision.CompareTag("Player"))
        {
            if (boss != null)
            {
                boss.ActivateBoss(); // Wake him up!
            }

            // Optional: Destroy this trigger so it doesn't run again
            Destroy(gameObject);
        }
    }
}