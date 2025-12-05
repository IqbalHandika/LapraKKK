using UnityEngine;

public class TPPMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;
    
    private CharacterController characterController;
    private Vector3 moveDirection;
    private bool isRunning = false;
    
    // Animation parameter names (sesuaikan dengan Animator Controller lu)
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Lock cursor (opsional, buat game)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateAnimations();
    }
    
    void HandleMovement()
    {
        // Input WASD
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S
        
        // Check run (Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate movement direction (relative to camera)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Flatten vectors (ignore Y axis)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // Combine input
        moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Apply speed
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        
        // Apply gravity
        movement.y = -9.81f * Time.deltaTime;
        
        // Move character
        if (characterController != null)
        {
            characterController.Move(movement);
        }
    }
    
    void HandleRotation()
    {
        if (moveDirection != Vector3.zero)
        {
            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Calculate speed for animation
        float speed = moveDirection.magnitude;
        
        // Set animation parameters
        if (isRunning && speed > 0.1f)
        {
            animator.SetFloat(speedHash, 2f); // Run animation
        }
        else if (speed > 0.1f)
        {
            animator.SetFloat(speedHash, 1f); // Walk animation
        }
        else
        {
            animator.SetFloat(speedHash, 0f); // Idle animation
        }
        
        // Alternative: bool parameter
        animator.SetBool(isWalkingHash, speed > 0.1f);
    }
}