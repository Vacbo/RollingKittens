using UnityEngine;
using TMPro;

public enum SlimeAnimationState { Idle, Walk, Jump, Attack, Damage }

public class PlayerControllerAI : MonoBehaviour
{
    public Face faces;
    public GameObject SmileBody;
    public SlimeAnimationState currentState;

    public Animator animator;
    public float moveSpeed = 3f;  // Adjusted moveSpeed for faster movement
    public float jumpForce = 6.5f; // Adjusted jumpForce for better jumping
    public float rotationSpeed = 720f; // Degrees per second

    private bool isGrounded; // To check if the player is on the ground

    // Collectible-related variables
    public TextMeshProUGUI scoreText;
    public int score;

    // Gravity and movement
    private Rigidbody rb;
    public int damType; // Field to store damage type
    private Material faceMaterial;

    // Checkpoint-related variables
    private Vector3 lastCheckpointPosition;

    // Puzzle reset system
    public GameObject[] puzzleObjects;
    private Vector3[] initialPuzzlePositions;

    // Softlock detection
    private float idleTime = 0f;
    private const float softlockTimeout = 30f;

    // Audio
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip jumpSound; // AudioClip for jump sound
    public AudioClip attackSound; // AudioClip for attack sound

    // Chronometer
    private float startTime;
    private float elapsedTime;
    public TextMeshProUGUI timeText; // Reference to the UI text for displaying the time
    public bool pauseGame = false;
    // UI elements for the win screen
    public GameObject winScreen;
    public TextMeshProUGUI finalScoreText; // UI element to display the final score on Game Over screen
    public TextMeshProUGUI finalTimeText; // Final time UI text (Chronometer)

    void Start()
    {
        // Reset the game-over flag when the game starts
        pauseGame = false;

        rb = GetComponent<Rigidbody>();
        faceMaterial = SmileBody.GetComponent<Renderer>().materials[1];
        currentState = SlimeAnimationState.Idle; // Start in Idle state
        lastCheckpointPosition = transform.position; // Initialize checkpoint at the start
        // Initialize collectibles
        score = 0;
        SetScoreText();

        // Lock rotation on the X and Z axes (only rotate around Y)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Ensure the AudioSource is attached to the player
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Initialize puzzle object positions for reset
        initialPuzzlePositions = new Vector3[puzzleObjects.Length];
        for (int i = 0; i < puzzleObjects.Length; i++)
        {
            initialPuzzlePositions[i] = puzzleObjects[i].transform.position;
        }

        // Initialize the timer
        startTime = Time.time;
        elapsedTime = 0;
    }

    void Update()
    {
        // If the game is over, stop all updates (no input or timer updates)
        if (pauseGame) return;

        HandlePlayerInput();
        UpdateAnimationState();
        DetectSoftlock();
        UpdateTime();
    }

    // Update the time
    void UpdateTime()
    {
        elapsedTime = Time.time - startTime;
        DisplayTime(elapsedTime);
    }

    void Jump()
    {
        isGrounded = false; // Set to false immediately after jump
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        PlaySound(jumpSound); // Play the jump sound
        currentState = SlimeAnimationState.Jump;
        SetFace(faces.jumpFace);
        animator.SetTrigger("Jump");
    }

    void HandlePlayerInput()
    {
        // Get player input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create a movement vector based on input
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // Move the player if there is any input
        if (direction.magnitude >= 0.2f)
        {
            // Calculate the target angle to rotate towards (Y axis only)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Smoothly rotate towards the target angle
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);

            // Rotate the player on the Y axis only
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

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
            Jump();
        }

        // Attack logic (bind to a specific key, e.g., spacebar)
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            currentState = SlimeAnimationState.Attack;
            PlaySound(attackSound); // Play the attack sound
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
            score += 100; // Increase the count of collected items

            SetScoreText(); // Update the UI
        }
        else if (other.gameObject.CompareTag("PickUpW")) // Check if the player reached a checkpoint
        {
            other.gameObject.SetActive(false);
            TriggerWinScreen();
        }
    }

    // Update the UI with the current collectible count
    void SetScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Ensure proper ground tagging
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // Softlock detection based on player idle time
    void DetectSoftlock()
    {
        if (rb.velocity.magnitude < 0.1f)
        {
            idleTime += Time.deltaTime;
            if (idleTime > softlockTimeout)
            {
                idleTime = 0; // Reset idle timer
                ResetPuzzle(); // Reset the puzzle objects
                ReloadLastCheckpoint(); // Reload the last checkpoint
            }
        }
        else
        {
            idleTime = 0;
        }
    }

    // Reset the puzzle objects to their initial positions
    public void ResetPuzzle()
    {
        for (int i = 0; i < puzzleObjects.Length; i++)
        {
            puzzleObjects[i].transform.position = initialPuzzlePositions[i];
        }
    }

    // Reload the last checkpoint
    void ReloadLastCheckpoint()
    {
        transform.position = lastCheckpointPosition;
    }

    // Function to play a given sound
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Display the time in the UI
    void DisplayTime(float timeToDisplay)
    {
        // Format the time as minutes:seconds:milliseconds
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = timeToDisplay % 1 * 1000;

        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    // Stop the chronometer and prevent further updates when the game is over
    public void StopChronometer()
    {
        pauseGame = true; // Set the flag to true, stopping all updates
    }

    // Function to trigger the win screen
    void TriggerWinScreen()
    {
        if (!winScreen.activeSelf) // Prevent re-triggering
        {
            winScreen.SetActive(true); // Show the win screen UI
            StopChronometer(); // Stop the game chronometer (if required)
            StopMovement(); // Stop the player's movement

            // Display the final score on the Win screen
            finalScoreText.text = "Final Score: " + score.ToString();
            DisplayFinalTime(elapsedTime);
        }
    }

    void DisplayFinalTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = timeToDisplay % 1 * 1000;

        finalTimeText.text = string.Format("Final Time: {0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

}