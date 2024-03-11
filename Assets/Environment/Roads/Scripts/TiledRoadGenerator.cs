using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralRoadGenerator : MonoBehaviour
{
    public enum MainRoadDirection { Horizontal, Vertical }
    public MainRoadDirection mainRoadDirection;
    public int sideRoadsCount = 3; // Default number of side roads
    public GameObject straightRoadPrefab; // Assign this in the editor
    public GameObject crossingPrefab; // Assign this in the editor

    public float roadSegmentLength = 1f; // The length of each road segment
    public int mainRoadLength = 10; // The number of segments in the main road
    public int sideRoadLength = 5; // The number of segments in each side road

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        // Generate main road at a random position
        Vector3 mainRoadStartPosition = mainRoadDirection == MainRoadDirection.Horizontal ?
                                        new Vector3(0, Random.Range(-mainRoadLength / 2, mainRoadLength / 2), 0) :
                                        new Vector3(Random.Range(-mainRoadLength / 2, mainRoadLength / 2), 0, 0);

        PlaceMainRoad(mainRoadStartPosition);

        // Generate side roads at random positions along the main road
        GenerateSideRoads(mainRoadStartPosition);

    }

    private void GenerateSideRoads(Vector3 mainRoadStartPosition)
    {
        for (int i = 0; i < sideRoadsCount; i++)
        {
            // Random position on the main road
            int randomSegment = Random.Range(0, mainRoadLength);

            //If the main road is horizontal, the side road will be vertical and vice versa
            Vector3 sideRoadStartPosition = mainRoadDirection == MainRoadDirection.Horizontal ?
                                            new Vector3(mainRoadStartPosition.x + randomSegment * roadSegmentLength, mainRoadStartPosition.y, 0) :
                                            new Vector3(mainRoadStartPosition.x, mainRoadStartPosition.y + randomSegment * roadSegmentLength, 0);

            PlaceSideRoad(sideRoadStartPosition);
        }
    }

    private void PlaceMainRoad(Vector3 startPosition)
    {
        for (int i = 0; i < mainRoadLength; i++)
        {
            Vector3 position = mainRoadDirection == MainRoadDirection.Horizontal ?
                               new Vector3(startPosition.x + i * roadSegmentLength, startPosition.y, 0) :
                               new Vector3(startPosition.x, startPosition.y + i * roadSegmentLength, 0);

            //Rotate the main road if it's vertical
            if (mainRoadDirection == MainRoadDirection.Vertical)
            {
                straightRoadPrefab.transform.Rotate(0, 0, 90); 
            }

            Instantiate(straightRoadPrefab, position, Quaternion.identity, this.transform);
        }
    }

    private void PlaceSideRoad(Vector3 startPosition)
    {
        // Place a crossing at the start of the side road
        Instantiate(crossingPrefab, startPosition, Quaternion.identity, this.transform);

        // Offset to avoid placing another segment on the crossing
        startPosition += mainRoadDirection == MainRoadDirection.Horizontal ?
                         new Vector3(0, roadSegmentLength, 0) :
                         new Vector3(roadSegmentLength, 0, 0);

        for (int i = 1; i < sideRoadLength; i++) // Start at 1 to avoid overlapping the crossing
        {
            Vector3 position = mainRoadDirection == MainRoadDirection.Horizontal ?
                               new Vector3(startPosition.x, startPosition.y + i * roadSegmentLength, 0) :
                               new Vector3(startPosition.x + i * roadSegmentLength, startPosition.y, 0);

            //Rotate the side road if it's vertical
            if (mainRoadDirection == MainRoadDirection.Vertical) { 
                straightRoadPrefab.transform.Rotate(0, 0, 90); 
            }

            Instantiate(straightRoadPrefab, position, Quaternion.identity, this.transform);
        }
    }

}