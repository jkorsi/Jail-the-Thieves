using UnityEngine;
using System.Collections;

public class MoveControl : MonoBehaviour
{
    public float initialMoveSpeed = 2f;
    public float roadSpeedMultiplier = 1.5f;
    public float powerUpSpeedMultiplier = 1.2f;

    private float actualMoveSpeed;
    private Vector2 movement;
    private Vector3 touchPosition;

    private Rigidbody2D rb;
    public float powerUpSizeMultiplier = 1.3f;
    private Vector3 actualSize;

    public Animator animator; // Reference to the Animator component

    public AudioSource walkingSound;
    public AudioSource kickThump;
    public AudioSource powerUpSound;

    private Camera mainCamera;


    private int eatenDonuts = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        actualMoveSpeed = initialMoveSpeed;
        actualSize = transform.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch
            touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
            touchPosition.z = 0; // Ensure the touchPosition is at the same Z as the player

            Vector2 touchDirection = (touchPosition - transform.position).normalized;

            // Use the touchDirection if there is significant movement
            if (touchDirection.magnitude > 0.1f)
            {
                movement = touchDirection;
            }
        }

        // Check if the player is moving
        if (movement.magnitude > 0)
        {
            movement.Normalize();

            if (!walkingSound.isPlaying)
            {
                walkingSound.Play();
            }
            animator.SetBool("isMoving", true);
        }
        else
        {
            walkingSound.Stop();
            animator.SetBool("isMoving", false);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * actualMoveSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Thief") // Make sure your thief object has the "Thief" tag
        {
            animator.SetBool("isHitting", true);
            kickThump.Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Road")
        {
            actualMoveSpeed = actualMoveSpeed * roadSpeedMultiplier;
        }

        if (collision.gameObject.tag == "Donut")
        {
            powerUpSound.Play();
            eatenDonuts++;
            UIManager.UpdateDonutText(eatenDonuts);
            // Add donut effect (Increase police size and speed for 5 seconds)
            actualMoveSpeed = actualMoveSpeed * powerUpSpeedMultiplier;

            // Uncomment the following line to increase the police size when eating a donut
            //IncreasePoliceSize();
        }
    }

    private void IncreasePoliceSize()
    {
        float increasedSizeX = actualSize.x * powerUpSizeMultiplier;
        float increasedSizeY = actualSize.y * powerUpSizeMultiplier;
        float increasedSizeZ = actualSize.z * powerUpSizeMultiplier;

        actualSize = new Vector3(increasedSizeX, increasedSizeY, increasedSizeZ);

        transform.localScale = actualSize;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {   
        if (collision.gameObject.tag == "Road")
        {
            actualMoveSpeed = actualMoveSpeed / roadSpeedMultiplier;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Thief")
        {
            animator.SetBool("isHitting", false);
        }
    }
}
