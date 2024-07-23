using System.Collections;
using UnityEngine;

public class CricketJump : MonoBehaviour
{
    public float jumpForce = 1f;
    public float minJumpCooldownInTrigger = 0.5f; 
    public float maxJumpCooldownInTrigger = 2f;
    public float minJumpCooldownOutTrigger = 5f;
    public float maxJumpCooldownOutTrigger = 10f;
    public float rotateDuration = 1f;
    public float moveSpeed = 5f;

    private float nextJumpTime;
    private Rigidbody rb;
    private bool playerInTrigger;
    private Vector3 targetDirection;
    private Quaternion targetRotation;
    private bool isRotating;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            if (Time.time > nextJumpTime)
            {
                RotateAndJump();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    void Update()
    {
        if (Time.time > nextJumpTime && playerInTrigger)
        {
            RotateAndJump();
        }
        else if (Time.time > nextJumpTime)
        {
            PerformJump();
        }

        if (isRotating)
        {
            rotateDuration = Mathf.Max(rotateDuration, 0.1f); // Ensure a positive non-zero duration
            float t = Mathf.Clamp(Time.deltaTime / rotateDuration, 0.01f, 1f); // Avoid zero values
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
    }

    void RotateAndJump()
    {
        float currentZRotation = transform.rotation.eulerAngles.z;
        float randomRotationZ = Random.Range(currentZRotation - 60f, currentZRotation + 60f);
        targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, randomRotationZ);
        isRotating = true;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        nextJumpTime = Time.time + Random.Range(minJumpCooldownInTrigger, maxJumpCooldownInTrigger);
        StartCoroutine(GradualFall(0.75f));
    }

    void FixedUpdate()
    {
        if (rb.velocity.y > 0)
        {
            Vector3 moveDirection = transform.up;
            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }

    IEnumerator GradualFall(float delay)
    {
        yield return new WaitForSeconds(delay);
        while (rb.velocity.y > 0 || !isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - (Time.deltaTime * jumpForce), rb.velocity.z);
            Vector3 moveDirection = transform.up;
            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void PerformJump()
    {
        float currentZRotation = transform.rotation.eulerAngles.z;
        float randomRotationZ = Random.Range(currentZRotation - 60f, currentZRotation + 60f);
        targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, randomRotationZ);
        isRotating = true;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        SetNextJumpTime();
    }

    void SetNextJumpTime()
    {
        nextJumpTime = Time.time + Random.Range(minJumpCooldownOutTrigger, maxJumpCooldownOutTrigger);
    }
}
