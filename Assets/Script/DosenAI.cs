using UnityEngine;
using Pathfinding;

public class DosenAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private AIState startingState = AIState.Patrol;
    
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 6f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private bool randomPatrol = false;
    
    [Header("Detection - Vision System")]
    [SerializeField] private Transform player;
    [Tooltip("Close-range detection (360° - detects player behind/sides)")]
    [SerializeField] private float detectionRange = 5f;
    [Tooltip("Long-range vision distance (forward cone only)")]
    [SerializeField] private float visionRange = 15f;
    [Tooltip("Vision cone angle (like flashlight/spotlight)")]
    [SerializeField] private float visionAngle = 60f;
    [Tooltip("Layers that block line of sight")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("Height of Dosen's eyes for vision raycast")]
    [SerializeField] private float eyeHeight = 1.6f;
    
    [Header("Kill System - Front Radius")]
    [Tooltip("Distance from Dosen to kill player (front-facing)")]
    [SerializeField] private float killRadius = 2f;
    [Tooltip("Angle in front where kill is possible (90 = front hemisphere)")]
    [SerializeField] private float killAngle = 90f;
    [Tooltip("How often to check for kill (performance)")]
    [SerializeField] private float killCheckInterval = 0.1f;
    private float killCheckTimer = 0f;
    private bool isKilling = false;
    
    [Header("Chase Settings")]
    [SerializeField] private float losePlayerTime = 5f;
    
    [Header("Door Detection")]
    [SerializeField] private float doorDetectionRange = 3f;
    [SerializeField] private LayerMask doorLayer;
    
    [Header("Room Entry")]
    [Tooltip("How close Dosen must be to outer/inner points before switching phase")]
    [SerializeField] private float roomPointReachThreshold = 1.5f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    [Header("Kill Sequence")]
    [Tooltip("Scream sound played when catching player")]
    [SerializeField] private AudioClip screamSound;
    [Tooltip("Delay before scream animation (idle first)")]
    [SerializeField] private float screamDelay = 1.5f;
    [Tooltip("Duration of scream animation before killing player")]
    [SerializeField] private float screamAnimationDuration = 2f;
    private AudioSource audioSource;
    
    [Header("Footstep Sounds")]
    [Tooltip("Walking sound (used during patrol)")]
    [SerializeField] private AudioClip walkSound;
    [Tooltip("Running sound (used during chase)")]
    [SerializeField] private AudioClip runSound;
    [Tooltip("Time between footstep sounds when walking")]
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [Tooltip("Time between footstep sounds when running")]
    [SerializeField] private float runFootstepInterval = 0.3f;
    [Tooltip("Volume of footstep sounds (0-1)")]
    [SerializeField] private float footstepVolume = 0.5f;
    private float footstepTimer = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private IAstarAI agent;
    private AIState currentState;
    
    private int currentPatrolIndex = 0;
    private float losePlayerTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool hasPath = false;
    
    // Room entry state machine
    private bool isEnteringRoom = false;
    private PatrolPoint.RoomEntry currentRoomEntry = null;
    private RoomEntryPhase roomEntryPhase = RoomEntryPhase.None;
    private float roomExploreTimer = 0f;
    private AIState stateBeforeRoomEntry = AIState.Patrol; // Store state to return to after room entry
    
    // Final chase mode (for win condition)
    private bool isFinalChase = false;
    
    private enum RoomEntryPhase
    {
        None,
        MovingToOuter,   // Moving to doorway (outer point)
        WaitingForDoor,  // Waiting for door to open
        MovingToInner,   // Moving inside room (inner point)
        Exploring,       // Exploring inside room
        ReturnToOuter    // Returning to outer point before continuing patrol
    }
    
    // Animation parameters
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int screamHash = Animator.StringToHash("Scream"); // Trigger parameter
    
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Search
    }
    
    void Start()
    {
        agent = GetComponent<IAstarAI>();
        
        if (agent == null)
        {
            Debug.LogError("DosenAI: AIPath component not found! Please add AIPath component to this GameObject.");
            return;
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource for 3D spatial sound
        audioSource.spatialBlend = 1.0f; // Full 3D (0 = 2D, 1 = 3D)
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 5f; // Full volume within 5 meters
        audioSource.maxDistance = 30f; // Inaudible beyond 30 meters
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        currentState = startingState;
        agent.maxSpeed = patrolSpeed;
    }
    
    void Update()
    {
        if (agent == null || player == null) return;
        
        CheckForDoors();
        
        // Play footstep sounds based on movement
        PlayFootstepSounds();
        
        // Check kill radius periodically (only when not in final chase or already killing)
        if (!isFinalChase && !isKilling)
        {
            killCheckTimer += Time.deltaTime;
            if (killCheckTimer >= killCheckInterval)
            {
                killCheckTimer = 0f;
                CheckKillRadius();
            }
        }
        
        // Handle room entry state machine (takes priority over normal patrol)
        if (isEnteringRoom)
        {
            HandleRoomEntry();
            return; // Skip normal state handling while entering room
        }
        
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdle();
                break;
                
            case AIState.Patrol:
                HandlePatrol();
                break;
                
            case AIState.Chase:
                HandleChase();
                break;
                
            case AIState.Search:
                HandleSearch();
                break;
        }
        
        UpdateAnimations();
    }
    
    void HandleIdle()
    {
        agent.isStopped = true;
        
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }
    }
    
    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("DosenAI: No patrol points assigned!");
            return;
        }
        
        agent.isStopped = false;
        agent.maxSpeed = patrolSpeed;
        
        // Check if reached patrol point
        if (agent.reachedEndOfPath && !agent.pathPending)
        {
            // Close any doors that Dosen opened (before entering room or moving to next point)
            CloseDosenDoors();
            
            // Check if current patrol point has PatrolPoint component
            PatrolPoint patrolPoint = patrolPoints[currentPatrolIndex].GetComponent<PatrolPoint>();
            
            bool roomEntryHandled = false;
            
            // Use PatrolPoint component if available (NEW SYSTEM)
            if (patrolPoint != null && patrolPoint.ShouldEnterRoom())
            {
                PatrolPoint.RoomEntry roomEntry = patrolPoint.GetRandomRoomEntry();
                if (roomEntry != null && roomEntry.IsValid())
                {
                    // Start room entry sequence
                    stateBeforeRoomEntry = currentState; // Save current state
                    currentRoomEntry = roomEntry;
                    isEnteringRoom = true;
                    roomEntryPhase = RoomEntryPhase.MovingToOuter;
                    hasPath = false;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Starting room entry sequence from patrol point '{patrolPoints[currentPatrolIndex].name}' -> Outer: '{roomEntry.outerPoint.name}' -> Inner: '{roomEntry.innerPoint.name}'");
                    }
                    
                    roomEntryHandled = true;
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning($"[DosenAI] PatrolPoint '{patrolPoints[currentPatrolIndex].name}' triggered room entry but no valid room entry found");
                }
            }
            
            // Continue normal patrol if no room entry happened
            if (!roomEntryHandled)
            {
                // Continue normal patrol (no wait time)
                if (randomPatrol)
                {
                    currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                
                agent.destination = patrolPoints[currentPatrolIndex].position;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[DosenAI] Moving to next patrol point: '{patrolPoints[currentPatrolIndex].name}'");
                }
            }
        }
        else if (!hasPath)
        {
            agent.destination = patrolPoints[currentPatrolIndex].position;
            hasPath = true;
        }
        
        // Check for player
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }
    }
    
    void HandleChase()
    {
        agent.isStopped = false;
        agent.maxSpeed = chaseSpeed;
        
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            agent.destination = player.position;
            losePlayerTimer = 0f;
        }
        else
        {
            losePlayerTimer += Time.deltaTime;
            
            if (losePlayerTimer >= losePlayerTime)
            {
                SwitchState(AIState.Search);
            }
        }
    }
    
    void HandleSearch()
    {
        agent.isStopped = false;
        agent.maxSpeed = patrolSpeed;
        
        // Go to last known position
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 2f)
        {
            agent.destination = lastKnownPlayerPosition;
        }
        else
        {
            // Reached last known position, search nearby
            Vector3 randomPoint = GetRandomPointAround(lastKnownPlayerPosition, 10f);
            agent.destination = randomPoint;
        }
        
        // Check if found player again
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }
        
        // Give up after some time
        losePlayerTimer += Time.deltaTime;
        if (losePlayerTimer >= losePlayerTime) // Reduced from losePlayerTime * 2f for faster transition
        {
            // Find nearest patrol point instead of returning to last one
            FindNearestPatrolPoint();
            SwitchState(AIState.Patrol);
        }
    }
    
    /// <summary>
    /// Find and set the nearest patrol point as the current patrol target
    /// </summary>
    void FindNearestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        
        float nearestDistance = float.MaxValue;
        int nearestIndex = 0;
        
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        currentPatrolIndex = nearestIndex;
        
        if (showDebugLogs)
        {
            Debug.Log($"[DosenAI] Lost player. Moving to nearest patrol point: '{patrolPoints[currentPatrolIndex].name}' (Distance: {nearestDistance:F1}m)");
        }
    }
    
    bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 dosenEyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 playerHeadPos = player.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = playerHeadPos - dosenEyePos;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        bool canDetect = false;
        
        // System 1: Vision Cone (long range, narrow angle - FRONT)
        if (distanceToPlayer <= visionRange && angleToPlayer <= visionAngle / 2f)
        {
            canDetect = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"[DosenAI] Player in VISION CONE! Distance: {distanceToPlayer:F1}m, Angle: {angleToPlayer:F1}°");
            }
        }
        // System 2: Proximity Detection (short range, 360° - BEHIND/SIDES)
        else if (distanceToPlayer <= detectionRange)
        {
            canDetect = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"[DosenAI] Player in PROXIMITY! Distance: {distanceToPlayer:F1}m (360° detection)");
            }
        }
        
        if (!canDetect) return false;
        
        // Line of Sight check (walls block vision for both systems)
        RaycastHit hit;
        if (Physics.Raycast(dosenEyePos, directionToPlayer.normalized, out hit, distanceToPlayer, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void CheckForDoors()
    {
        // Raycast forward to detect doors
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up;
        
        if (Physics.Raycast(rayStart, transform.forward, out hit, doorDetectionRange, doorLayer))
        {
            // Check if hit a door
            IInteractable door = hit.collider.GetComponent<IInteractable>();
            if (door != null)
            {
                door.Interact();
            }
            
            // Alternative: Direct Door component
            Door doorComponent = hit.collider.GetComponent<Door>();
            if (doorComponent != null)
            {
                doorComponent.Interact();
            }
            
            // Alternative: DoubleDoor component
            DoubleDoor doubleDoor = hit.collider.GetComponent<DoubleDoor>();
            if (doubleDoor != null)
            {
                doubleDoor.Interact();
            }
        }
    }
    
    /// <summary>
    /// Close all doors that Dosen opened (when reaching patrol point)
    /// </summary>
    void CloseDosenDoors()
    {
        // Find all doors in scene and request close
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door door in doors)
        {
            door.DosenRequestClose();
        }
        
        DoubleDoor[] doubleDoors = FindObjectsOfType<DoubleDoor>();
        foreach (DoubleDoor door in doubleDoors)
        {
            door.DosenRequestClose();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[DosenAI] Reached patrol point - closing Dosen-opened doors");
        }
    }
    
    /// <summary>
    /// Play footstep sounds based on Dosen's movement speed
    /// </summary>
    void PlayFootstepSounds()
    {
        // Don't play footsteps if not moving or if killing
        if (agent.velocity.magnitude < 0.1f || isKilling)
        {
            return;
        }
        
        // Update footstep timer
        footstepTimer -= Time.deltaTime;
        
        if (footstepTimer <= 0f)
        {
            // Determine which sound to play based on state
            AudioClip soundToPlay = null;
            float interval = walkFootstepInterval;
            
            // Use run sound during chase, walk sound otherwise
            if (currentState == AIState.Chase)
            {
                soundToPlay = runSound;
                interval = runFootstepInterval;
            }
            else if (currentState == AIState.Patrol || currentState == AIState.Search)
            {
                soundToPlay = walkSound;
                interval = walkFootstepInterval;
            }
            
            // Play sound if available
            if (soundToPlay != null && audioSource != null)
            {
                audioSource.PlayOneShot(soundToPlay, footstepVolume);
            }
            
            // Reset timer
            footstepTimer = interval;
        }
    }
    
    void HandleRoomEntry()
    {
        if (currentRoomEntry == null || !currentRoomEntry.IsValid())
        {
            if (showDebugLogs)
            {
                Debug.LogWarning("[DosenAI] HandleRoomEntry called but currentRoomEntry is invalid. Returning to patrol.");
            }
            isEnteringRoom = false;
            roomEntryPhase = RoomEntryPhase.None;
            SwitchState(AIState.Patrol);
            return;
        }

        agent.isStopped = false;
        agent.maxSpeed = patrolSpeed;

        switch (roomEntryPhase)
        {
            case RoomEntryPhase.MovingToOuter:
                // Move to outer point (before door)
                if (!hasPath)
                {
                    agent.destination = currentRoomEntry.outerPoint.position;
                    hasPath = true;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: MovingToOuter -> '{currentRoomEntry.outerPoint.name}'");
                    }
                }

                // Check if reached outer point (both path reached AND close enough)
                float distanceToOuter = Vector3.Distance(transform.position, currentRoomEntry.outerPoint.position);
                
                if (agent.reachedEndOfPath && !agent.pathPending && distanceToOuter <= roomPointReachThreshold)
                {
                    roomEntryPhase = RoomEntryPhase.WaitingForDoor;
                    roomExploreTimer = 0f;
                    hasPath = false;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: Reached outer point (dist: {distanceToOuter:F2}m), waiting for door to open");
                    }
                }
                break;

            case RoomEntryPhase.WaitingForDoor:
                // Wait for door to open (0.8 seconds)
                roomExploreTimer += Time.deltaTime;
                
                if (roomExploreTimer >= 0.8f)
                {
                    roomEntryPhase = RoomEntryPhase.MovingToInner;
                    roomExploreTimer = 0f;
                    hasPath = false;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log("[DosenAI] Room Entry Phase: Door should be open, proceeding to inner point");
                    }
                }
                break;

            case RoomEntryPhase.MovingToInner:
                // Move to inner point (inside room)
                if (!hasPath)
                {
                    agent.destination = currentRoomEntry.innerPoint.position;
                    hasPath = true;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: MovingToInner -> '{currentRoomEntry.innerPoint.name}'");
                    }
                }

                // Check if reached inner point (both path reached AND close enough)
                float distanceToInner = Vector3.Distance(transform.position, currentRoomEntry.innerPoint.position);
                
                if (agent.reachedEndOfPath && !agent.pathPending && distanceToInner <= roomPointReachThreshold)
                {
                    // Now switch to exploring phase
                    roomEntryPhase = RoomEntryPhase.Exploring;
                    roomExploreTimer = 0f;
                    
                    // Stop movement and switch to Idle (using patrolWaitTime for explore duration)
                    agent.isStopped = true;
                    agent.destination = transform.position;
                    currentState = AIState.Idle;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: Reached inner point (dist: {distanceToInner:F2}m), exploring for {patrolWaitTime}s (State: Idle, Stopped)");
                    }
                }
                break;

            case RoomEntryPhase.Exploring:
                // Stay at inner point and explore - using patrolWaitTime as explore duration
                agent.isStopped = true;
                agent.destination = transform.position;
                
                roomExploreTimer += Time.deltaTime;
                
                if (roomExploreTimer >= patrolWaitTime) // Use patrolWaitTime instead of exploreTime
                {
                    // Finished exploring, resume movement and return to outer point
                    agent.isStopped = false;
                    roomEntryPhase = RoomEntryPhase.ReturnToOuter;
                    hasPath = false;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log("[DosenAI] Room Entry Phase: Finished exploring, returning to outer point");
                    }
                }
                break;

            case RoomEntryPhase.ReturnToOuter:
                // Return to outer point before continuing patrol
                if (!hasPath)
                {
                    agent.destination = currentRoomEntry.outerPoint.position;
                    hasPath = true;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: ReturnToOuter -> '{currentRoomEntry.outerPoint.name}'");
                    }
                }

                // Check if reached outer point
                if (agent.reachedEndOfPath && !agent.pathPending)
                {
                    // Now return to previous state
                    isEnteringRoom = false;
                    roomEntryPhase = RoomEntryPhase.None;
                    currentRoomEntry = null;
                    hasPath = false;
                    SwitchState(stateBeforeRoomEntry); // Return to saved state
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[DosenAI] Room Entry Phase: Back at outer point, returning to {stateBeforeRoomEntry} state");
                    }
                }
                break;
        }

        // Check for player during room entry
        if (CanSeePlayer())
        {
            isEnteringRoom = false;
            roomEntryPhase = RoomEntryPhase.None;
            currentRoomEntry = null;
            SwitchState(AIState.Chase);
        }
    }

    Vector3 GetRandomPointAround(Vector3 center, float range)
    {
        // For A* we can use simple random point generation
        // The pathfinding system will handle if the point is not walkable
        Vector3 randomDirection = Random.insideUnitCircle * range;
        return center + new Vector3(randomDirection.x, 0, randomDirection.y);
    }
    
    void SwitchState(AIState newState)
    {
        currentState = newState;
        losePlayerTimer = 0f;
        hasPath = false;
        
        // Immediately set destination when switching to Patrol
        if (newState == AIState.Patrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            if (patrolPoints[currentPatrolIndex] != null)
            {
                agent.destination = patrolPoints[currentPatrolIndex].position;
                hasPath = true;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[DosenAI] Switched to Patrol - immediately setting destination to '{patrolPoints[currentPatrolIndex].name}'");
                }
            }
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        float speed = agent.velocity.magnitude;
        
        if (speed > 0.1f)
        {
            animator.SetFloat(speedHash, currentState == AIState.Chase ? 2f : 1f);
        }
        else
        {
            animator.SetFloat(speedHash, 0f);
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        
        // Proximity detection (green sphere - 360°, short range)
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Vision cone (yellow - long range, narrow angle)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePos, visionRange);
        
        // Draw vision cone boundaries
        Vector3 leftBoundary = Quaternion.AngleAxis(-visionAngle / 2f, Vector3.up) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.AngleAxis(visionAngle / 2f, Vector3.up) * transform.forward * visionRange;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eyePos, eyePos + leftBoundary);
        Gizmos.DrawLine(eyePos, eyePos + rightBoundary);
        Gizmos.DrawLine(eyePos, eyePos + transform.forward * visionRange);
        
        // Kill radius (red sphere)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);
        
        // Kill angle cone (red)
        Vector3 killLeft = Quaternion.AngleAxis(-killAngle / 2f, Vector3.up) * transform.forward * killRadius;
        Vector3 killRight = Quaternion.AngleAxis(killAngle / 2f, Vector3.up) * transform.forward * killRadius;
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + killLeft);
        Gizmos.DrawLine(transform.position, transform.position + killRight);
        
        // Door detection range (white)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + transform.forward * doorDetectionRange);
    }
    
    /// <summary>
    /// Enable final chase mode (called by ExitHallwayTrigger)
    /// </summary>
    public void EnableFinalChaseMode(float speed)
    {
        isFinalChase = true;
        currentState = AIState.Chase;
        agent.maxSpeed = speed;
        
        if (showDebugLogs)
        {
            Debug.Log("[DosenAI] FINAL CHASE MODE ENABLED! Hunting player...");
        }
    }
    
    /// <summary>
    /// Check if player is within kill radius (front-facing only)
    /// </summary>
    void CheckKillRadius()
    {
        if (player == null || isKilling) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player within kill radius
        if (distanceToPlayer <= killRadius)
        {
            // Check if player is in front of Dosen
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer <= killAngle / 2f)
            {
                // Player is close AND in front - KILL!
                TriggerKillSequence();
            }
        }
    }
    
    /// <summary>
    /// Trigger kill sequence with scream animation
    /// </summary>
    void TriggerKillSequence()
    {
        if (isKilling) return;
        isKilling = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[DosenAI] Player in kill radius! Triggering kill sequence...");
        }
        
        // COMPLETELY stop AI - disable the component to freeze movement and rotation
        (agent as MonoBehaviour).enabled = false;
        
        // IMMEDIATELY freeze player and start camera zoom (before delay)
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Die(transform, screamDelay + screamAnimationDuration);
        }
        else
        {
            Debug.LogError("[DosenAI] PlayerHealth component not found on player!");
        }
        
        // Start coroutine for delayed scream
        StartCoroutine(DelayedScreamSequence());
    }
    
    /// <summary>
    /// Coroutine: Wait, then trigger scream animation and sound
    /// </summary>
    System.Collections.IEnumerator DelayedScreamSequence()
    {
        // Wait for delay (idle animation plays while camera already zooming)
        yield return new WaitForSeconds(screamDelay);
        
        // Trigger scream animation
        if (animator != null)
        {
            animator.SetTrigger(screamHash);
        }
        
        // Play scream sound
        if (screamSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(screamSound);
        }
    }
    
    /// <summary>
    /// Set chase target and force chase state (for scripted events like exit chase)
    /// </summary>
    public void SetChaseTarget(Transform target)
    {
        player = target;
        currentState = AIState.Chase;
        lastKnownPlayerPosition = target.position;
        losePlayerTimer = 0f;
        
        if (showDebugLogs)
        {
            Debug.Log($"[DosenAI] Force chase target set to: {target.name}");
        }
    }
    
    /// <summary>
    /// Enable final chase mode (player wins even when caught)
    /// </summary>
    public void EnableFinalChaseMode(bool enable)
    {
        isFinalChase = enable;
        
        if (showDebugLogs)
        {
            Debug.Log($"[DosenAI] Final chase mode: {enable}");
        }
    }
    
    /// <summary>
    /// Detect collision with player
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        
        // Only handle final chase collision (for WIN condition)
        if (isFinalChase)
        {
            if (showDebugLogs)
            {
                Debug.Log("[DosenAI] Player caught in final chase! Triggering WIN cutscene...");
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerWin();
            }
        }
        // Normal chase uses kill radius system (CheckKillRadius), not collision
    }
}