using UnityEngine;

public class MoveTowardsBuilding : MonoBehaviour
{
    public float speed = 5f; // Movement speed
    public float distanceToTargetThreshold = 0.5f; // Distance to target building to stop moving

    private Rigidbody2D rb;
    private GameObject targetBuilding = null; // The current target building
    private bool targetReached = false; // Whether the AI has reached the target building

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (targetBuilding != null && !targetReached)
        {
            MoveTowardsTarget();
        } else if (targetBuilding == null)
        {
            FindRandomBuilding(); // Find a new target building if the current one is destroyed
        } else
        {
            //Wait for a couple seconds and run away
            Debug.Log("Target reached, implement fleeing");
        }
    }

    void FindRandomBuilding()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        // Check if there are any buildings found
        if (buildings.Length > 0)
        {
            int randomIndex = Random.Range(0, buildings.Length); // Select a random index
            targetBuilding = buildings[randomIndex]; // Set the target building to the randomly selected one
            Debug.Log("Randomly selected building: " + targetBuilding.name);
        }
        else
        {
            Debug.LogError("No buildings found with tag 'Building'.");
            targetBuilding = null; // Ensure targetBuilding is null if no buildings are found
        }
    }

    void MoveTowardsTarget()
    {
        if (targetBuilding == null) return;

        Debug.Log("Moving towards target building");
        Vector2 position = rb.position;
        Vector2 targetPosition = new Vector2(targetBuilding.transform.position.x, targetBuilding.transform.position.y);
        Vector2 direction = (targetPosition - position).normalized;

        // Move the Rigidbody2D directly towards the target without applying force
        rb.MovePosition(position + direction * speed * Time.fixedDeltaTime);

        // Check if we are close enough to the target building to stop
        float distanceToTarget = Vector2.Distance(position, targetPosition);
        if (distanceToTarget < distanceToTargetThreshold) // Adjust this value as needed
        {
            targetReached = true;
            Debug.Log("Reached target building");
            rb.velocity = Vector2.zero; // Optionally, you can stop the AI's movement more abruptly
            // Consider what should happen when the target is reached. For example, finding a new target.
        }
    }

    // Optionally, re-evaluate the nearest building if the current target is destroyed or changes
    // Implement this logic based on your game requirements
}