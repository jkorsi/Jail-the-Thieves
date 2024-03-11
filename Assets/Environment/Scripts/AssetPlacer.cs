using System.Collections.Generic;
using UnityEngine;

public class AssetPlacer : MonoBehaviour, IAssetPlacer
{
    public List<GameObject> prefabsToPlace; // List of prefabs to choose from
    public float minimumDistanceToExistingObject = 1.0f; // Configurable minimum distance from any object
    public LayerMask existingObjectsLayer; // Layer containing existing objects to check distance against

    public bool minUseMinSpreadForSimilars = false;
    public float minSimilarsSpread = 1.0f;

    // Variables for optional max distance feature
    public bool useMaxDistanceFromSpecificObject = false;
    public string specificObjectTag = ""; // Tag of the specific object type to stay close to
    public float maxDistanceFromSpecificObject = 10f; // Max distance from the specific object

    public int amountOfAssetsToPlace = 5; // Amount of assets to place
    public int maxPlacementAttemptsPerAsset = 10; // Maximum attempts to place each asset


    public GameObject mapGameObject; // Drag and drop the map GameObject here in the inspector
    public float minDistanceFromMapEdge = 2f; // Min distance from the map edge

    public void PlacePowerUpInFreeSpace()
    {
        // Place the specified amount of assets
        for (int i = 0; i < amountOfAssetsToPlace; i++)
        {
            PlaceSingleAsset();
        }
    }

    private void PlaceSingleAsset()
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

    private GameObject SelectRandomPrefab()
    {
        if (prefabsToPlace == null || prefabsToPlace.Count == 0)
        {
            Debug.LogError("No prefabs specified for placement.");
            return null;
        }

        int randomIndex = Random.Range(0, prefabsToPlace.Count);
        return prefabsToPlace[randomIndex];
    }

    private Vector2 FindFreePosition(GameObject prefabToPlace)
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
        int maxAttempts = amountOfAssetsToPlace * maxPlacementAttemptsPerAsset; // Maximum attempts to find a free spot

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPosition = new(Random.Range(-paddedMapWidth / 2, paddedMapWidth / 2), Random.Range(-paddedMapHeight / 2, paddedMapHeight / 2));

            if (IsPositionValid(randomPosition, prefabToPlace))
            {
                return randomPosition;
            }
        }

        return Vector2.zero; // Return zero vector if no free spot is found
    }

    private bool IsPositionValid(Vector2 position, GameObject prefabToPlace)
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
        Collider2D tempColliderFromPrefab = CopyCollider(prefabCollider, tempObject);

        if (tempColliderFromPrefab == null)
        {
            Debug.Log("Destroying tempObject, collider copy failed");
            Destroy(tempObject);
            return false; // If copying the collider failed
        }

        if (useMaxDistanceFromSpecificObject && !IsWithinMaxDistanceOfSpecificObject(tempObject))
        {
            Destroy(tempObject);
            return false; // The position is not within max distance of the specific object

        }

        // Prepare for overlap check
        List<Collider2D> collisionResults = new();
        ContactFilter2D layerFilter = new();
        layerFilter.SetLayerMask(existingObjectsLayer);

        // Check for overlap using the collider's shape
        int overlapCount = tempColliderFromPrefab.OverlapCollider(layerFilter, collisionResults);


        // Check for overlap with existing trigger colliders
        if (overlapCount == 0)
        {
            layerFilter.useTriggers = true;
            overlapCount = tempColliderFromPrefab.OverlapCollider(layerFilter, collisionResults);
        }

        // Cleanup: Destroy the temporary object
        Destroy(tempObject);


        // If the minUseMinSpreadForSimilars flag is set, check for overlap with similar objects from the same layer
        if (minUseMinSpreadForSimilars && overlapCount > 0)
        {
            GameObject[] existingObjects = GameObject.FindGameObjectsWithTag(prefabToPlace.tag);

            foreach (GameObject obj in existingObjects)
            {
                if (obj == prefabToPlace) continue; // Skip the prefab itself

                Collider2D existingObjectCollider = obj.GetComponent<Collider2D>();

                if (existingObjectCollider == null)
                {
                    Debug.LogWarning("Existing object lacks a Collider2D component: " + obj.name);
                    continue; // Skip this existing object if it lacks a Collider2D
                }

                float colliderDistance = existingObjectCollider.Distance(tempColliderFromPrefab).distance;

                if (colliderDistance <= minSimilarsSpread)
                {
                    return false; // The position is not free if a similar object is found within the minSimilarsSpread
                }
            }

        }
        return overlapCount == 0; // Position is free if no objects are found within the collider
    }

    // Helper method to copy a Collider2D component from one GameObject to another
    private Collider2D CopyCollider(Collider2D sourceCollider, GameObject targetObject)
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

    private void CopyCompositeCollider2D(CompositeCollider2D source, CompositeCollider2D target)
    {
        target.geometryType = source.geometryType;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        target.edgeRadius = source.edgeRadius;
        target.generationType = source.generationType;
    }

    private void CopyCapsuleColliderProperties(CapsuleCollider2D source, CapsuleCollider2D target)
    {
        target.size = source.size;
        target.direction = source.direction;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
    }

    private void CopyBoxColliderProperties(BoxCollider2D source, BoxCollider2D target)
    {
        target.size = source.size;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;

    }

    private void CopyCircleColliderProperties(CircleCollider2D source, CircleCollider2D target)
    {
        target.radius = source.radius;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        // Copy other relevant properties as needed
    }

    private void CopyPolygonColliderProperties(PolygonCollider2D source, PolygonCollider2D target)
    {
        target.points = source.points;
        target.offset = source.offset;
        target.isTrigger = source.isTrigger;
        // Copy other relevant properties as needed
    }

    private bool IsWithinMaxDistanceOfSpecificObject(GameObject objectToPlace)
    {
        if (!useMaxDistanceFromSpecificObject)
        {
            return true; // If the feature is not used, always return true
        }

        GameObject[] specificObjects = GameObject.FindGameObjectsWithTag(specificObjectTag);

        foreach (GameObject obj in specificObjects)
        {
            if (obj.GetComponent<Collider2D>() == null)
            {
                Debug.LogWarning("Specific object lacks a Collider2D component: " + obj.name);
                continue; // Skip this specific object if it lacks a Collider2D
            }

            Collider2D specificObjCollider = obj.GetComponent<Collider2D>();
            Collider2D objectToPlaceCollider = objectToPlace.GetComponent<Collider2D>();

            float colliderDistance = specificObjCollider.Distance(objectToPlaceCollider).distance;

            if (colliderDistance <= maxDistanceFromSpecificObject)
            {
                return true; // The object to place is within max distance of at least one specific object
            }
        }

        return false;
    }
}