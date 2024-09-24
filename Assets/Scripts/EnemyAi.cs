
using UnityEngine;
using UnityEngine.AI;
public class EnemyAi : MonoBehaviour
{

    public Face faces;
    public GameObject SmileBody;
    public SlimeAnimationState currentState; 
   
    public Animator animator;
    public int damType;
    private Material faceMaterial;
    private Vector3 originPos;

    // Player-related variables
    public Transform player; // Reference to the player's transform
    public float moveSpeed = 1.75f; // Speed at which the enemy moves
    public float increasedSpeed = 2.5f; // Speed after 1 minute
    public float attackRange = 1f; // The range at which the enemy can attack the player
    public float damage = 15f; // Damage dealt to the player
    private PlayerHealth playerHealth; // Reference to the player's health script

    public PlayerControllerAI playerController; // Reference to the player's script for tracking death

    // Timer-related variables
    private float startTime; // Time the game started
    private float elapsedTime; // Elapsed time since the game started
    public float attackCooldown = 2f; // Time in seconds between attacks
    private float lastAttackTime = 0f; // Time when the last attack happened

    // Audio
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip attackSound; // AudioClip for attack sound

    void Start()
    {
        faceMaterial = SmileBody.GetComponent<Renderer>().materials[1];

        // Dynamically find the player by tag when the enemy is instantiated
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerControllerAI>();
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Record the start time
        startTime = Time.time;
    }
    void Update()
    {
        if (player == null || playerController.pauseGame) return;

        // Calculate the elapsed time
        elapsedTime = Time.time - startTime;

        // Check if more than 1 minute has passed
        if (elapsedTime > 60f)
        {
            // Increase the enemy's speed after 1 minute
            moveSpeed = increasedSpeed;
        }
        
        HandleEnemyMovement();
    }

    void HandleEnemyMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If within attack range, stop moving and attack
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            FollowPlayer();
        }
    }

    void FollowPlayer()
    {    
        // Move towards the player
        Vector3 direction = (player.position - transform.position).normalized;
        float speed = moveSpeed * Time.deltaTime;

        // Set the Speed parameter in the Animator to control the walking animation
        animator.SetFloat("Speed", moveSpeed);

        // Move the enemy towards the player
        transform.position += direction * speed;

        // Rotate to face the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    void AttackPlayer()
    {

        // Check if enough time has passed since the last attack
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            
            PlaySound(attackSound); // Play the attack sound

            animator.SetFloat("Speed", 0);
            animator.SetTrigger("Attack");

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Record the time of the attack
            lastAttackTime = Time.time;
        }
    }

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

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
