using System.Collections;
using UnityEngine;
using Pathfinding;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool canClose = true;
    
    [Header("Lock Settings")]
    [Tooltip("Is this door locked? Requires key to unlock first")]
    [SerializeField] private bool isLocked = false;
    [Tooltip("Required key item ID to unlock this door (e.g., 'room_key_01')")]
    [SerializeField] private string requiredKeyID = "";
    [Tooltip("Door name shown in unlock prompt (e.g., 'Office Door')")]
    [SerializeField] private string doorName = "Door";
    private bool isUnlocked = false; // Track if door has been unlocked
    private bool wasOpenedByPlayer = false; // Track if player manually opened this door
    
    [Header("Dosen Auto-Open/Close")]
    [Tooltip("Allow Dosen to open even when locked")]
    [SerializeField] private bool dosenCanOpenLocked = true;
    [Tooltip("Auto-close after Dosen leaves (only if player didn't open)")]
    [SerializeField] private bool autoCloseAfterDosen = true;
    [Tooltip("Time to wait before auto-closing after Dosen leaves")]
    [SerializeField] private float autoCloseDelay = 2f;
    private bool dosenRequestedOpen = false; // Track if Dosen opened the door
    private Coroutine autoCloseCoroutine = null;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [Tooltip("Sound when unlocking door with key")]
    [SerializeField] private AudioClip unlockSound;
    [Tooltip("Sound when trying locked door without key")]
    [SerializeField] private AudioClip lockedSound;
    
    [Header("Enemy Detection (Auto-Open)")]
    [SerializeField] private bool enableEnemyDetection = true;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private Vector3 detectionCenterOffset = Vector3.zero; // Offset from pivot (adjust if pivot is at top)
    [SerializeField] private float actionCooldown = 0.5f; // Prevent rapid open/close
    [SerializeField] private LayerMask enemyLayer = -1; // Layer mask for optimization (set to "Enemy" layer)
    
    [Header("Performance Settings")]
    [Tooltip("How often to check for enemies (seconds). Lower = more responsive, higher = better performance")]
    [SerializeField] private float checkInterval = 0.2f; // Check every 0.2 seconds instead of every frame
    
    [Header("A* Pathfinding Integration")]
    [SerializeField] private bool updateAstarGraph = true;
    [Tooltip("Bounds size for A* graph update (should cover door area)")]
    [SerializeField] private Vector3 graphUpdateBounds = new Vector3(2f, 3f, 0.5f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showPerformanceStats = false;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private bool isAnimating = false;
    
    // Enemy detection variables
    private bool enemyInRange = false;
    private float lastActionTime = -999f; // Track last open/close action
    
    // Performance optimization variables
    private float nextCheckTime = 0f; // Timer for interval-based checks
    private Collider[] colliderCache = new Collider[10]; // Reusable array to avoid allocations
    private int performanceCheckCount = 0; // Track number of checks for performance stats

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + openAngle, transform.eulerAngles.z);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        // OPTIMIZATION 1: Early exit if enemy detection is disabled
        if (!enableEnemyDetection) return;
        
        // OPTIMIZATION 2: Early exit if door is already open
        // Once opened by enemy, stays open (no auto-close)
        if (isOpen) return;
        
        // OPTIMIZATION 3: Interval-based checks instead of every frame
        // Reduces Physics.OverlapSphere calls by ~80% (60 FPS → 5 checks/second)
        if (Time.time >= nextCheckTime)
        {
            CheckForEnemies();
            nextCheckTime = Time.time + checkInterval;
            
            if (showPerformanceStats)
            {
                performanceCheckCount++;
            }
        }
    }
    
    /// <summary>
    /// Check if any enemy is within detection radius
    /// OPTIMIZED: Uses layer mask, cached array, and early exit
    /// </summary>
    void CheckForEnemies()
    {
        // OPTIMIZATION 4: Early exit if door is already open (unless we need to track enemies for closing)
        if (isOpen && !enemyInRange)
        {
            return; // Door is open but we're not tracking enemies, skip check
        }
        
        // OPTIMIZATION 5: Use layer mask to only check "Enemy" layer
        // This reduces the number of colliders checked dramatically
        // Expected improvement: 50-90% fewer colliders to iterate
        Vector3 detectionCenter = transform.position + transform.TransformDirection(detectionCenterOffset);
        int numColliders = Physics.OverlapSphereNonAlloc(
            detectionCenter, 
            detectionRadius, 
            colliderCache,
            enemyLayer
        );
        
        bool enemyDetected = false;
        string detectedEnemyName = "";
        
        // OPTIMIZATION 6: Cache component lookup results
        // Check if any collider has "Enemy" tag
        // Note: Layer mask already filters, but tag check adds extra safety
        for (int i = 0; i < numColliders; i++)
        {
            Collider col = colliderCache[i];
            
            // Skip null entries (shouldn't happen, but safety check)
            if (col == null) continue;
            
            if (col.CompareTag("Enemy"))
            {
                enemyDetected = true;
                detectedEnemyName = col.gameObject.name;
                break; // Early exit on first enemy found
            }
        }
        
        // Check cooldown to prevent rapid toggling
        bool canPerformAction = (Time.time - lastActionTime) >= actionCooldown;
        
        // Enemy entered range - open door
        if (enemyDetected && !isOpen && !isAnimating && canPerformAction)
        {
            enemyInRange = true;
            lastActionTime = Time.time;
            
            if (showDebugLogs)
            {
                Debug.Log($"[Door] '{gameObject.name}' detected enemy '{detectedEnemyName}' - Opening door");
            }
            
            // Dosen can open locked doors
            if (isLocked && !isUnlocked && dosenCanOpenLocked)
            {
                DosenRequestOpen();
            }
            else
            {
                Interact(); // Normal open
            }
        }
        // Enemy left range - only track state (close will be triggered by patrol point)
        else if (!enemyDetected && enemyInRange)
        {
            enemyInRange = false;
            
            if (showDebugLogs)
            {
                Debug.Log($"[Door] Enemy left detection range of '{doorName}'");
            }
            
            // Don't auto-close here - let DosenAI close door when reaching patrol point
        }
    }
    public void Interact()
    {
        if (isAnimating) return;
        
        // STEP 1: Check if door is locked and needs unlocking
        if (isLocked && !isUnlocked)
        {
            // Try to unlock
            if (GameManager.Instance != null && GameManager.Instance.HasKey(requiredKeyID))
            {
                // Unlock successful!
                isUnlocked = true;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[Door] '{doorName}' unlocked with key '{requiredKeyID}'!");
                }
                
                // Play unlock sound
                if (audioSource != null && unlockSound != null)
                {
                    audioSource.PlayOneShot(unlockSound);
                }
            }
            else
            {
                // Don't have key - door stays locked
                if (showDebugLogs)
                {
                    Debug.Log($"[Door] '{doorName}' is locked! Need key: '{requiredKeyID}'");
                }
                
                // Play locked sound
                if (audioSource != null && lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
            }
            return; // Exit - don't open door yet
        }
        
        // STEP 2: Door is unlocked (or not locked) - toggle open/close
        if (isOpen && !canClose) return;
        
        // Mark that player opened this door (prevents auto-close)
        if (!isOpen)
        {
            wasOpenedByPlayer = true;
            
            // Cancel any auto-close timer
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
        
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(ToggleDoor());
    }

    public string GetInteractPrompt()
    {
        if (isAnimating) return "";
        
        // Show unlock prompt if locked
        if (isLocked && !isUnlocked)
        {
            bool hasKey = GameManager.Instance != null && GameManager.Instance.HasKey(requiredKeyID);
            if (hasKey)
            {
                return $"E - unlock {doorName}";
            }
            else
            {
                return $"{doorName} is Locked";
            }
        }
        
        // Show open/close prompt if unlocked
        if (isOpen && !canClose) return "";
        
        return isOpen ? "E - close the door" : "E - open the door";
    }

    public bool IsOpen()
    {
        return isOpen;
    }
    
    /// <summary>
    /// Dosen requests to open door (can open locked doors)
    /// </summary>
    public void DosenRequestOpen()
    {
        if (isAnimating || isOpen) return;
        
        if (showDebugLogs)
        {
            Debug.Log($"[Door] Dosen opening '{doorName}' (locked: {isLocked})");
        }
        
        // Mark that Dosen opened this (for auto-close)
        dosenRequestedOpen = true;
        
        // Open door (bypass lock check)
        isOpen = true;
        StopAllCoroutines();
        StartCoroutine(ToggleDoor());
    }
    
    /// <summary>
    /// Close door when Dosen reaches patrol point (called by DosenAI)
    /// </summary>
    public void DosenRequestClose()
    {
        // Only close if: door was opened by Dosen AND player hasn't opened it
        if (!isOpen || !dosenRequestedOpen || wasOpenedByPlayer)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[Door] DosenRequestClose ignored for '{doorName}' (opened by player or already closed)");
            }
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[Door] Dosen closing and locking '{doorName}'");
        }
        
        // Close and reset state
        isOpen = false;
        dosenRequestedOpen = false;
        
        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(ToggleDoor());
        }
    }
    
    /// <summary>
    /// Auto-close door after Dosen leaves (only if player didn't open)
    /// </summary>
    IEnumerator AutoCloseAfterDelay()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[Door] Dosen left - auto-closing '{doorName}' in {autoCloseDelay}s");
        }
        
        yield return new WaitForSeconds(autoCloseDelay);
        
        // Double-check: only close if player still hasn't opened
        if (isOpen && !wasOpenedByPlayer && dosenRequestedOpen)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[Door] Auto-closing and locking '{doorName}'");
            }
            
            isOpen = false;
            dosenRequestedOpen = false;
            StopAllCoroutines();
            StartCoroutine(ToggleDoor());
        }
        
        autoCloseCoroutine = null;
    }

    private void UpdateAstarGraph()
    {
        if (!updateAstarGraph || AstarPath.active == null) return;

        // Create bounds for graph update centered on door
        Bounds bounds = new Bounds(transform.position, graphUpdateBounds);
        
        // Create graph update object
        GraphUpdateObject guo = new GraphUpdateObject(bounds);
        
        // Update the graph - this will recalculate walkability for nodes in the bounds
        AstarPath.active.UpdateGraphs(guo);
        
        if (showDebugLogs)
        {
            Debug.Log($"[Door] A* graph updated at {transform.position} - Door is now {(isOpen ? "OPEN (walkable)" : "CLOSED (unwalkable)")}");
        }
    }

    private IEnumerator ToggleDoor()
    {
        isAnimating = true;
        
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        
        if (audioSource != null)
        {
            AudioClip clipToPlay = isOpen ? openSound : closeSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isAnimating = false;
        
        // Update A* graph after door animation completes
        UpdateAstarGraph();
    }
    
    /// <summary>
    /// Draw detection radius in Scene View
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (enableEnemyDetection)
        {
            // Calculate detection center with offset
            Vector3 detectionCenter = Application.isPlaying 
                ? transform.position + transform.TransformDirection(detectionCenterOffset)
                : transform.position + detectionCenterOffset;
            
            // Draw yellow wire sphere showing detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionCenter, detectionRadius);
            
            // Draw line from pivot to detection center
            if (detectionCenterOffset != Vector3.zero)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, detectionCenter);
                Gizmos.DrawWireSphere(transform.position, 0.1f); // Show pivot point
            }
            
            // Draw A* graph update bounds
            if (updateAstarGraph)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
                Gizmos.DrawWireCube(transform.position, graphUpdateBounds);
            }
            
            // Performance stats visualization
            if (showPerformanceStats && Application.isPlaying)
            {
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    detectionCenter + Vector3.up * 2f,
                    $"Checks: {performanceCheckCount}\nInterval: {checkInterval}s\nEstimated FPS impact: {(1f / checkInterval):F1} checks/sec"
                );
                #endif
            }
        }
    }
}