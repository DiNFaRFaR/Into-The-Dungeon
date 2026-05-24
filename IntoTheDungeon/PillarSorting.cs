using UnityEngine;
using UnityEngine.Tilemaps;

public class PillarSorting : MonoBehaviour
{
    private Transform player;
    private TilemapRenderer tilemapRenderer;
    [SerializeField] private float pillarBaseY;
    [SerializeField] private float baseYOffset = 0f;


    void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        pillarBaseY = transform.position.y + baseYOffset;
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.y < pillarBaseY)
            tilemapRenderer.sortingLayerName = "Default";
        else
            tilemapRenderer.sortingLayerName = "Foreground";
    }
}