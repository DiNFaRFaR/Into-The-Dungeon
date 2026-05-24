using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [Tooltip("Assign one DoorPoint child per possible direction (N/S/E/W).")]
    public DoorPoint[] allDoors;

    private DoorPoint entranceDoor; // the door we came IN from (stays closed)
    private Tilemap tilemap;

    // Called once after the room is positioned
    public void Initialize(DoorPoint entrance)
    {
        tilemap = GetComponentInChildren<Tilemap>();
        tilemap.CompressBounds();

        entranceDoor = entrance;

        // Close the entrance door (seal the wall we came through)
        if (entranceDoor != null)
            entranceDoor.Close();
    }

    // Doors available to attach new rooms to
    public List<DoorPoint> GetOpenDoors()
    {
        return allDoors
            .Where(d => d != entranceDoor)
            .ToList();
    }

    public bool HasDoor(DoorDirection dir) =>
        allDoors.Any(d => d.direction == dir);

    public DoorPoint GetDoor(DoorDirection dir) =>
        allDoors.FirstOrDefault(d => d.direction == dir);

    // Accurate world-space AABB from the tilemap
    public Bounds GetWorldBounds()
    {
        tilemap.CompressBounds();
        Bounds local = tilemap.localBounds;

        // Convert local min/max corners to world space
        Vector3 worldMin = transform.TransformPoint(local.min);
        Vector3 worldMax = transform.TransformPoint(local.max);

        Bounds world = new Bounds();
        world.SetMinMax(worldMin, worldMax);
        return world;
    }
}