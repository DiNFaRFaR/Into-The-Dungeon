using UnityEngine;
using UnityEngine.Tilemaps;

public class SwitchTileChanger : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase newTile;
    public Vector3Int[] tilesToChange;

    private bool playerInRange;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ChangeTiles();
        }
    }

    void ChangeTiles()
    {
        foreach (Vector3Int position in tilesToChange)
        {
            tilemap.SetTile(position, newTile);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
