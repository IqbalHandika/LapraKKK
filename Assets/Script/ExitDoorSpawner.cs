using UnityEngine;

/// <summary>
/// Spawn Dosen when exit door is opened - creates dramatic chase sequence
/// </summary>
public class ExitDoorSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject dosenPrefab;
    [Tooltip("Posisi spawn Dosen (di depan pintu exit)")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    
    [Header("Dosen Behavior")]
    [Tooltip("Target yang akan dikejar Dosen (biasanya Player)")]
    [SerializeField] private Transform chaseTarget;
    [SerializeField] private bool dosenChasesImmediately = true;
    
    [Header("Door Reference")]
    [Tooltip("Optional: Reference ke ExitDoor script untuk auto-detect opening")]
    [SerializeField] private ExitDoor exitDoor;
    
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasSpawned = false;
    private GameObject spawnedDosen;
    
    void Start()
    {
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Auto-find player if not set
        if (chaseTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                chaseTarget = player.transform;
            }
        }
        
        // Validate settings
        if (dosenPrefab == null)
        {
            Debug.LogError("[ExitDoorSpawner] Dosen Prefab not assigned!");
        }
        
        if (spawnPoint == null)
        {
            Debug.LogWarning("[ExitDoorSpawner] Spawn Point not set - will use this object's position");
            spawnPoint = transform;
        }
        
        if (exitDoor == null)
        {
            Debug.LogWarning("[ExitDoorSpawner] ExitDoor not assigned - auto-detection disabled");
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[ExitDoorSpawner] Initialized. Ready to spawn Dosen at {spawnPoint.position}");
        }
    }
    
    private bool doorWasOpen = false; // Track previous state to detect transition
    
    void Update()
    {
        // Early exit if already spawned
        if (hasSpawned) return;
        
        // Auto-detect door opening if ExitDoor reference is set
        if (exitDoor != null)
        {
            bool doorIsOpen = exitDoor.IsUnlocked && exitDoor.IsOpen;
            
            // Only spawn on transition from closed to open (prevent multiple spawns)
            if (doorIsOpen && !doorWasOpen)
            {
                if (showDebugLogs)
                {
                    Debug.Log("[ExitDoorSpawner] Exit door just opened! Spawning Dosen...");
                }
                SpawnDosen();
            }
            
            doorWasOpen = doorIsOpen;
        }
    }
    
    /// <summary>
    /// Spawn Dosen at spawn point (call this manually or auto-triggered by door)
    /// </summary>
    public void SpawnDosen()
    {
        // Double-check to prevent multiple spawns
        if (hasSpawned)
        {
            if (showDebugLogs)
            {
                Debug.Log("[ExitDoorSpawner] Already spawned Dosen - ignoring spawn request");
            }
            return;
        }
        
        if (dosenPrefab == null)
        {
            Debug.LogError("[ExitDoorSpawner] Cannot spawn - Dosen Prefab not assigned!");
            return;
        }
        
        // Mark as spawned IMMEDIATELY to prevent duplicate Invoke calls
        hasSpawned = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[ExitDoorSpawner] Spawn requested - will execute in " + spawnDelay + "s");
        }
        
        // Delay spawn for dramatic effect
        Invoke(nameof(ExecuteSpawn), spawnDelay);
    }
    
    void ExecuteSpawn()
    {
        // hasSpawned already set to true in SpawnDosen()
        
        // Spawn Dosen
        spawnedDosen = Instantiate(dosenPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedDosen.name = "Dosen_ExitChaser";
        
        if (showDebugLogs)
        {
            Debug.Log($"[ExitDoorSpawner] Spawned Dosen at {spawnPoint.position}");
        }
        
        // Play spawn sound
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        // Setup chase behavior
        if (dosenChasesImmediately && chaseTarget != null)
        {
            // Find DosenAI component
            DosenAI dosenAI = spawnedDosen.GetComponent<DosenAI>();
            if (dosenAI != null)
            {
                // Force Dosen to chase player immediately
                dosenAI.SetChaseTarget(chaseTarget);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[ExitDoorSpawner] Dosen now chasing: {chaseTarget.name}");
                }
            }
            else
            {
                Debug.LogWarning("[ExitDoorSpawner] DosenAI component not found on spawned Dosen!");
            }
        }
    }
    
    /// <summary>
    /// Check if Dosen has been spawned
    /// </summary>
    public bool HasSpawned()
    {
        return hasSpawned;
    }
    
    /// <summary>
    /// Get reference to spawned Dosen
    /// </summary>
    public GameObject GetSpawnedDosen()
    {
        return spawnedDosen;
    }
    
    /// <summary>
    /// Manually trigger spawn (untuk testing atau custom events)
    /// </summary>
    public void TriggerSpawn()
    {
        SpawnDosen();
    }
    
    void OnDrawGizmos()
    {
        // Draw spawn point
        if (spawnPoint != null)
        {
            Gizmos.color = hasSpawned ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 2f, "Dosen Spawn Point");
            #endif
        }
        
        // Draw line to chase target
        if (chaseTarget != null && spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnPoint.position, chaseTarget.position);
        }
    }
}
