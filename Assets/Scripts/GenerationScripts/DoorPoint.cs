using UnityEngine;

public class DoorPoint : MonoBehaviour
{
    public DoorDirection direction;

    [Tooltip("The wall GameObject that gets disabled when this door is open.")]
    public GameObject wall;

    [Tooltip("Leave wall OPEN in prefab (wall disabled) — generator closes entrance automatically.")]
    public bool startsOpen = true;

    void Awake()
    {
        // Ensure wall state matches the prefab intent
        if (wall != null)
            wall.SetActive(!startsOpen);
    }

    // Seal this doorway (place wall)
    public void Close()
    {
        if (wall != null)
            wall.SetActive(true);
    }

    // Open this doorway (remove wall)  
    public void Open()
    {
        if (wall != null)
            wall.SetActive(false);
    }
}

