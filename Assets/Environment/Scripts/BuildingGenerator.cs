using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    // Public variables
    public List<GameObject> buildingPrefabs; // List to hold building prefabs
    public int numberOfHouses = 5; // Number of houses to place
    public float placementRadius = 1.0f; // Distance from the road to attempt placing buildings
    public float minimumDistanceFromRoad = 2f; // Minimum distance from the road
    public float minimumDistanceBetweenHouses = 2.0f; // Minimum distance between houses

    // Private variables
    private GameObject[] roads; // Array to hold all roads in the scene
    public GameObject map; // Reference to the map object to get its bounds

    public float edgeSafetyMargin = 3.0f; // Distance from the map edges where buildings won't be placed


    void Start()
    {
        // For testing purposes, we'll call the PlaceBuildings method from the Start method
        //PlaceBuildings();
    }

    public void PlaceBuildings()
    {
        roads = GameObject.FindGameObjectsWithTag("Road");
        Debug.Log("Found " + roads.Length + " roads in the scene");

        int maxTries = numberOfHouses * 5; // Maximum number of tries to place a house
        int placedHouses = 0;
        while (placedHouses < numberOfHouses && maxTries > 0)
        {
            GameObject road = roads[Random.Range(0, roads.Length)];
            Vector2 roadPosition = road.transform.position;
            //Vector2 directionFromRoad = Random.insideUnitCircle.normalized;
            Vector2 potentialPosition = CalculatePositionNearRoad(road.GetComponent<Collider2D>());

            if (!HasOverlappingObjects(potentialPosition))
            {
                GameObject buildingPrefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
                Instantiate(buildingPrefab, potentialPosition, Quaternion.identity);
                placedHouses++;
            }
            maxTries--;
        }
    }

    private Vector2 CalculatePositionNearRoad(Collider2D roadCollider)
    {
        // Get the bounds of the road collider
        Bounds bounds = roadCollider.bounds;

        // Randomly choose a side: -1 (left), 1 (right) for x-axis placement
        float sideX = Random.Range(0, 2) * 2 - 1;
        // Randomly choose: -1 (below), 1 (above) for y-axis placement
        float sideY = Random.Range(0, 2) * 2 - 1;

        // Offset x by half to one and a half times the collider's extents (modify these values to control building distance from road)
        float xOffset = sideX * (bounds.extents.x + Random.Range(0.5f, 1.5f));
        // Similarly, offset y for placing buildings above or below the road
        float yOffset = sideY * (bounds.extents.y + Random.Range(0.5f, 1.5f));

        // Random y position along the height of the road for side placement
        float randomYOffset = Random.Range(bounds.extents.y, 2* bounds.extents.y);

        // Determine placement mode (side, above, below) and calculate position accordingly
        int placementMode = Random.Range(0, 3); // 0 for side, 1 for above, 2 for below
        Bounds mapBounds = map.GetComponent<Collider2D>().bounds;
        Bounds adjustedBounds = new Bounds(mapBounds.center, mapBounds.size - new Vector3(edgeSafetyMargin * 2, edgeSafetyMargin * 2, 0));


        Vector2 positionNearRoad;

        for (int attempts = 0; attempts < 10; attempts++)
        {
            switch (placementMode)
            {
                case 0: // Side placement
                    positionNearRoad = new Vector2(bounds.center.x + xOffset, bounds.center.y + randomYOffset);
                    break;
                case 1: // Above
                    positionNearRoad = new Vector2(bounds.center.x + Random.Range(-bounds.extents.x, bounds.extents.x), bounds.center.y + yOffset);
                    break;
                case 2: // Below
                    positionNearRoad = new Vector2(bounds.center.x + Random.Range(-bounds.extents.x, bounds.extents.x), bounds.center.y - yOffset);
                    break;
                default:
                    positionNearRoad = new Vector2(bounds.center.x, bounds.center.y);
                    break;
            }

            // Check that the position is within the map bounds
            if (adjustedBounds.Contains(new Vector3(positionNearRoad.x, positionNearRoad.y, adjustedBounds.center.z)))
            {
                return positionNearRoad; // Position is within map boundaries
            }
            // This is a simplification; you might want to handle this case more gracefully
        }

        return adjustedBounds.center;
    }

    bool HasOverlappingObjects(Vector2 position)
    {
        // Check for overlap with other buildings
        Collider2D[] buildingHitColliders = Physics2D.OverlapCircleAll(position, minimumDistanceBetweenHouses);
        foreach (var hitCollider in buildingHitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Building"))
            {
                Debug.Log("Overlapping building");
                return true;
            }
        }

        // Expanded search radius to include the minimum distance from roads
        Collider2D[] roadHitColliders = Physics2D.OverlapCircleAll(position, minimumDistanceFromRoad);
        foreach (var hitCollider in roadHitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Road"))
            {
                Debug.Log("Overlapping road");
                return true;
            }
        }

        return false; // No overlap found
    }
}