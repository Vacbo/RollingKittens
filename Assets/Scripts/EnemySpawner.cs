using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;       // Reference to the enemy prefab
    public int numberOfEnemies = 5;      // Number of enemies to spawn
    public Transform player;             // Reference to the player's transform
    public float spawnRadius = 10f;      // Radius around the player within which enemies will spawn
    public float minSpawnDistance = 2f;  // Minimum distance from the player to avoid spawning too close
    public string groundTag = "Ground";  // The tag of the ground object
    public float spawnHeightOffset = 10f; // Height above player position to cast the ray from


    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // If a valid position is found, spawn the enemy
            if (spawnPosition != Vector3.zero)
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                // Assign the player's transform to the enemy's AI script
                EnemyAi enemyAi = newEnemy.GetComponent<EnemyAi>();
                if (enemyAi != null)
                {
                    enemyAi.player = player; // Set the player reference
                }
            }
        }
    }

    // Generate a random position within the spawnRadius around the player
    Vector3 GetRandomSpawnPosition()
    {
        for (int attempt = 0; attempt < 10; attempt++) // Try up to 10 times to find a valid position
        {
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += player.position;

            // Ensure the Y is above the player to cast downward
            randomDirection.y = player.position.y + spawnHeightOffset;

            // Ensure the enemy is not spawned too close to the player
            float distanceToPlayer = Vector3.Distance(player.position, randomDirection);
            if (distanceToPlayer < minSpawnDistance)
            {
                continue; // Skip this position and try again
            }

            // Cast a ray downward to find the ground
            RaycastHit hit;
            if (Physics.Raycast(randomDirection, Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag(groundTag))
                {
                    return hit.point; // Return the ground point where the enemy should spawn
                }
            }
        }

        return Vector3.zero; // If no valid position found after 10 attempts, return zero
    }
}