using UnityEngine;

/// <summary>
/// Syncs player movement speed with animator parameters
/// Same system as DosenAI - updates Speed parameter for Idle/Walk/Run transitions
/// Attach to Player GameObject (same object as SimpleMovement)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator component on player model (child object)")]
    [SerializeField] private Animator animator;
    
    [Header("Animation Settings")]
    [Tooltip("Smoothing speed for animation transitions")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private CharacterController controller;
    private readonly int speedHash = Animator.StringToHash("Speed");
    private float currentAnimSpeed = 0f;
    private Vector3 lastPosition;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimator] Animator not found! Assign manually or add to child model.");
            enabled = false;
            return;
        }
        
        if (controller == null)
        {
            Debug.LogError("[PlayerAnimator] CharacterController not found!");
            enabled = false;
            return;
        }
        
        // Initialize position tracking
        lastPosition = transform.position;
        
        if (showDebugLogs)
        {
            Debug.Log("[PlayerAnimator] Initialized successfully");
        }
    }
    
    void Update()
    {
        // Calculate horizontal velocity (same as DosenAI)
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        // Smooth transition (prevents jerky animations)
        currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, speed, Time.deltaTime / animationSmoothTime);
        
        // Update animator Speed parameter
        animator.SetFloat(speedHash, currentAnimSpeed);
        
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PlayerAnimator] Speed: {currentAnimSpeed:F2}");
        }
    }
}
