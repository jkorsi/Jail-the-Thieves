using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Player's transform
    public GameObject map; // Map GameObject

    private Camera cam; // Camera component
    private Vector2 minCameraPos, maxCameraPos; // Calculated minimum and maximum camera positions

    void Start()
    {
        cam = GetComponent<Camera>();

        // Ensure the map has a Collider2D component
        if (map.GetComponent<Collider2D>() != null)
        {
            // Use the bounds of the Collider2D to set the map boundaries
            Bounds mapBounds = map.GetComponent<Collider2D>().bounds;
            CalculateCameraBounds(mapBounds);
        }
        else
        {
            Debug.LogError("Map GameObject does not have a Collider2D component.");
        }
    }

    void Update()
    {
        if (player != null && cam != null)
        {
            // Get the player's position
            Vector3 playerPos = player.position;

            // Clamp the camera's position to ensure it stays within the calculated bounds
            float clampedX = Mathf.Clamp(playerPos.x, minCameraPos.x, maxCameraPos.x);
            float clampedY = Mathf.Clamp(playerPos.y, minCameraPos.y, maxCameraPos.y);

            // Set the camera's position to follow the player, while staying within the bounds
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }

    void CalculateCameraBounds(Bounds mapBounds)
    {
        // Calculate the camera's viewable extents
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Calculate the minimum and maximum camera positions with the offsets
        minCameraPos = new Vector2(mapBounds.min.x + camWidth / 2, mapBounds.min.y + camHeight / 2);
        maxCameraPos = new Vector2(mapBounds.max.x - camWidth / 2, mapBounds.max.y - camHeight / 2);
    }
}