using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform camera;
    private float _turnSmoothVelocity;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float gravity = 20f; // Adjust gravity as per needs
    [SerializeField] private float jumpForce = 8f; // Adjust jump force as per needs
    [SerializeField] private float speedMultiplier = 2.0f;
    private float _verticalVelocity = 0f;

    void Update()
    {
        // WASD and arrow key movemet
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            // Jumping while player is moving
            if (Input.GetKey(KeyCode.Space) && controller.isGrounded)
            {
                _verticalVelocity = jumpForce;
            }
        }

        // Jumping while player is stationary
        if (Input.GetKey(KeyCode.Space) && controller.isGrounded)
        {
            _verticalVelocity = jumpForce;
        }

        if (!controller.isGrounded)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }
        controller.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }
}
