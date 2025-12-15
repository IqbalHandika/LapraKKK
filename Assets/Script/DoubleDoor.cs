using System.Collections;
using UnityEngine;
using Pathfinding;

public class DoubleDoor : MonoBehaviour, IInteractable
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    
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
    [Tooltip("Door name shown in unlock prompt (e.g., 'Main Hall Door')")]
    [SerializeField] private string doorName = "Double Door";
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
    [SerializeField] private LayerMask enemyLayer = -1; // Layer mask for optimization
    
    [Header("Performance Settings")]
    [Tooltip("How often to check for enemies (seconds). Lower = more responsive, higher = better performance")]
    [SerializeField] private float checkInterval = 0.2f;
    
    [Header("A* Pathfinding Integration")]
    [SerializeField] private bool updateAstarGraph = true;
    [Tooltip("Bounds size for A* graph update (should cover door area)")]
    [SerializeField] private Vector3 graphUpdateBounds = new Vector3(2f, 3f, 0.5f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showPerformanceStats = false;
    
    private Quaternion leftClosedRotation;
    private Quaternion leftOpenRotation;
    private Quaternion rightClosedRotation;
    private Quaternion rightOpenRotation;
    
    private AudioSource audioSource;
    private bool isAnimating = false;
    
    // Enemy detection variables
    private bool enemyInRange = false;
    private float lastActionTime = -999f; // Track last open/close action
    
    // Performance optimization variables
    private float nextCheckTime = 0f;
    private Collider[] colliderCache = new Collider[10];
    private int performanceCheckCount = 0;

    void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("DoubleDoor: Left Door or Right Door reference is missing!");
            return;
        }
        
        leftClosedRotation = leftDoor.rotation;
        leftOpenRotation = Quaternion.Euler(leftDoor.eulerAngles.x, leftDoor.eulerAngles.y - openAngle, leftDoor.eulerAngles.z);
        
        rightClosedRotation = rightDoor.rotation;
        rightOpenRotation = Quaternion.Euler(rightDoor.eulerAngles.x, rightDoor.eulerAngles.y + openAngle, rightDoor.eulerAngles.z);
        
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
        // OPTIMIZATION 4: Early exit if door is already open
        if (isOpen && !enemyInRange)
        {
            return;
        }
        
        // OPTIMIZATION 5: Use layer mask and NonAlloc version
        Vector3 detectionCenter = transform.position + transform.TransformDirection(detectionCenterOffset);
        int numColliders = Physics.OverlapSphereNonAlloc(
            detectionCenter, 
            detectionRadius, 
            colliderCache,
            enemyLayer
        );
        
        bool enemyDetected = false;
        string detectedEnemyName = "";
        
        // OPTIMIZATION 6: Efficient iteration with early exit
        for (int i = 0; i < numColliders; i++)
        {
            Collider col = colliderCache[i];
            if (col == null) continue;
            
            if (col.CompareTag("Enemy"))
            {
                enemyDetected = true;
                detectedEnemyName = col.gameObject.name;
                break;
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
                Debug.Log($"[DoubleDoor] '{gameObject.name}' detected enemy '{detectedEnemyName}' - Opening doors");
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
                Debug.Log($"[DoubleDoor] Enemy left detection range of '{doorName}'");
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
                    Debug.Log($"[DoubleDoor] '{doorName}' unlocked with key '{requiredKeyID}'!");
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
                    Debug.Log($"[DoubleDoor] '{doorName}' is locked! Need key: '{requiredKeyID}'");
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
        StartCoroutine(ToggleDoubleDoor());
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
            Debug.Log($"[DoubleDoor] Dosen opening '{doorName}' (locked: {isLocked})");
        }
        
        // Mark that Dosen opened this (for auto-close)
        dosenRequestedOpen = true;
        
        // Open door (bypass lock check)
        isOpen = true;
        StopAllCoroutines();
        StartCoroutine(ToggleDoubleDoor());
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
                Debug.Log($"[DoubleDoor] DosenRequestClose ignored for '{doorName}' (opened by player or already closed)");
            }
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[DoubleDoor] Dosen closing and locking '{doorName}'");
        }
        
        // Close and reset state
        isOpen = false;
        dosenRequestedOpen = false;
        
        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(ToggleDoubleDoor());
        }
    }
    
    /// <summary>
    /// Auto-close door after Dosen leaves (only if player didn't open)
    /// </summary>
    IEnumerator AutoCloseAfterDelay()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[DoubleDoor] Dosen left - auto-closing '{doorName}' in {autoCloseDelay}s");
        }
        
        yield return new WaitForSeconds(autoCloseDelay);
        
        // Double-check: only close if player still hasn't opened
        if (isOpen && !wasOpenedByPlayer && dosenRequestedOpen)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[DoubleDoor] Auto-closing and locking '{doorName}'");
            }
            
            isOpen = false;
            dosenRequestedOpen = false;
            StopAllCoroutines();
            StartCoroutine(ToggleDoubleDoor());
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
            Debug.Log($"[DoubleDoor] A* graph updated at {transform.position} - Door is now {(isOpen ? "OPEN (walkable)" : "CLOSED (unwalkable)")}");
        }
    }

    private IEnumerator ToggleDoubleDoor()
    {
        isAnimating = true;
        
        Quaternion leftTarget = isOpen ? leftOpenRotation : leftClosedRotation;
        Quaternion rightTarget = isOpen ? rightOpenRotation : rightClosedRotation;
        
        if (audioSource != null)
        {
            AudioClip clipToPlay = isOpen ? openSound : closeSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        while (Quaternion.Angle(leftDoor.rotation, leftTarget) > 0.1f || 
               Quaternion.Angle(rightDoor.rotation, rightTarget) > 0.1f)
        {
            leftDoor.rotation = Quaternion.Slerp(leftDoor.rotation, leftTarget, Time.deltaTime * openSpeed);
            rightDoor.rotation = Quaternion.Slerp(rightDoor.rotation, rightTarget, Time.deltaTime * openSpeed);
            yield return null;
        }
        
        leftDoor.rotation = leftTarget;
        rightDoor.rotation = rightTarget;
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