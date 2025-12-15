using UnityEngine;

/// <summary>
/// Manages player health and death state
/// Attach to Player GameObject
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isAlive = true;
    
    [Header("References")]
    [SerializeField] private SimpleMovement movementController; // FPP movement
    [SerializeField] private Transform deathCamera; // Optional: camera that looks at enemy
    [SerializeField] private Camera playerCamera;
    
    [Header("Death Settings")]
    [Tooltip("Time to wait before triggering lose screen (for death animation)")]
    [SerializeField] private float deathDelay = 2f;
    
    [Tooltip("Should player rotate to face killer?")]
    [SerializeField] private bool rotateToFaceKiller = true;
    
    [Tooltip("Camera height offset to align with enemy face")]
    [SerializeField] private float cameraHeightOffset = 0.3f;
    
    [Tooltip("Rotation speed when facing killer (higher = faster)")]
    [SerializeField] private float faceRotationSpeed = 5f;
    
    [Header("Camera Zoom Effect")]
    [Tooltip("Enable camera zoom toward killer's face")]
    [SerializeField] private bool enableZoomEffect = true;
    
    [Tooltip("Target Field of View during zoom (lower = more zoomed)")]
    [SerializeField] private float zoomTargetFOV = 30f;
    
    [Tooltip("Zoom speed (higher = faster zoom)")]
    [SerializeField] private float zoomSpeed = 2f;
    
    [Tooltip("Move camera closer to killer (meters)")]
    [SerializeField] private float zoomMoveDistance = 1.5f;
    
    [Tooltip("Speed of camera movement toward killer")]
    [SerializeField] private float zoomMoveSpeed = 1f;
    
    [Tooltip("Camera X rotation offset (degrees, positive = look up, negative = look down)")]
    [SerializeField] private float cameraXRotationOffset = -17f;
    
    [Tooltip("Target camera Y position (local height)")]
    [SerializeField] private float cameraTargetYPosition = 0.75f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Delay before playing death scream (sync with animation)")]
    [SerializeField] private float audioDelay = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Public property
    public bool IsAlive => isAlive;
    
    // Win zone state
    private bool isInWinZone = false;
    
    // Reference to enemy that killed player (for camera facing)
    private Transform killerEnemy;
    private bool isRotatingToKiller = false;
    private float deathAnimationTimer = 0f;
    
    // Camera zoom variables
    private float originalFOV;
    private Vector3 originalCameraLocalPos;
    private bool isZooming = false;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (movementController == null)
        {
            movementController = GetComponent<SimpleMovement>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Store original camera settings for zoom effect
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
            originalCameraLocalPos = playerCamera.transform.localPosition;
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    void Update()
    {
        // Smoothly rotate player to face killer during death animation
        if (isRotatingToKiller && killerEnemy != null)
        {
            RotatePlayerToFaceKiller();
            deathAnimationTimer += Time.deltaTime;
            
            // Stop rotating after animation duration
            if (deathAnimationTimer >= deathDelay)
            {
                isRotatingToKiller = false;
            }
        }
        
        // Zoom camera toward killer's face
        if (isZooming && killerEnemy != null && playerCamera != null)
        {
            ZoomCameraToKiller();
        }
    }
    
    /// <summary>
    /// Kill the player (called by enemy or hazard)
    /// </summary>
    /// <param name="killer">Transform of the enemy that killed player (optional, for camera facing)</param>
    /// <param name="animationDuration">Duration of enemy's kill animation (for camera rotation timing)</param>
    public void Die(Transform killer = null, float animationDuration = 2f)
    {
        if (!isAlive) return; // Already dead
        
        // Get components once
        CharacterController charController = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();
        
        // Check if player is in win zone - trigger WIN instead of death!
        if (isInWinZone)
        {
            if (showDebugLogs)
            {
                Debug.Log("[PlayerHealth] Player caught in win zone - triggering WIN sequence!");
            }
            
            // Freeze player during win sequence
            if (movementController != null)
            {
                movementController.enabled = false;
            }
            
            if (charController != null)
            {
                charController.enabled = false;
            }
            
            // Trigger win after short delay (untuk dramatic effect - player "caught" dulu)
            Invoke(nameof(TriggerWinCondition), animationDuration * 0.5f);
            return; // Don't continue to death code
        }
        
        isAlive = false;
        killerEnemy = killer;
        deathDelay = animationDuration;
        
        if (showDebugLogs)
        {
            Debug.Log("[PlayerHealth] Player died! Freezing and zooming to killer...");
        }
        
        // FREEZE PLAYER COMPLETELY
        if (movementController != null)
        {
            movementController.enabled = false;
        }
        
        // Freeze CharacterController
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        // Freeze Rigidbody if exists
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Play death sound with delay (sync with animation)
        if (deathSound != null && audioSource != null)
        {
            if (audioDelay > 0f)
            {
                Invoke(nameof(PlayDeathSound), audioDelay);
            }
            else
            {
                audioSource.PlayOneShot(deathSound);
            }
        }
        
        // Start rotating to face killer
        if (rotateToFaceKiller && killerEnemy != null)
        {
            isRotatingToKiller = true;
            deathAnimationTimer = 0f;
        }
        
        // Start zoom effect
        if (enableZoomEffect && killerEnemy != null && playerCamera != null)
        {
            isZooming = true;
        }
        
        // Call GameManager lose after delay
        Invoke(nameof(TriggerGameOver), deathDelay);
    }
    
    /// <summary>
    /// Smoothly rotate player (and camera) to face the killer
    /// </summary>
    void RotatePlayerToFaceKiller()
    {
        // Calculate direction to killer's face
        Vector3 killerFacePosition = killerEnemy.position + Vector3.up * 1.6f; // Dosen's face height
        Vector3 playerEyePosition = transform.position + Vector3.up * cameraHeightOffset;
        Vector3 directionToKiller = (killerFacePosition - playerEyePosition).normalized;
        
        // Calculate target rotation (only Y axis - horizontal rotation)
        directionToKiller.y = 0; // Keep rotation on horizontal plane
        if (directionToKiller.sqrMagnitude < 0.001f) return; // Avoid zero direction
        
        Quaternion targetRotation = Quaternion.LookRotation(directionToKiller);
        
        // Smooth rotate player body (camera follows because it's child of player)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * faceRotationSpeed);
        
        if (showDebugLogs && Time.frameCount % 30 == 0)
        {
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            Debug.Log($"[PlayerHealth] Rotating to killer... Remaining: {angle:F1}°");
        }
    }
    
    /// <summary>
    /// Zoom camera FOV and position toward killer's face
    /// </summary>
    void ZoomCameraToKiller()
    {
        // Zoom FOV (narrow field of view)
        float currentFOV = playerCamera.fieldOfView;
        if (currentFOV > zoomTargetFOV)
        {
            playerCamera.fieldOfView = Mathf.Lerp(currentFOV, zoomTargetFOV, Time.deltaTime * zoomSpeed);
        }
        
        // Move camera closer to killer
        Vector3 killerFacePos = killerEnemy.position + Vector3.up * 1.6f;
        Vector3 directionToKiller = (killerFacePos - playerCamera.transform.position).normalized;
        
        // Target position: move camera forward toward killer + set Y position
        Vector3 targetLocalPos = originalCameraLocalPos + playerCamera.transform.parent.InverseTransformDirection(directionToKiller * zoomMoveDistance);
        targetLocalPos.y = cameraTargetYPosition; // Set specific Y height
        
        // Smooth move
        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            targetLocalPos,
            Time.deltaTime * zoomMoveSpeed
        );
        
        // Apply X rotation offset to camera (tilt up/down)
        if (cameraXRotationOffset != 0f)
        {
            Vector3 currentEuler = playerCamera.transform.localEulerAngles;
            currentEuler.x = cameraXRotationOffset;
            playerCamera.transform.localEulerAngles = currentEuler;
        }
    }
    
    /// <summary>
    /// Play death scream sound (delayed)
    /// </summary>
    void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
    
    /// <summary>
    /// Trigger game over screen
    /// </summary>
    void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerLose();
        }
        else
        {
            Debug.LogError("[PlayerHealth] GameManager not found! Cannot trigger lose state.");
        }
    }
    
    /// <summary>
    /// Detect collision with enemy
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return; // Already dead
        
        // Check if collided with enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerHealth] Collided with enemy: {collision.gameObject.name}");
            }
            
            // Die and pass enemy transform for camera facing
            Die(collision.transform);
        }
    }
    
    /// <summary>
    /// Detect trigger collision with enemy (if using trigger colliders)
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!isAlive) return; // Already dead
        
        // Check if triggered by enemy
        if (other.CompareTag("Enemy"))
        {
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerHealth] Triggered by enemy: {other.gameObject.name}");
            }
            
            // Die and pass enemy transform for camera facing
            Die(other.transform);
        }
    }
    
    /// <summary>
    /// Set player in win zone (makes player immune to death)
    /// </summary>
    public void SetInWinZone(bool inWinZone)
    {
        isInWinZone = inWinZone;
        
        if (showDebugLogs)
        {
            Debug.Log($"[PlayerHealth] Win zone immunity: {inWinZone}");
        }
    }
    
    /// <summary>
    /// Trigger win condition (called when caught in win zone)
    /// </summary>
    void TriggerWinCondition()
    {
        if (showDebugLogs)
        {
            Debug.Log("[PlayerHealth] Triggering WIN after catch sequence!");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin();
        }
        else
        {
            Debug.LogError("[PlayerHealth] GameManager not found! Cannot trigger win.");
        }
    }
    
    /// <summary>
    /// Revive player (for debugging or respawn system)
    /// </summary>
    public void Revive()
    {
        isAlive = true;
        isInWinZone = false;
        
        if (movementController != null)
        {
            movementController.enabled = true;
        }
        
        // Re-enable main camera if death camera was used
        if (deathCamera != null && playerCamera != null)
        {
            deathCamera.gameObject.SetActive(false);
            playerCamera.enabled = true;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[PlayerHealth] Player revived!");
        }
    }
}
