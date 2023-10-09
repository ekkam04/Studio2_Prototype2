using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip landSound;
    [SerializeField] AudioClip bounceSound;
    [SerializeField] AudioClip crateSound;
    [SerializeField] AudioClip checkpointSound;
    [SerializeField] public AudioClip failSound;

    public int lives = 3;
    public Vector3 currentCheckpoint;
    UIManager uiManager;

    [SerializeField] Material[] materials;
    [SerializeField] float initialVisibilityRadius = 30f;
    [SerializeField] float visibilityRadius = 5f;
    [SerializeField] float visibilitySoftness = 0.5f;

    [SerializeField] ParticleSystem checkpointParticles;
    [SerializeField] ParticleSystem starParticles;

    public Transform orientation;
    public Transform cameraObj;

    public float rotationSpeed = 3f;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public Rigidbody rb;
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
    public bool hasLanded = true;
    public bool isGrounded;
    public bool allowDoubleJump = false;

    bool doubleJumped = false;

    float gravity;
    float initialJumpVelocity;
    float jumpStartTime;

    public float groundDistance = 1f;

    private void Start()
    {
        ChangeMaterialShaderSpherical();

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        uiManager = GameObject.FindObjectOfType<UIManager>();

        currentCheckpoint = transform.position;

        gravity = -2 * jumpHeightApex / (jumpDuration * jumpDuration);
        initialJumpVelocity = Mathf.Abs(gravity) * jumpDuration;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (Material mat in materials)
        {
            mat.SetFloat("_Radius", 30f);
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

            if (!hasLanded)
            {
                audioSource.PlayOneShot(landSound);
                hasLanded = true;
            }

            switch (hit.collider.tag)
            {
                case "isTrampolineMetal":
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    audioSource.PlayOneShot(bounceSound);
                    StartJump(trampolineHeightApex, trampolineDuration);
                    break;

                case "isBoxNormal":
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    audioSource.PlayOneShot(crateSound);
                    StartJump(boxHeightApex, boxDuration);

                    // turn off box collider and mesh renderer
                    hit.collider.gameObject.GetComponent<BoxCollider>().enabled = false;
                    hit.collider.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    break;

                case "isBoxNormalPlus":
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    audioSource.PlayOneShot(crateSound);
                    StartJump(boxHeightApex + 0.5f, boxDuration);

                    // turn off box collider and mesh renderer
                    hit.collider.gameObject.GetComponent<BoxCollider>().enabled = false;
                    hit.collider.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    break;

                case "isCheckpoint":
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    audioSource.PlayOneShot(checkpointSound);
                    StartJump(boxHeightApex, boxDuration);
                    lives = 3;
                    uiManager.ReplenishHearts();

                    // set current checkpoint
                    Vector3 checkpointPosition = hit.collider.gameObject.transform.position;
                    currentCheckpoint = checkpointPosition + new Vector3(0, 1, 0);
                    hit.collider.gameObject.transform.position = checkpointPosition + new Vector3(0, -0.75f, 0);
                    hit.collider.gameObject.GetComponent<BoxCollider>().enabled = false;
                    checkpointParticles.Play();
                    break;

                case "isFinishLine":
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    audioSource.PlayOneShot(checkpointSound);
                    StartJump(trampolineHeightApex, trampolineDuration);

                    // set current checkpoint
                    Vector3 finishLinePosition = hit.collider.gameObject.transform.position;
                    currentCheckpoint = finishLinePosition + new Vector3(0, 1, 0);
                    hit.collider.gameObject.transform.position = finishLinePosition + new Vector3(0, -0.75f, 0);
                    hit.collider.gameObject.GetComponent<BoxCollider>().enabled = false;
                    starParticles.Play();
                    Invoke("GameWin", 1f);
                    break;
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
                audioSource.PlayOneShot(jumpSound);
                StartJump(jumpHeightApex, jumpDuration);
            }
            else if (isGrounded)
            {
                doubleJumped = false;
                audioSource.PlayOneShot(jumpSound);
                StartJump(jumpHeightApex, jumpDuration);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeMaterialShaderNormal();
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

    private void OnApplicationQuit() {
        print("Application ending after " + Time.time + " seconds. Resetting shader.");
        ChangeMaterialShaderNormal();
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
                hasLanded = false;
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

        // Enable box colliders and mesh renderers
        GameObject[] normalBoxes = GameObject.FindGameObjectsWithTag("isBoxNormal");
        foreach (GameObject box in normalBoxes)
        {
            box.GetComponent<BoxCollider>().enabled = true;
            box.GetComponent<MeshRenderer>().enabled = true;
        }

        GameObject[] normalPlusBoxes = GameObject.FindGameObjectsWithTag("isBoxNormalPlus");
        foreach (GameObject box in normalPlusBoxes)
        {
            box.GetComponent<BoxCollider>().enabled = true;
            box.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void ChangeMaterialShaderSpherical()
    {
        // Change the shader of the material to SphericalShaderGraph
        foreach (Material mat in materials)
        {
            mat.shader = Shader.Find("Shader Graphs/SphericalShaderGraph");
        }
        StartCoroutine(AnimateVisibilityRadius());
    }

    void ChangeMaterialShaderNormal()
    {
        // Change the shader of the material back to Standard
        foreach (Material mat in materials)
        {
            mat.shader = Shader.Find("Standard");
        }
    }

    IEnumerator AnimateVisibilityRadius()
    {
        // change radius
        for (float i = initialVisibilityRadius; i >= visibilityRadius; i -= 0.1f)
        {
            foreach (Material mat in materials)
            {
                mat.SetFloat("_Radius", i);
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    void GameWin()
    {
        uiManager.PlayGameWinAnimation();
    }

    public void PlayFootstepSound()
    {
        audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }
}