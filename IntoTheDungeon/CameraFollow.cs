using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           // Drag your Player here
    public Vector3 offset = new Vector3(0, 0, -10); // Z offset for 2D

    [Header("Smooth Settings")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("Look Ahead")]
    public bool useLookAhead = true;
    public float lookAheadAmount = 2f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Optional: look ahead in movement direction
        if (useLookAhead && target.TryGetComponent<Rigidbody2D>(out var rb))
        {
            desiredPosition += (Vector3)rb.linearVelocity * lookAheadAmount * 0.1f;
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothSpeed
        );

        transform.position = smoothedPosition;
    }

    // Auto-assign player if not set
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }
}