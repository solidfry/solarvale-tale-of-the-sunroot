using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CricketJump : MonoBehaviour
{
    // Public variables for jump force and cooldown time
    public float jumpForce = 1f;
    public float minJumpCooldown = 0.5f; // Minimum cooldown time in seconds
    public float maxJumpCooldown = 2f; // Maximum cooldown time in seconds
    public float rotateDuration = 1f; // Duration for rotation to target direction
    public float moveSpeed = 5f; // Speed of movement towards target direction

    // Private variable to track when the GameObject can jump again
    private float nextJumpTime;

    // Reference to the Rigidbody component
    private Rigidbody rb;

    // Flag to track if the player is in the trigger area
    private bool playerInTrigger;

    // Target direction to move towards after jumping
    private Vector3 targetDirection;

    // Target rotation for smooth rotation transition
    private Quaternion targetRotation;

    // Flag to check if the GameObject is currently rotating
    private bool isRotating;

    // Flag to check if the GameObject is grounded
    private bool isGrounded;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the GameObject can jump immediately (player is in trigger)
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;

            // Check if enough time has passed since last jump
            if (Time.time > nextJumpTime)
            {
                RotateAndJump();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Mark that the player is no longer in trigger area
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    void Update()
    {
        // Check if enough time has passed since the last jump and player is in trigger
        if (Time.time > nextJumpTime && playerInTrigger)
        {
            RotateAndJump();
        }

        // Smoothly rotate towards the target rotation if rotating
        if (isRotating)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / rotateDuration);

            // Check if rotation is complete
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    void RotateAndJump()
    {
        // Calculate a random rotation around the Z-axis within +/- 60 degrees from the current Z rotation
        float currentZRotation = transform.rotation.eulerAngles.z;
        float randomRotationZ = Random.Range(currentZRotation - 60f, currentZRotation + 60f);

        // Calculate target rotation to face the new Z direction, only modify Z-axis
        targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, randomRotationZ);

        // Start rotating towards the target direction
        isRotating = true;

        // Apply an upward force to the Rigidbody to make the GameObject jump
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Set the next jump time based on a random cooldown between minJumpCooldown and maxJumpCooldown
        nextJumpTime = Time.time + Random.Range(minJumpCooldown, maxJumpCooldown);

        // Start coroutine to handle gradual falling after 0.75 seconds
        StartCoroutine(GradualFall(0.75f));
    }

    void FixedUpdate()
    {
        // Move towards the target direction while jumping
        if (rb.velocity.y > 0) // Check if the Rigidbody is moving upwards
        {
            Vector3 moveDirection = transform.up; // Use the object's up vector for movement
            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }

    IEnumerator GradualFall(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Gradually apply downward force to simulate falling
        while (rb.velocity.y > 0 || !isGrounded)
        {
            // Reduce the upward velocity gradually
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - (Time.deltaTime * jumpForce), rb.velocity.z);

            // Continue moving towards the target direction
            Vector3 moveDirection = transform.up; // Use the object's up vector for movement
            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the GameObject collides with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if the GameObject is no longer colliding with the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
