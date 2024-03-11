using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject thiefPrefab; // Assign in the editor
    public Camera playerCamera; // Assign in the editor
    public float mapWidth = 20f; // Width of the map
    public float mapHeight = 20f; // Height of the map
    public int initialSpawnCount = 5;
    public float spawnRate = 5f; // Enemies per minute at start

    private float timeSinceLastSpawn;
    private float spawnInterval => 60 / spawnRate; // Calculate spawn interval from spawn rate

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnEnemy();
            timeSinceLastSpawn = 0;

            // Optional: Increase spawn rate over time
            IncreaseSpawnRateOverTime();
        }
    }
    void SpawnEnemy()
    {
        int avoidancePriority = Random.Range(0,100); // Set avoidance priority for the new enemy to avoid navigation conflicts

        Vector2 spawnPoint = GetSpawnLocationOutsideCameraView();

        // Make sure the quaternion is set to identity to avoid rotation
        GameObject newThief = Instantiate(thiefPrefab, spawnPoint, Quaternion.identity);
        ThiefNavigation thiefNavigation = newThief.GetComponent<ThiefNavigation>();

        thiefNavigation.SetSpawnPoint(spawnPoint);
        thiefNavigation.SetAgentAvoidancePriority(avoidancePriority);
    }


    Vector2 GetSpawnLocationOutsideCameraView()
    {
        // Generate position 1 unit off the edge of the map, ensuring it's outside camera view
        Vector2 position;
        float spawnEdge = Random.Range(0, 4); // Randomly choose an edge

        float leftSpawnEdge = (-mapWidth / 2) + 1;
        float rightSpawnEdge = (mapWidth / 2) - 1;
        float topSpawnEdge = (mapHeight / 2) - 1;
        float bottomSpawnEdge = (-mapHeight / 2) + 1;

        if (spawnEdge < 1) // Top
        {
            position = new Vector2(Random.Range(leftSpawnEdge, rightSpawnEdge), topSpawnEdge);
        }
        else if (spawnEdge < 2) // Bottom
        {
            position = new Vector2(Random.Range(leftSpawnEdge, rightSpawnEdge), bottomSpawnEdge);
        }
        else if (spawnEdge < 3) // Left
        {
            position = new Vector2(leftSpawnEdge, Random.Range(bottomSpawnEdge, topSpawnEdge));
        }
        else // Right
        {
            position = new Vector2(rightSpawnEdge, Random.Range(bottomSpawnEdge, topSpawnEdge));
        }

        return position;
    }

    void IncreaseSpawnRateOverTime()
    {
        // Slowly increase the spawn rate to make the game more difficult
        spawnRate += 1; // Example: Increase rate by 1 per minute
    }
}