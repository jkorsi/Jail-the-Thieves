using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public Vector2 mapSize = new Vector2(20, 20);
    public float roadWidth = 2f;

    [HideInInspector]
    public Vector2 startHorizontalRoad, endHorizontalRoad; // Endpoints for road 1
    [HideInInspector]
    public Vector2 startVerticalRoad, endVerticalRoad; // Endpoints for road 2

    public Material roadMaterial; // Material to apply to the road texture

    public float horizontalRoadOffset = 20f; // Offset for horizontal road to ensure it's within the view
    public float verticalRoadOffset = 20f; // Offset for vertical road to ensure it's within the view

    // Initialization
    void Start()
    {
        GenerateRoads();
    }

    void GenerateRoads()
    {
        // Adjusted to include offsets, ensuring roads start within the viewable area
        startHorizontalRoad = new Vector2(-mapSize.x / 2 + horizontalRoadOffset, Random.Range(-mapSize.y / 2, mapSize.y / 2));
        endHorizontalRoad = new Vector2(mapSize.x / 2 - horizontalRoadOffset, Random.Range(-mapSize.y / 2, mapSize.y / 2));

        startVerticalRoad = new Vector2(Random.Range(-mapSize.x / 2, mapSize.x / 2), -mapSize.y / 2 + verticalRoadOffset);
        endVerticalRoad = new Vector2(Random.Range(-mapSize.x / 2, mapSize.x / 2), mapSize.y / 2 - verticalRoadOffset);

        // Generate the Bezier curve points for both roads
        List<Vector2> horizontalRoadPoints = CalculateBezierCurve(startHorizontalRoad, endHorizontalRoad, true);
        List<Vector2> verticalRoadPoints = CalculateBezierCurve(startVerticalRoad, endVerticalRoad, false);

        // Draw the roads (initially as lines or using a LineRenderer for simplicity)
        DrawRoad(horizontalRoadPoints);
        DrawRoad(verticalRoadPoints);
    }

    List<Vector2> CalculateBezierCurve(Vector2 start, Vector2 end, bool isHorizontal)
    {
        Vector2 controlPoint;
        if (isHorizontal)
        {
            controlPoint = new Vector2((start.x + end.x) / 2, Random.Range(0, mapSize.y));
        }
        else
        {
            controlPoint = new Vector2(Random.Range(0, mapSize.x), (start.y + end.y) / 2);
        }

        List<Vector2> curvePoints = new List<Vector2>();
        for (float t = 0; t <= 1; t += 0.001f) // Increment can be adjusted for more/less detail
        {
            Vector2 bezierPoint = Mathf.Pow(1 - t, 2) * start +
                                  2 * (1 - t) * t * controlPoint +
                                  Mathf.Pow(t, 2) * end;
            curvePoints.Add(bezierPoint);
        }
        return curvePoints;
    }

    void DrawRoad(List<Vector2> points)
    {
        GameObject road = new GameObject("Road");
        MeshFilter meshFilter = road.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = road.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>(); // Add this line to declare a list for UVs
        Vector2 previousPoint = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 direction = (currentPoint - previousPoint).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x);

            Vector3 leftVertex = previousPoint + normal * roadWidth / 2;
            Vector3 rightVertex = previousPoint - normal * roadWidth / 2;
            vertices.Add(leftVertex);
            vertices.Add(rightVertex);

            // Calculate UVs here, mapping them according to the vertex positions
            float progress = (float)i / (points.Count - 1);
            uvs.Add(new Vector2(0, progress)); // Left vertex
            uvs.Add(new Vector2(1, progress)); // Right vertex

            if (i > 1)
            {
                int baseIndex = vertices.Count - 4;
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }

            previousPoint = currentPoint;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray(); // Assign the UVs to the mesh
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = roadMaterial; // Assume roadMaterial is assigned in the editor
    }
}
