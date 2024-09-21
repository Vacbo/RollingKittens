using UnityEngine;
using TMPro;

public enum SlimeAnimationState { Idle, Walk, Jump, Attack, Damage }

public class PlayerControllerAI : MonoBehaviour
{
    public Face faces;
    public GameObject SmileBody;
    public SlimeAnimationState currentState;

    public Animator animator;
    public float moveSpeed = 3.5f;  // Adjusted moveSpeed for faster movement
    public float jumpForce = 10f; // Adjusted jumpForce for better jumping
    public float rotationSpeed = 720f; // Degrees per second

    private bool isGrounded; // To check if the player is on the ground

    // Collectible-related variables
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    private int count;

    // Gravity and movement
    private Rigidbody rb;
    public int damType; // Field to store damage type
    private Material faceMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        faceMaterial = SmileBody.GetComponent<Renderer>().materials[1];
        currentState = SlimeAnimationState.Idle; // Start in Idle state

        // Initialize collectibles
        count = 0;
        SetCountText();
        winTextObject.SetActive(false); // Hide win text initially

        // Lock rotation on the X and Z axes (only rotate around Y)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        HandlePlayerInput();
        UpdateAnimationState();
    }

    void HandlePlayerInput()
    {
        // Get player input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create a movement vector based on input
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // Move the player if there is any input
        if (direction.magnitude >= 0.1f)
        {
            // Calculate the target angle to rotate towards (Y axis only)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);

            // Rotate the player on the Y axis only
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // Move the player in the direction
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Update the animation state to Walking
            currentState = SlimeAnimationState.Walk;
            SetFace(faces.WalkFace);
        }
        else
        {
            // If no input, set to idle
            currentState = SlimeAnimationState.Idle;
            SetFace(faces.Idleface);

            // Stop the player's movement on XZ axes if no input (but keep Y for gravity)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // Jump logic (apply force to Rigidbody)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false; // Set to false to prevent double jumps
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            currentState = SlimeAnimationState.Jump;
            SetFace(faces.jumpFace);
            animator.SetTrigger("Jump");
        }

        // Attack logic (bind to a specific key, e.g., spacebar)
        if (Input.GetButtonDown("Fire1"))
        {
            currentState = SlimeAnimationState.Attack;
            SetFace(faces.attackFace);
            animator.SetTrigger("Attack");
        }
    }

    void UpdateAnimationState()
    {
        switch (currentState)
        {
            case SlimeAnimationState.Idle:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    StopMovement();
                }
                break;

            case SlimeAnimationState.Walk:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    // Set Speed parameter synchronized with movement
                    animator.SetFloat("Speed", moveSpeed);
                }
                break;

            case SlimeAnimationState.Jump:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                {
                    StopMovement();
                    animator.SetTrigger("Jump");
                }
                break;

            case SlimeAnimationState.Attack:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    StopMovement();
                    animator.SetTrigger("Attack");
                }
                break;

            case SlimeAnimationState.Damage:
                HandleDamageAnimation();
                break;
        }
    }

    // Handle the different damage types
    void HandleDamageAnimation()
    {
        // Set the damage type parameter in the Animator
        animator.SetInteger("DamageType", damType);

        // Trigger the damage animation
        animator.SetTrigger("Damage");

        // Update the face for damage
        SetFace(faces.damageFace);
    }

    void SetFace(Texture tex)
    {
        faceMaterial.SetTexture("_MainTex", tex);
    }

    private void StopMovement()
    {
        // Stop the animation
        animator.SetFloat("Speed", 0);
    }

    // Animation Events
    public void AlertObservers(string message)
    {
        if (message.Equals("AnimationAttackEnded"))
        {
            currentState = SlimeAnimationState.Idle;
        }

        if (message.Equals("AnimationJumpEnded"))
        {
            currentState = SlimeAnimationState.Idle;
        }

        if (message.Equals("AnimationDamageEnded"))
        {
            currentState = SlimeAnimationState.Idle;
        }
    }

    // Check if the player is grounded using collider detection
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    // Collectibles logic (OnTriggerEnter is called when the player enters a trigger collider)
    void OnTriggerEnter(Collider other)
    {
        // Check if the player collided with a collectible item
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false); // Deactivate the item
            count++; // Increase the count of collected items

            SetCountText(); // Update the UI
        }
    }

    // Update the UI with the current collectible count
    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();

        // If the player collected 12 items, display the win text
        if (count >= 12)
        {
            winTextObject.SetActive(true);
        }
    }
}