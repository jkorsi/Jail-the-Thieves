using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    /** This script is a simple power-up swpaner based on AssetPlacer.cs
     * It places a single power-up at a random position on the map.
     * The power-up is selected from a list of prefabs.
     * The script checks for overlap with existing objects and the map edge.
     * It also has an optional feature to stay close to a specific object type.
     */

    public List<GameObject> powerUpPrefabs; // List of power-up prefabs to choose from
    private static List<GameObject> staticPowerUpPrefabs; // List of prefabs to choose from
    public static LayerMask existingObjectsLayer; // Layer containing existing objects to check distance against

    public int maxPlacementAttempts; 
    private static int staticMaxPlacementAttempts = 100; // Maximum attempts to place each asset

    public static GameObject mapGameObject; // Drag and drop the map GameObject here in the inspector
    public static float minDistanceFromMapEdge = 2f; // Min distance from the map edge

    // Call this method to spawn single, random power-up to random free position on the map
    private void Start()
    {
        staticPowerUpPrefabs = powerUpPrefabs;
        Debug.Log("PowerUpSpawner.Start() called");
        Debug.Log("PowerUpSpawner.staticPowerUpPrefabs: " + staticPowerUpPrefabs);
        Debug.Log("powerUpPrefabs: " + powerUpPrefabs);


        staticMaxPlacementAttempts = maxPlacementAttempts;
        existingObjectsLayer = LayerMask.GetMask("Default");
        mapGameObject = GameObject.Find("Map");

        PowerUpSpawner.PlaceSinglePowerUp();
    }

    private void OnDestroy()
    {
        // Clean up static variables when the script is destroyed
        staticPowerUpPrefabs = null;
        existingObjectsLayer = 0;
        mapGameObject = null;
    }

    public static void PlaceSinglePowerUp()
    {
        GameObject prefabToPlace = SelectRandomPrefab(); // Select a random prefab from the list

        Vector2 position = FindFreePosition(prefabToPlace);
        if (position != Vector2.zero) // Check if a free spot was found
        {
            Instantiate(prefabToPlace, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No free space found to place the asset.");
        }
    }

    private static GameObject SelectRandomPrefab()
    {
        if (staticPowerUpPrefabs == null || staticPowerUpPrefabs.Count == 0)
        {
            Debug.LogError("No prefabs specified for placement.");
            return null;
        }

        int randomIndex = Random.Range(0, staticPowerUpPrefabs.Count);
        return staticPowerUpPrefabs[randomIndex];
    }

    private static Vector2 FindFreePosition(GameObject prefabToPlace)
    {
        // Use the size of the mapGameObject to determine map dimensions
        if (!mapGameObject)
        {
            Debug.LogError("Map GameObject not assigned.");
            return Vector2.zero;
        }
        var bounds = mapGameObject.GetComponent<Renderer>().bounds;
        float paddedMapWidth = bounds.size.x - minDistanceFromMapEdge;
        float paddedMapHeight = bounds.size.y - minDistanceFromMapEdge;

        for (int i = 0; i < staticMaxPlacementAttempts; i++)
        {
            Vector2 randomPosition = new(Random.Range(-paddedMapWidth / 2, paddedMapWidth / 2), Random.Range(-paddedMapHeight / 2, paddedMapHeight / 2));

            if (IsPositionValid(randomPosition, prefabToPlace))
            {
                return randomPosition;
            }
        }

        return Vector2.zero; // Return zero vector if no free spot is found
    }

    private static bool IsPositionValid(Vector2 position, GameObject prefabToPlace)
    {
        // Create a temporary GameObject for collision checking
        GameObject tempObject = new("TempCollider");
        tempObject.transform.position = position;

        if (prefabToPlace == null) return false; // Safety check in case prefab selection fails

        // Assuming prefabToPlace has a Collider2D component for bounds calculation
        Collider2D prefabCollider = prefabToPlace.GetComponent<Collider2D>();

        if (prefabCollider == null)
        {
            Debug.Log("Destroying tempObject, no collider found");
            Destroy(tempObject);
            return false; // If prefab lacks a Collider2D, we can't compute overlap properly
        }

        // Copy the prefab's Collider2D to the temporary GameObject
        Collider2D tempCollider = CopyCollider(prefabCollider, tempObject);

        if (tempCollider == null)
        {
            Debug.Log("Destroying tempObject, collider copy failed");
            Destroy(tempObject);
            return false; // If copying the collider failed
        }

        // Prepare for overlap check
        List<Collider2D> results = new();
        ContactFilter2D filter = new();
        filter.SetLayerMask(existingObjectsLayer);

        // Check for overlap using the collider's shape (ONLY NON-TRIGGER COLLIDERS for power-ups)
        int overlapCount = tempCollider.OverlapCollider(filter, results);

        // Cleanup: Destroy the temporary object
        Destroy(tempObject);

        return overlapCount == 0; // Position is free if no objects are found within the collider
    }

    // Helper method to copy a Collider2D component from one GameObject to another
    private static Collider2D CopyCollider(Collider2D sourceCollider, GameObject targetObject)
    {
        Collider2D targetCollider = null;

        switch (sourceCollider)
        {
            case BoxCollider2D sourceBox:
                BoxCollider2D targetBox = targetObject.AddComponent<BoxCollider2D>();
                CopyBoxColliderProperties(sourceBox, targetBox);
                targetCollider = targetBox;
                break;

            case CircleCollider2D sourceCircle:
                CircleCollider2D targetCircle = targetObject.AddComponent<CircleCollider2D>();
                CopyCircleColliderProperties(sourceCircle, targetCircle);
                targetCollider = targetCircle;
                break;

            case PolygonCollider2D sourcePolygon:
                PolygonCollider2D targetPolygon = targetObject.AddComponent<PolygonCollider2D>();
                CopyPolygonColliderProperties(sourcePolygon, targetPolygon);
                targetCollider = targetPolygon;
                break;

            case EdgeCollider2D sourceEdge:
                EdgeCollider2D edgeCollider2D = targetObject.AddComponent<EdgeCollider2D>();
                edgeCollider2D.points = sourceEdge.points;
                targetCollider = edgeCollider2D;
                break;

            case CapsuleCollider2D capsuleCollider2D:
                CapsuleCollider2D targetCapsule = targetObject.AddComponent<CapsuleCollider2D>();
                CopyCapsuleColliderProperties(capsuleCollider2D, targetCapsule);
                targetCollider = targetCapsule;
                break;

            case CompositeCollider2D sourceComposite:
                CompositeCollider2D compositeCollider2D = targetObject.AddComponent<CompositeCollider2D>();
                CopyCompositeCollider2D(sourceComposite, compositeCollider2D);
                targetCollider = compositeCollider2D;
                break;

            default:
                Debug.LogWarning("Collider type not supported: " + sourceCollider.GetType());
                break;
        }

        return targetCollider;
    }

    private static void CopyCompositeCollider2D(CompositeCollider2D source, CompositeCollider2D target)
    {
        target.geometryType = source.geometryType;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        target.edgeRadius = source.edgeRadius;
        target.generationType = source.generationType;
    }

    private static void CopyCapsuleColliderProperties(CapsuleCollider2D source, CapsuleCollider2D target)
    {
        Debug.Log("Copying CapsuleCollider2D properties");
        Debug.Log("Size: " + source.size);
        Debug.Log("Direction: " + source.direction);
        Debug.Log("Offset: " + source.offset);
        Debug.Log("IsTrigger: " + source.isTrigger);
        target.size = source.size;
        target.direction = source.direction;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
    }

    private static void CopyBoxColliderProperties(BoxCollider2D source, BoxCollider2D target)
    {
        target.size = source.size;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;

    }

    private static void CopyCircleColliderProperties(CircleCollider2D source, CircleCollider2D target)
    {
        target.radius = source.radius;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        // Copy other relevant properties as needed
    }

    private static void CopyPolygonColliderProperties(PolygonCollider2D source, PolygonCollider2D target)
    {
        target.points = source.points;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        // Copy other relevant properties as needed
    }
}