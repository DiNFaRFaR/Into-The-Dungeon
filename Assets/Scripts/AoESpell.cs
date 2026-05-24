using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AoESpell : MonoBehaviour
{
    [Header("Spell Settings")]
    public GameObject aoePrefab;       // Small AoE prefab (circle collider + particle)
    public float totalSpawnTime = 2f;  // Total time to spawn AoEs along mouse
    public int totalSpawns = 50;       // Total AoEs to spawn
    public float aoeLifetime = 2f;     // How long each AoE lasts

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

   public void StartFirewall()
    {
        StartCoroutine(SpawnFirewall());
    }

    private IEnumerator SpawnFirewall()
    {
        float spawnInterval = totalSpawnTime / totalSpawns; // Time between each spawn

        for (int i = 0; i < totalSpawns; i++)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            GameObject instance = Instantiate(aoePrefab, mousePos, Quaternion.identity);
            Destroy(instance, aoeLifetime);

            yield return new WaitForSeconds(spawnInterval);
        }

        Destroy(gameObject); // destroy the controller after spawning
    }
}