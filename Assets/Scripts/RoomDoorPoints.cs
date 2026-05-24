using System.Collections.Generic;
using UnityEngine;

public class RoomDoorPoints : MonoBehaviour
{
    [Tooltip("Local-space positions of all door points on this room")]
    public List<Vector2> doorPoints = new List<Vector2>();

    // Returns all door positions in world space
    public List<Vector2> GetWorldDoorPoints()
    {
        var result = new List<Vector2>();
        foreach (var p in doorPoints)
            result.Add((Vector2)transform.position + p);
        return result;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var p in doorPoints)
        {
            Vector3 world = transform.position + (Vector3)(p);
            Gizmos.DrawWireSphere(world, 0.2f);
            Gizmos.DrawLine(world + Vector3.up * 0.3f, world - Vector3.up * 0.3f);
            Gizmos.DrawLine(world + Vector3.right * 0.3f, world - Vector3.right * 0.3f);
        }
    }
#endif
}