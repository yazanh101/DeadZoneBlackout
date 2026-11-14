
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float smoothTime = 0.2f;

    [Header("Map Size")]
    public float mapWidth = 50f;
    public float mapHeight = 30f;

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player == null || cam == null)
            return;

        // Follow the player's position
        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        // Keep the camera inside the map
        if (cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float maxX = Mathf.Max(
                0f, mapWidth / 2f - halfWidth
            );

            float maxY = Mathf.Max(
                0f, mapHeight / 2f - halfHeight
            );

            targetPosition.x = Mathf.Clamp(
                targetPosition.x, -maxX, maxX
            );

            targetPosition.y = Mathf.Clamp(
                targetPosition.y, -maxY, maxY
            );
        }

        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
