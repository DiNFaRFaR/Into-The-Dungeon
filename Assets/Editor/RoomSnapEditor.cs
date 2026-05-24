using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomDoorPoints))]
public class RoomSnapEditor : Editor
{
    // How close (world units) a door must be to snap
    const float SNAP_THRESHOLD = 1.5f;

    void OnSceneGUI()
    {
        RoomDoorPoints room = (RoomDoorPoints)target;

        // Only act while the user is dragging
        if (Event.current.type != EventType.MouseDrag) return;

        TrySnapToNearestDoor(room);
    }

    void TrySnapToNearestDoor(RoomDoorPoints draggedRoom)
    {
        // Collect all OTHER rooms in the scene
        RoomDoorPoints[] allRooms = FindObjectsByType<RoomDoorPoints>(FindObjectsSortMode.None);

        Vector2 bestOffset = Vector2.zero;
        float bestDist = SNAP_THRESHOLD;
        bool found = false;

        List<Vector2> draggedDoors = draggedRoom.GetWorldDoorPoints();

        foreach (var otherRoom in allRooms)
        {
            if (otherRoom == draggedRoom) continue;

            foreach (var myDoor in draggedDoors)
            {
                foreach (var theirDoor in otherRoom.GetWorldDoorPoints())
                {
                    float dist = Vector2.Distance(myDoor, theirDoor);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestOffset = theirDoor - myDoor;
                        found = true;
                    }
                }
            }
        }

        if (found)
        {
            Undo.RecordObject(draggedRoom.transform, "Snap Room");
            draggedRoom.transform.position += (Vector3)bestOffset;
        }
    }
}