using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.ComponentModel;

public class PlayerController : MonoBehaviour
{
    public int lives = 3;
    public Vector3 currentCheckpoint;

    [SerializeField] Material[] materials;
    [SerializeField] float visibilityRadius = 5f;
    [SerializeField] float visibilitySoftness = 0.5f;

    public Transform orientation;
    public Transform cameraObj;

    public float rotationSpeed = 3f;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    public Animator anim;

    public float jumpHeightApex = 2f;
    public float jumpDuration = 1f;

    public float trampolineHeightApex = 2f;
    public float trampolineDuration = 1f;

    public float boxHeightApex = 2f;
    public float boxDuration = 1f;

    float currentJumpDuration;

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

        currentCheckpoint = transform.position;

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (Material mat in materials)
        {
            mat.SetFloat("_Radius", visibilityRadius);
            mat.SetFloat("_Softness", visibilitySoftness);
        }
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out hit, groundDistance + 0.1f))
        {
            isGrounded = true;
            if (hit.collider.tag == "isTrampolineMetal")
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                StartJump(trampolineHeightApex, trampolineDuration);
            }
            else if (hit.collider.tag == "isBoxNormal")
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                StartJump(boxHeightApex, boxDuration);
                Destroy(hit.collider.gameObject);
            }
            else if (hit.collider.tag == "isBoxNormalPlus")
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                StartJump(boxHeightApex, boxDuration);
                Destroy(hit.collider.gameObject);
            }
        }
        else
        {
            isGrounded = false;
        }

        Debug.DrawRay(transform.position + new Vector3(0, 1, 0), Vector3.down * (groundDistance + 0.1f), Color.red);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGrounded && allowDoubleJump && !doubleJumped)
            {
                doubleJumped = true;
                StartJump(jumpHeightApex, jumpDuration);
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                StartJump(jumpHeightApex, jumpDuration);
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

        // Update the Position variable in the all the materials
        foreach (Material mat in materials)
        {
            mat.SetVector("_Position", transform.position);
        }

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

            if (Time.time - jumpStartTime >= currentJumpDuration)
            {
                isJumping = false;
            }
        }
        else
        {
            rb.AddForce(Vector3.down * -gravity * downwardsGravityMultiplier, ForceMode.Acceleration);
        }
    }

    void StartJump(float heightApex, float duration)
    {
        // Recalculate gravity and initial velocity
        gravity = -2 * heightApex / (duration * duration);
        initialJumpVelocity = Mathf.Abs(gravity) * duration;
        currentJumpDuration = duration;

        isJumping = true;
        anim.SetBool("isJumping", true);
        jumpStartTime = Time.time;
        rb.velocity = Vector3.up * initialJumpVelocity;
    }

    public void Respawn()
    {
        transform.position = currentCheckpoint;
        rb.velocity = Vector3.zero;
    }
}