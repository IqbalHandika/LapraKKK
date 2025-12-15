using UnityEngine;

/// <summary>
/// Camera head bobbing effect for realistic first-person movement
/// Attach to Player Camera (child of player)
/// Syncs with player movement speed for immersive feeling
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraHeadBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [Tooltip("How fast the camera bobs (higher = faster bob cycle)")]
    [SerializeField] private float walkBobSpeed = 10f;
    
    [Tooltip("How fast the camera bobs when running")]
    [SerializeField] private float runBobSpeed = 14f;
    
    [Tooltip("Vertical bob amount (up/down movement in meters)")]
    [SerializeField] private float walkBobAmount = 0.15f;
    
    [Tooltip("Vertical bob amount when running")]
    [SerializeField] private float runBobAmount = 0.25f;
    
    [Tooltip("Horizontal bob amount (left/right sway)")]
    [SerializeField] private float horizontalBobAmount = 0.1f;
    
    [Header("Movement Detection")]
    [Tooltip("Player transform (auto-detected if null)")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("Minimum speed to trigger bobbing (prevent bob when barely moving)")]
    [SerializeField] private float minSpeedThreshold = 1f;
    
    [Tooltip("Speed threshold to switch from walk to run bob")]
    [SerializeField] private float runSpeedThreshold = 13f;
    
    [Header("FOV Settings (Sprint Effect)")]
    [Tooltip("Enable FOV change when sprinting")]
    [SerializeField] private bool enableSprintFOV = true;
    
    [Tooltip("Normal field of view")]
    [SerializeField] private float normalFOV = 60f;
    
    [Tooltip("Sprint field of view (wider = more speed sense)")]
    [SerializeField] private float sprintFOV = 70f;
    
    [Tooltip("FOV transition speed")]
    [SerializeField] private float fovLerpSpeed = 5f;
    
    [Header("Camera Shake (Running)")]
    [Tooltip("Enable camera shake when running")]
    [SerializeField] private bool enableRunShake = true;
    
    [Tooltip("Maximum rotation shake amount (degrees)")]
    [SerializeField] private float shakeAmount = 0.3f;
    
    [Tooltip("Shake frequency (higher = more jittery)")]
    [SerializeField] private float shakeFrequency = 20f;
    
    [Header("Smoothing")]
    [Tooltip("How fast camera returns to rest position (higher = snappier)")]
    [SerializeField] private float restPositionSpeed = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Components
    private Camera playerCamera;
    private CharacterController characterController;
    
    // Bob tracking
    private float bobTimer = 0f;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 lastPlayerPosition; // Track player position for velocity calculation
    
    // State
    private bool isMoving = false;
    private bool isRunning = false;
    private float currentSpeed = 0f;
    
    void Start()
    {
        // Get camera component
        playerCamera = GetComponent<Camera>();
        
        // Auto-detect player transform (parent)
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("[CameraHeadBob] Player transform not found! Camera must be child of player.");
            enabled = false;
            return;
        }
        
        // Get CharacterController from player
        characterController = playerTransform.GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogWarning("[CameraHeadBob] CharacterController not found. Using transform position for movement detection.");
        }
        
        // Store original camera position and rotation
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        
        // Set initial FOV
        if (playerCamera != null)
        {
            // FORCE set to normal FOV on start (reset any previous values)
            playerCamera.fieldOfView = normalFOV;
        }
        
        // Initialize player position tracking
        lastPlayerPosition = playerTransform.position;
    }
    
    void Update()
    {
        // Detect player movement
        DetectMovement();
        
        // Apply camera effects
        if (isMoving)
        {
            ApplyHeadBob();
            
            if (enableRunShake && isRunning)
            {
                ApplyCameraShake();
            }
        }
        else
        {
            // Return to rest position smoothly
            ReturnToRestPosition();
        }
        
        // Update FOV for sprint effect
        if (enableSprintFOV)
        {
            UpdateSprintFOV();
        }
        
        // Debug info
        if (showDebugInfo && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[CameraHeadBob] Speed: {currentSpeed:F2} | Moving: {isMoving} | Running: {isRunning}");
        }
    }
    
    /// <summary>
    /// Detect if player is moving and how fast
    /// </summary>
    void DetectMovement()
    {
        // ALWAYS use position-based calculation (most reliable for SimpleMovement)
        Vector3 velocity = (playerTransform.position - lastPlayerPosition) / Time.deltaTime;
        lastPlayerPosition = playerTransform.position;
        
        // Get horizontal speed (ignore vertical for bob)
        currentSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        // Check if moving
        isMoving = currentSpeed > minSpeedThreshold;
        
        // Check if sprinting - USE SHIFT KEY INPUT (not speed threshold!)
        // Only sprint if actually moving forward
        isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;
        
        if (showDebugInfo && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[CameraHeadBob] Speed: {currentSpeed:F2} | Velocity: {velocity} | Moving: {isMoving} | Running: {isRunning}");
        }
    }
    
    /// <summary>
    /// Apply head bob effect based on movement
    /// </summary>
    void ApplyHeadBob()
    {
        // Get current bob parameters based on run/walk
        float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
        float verticalBobAmount = isRunning ? runBobAmount : walkBobAmount;
        
        // Increment bob timer
        bobTimer += Time.deltaTime * bobSpeed;
        
        // Calculate bob offset using sine wave
        // Vertical (Y): Simple sine wave
        float verticalOffset = Mathf.Sin(bobTimer) * verticalBobAmount;
        
        // Horizontal (X): Sine wave at double frequency for left-right sway
        // (feet alternate, so horizontal bob is twice as fast)
        float horizontalOffset = Mathf.Cos(bobTimer * 2f) * horizontalBobAmount;
        
        // Apply bob to camera position DIRECTLY (no lerp for more pronounced effect)
        Vector3 targetPosition = originalLocalPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
        transform.localPosition = targetPosition;
    }
    
    /// <summary>
    /// Apply subtle camera shake when running
    /// </summary>
    void ApplyCameraShake()
    {
        // Random shake using Perlin noise
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeAmount;
        float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeAmount;
        float shakeZ = (Mathf.PerlinNoise(Time.time * shakeFrequency, Time.time * shakeFrequency) - 0.5f) * 2f * shakeAmount;
        
        // Apply shake rotation
        Quaternion shakeRotation = Quaternion.Euler(shakeX, shakeY, shakeZ);
        transform.localRotation = originalLocalRotation * shakeRotation;
    }
    
    /// <summary>
    /// Smoothly return camera to rest position when not moving
    /// </summary>
    void ReturnToRestPosition()
    {
        // Lerp position back to original
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * restPositionSpeed);
        
        // Lerp rotation back to original
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originalLocalRotation, Time.deltaTime * restPositionSpeed);
        
        // Reset bob timer when stopped (prevents jump when starting to move again)
        bobTimer = 0f;
    }
    
    /// <summary>
    /// Update FOV based on sprint state
    /// </summary>
    void UpdateSprintFOV()
    {
        if (playerCamera == null) return;
        
        // Target FOV based on running state
        // IMPORTANT: Only increase FOV if BOTH moving AND running (not just moving)
        float targetFOV = (isMoving && isRunning) ? sprintFOV : normalFOV;
        
        // Smooth lerp to target FOV
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }
    
    /// <summary>
    /// Reset camera to original position (call when respawning, etc.)
    /// </summary>
    public void ResetCamera()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        bobTimer = 0f;
        
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }
    }
    
    /// <summary>
    /// Temporarily disable head bob (useful for cutscenes, death, etc.)
    /// </summary>
    public void SetHeadBobEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Return to rest position immediately
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
        }
    }
}
