using UnityEngine;

/// <summary>
/// Trigger area at the end of exit hallway
/// Spawns enemy at exit door and triggers final chase sequence
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitHallwayTrigger : MonoBehaviour
{
    [Header("Enemy Spawn")]
    [Tooltip("Enemy prefab to spawn (DosenAI)")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Tooltip("Transform where enemy will spawn (at exit door)")]
    [SerializeField] private Transform enemySpawnPoint;
    
    [Tooltip("Or use manual spawn position")]
    [SerializeField] private Vector3 manualSpawnPosition;
    
    [Tooltip("Use spawn point Transform or manual position")]
    [SerializeField] private bool useSpawnPoint = true;
    
    [Header("Final Chase Settings")]
    [Tooltip("Enemy move speed during final chase")]
    [SerializeField] private float finalChaseSpeed = 7f;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound played when enemy spawns (dramatic music sting)")]
    [SerializeField] private AudioClip spawnSound;
    
    [Tooltip("Background music for final chase")]
    [SerializeField] private AudioClip chaseMusic;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect at spawn location")]
    [SerializeField] private GameObject spawnEffect;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggered = false;
    private GameObject spawnedEnemy;
    
    void Start()
    {
        // Validate collider is trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[ExitHallwayTrigger] Collider is not set to trigger! Setting isTrigger = true.");
            col.isTrigger = true;
        }
        
        if (enemyPrefab == null)
        {
            Debug.LogError("[ExitHallwayTrigger] Enemy Prefab not assigned! Assign DosenAI prefab.");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Only trigger once
        if (hasTriggered) return;
        
        // Check if player entered
        if (!other.CompareTag("Player")) return;
        
        hasTriggered = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[ExitHallwayTrigger] Player reached end of hallway! Spawning enemy...");
        }
        
        // Spawn enemy
        SpawnEnemy();
        
        // Play sounds
        PlayAudio();
    }
    
    /// <summary>
    /// Spawn enemy at exit door
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[ExitHallwayTrigger] Cannot spawn enemy - prefab not assigned!");
            return;
        }
        
        // Determine spawn position
        Vector3 spawnPos = useSpawnPoint && enemySpawnPoint != null 
            ? enemySpawnPoint.position 
            : manualSpawnPosition;
        
        // Spawn enemy
        spawnedEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        if (showDebugLogs)
        {
            Debug.Log($"[ExitHallwayTrigger] Enemy spawned at {spawnPos}");
        }
        
        // Spawn visual effect
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);
        }
        
        // Configure enemy for final chase
        ConfigureEnemyForFinalChase();
    }
    
    /// <summary>
    /// Configure spawned enemy for final chase sequence
    /// </summary>
    void ConfigureEnemyForFinalChase()
    {
        if (spawnedEnemy == null) return;
        
        // Get DosenAI component
        DosenAI dosenAI = spawnedEnemy.GetComponent<DosenAI>();
        
        if (dosenAI != null)
        {
            // Enable final chase mode
            dosenAI.EnableFinalChaseMode(finalChaseSpeed);
            
            if (showDebugLogs)
            {
                Debug.Log("[ExitHallwayTrigger] Enemy configured for final chase mode");
            }
        }
        else
        {
            Debug.LogWarning("[ExitHallwayTrigger] Spawned enemy doesn't have DosenAI component!");
        }
    }
    
    /// <summary>
    /// Play audio effects
    /// </summary>
    void PlayAudio()
    {
        // Play spawn sound
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position);
        }
        
        // Play chase music (background)
        if (chaseMusic != null)
        {
            // Find or create audio source for music
            GameObject musicObj = new GameObject("FinalChaseMusic");
            AudioSource musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.clip = chaseMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// Draw gizmos for trigger area
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw trigger area
        Gizmos.color = Color.red;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        
        // Draw spawn point
        Vector3 spawnPos = useSpawnPoint && enemySpawnPoint != null 
            ? enemySpawnPoint.position 
            : manualSpawnPosition;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPos, 1f);
        Gizmos.DrawLine(spawnPos, spawnPos + Vector3.up * 3f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw line from trigger to spawn point
        Vector3 spawnPos = useSpawnPoint && enemySpawnPoint != null 
            ? enemySpawnPoint.position 
            : manualSpawnPosition;
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
}
