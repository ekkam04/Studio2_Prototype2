using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    public Transform orientation;
    public Transform cameraObj;

    public float rotationSpeed = 3f;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    // public Vector3 currentCheckpoint;

    // public List<Vector3> previousPositions = new();
    // public GameObject pastPlayer;
    // public GameObject pastPlayerTarget;
    // public GameObject pathOrb;

    Rigidbody rb;
    Animator anim;

    public float jumpHeightApex = 2f;
    public float jumpDuration = 1f;
    public float downwardsGravityMultiplier = 1f;

    public float speed = 1.0f;
    public float maxSpeed = 5.0f;
    public float groundDrag;

    public bool isJumping = false;
    public bool isGrounded;
    public bool allowDoubleJump = false;

    bool doubleJumped = false;

    float gravity;
    float initialJumpVelocity;
    float jumpStartTime;

    public float groundDistance = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Rotate orientation
        Vector3 viewDirection = transform.position - new Vector3(cameraObj.position.x, transform.position.y, cameraObj.position.z);
        orientation.forward = viewDirection.normalized;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
         
        if(moveDirection != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, Time.deltaTime * rotationSpeed);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        // Ground check
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, groundDistance + 0.1f);
        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), Vector3.down * (groundDistance + 0.1f), Color.red);

        // anim.SetFloat("PosX", rb.velocity.x);
        // anim.SetFloat("PosY", rb.velocity.z);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGrounded && allowDoubleJump && !doubleJumped)
            {
                doubleJumped = true;
                anim.SetBool("isJumping", true);
                StartJump();
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                anim.SetBool("isJumping", true);
                StartJump();
            }
        }

        if (isGrounded && !isJumping)
        {
            anim.SetBool("isJumping", false);
        }

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        // Limit velocity
        ControlSpeed();

    }

    void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection * speed * 10f, ForceMode.Force);
    }

    void ControlSpeed()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Limit velocity if needed
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    void FixedUpdate()
    {

        // Move player
        MovePlayer();

        // Jumping
        if (isJumping)
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);

            if (Time.time - jumpStartTime >= jumpDuration)
            {
                isJumping = false;
            }
        }
        else
        {
            rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
        }
    }

    void StartJump()
    {
        // Recalculate gravity and initial velocity in case they were changed in the inspector
        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        isJumping = true;
        jumpStartTime = Time.time;
        rb.velocity = Vector3.up * initialJumpVelocity;
    }
}