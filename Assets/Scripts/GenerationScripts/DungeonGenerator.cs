using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Rooms")]
    public Room startRoomPrefab;
    public Room[] roomPrefabs;

    [Header("Settings")]
    public int targetRoomCount = 10;
    public int maxAttempts = 4; // attempts per open door before giving up

    // All successfully placed rooms
    private List<Room> placedRooms = new List<Room>();

    // Open door points that still need a room attached
    private List<DoorPoint> openDoors = new List<DoorPoint>();

    void Start() => StartCoroutine(Generate());

    IEnumerator Generate()
    {
        yield return null; // wait one frame so tilemaps are ready

        // --- Place starter room at world origin ---
        Room starter = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        starter.Initialize(null); // no entrance for start room
        RegisterRoom(starter);

        // --- Main generation loop ---
        int safetyCounter = 0;

        while (openDoors.Count > 0 && placedRooms.Count < targetRoomCount)
        {
            if (safetyCounter++ > 10000)
            {
                Debug.LogWarning("DungeonGenerator: safety limit hit, stopping.");
                break;
            }

            // Pick a random open door
            int doorIndex = Random.Range(0, openDoors.Count);
            DoorPoint sourceDoor = openDoors[doorIndex];
            openDoors.RemoveAt(doorIndex);

            TryAttachRoom(sourceDoor);
        }

        Debug.Log($"Dungeon done: {placedRooms.Count} rooms placed.");
    }

    void RegisterRoom(Room room)
    {
        placedRooms.Add(room);

        // Add all of this room's outward-facing doors to the open list
        foreach (DoorPoint door in room.GetOpenDoors())
            openDoors.Add(door);
    }

    void TryAttachRoom(DoorPoint sourceDoor)
    {
        DoorDirection neededDir = sourceDoor.direction.Opposite();

        // Gather prefabs that have a matching door, then shuffle
        List<Room> candidates = roomPrefabs
            .Where(p => p.HasDoor(neededDir))
            .OrderBy(_ => Random.value)
            .ToList();

        for (int attempt = 0; attempt < Mathf.Min(maxAttempts, candidates.Count); attempt++)
        {
            Room prefab = candidates[attempt];
            Room newRoom = Instantiate(prefab);

            // Align: the new room's matching door snaps to sourceDoor
            DoorPoint matchingDoor = newRoom.GetDoor(neededDir);
            Vector3 offset = sourceDoor.transform.position - matchingDoor.transform.position;
            newRoom.transform.position += offset;

            // Wait one frame so the tilemap recalculates world bounds
            // (needed for GetWorldBounds to be accurate after repositioning)
            Bounds newBounds = newRoom.GetWorldBounds();

            if (!Overlaps(newBounds))
            {
                // Success!
                newRoom.Initialize(matchingDoor);
                RegisterRoom(newRoom);
                return;
            }

            // Failure — destroy and try next candidate
            Destroy(newRoom.gameObject);
        }

        // All attempts failed: close the source door visually
        sourceDoor.Close();
    }

    bool Overlaps(Bounds candidate)
    {
        float padding = 0.1f;
        foreach (Room placed in placedRooms)
        {
            Bounds existing = placed.GetWorldBounds();
            existing.Expand(-padding); // small shrink avoids false positives at shared edges

            if (candidate.Intersects(existing))
                return true;
        }
        return false;
    }
}