using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ThiefNavigation : MonoBehaviour
{
    public float fleeTime = 2f; // Time to flee after reaching the target building
    public float minimumBuildingDistance = 7.0f;
    public bool navigationEnabled = true; // If the thief is allowed to navigate

    public Animator animator;

    public List<GameObject> stolenObjectsPrefabs;

    private GameObject currentStolenObject; // Holds the currently stolen object instance


    public AudioSource stealSound;
    public AudioSource walkingSound;
    public AudioSource flyingSound;
    public AudioSource escapeSound;

    private NavMeshAgent agent;
    private GameObject targetBuilding = null; // The current target building

    public float distanceToTargetThreshold = 1f; // Distance to target building to stop moving
    public float distanceToDisappear = 0.3f; // Distance to disappear
    private float distanceToTarget = 0.0f; // Distance to the target building
    private bool closeToTarget = false; // If the target building has been reached

    // Defaulted to 4,4 in case the spawn won't be set for some reason
    private Vector3 spawnPoint = new(9.5f,9.5f,-1); // The spawn point of the thief
    private bool goalAchieved = false; // If the thief is returning to the spawn point

    private int agentAvoidancePriority = -1; // The avoidance priority of the NavMeshAgent


    // Booleans to determine the state of the thief for sound effects
    private bool isFlying = false; // If the thief is flying
    private bool isCaught = false; // If the thief is jailed
    private bool isMoving = false; // If the thief is moving
    private bool isStealing = false; // If the thief is stealing
    private bool isJailed = false; // If the thief is jailed    
    private bool hasFled = false; // If the thief has fled
    private bool stealSoundPlayed = false;

    private SpriteRenderer spriteRenderer;
    private DynamicAnimatedSorting dynamicSorting;

    // Start is called before the first frame update
    void Start()
    {
        if (navigationEnabled)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        dynamicSorting = GetComponent<DynamicAnimatedSorting>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (navigationEnabled && !isCaught)
        {
            NavigateThief();
        }

        // Determine if the thief is moving
        isMoving = agent.velocity.magnitude > 0.1f;
        if (isMoving && !walkingSound.isPlaying && !isCaught && !isStealing && !isFlying)
        {
            walkingSound.Play();
        }
        else if (!isMoving || isCaught || isStealing || isFlying)
        {
            walkingSound.Stop();
        }
    }

    private void NavigateThief()
    {
        TryGetBuildingAndMoveToIt();

        if (targetBuilding == null)
        {
            Debug.LogError("Target building not set for thief");
            return;
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent not set for thief");
            return;
        }

        if (agent.avoidancePriority == -1 || agent.avoidancePriority != agentAvoidancePriority)
        {
            // Use the avoidance priority of the NavMeshAgent that's been set in the EnemySpawner script
            agent.avoidancePriority = agentAvoidancePriority;
        }

        DetermineIfCloseToTarget();

        DepleteFleeTimeIfOnTarget();

        ReturnToSpawnIfFleeTimeDepleted();

        DestroySelfOnReturnToSpawn();
    }

    float CalculateDistanceToTarget(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition);
    }

    private void DetermineIfCloseToTarget()
    {
        if (isJailed || isFlying || isCaught)
        {
            return;
        }

        if (targetBuilding == null)
        {
            Debug.LogError("Target building not set for thief");
            return;
        }

        distanceToTarget = CalculateDistanceToTarget(transform.position, targetBuilding.transform.position);

        if (distanceToTarget < distanceToTargetThreshold)
        {
            isStealing = true;
            animator.SetBool("isStealing", true);
            closeToTarget = true;
            if (!stealSoundPlayed)
            {
                stealSound.Play();
                stealSoundPlayed = true;
            }
        }
        else if (isStealing)
        {
            SpawnStolenObjectInHand();
            isStealing = false;
            animator.SetBool("isStealing", false);
            closeToTarget = false;
            //stealSound.Stop();
        }
    }

    private void SpawnStolenObjectInHand()
    {
        if (stolenObjectsPrefabs.Count > 0)
        {
            Debug.Log("Spawning stolen object in hand");
            int index = Random.Range(0, stolenObjectsPrefabs.Count);
            GameObject stolenObjectPrefab = stolenObjectsPrefabs[index];

            // Find the hand transform child game object. Adjust the name as per your hierarchy
            Transform rightHandTransform = FindDeepChild(transform, "RightHand");

            if (rightHandTransform != null && stolenObjectPrefab != null)
            {
                Debug.Log("Hand transform and stolen object prefab found");
                // Instantiate the stolen object as a child of the hand
                currentStolenObject = Instantiate(stolenObjectPrefab, rightHandTransform.position, Quaternion.identity, rightHandTransform);
                currentStolenObject.name = stolenObjectPrefab.name; // Optional: rename to match the prefab
            }
            else if(rightHandTransform == null)
            {
                Debug.LogError("Hand transform prefab not found.");
            }
            else if(stolenObjectPrefab == null)
            {
                Debug.LogError("Stolen object prefab not found.");
            }
        }
        else
        {
            Debug.LogError("No stolen object prefabs found");
        }
    }

    private Transform FindDeepChild(Transform parent, string tagName)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tagName))
            {
                return child;
            }
            Transform found = FindDeepChild(child, tagName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private void DestroySelfOnReturnToSpawn()
    {
        if (spawnPoint != null && goalAchieved && !hasFled)
        {
            float distanceToSpawn = Vector3.Distance(transform.position, spawnPoint);

            if (distanceToSpawn < distanceToDisappear)
            {
                if (!escapeSound.isPlaying)
                {
                    hasFled = true;
                    escapeSound.Play();
                    StartCoroutine(PlayEscapeSound());
                }
            }
        }
    }

    IEnumerator PlayEscapeSound()
    {      
        while (escapeSound.isPlaying)
        {
            yield return null;
        }

        ReputationManager.DecreaseReputation();
        Destroy(gameObject);
    }

    private void ReturnToSpawnIfFleeTimeDepleted()
    {
        if (closeToTarget && fleeTime <= 0 && !goalAchieved)
        {
            goalAchieved = true;
            agent.SetDestination(spawnPoint); // Move back to the spawn point

            // Reset the distance to target to avoid the thief being destroyed before reaching the spawn point
            distanceToTarget = 999;
        }
    }

    private void DepleteFleeTimeIfOnTarget()
    {
        if (closeToTarget && fleeTime > 0)
        {
            // Flee time is set on instatiation, and will be depleted here after reaching the target building
            fleeTime -= Time.deltaTime;
        }
    }

    private void TryGetBuildingAndMoveToIt()
    {
        if (goalAchieved)
        {
            // If the goal is achieved, don't try to get a building
            return;
        }

        if (targetBuilding == null)
        {
            FindRandomBuilding(); // Find a new target building if there's currently no target building
            distanceToTarget = CalculateDistanceToTarget(transform.position, targetBuilding.transform.position);

            if (distanceToTarget < minimumBuildingDistance)
            { 
                // If the building is too close, find another building, max 20 times
                for (int i = 0; i < 20; i++)
                {
                    if (targetBuilding != null && distanceToTarget < minimumBuildingDistance)
                    {
                        FindRandomBuilding();
                    
                        distanceToTarget = CalculateDistanceToTarget(transform.position, targetBuilding.transform.position);
                    }
                    else // If the building is far enough, break the loop
                    {
                        break;
                    }
                }

                // There's probably no far away buildings at this point, so go with the last found building
            }
        }

        if (targetBuilding != null && !closeToTarget)
        {
            NavigateToDestination();
        }
    }

    private void NavigateToDestination()
    {
        float targetY = targetBuilding.transform.position.y;
        float targetX = targetBuilding.transform.position.x;
        Vector3 targetPosition = new Vector3(targetX, targetY, -1);

        agent.SetDestination(targetPosition);
    }

    //Randomly select a building from the scene
    public void FindRandomBuilding()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        // Check if there are any buildings found
        if (buildings.Length > 0)
        {         
            int randomIndex = Random.Range(0, buildings.Length); // Select a random index
            targetBuilding = buildings[randomIndex]; // Set the target building         
        }
    }

    // Set the spawn point of the thief from the Spawner script
    public void SetSpawnPoint(Vector2 spawnPoint)
    {
        this.spawnPoint = spawnPoint;
    }

    // Set the avoidance priority of the NavMeshAgent
    public void SetAgentAvoidancePriority(int priority)
    {
        agentAvoidancePriority = priority;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collider is a police, and the thief is not jailed to prevent multiple collisions
        if (!isCaught && collision.gameObject.CompareTag("Police"))
        {          
            ScoreManager.AddScore(1);

            isCaught = true;
            isFlying = true;
            isStealing = false;
            animator.SetBool("isFlying", true); 
            flyingSound.Play();

            if (agent != null)
            {

                // Disable the NavMeshAgent to stop the thief from moving
                DisableThiefNavigation(agent);

                // Find the jail floor
                GameObject jailFloor = FindJailFloor();


                // Find random position within the jail floor bounds
                // Get the jail floor bounds
                BoxCollider2D jailFloorCollider = jailFloor.GetComponent<BoxCollider2D>();
                float x = Random.Range(jailFloorCollider.bounds.min.x, jailFloorCollider.bounds.max.x);
                float y = Random.Range(jailFloorCollider.bounds.min.y, jailFloorCollider.bounds.max.y);
                float z = jailFloor.transform.position.z;

                Vector3 targetPosition = new(x, y, z);

                // Start moving the thief to jail
                StartCoroutine(MoveThiefToJail(targetPosition));
            }
        } 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("JailSortingTrigger"))
        {
            //Get jail-front's sorting order
            GameObject jailFront = GameObject.FindGameObjectWithTag("JailFront");
            SpriteRenderer jailFrontSpriteRenderer = jailFront.GetComponent<SpriteRenderer>();

            //Set the sorting order of the thief to be behind the jail-front
            dynamicSorting.SetCustomSortingORderToAllChildren(jailFrontSpriteRenderer.sortingOrder - 1);
            //spriteRenderer.sortingOrder = jailFrontSpriteRenderer.sortingOrder - 1;
        }
    }

    GameObject FindJailFloor()
    {
        return GameObject.FindGameObjectWithTag("JailFloor");
    }

    void DisableThiefNavigation(NavMeshAgent agent)
    {
        agent.enabled = false;
    }

    IEnumerator MoveThiefToJail(Vector3 targetPosition)
    {
        // Get thief's dynamic sorter script
        dynamicSorting.DisableDynamicSorting();

        // Set sorting order to 1000 to ensure the thief is rendered on top of everything for the flying effect
        spriteRenderer.sortingOrder = 1000;

        // Initial position
        Vector3 startPosition = gameObject.transform.position;
        // Total time to move to the jail
        float duration = 2.0f; // Adjust this time as needed
                               // Calculate the height of the curve at its peak
        float peakHeight = 5.0f; // Adjust based on your game's scale

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            // Calculate current position on the curve
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            currentPosition.y += Mathf.Sin(Mathf.PI * normalizedTime) * peakHeight;
            gameObject.transform.position = currentPosition;
            yield return null;
        }

        // Ensure the thief is exactly at the target position at the end
        gameObject.transform.position = targetPosition;

        isFlying = false;
        flyingSound.Stop();
        animator.SetBool("isFlying", false);

        isJailed = true;
        dynamicSorting.EnableDynamicSorting();

    }



}
