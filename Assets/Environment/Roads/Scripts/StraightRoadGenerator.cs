using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StraightRoadGenerator : MonoBehaviour
{
    private GameObject roadParent; // To hold the reference to the parent GameObject

    private enum MainRoadDirection { Horizontal, Vertical }
    private MainRoadDirection mainRoadDirection;
    private int mainRoadLength = 0; // The number of segments in the main road
    private float maxSideRoadLength = 15f;

    private float segmentWidth; // The width of a road segment
    public float sideRoadMinSeparation = 2f; // Minimum distance between side roads

    public int sideRoadsCount = 3; // Default number of side roads
    public GameObject straightRoadPrefab; // Assign this in the editor
    public GameObject crossingPrefab; // Assign this in the editor

    public float mapWidth = 10f; // The width of the map
    public float mapHeight = 10f; // The height of the map

    public GameObject restrictedAreaObject; // Eg. Jail, assign this in the editor
    private Bounds restrictedAreaBounds;

    // Start is called before the first frame update
    void Start() {

        if(straightRoadPrefab == null || crossingPrefab == null)
        {
            Debug.LogError("Please assign the road and crossing prefabs in the editor");
            return;
        }

        // Calculate restricted area bounds
        if (restrictedAreaObject != null)
        {
            var collider = restrictedAreaObject.GetComponent<Collider2D>(); // Assuming the object has a Collider2D
            if (collider != null)
            {
                restrictedAreaBounds = collider.bounds;
            }
            else
            {
                Debug.LogError("Restricted area object does not have a Collider2D component.");
            }
        }

        segmentWidth = straightRoadPrefab.transform.localScale.x * 5;
    }

    public void GenerateRoad()
    {
        GenerateMainRoad();
    }

    private void GenerateMainRoad()
    {
        roadParent = new GameObject("RoadParent");

        // Randomize mainroad direction
        Vector3 mainRoadStartPosition = GetMainRoadStartingPosition();

        // Create main road using the prefab. Main road always goes from the edge of the map to the opposite edge
        PlaceMainRoad(mainRoadStartPosition);

        // Generate side roads at random positions along the main road
        PlaceSideRoads(mainRoadStartPosition);

        // Add composite collider to the road parent from all the road pieces
        AddCompositeCollider();
    }

    private Vector3 GetMainRoadStartingPosition()
    {
        mainRoadDirection = (MainRoadDirection)Random.Range(0, 2);

        Vector3 mainRoadStartPosition;

        // Generate main road at a random position along the map withing the map's padded area
        if (!restrictedAreaObject)
        {
            // If no restricted area is set, use the map's dimensions
            mainRoadStartPosition = mainRoadDirection == MainRoadDirection.Horizontal ?
                                        new Vector3(0, Random.Range(-mapHeight / 4, mapHeight / 4), 0) :
                                        new Vector3(Random.Range(-mapWidth / 4, mapWidth / 4), 0, 0);
        }
        else
        {
            float restrictedAreaXMin = restrictedAreaBounds.min.x;
            float restrictedAreaXMax = restrictedAreaBounds.max.x;

            float restrictedAreaYMin = restrictedAreaBounds.min.y;
            float restrictedAreaYMax = restrictedAreaBounds.max.y;

            // Randomize the main road position while preventing it from being placed inside the restricted area
            if (mainRoadDirection == MainRoadDirection.Horizontal)
            {
                // Randomize the Y position within the map's padded area but outside the restricted area
                bool aboveRestrictedArea = Random.Range(0, 2) == 0;

                float randomY = aboveRestrictedArea ?
                    Random.Range(restrictedAreaYMax, mapHeight / 4) :
                    Random.Range(-mapHeight / 4, restrictedAreaYMin);

                mainRoadStartPosition = new Vector3(0, randomY, 0);
            }
            else
            {
                bool rightOfRestrictedArea = Random.Range(0, 2) == 0;

                float randomX = rightOfRestrictedArea ?
                    Random.Range(restrictedAreaXMax, mapWidth / 4) :
                    Random.Range(-mapWidth / 4, restrictedAreaXMin);
                mainRoadStartPosition = new Vector3(randomX, 0, 0);
            }
        }

        return mainRoadStartPosition;
    }

    private void PlaceMainRoad(Vector3 startPosition)
    {   
        //Start creating the road from the edge of the map, and move towards the opposite edge

        //If the main road is horizontal, start from the left edge and move towards the right edge

        if (mainRoadDirection == MainRoadDirection.Horizontal)
        {
            mainRoadLength = (int)mapWidth;

            for (float i = -mapWidth / 2; i <= mapWidth / 2; i += segmentWidth)
            {
                Vector3 position = new Vector3(i, startPosition.y, 0);

                // Rotate the road 90 deg when the road is horizontal
                Quaternion rotation = Quaternion.Euler(0, 0, 90);

                InstantiateRoadPiece(position, rotation);
            }
        }
        else
        {
            //The main road is vertical, start from the bottom edge and move towards the top edge
            mainRoadLength = (int)mapHeight;

            for (float i = -mapHeight / 2; i <= mapHeight / 2; i += segmentWidth)
            {
                Vector3 position = new Vector3(startPosition.x, i, 0);

                // Rotation is 0 when the road is vertical
                Quaternion rotation = Quaternion.Euler(0, 0, 0);

                InstantiateRoadPiece(position, rotation);
            }
        }        
    }

    // Side road generation
    private void PlaceSideRoads(Vector3 mainRoadStartPosition)
    {
        // Generate side roads at random positions along the main road
        // Get random positions on the main road, checking that the side road doesn't go out of the map and that they don't overlap with each other
        // Position array to store the random positions
        Vector3[] sideRoadPositions = GetRandomPositions(sideRoadsCount, mainRoadStartPosition);
     
        PlaceIndividualSideRoads(sideRoadPositions);
    }

    private Vector3[] GetRandomPositions(int count, Vector3 mainRoadStartPosition)
    {
        // Array to store the random positions
        Vector3[] positions = new Vector3[count];

        // Define a minimum distance between side roads to prevent overlap
        List<float> usedPositions = new List<float>(); // Keep track of used positions

        for (int i = 0; i < count; i++)
        {
            float newSegmentPos;

            int maxTries = 30;
            bool positionValid;
            do
            {
                positionValid = true;
                // Random non-overlapping position on the main road

                //Divisor 2 would allow the side road to be placed anywhere along the main road
                //Bigger divisors will limit the side road to be placed closer to the center of the main road
                float minSideRoadPos = -mainRoadLength / 3;
                float maxSideRoadPos = mainRoadLength / 3;

                newSegmentPos = Random.Range(minSideRoadPos, maxSideRoadPos);

                // Check the new position against all used positions
                foreach (float usedPos in usedPositions)
                {
                    if (Mathf.Abs(newSegmentPos - usedPos) < sideRoadMinSeparation)
                    {
                        positionValid = false; // The new position is too close to an existing one
                        maxTries--;
                        break;
                    }
                }
            }

            while (!positionValid && maxTries > 0); // Keep trying until a valid position is found

            usedPositions.Add(newSegmentPos); // Add the valid position to the list of used positions

            // Calculate the world position for the side road start
            Vector3 sideRoadStartPosition = mainRoadDirection == MainRoadDirection.Horizontal ?
                                            new Vector3(mainRoadStartPosition.x + newSegmentPos, mainRoadStartPosition.y, 0) :
                                            new Vector3(mainRoadStartPosition.x, mainRoadStartPosition.y + newSegmentPos, 0);


            positions[i] = sideRoadStartPosition;
        }

        return positions;
    }

    private void PlaceIndividualSideRoads(Vector3[] sideRoadPositions)
    {
        for (int i = 0; i < sideRoadPositions.Length; i++)
        {
            // Randomize a direction for the side road (left or right relative to the main road)
            bool sideRoadDirectionLeft = Random.Range(0, 2) == 0;

            // Generate a random length for the side road in world units
            float sideRoadLengthUnits = Random.Range(segmentWidth * 2, maxSideRoadLength);

            // Calculate the number of segments that fit into the random length
            int sideRoadSegmentCount = Mathf.FloorToInt(sideRoadLengthUnits / segmentWidth);

            // If the main road is horizontal, then the side roads will be vertical, and vice versa
            Quaternion rotation = mainRoadDirection == MainRoadDirection.Horizontal ?
                                  Quaternion.Euler(0, 0, 0) : // Side roads vertical
                                  Quaternion.Euler(0, 0, 90); // Side roads horizontal

            // Calculate the increment vector based on the main road direction
            Vector3 increment = mainRoadDirection == MainRoadDirection.Horizontal ?
                                new Vector3(0, segmentWidth, 0) : // Side roads vertical
                                new Vector3(segmentWidth, 0, 0); // Side roads horizontal

            if (sideRoadDirectionLeft)
            {
                // Reverse the increment vector to place the side road to the left of the main road
                // This will make the side road go in the opposite direction
                increment *= -1;
            }

            // Instantiate side road segments. J starts from 1 to avoid placing a road segment over the crossing
            for (int j = 1; j < sideRoadSegmentCount; j++)
            {
                Vector3 position = sideRoadPositions[i] + increment * j;
                InstantiateRoadPiece(position, rotation);
            }
        }
    }

    private void InstantiateRoadPiece(Vector3 position, Quaternion rotation)
    {
        GameObject segment = Instantiate(straightRoadPrefab, position, rotation);
        SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = -9998;
        segment.transform.SetParent(roadParent.transform);
    }

    private void AddCompositeCollider()
    {

        // Ensure the roadParent has a Rigidbody2D component with bodyType set to Static.
        // This is required for the CompositeCollider2D to work.
        Rigidbody2D rb = roadParent.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = roadParent.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Static;

        // Add a CompositeCollider2D to the roadParent if it doesn't already have one.
        CompositeCollider2D compositeCollider = roadParent.GetComponent<CompositeCollider2D>();
        if (compositeCollider == null)
        {
            compositeCollider = roadParent.AddComponent<CompositeCollider2D>();
        }
        roadParent.tag = "Road";

        // Configure the CompositeCollider2D.
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        compositeCollider.isTrigger = true; // Set to true if you want the composite collider to act as a trigger.
        compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous; // Ensure collider generation is immediate.

        // Add a PolygonCollider2D to each child (road piece) if they don't already have one, and configure them to be used by Composite.
        foreach (Transform child in roadParent.transform)
        {
            PolygonCollider2D childCollider = child.GetComponent<PolygonCollider2D>();
            if (childCollider == null)
            {
                childCollider = child.gameObject.AddComponent<PolygonCollider2D>();
            }
            childCollider.isTrigger = true; // Match the trigger setting to the composite collider.
            childCollider.usedByComposite = true;
        }

        // Ensure the roadParent also has a Collider2D component configured to work with the CompositeCollider.
        Collider2D roadParentCollider = roadParent.GetComponent<Collider2D>();
        if (roadParentCollider == null)
        {
            roadParentCollider = roadParent.AddComponent<EdgeCollider2D>(); // Or another type of Collider2D depending on your needs.
        }
        roadParentCollider.isTrigger = true; // Optional: only if you need the parent itself to also act as a trigger.

        //// Add a rigidbody to the road parent
        //Rigidbody2D rb = roadParent.AddComponent<Rigidbody2D>();
        //rb.bodyType = RigidbodyType2D.Static;

        //// Add composite collider to the road parent from all the road pieces
        //CompositeCollider2D compositeCollider = roadParent.AddComponent<CompositeCollider2D>();
        //compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        //compositeCollider.offset = Vector2.zero;
        //compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
        //compositeCollider.vertexDistance = 0.1f;
        //compositeCollider.edgeRadius = 0.1f;
        //compositeCollider.isTrigger = true;

        //// Add a tag to the road parent
        //
    }
}
