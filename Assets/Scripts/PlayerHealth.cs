using UnityEngine;
using UnityEngine.UI; // For UI components

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Maximum health of the player
    private float currentHealth;   // Current health of the player

    public Slider healthBar;       // Reference to the UI health bar slider

    public bool isDead = false;   // Track if the player is dead
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip hitSound; // AudioClip for jump sound

    void Start()
    {
        // Initialize player's health to max at the start
        currentHealth = maxHealth;

        // Initialize the health bar to the correct value
        healthBar.value = CalculateHealth();

        // Ensure the AudioSource is attached to the player
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // Function to apply damage to the player
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return; // If the player is already dead, do nothing

        // Reduce current health by the damage amount
        currentHealth -= damageAmount;
        PlaySound(hitSound); // Play the hit sound

        // Update the health bar
        healthBar.value = CalculateHealth();

        // Check if the player's health has reached 0
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Function to calculate the current health as a percentage
    float CalculateHealth()
    {
        return currentHealth / maxHealth;
    }

    // Function to handle the player's death
    void Die()
    {
        isDead = true;
        // Here you can add additional logic for what happens when the player dies, such as triggering a Game Over screen
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

}
