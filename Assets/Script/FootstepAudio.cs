using UnityEngine;

/// <summary>
/// Footstep audio system that syncs with player movement
/// Attach to Player GameObject
/// Plays different sounds for walking vs running, with random variation
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Footstep Sounds")]
    [Tooltip("Array of walking footstep sounds (random variation)")]
    [SerializeField] private AudioClip[] walkSounds;
    
    [Tooltip("Array of running footstep sounds")]
    [SerializeField] private AudioClip[] runSounds;
    
    [Header("Timing Settings")]
    [Tooltip("Time between footsteps when walking (seconds)")]
    [SerializeField] private float walkStepInterval = 0.5f;
    
    [Tooltip("Time between footsteps when running (seconds)")]
    [SerializeField] private float runStepInterval = 0.3f;
    
    [Header("Movement Detection")]
    [Tooltip("Minimum speed to play footsteps (prevent sound when barely moving)")]
    [SerializeField] private float minSpeedThreshold = 1f;
    
    [Tooltip("Speed threshold to switch from walk to run sounds")]
    [SerializeField] private float runSpeedThreshold = 13f;
    
    [Header("Audio Settings")]
    [Tooltip("Volume for walking sounds")]
    [SerializeField] private float walkVolume = 0.5f;
    
    [Tooltip("Volume for running sounds")]
    [SerializeField] private float runVolume = 0.7f;
    
    [Tooltip("Random pitch variation (adds realism)")]
    [SerializeField] private float pitchVariation = 0.1f;
    
    [Header("Ground Detection")]
    [Tooltip("Only play footsteps when grounded (uses CharacterController)")]
    [SerializeField] private bool requireGrounded = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Components
    private AudioSource audioSource;
    private CharacterController characterController;
    
    // State tracking
    private float stepTimer = 0f;
    private bool isMoving = false;
    private bool isRunning = false;
    private bool isGrounded = false;
    private float currentSpeed = 0f;
    
    // Last position for velocity calculation
    private Vector3 lastPosition;
    
    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        
        // Validate footstep sounds
        if (walkSounds == null || walkSounds.Length == 0)
        {
            Debug.LogWarning("[FootstepAudio] No walk sounds assigned! Assign footstep audio clips in Inspector.");
        }
        
        if (runSounds == null || runSounds.Length == 0)
        {
            Debug.LogWarning("[FootstepAudio] No run sounds assigned. Will use walk sounds for running.");
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound (first person)
        
        // Initialize position tracking
        lastPosition = transform.position;
    }
    
    void Update()
    {
        // Detect movement state
        DetectMovement();
        
        // Check if grounded (if required)
        if (requireGrounded)
        {
            isGrounded = characterController != null ? characterController.isGrounded : true;
        }
        else
        {
            isGrounded = true; // Always play if ground check disabled
        }
        
        // Play footsteps if moving and grounded
        if (isMoving && isGrounded)
        {
            UpdateFootstepTimer();
        }
        else
        {
            // Reset timer when not moving
            stepTimer = 0f;
        }
    }
    
    /// <summary>
    /// Detect player movement speed
    /// </summary>
    void DetectMovement()
    {
        // Calculate velocity
        Vector3 velocity = Vector3.zero;
        
        if (characterController != null)
        {
            // Use CharacterController velocity (more accurate)
            velocity = characterController.velocity;
        }
        else
        {
            // Fallback: Calculate from position change
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;
        }
        
        // Get horizontal speed (ignore vertical)
        currentSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        // Check if moving
        isMoving = currentSpeed > minSpeedThreshold;
        
        // Check if running
        isRunning = currentSpeed > runSpeedThreshold;
        
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[FootstepAudio] Speed: {currentSpeed:F2} | Moving: {isMoving} | Running: {isRunning} | Grounded: {isGrounded}");
        }
    }
    
    /// <summary>
    /// Update footstep timing and play sounds
    /// </summary>
    void UpdateFootstepTimer()
    {
        // Get current step interval based on run/walk
        float currentInterval = isRunning ? runStepInterval : walkStepInterval;
        
        // Increment timer
        stepTimer += Time.deltaTime;
        
        // Check if it's time to play a step
        if (stepTimer >= currentInterval)
        {
            PlayFootstep();
            stepTimer = 0f; // Reset timer
        }
    }
    
    /// <summary>
    /// Play a random footstep sound
    /// </summary>
    void PlayFootstep()
    {
        // Select appropriate sound array
        AudioClip[] soundArray = isRunning ? runSounds : walkSounds;
        
        // Fallback to walk sounds if run sounds not assigned
        if (soundArray == null || soundArray.Length == 0)
        {
            soundArray = walkSounds;
        }
        
        // Validate array
        if (soundArray == null || soundArray.Length == 0)
        {
            return; // No sounds available
        }
        
        // Pick random sound from array
        AudioClip clip = soundArray[Random.Range(0, soundArray.Length)];
        
        if (clip == null)
        {
            Debug.LogWarning("[FootstepAudio] Null audio clip in array!");
            return;
        }
        
        // Set volume based on walk/run
        float volume = isRunning ? runVolume : walkVolume;
        
        // Add random pitch variation for realism
        float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.pitch = pitch;
        
        // Play the sound
        audioSource.PlayOneShot(clip, volume);
        
        if (showDebugLogs)
        {
            Debug.Log($"[FootstepAudio] Playing {(isRunning ? "RUN" : "WALK")} footstep: {clip.name}");
        }
    }
    
    /// <summary>
    /// Manually trigger a footstep (useful for animation events)
    /// </summary>
    public void PlayManualFootstep()
    {
        PlayFootstep();
    }
    
    /// <summary>
    /// Enable/disable footstep sounds (useful for cutscenes, death, etc.)
    /// </summary>
    public void SetFootstepsEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled)
        {
            // Stop any playing footsteps
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            stepTimer = 0f;
        }
    }
    
    /// <summary>
    /// Set custom footstep interval (e.g., for injured/slow movement)
    /// </summary>
    public void SetStepInterval(float walkInterval, float runInterval)
    {
        walkStepInterval = walkInterval;
        runStepInterval = runInterval;
    }
}
