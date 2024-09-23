using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverMenu; // The Game Over UI Canvas
    public PlayerControllerAI playerController; // Reference to the player's script for tracking death
    public PlayerHealth playerHealth; // Reference to the player's health script
    public float fallThresholdY = -10f;
    public TextMeshProUGUI finalScoreText; // UI element to display the final score on Game Over screen
    public TextMeshProUGUI finalTimeText; // Final time UI text (Chronometer)

    void Start()
    {
        // Hide Game Over menu at the start
        gameOverMenu.SetActive(false);
    }

    void Update()
    {
        // Check if the player has fallen below a threshold or lost all health
        if (playerController.transform.position.y < fallThresholdY || playerHealth.isDead)
        {
            TriggerGameOver();
        }
    }

    // Function to show Game Over Menu
    public void TriggerGameOver()
    {
        // Stop the chronometer in the player controller
        playerController.StopChronometer();

        // Show the Game Over UI
        gameOverMenu.SetActive(true);

        // Display the final score on the Game Over screen
        finalScoreText.text = "Final Score: " + playerController.score.ToString();
        DisplayFinalTime(playerController.GetElapsedTime());
    }

    // Function for restarting the game
    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // Function for returning to the main menu
    public void ReturnToMainMenu()
    {
        // Load the Main Menu scene (you need to add the scene in the build settings)
        SceneManager.LoadScene("MainMenu");
    }

    // Display the final time in the Game Over screen
    void DisplayFinalTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float milliseconds = timeToDisplay % 1 * 1000;

        finalTimeText.text = string.Format("Final Time: {0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
