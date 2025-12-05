using UnityEngine;

public class TPPCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Camera Settings")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    [Header("Collision Detection")]
    [SerializeField] private bool checkCollision = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers;
    
    private float currentX = 0f;
    private float currentY = 0f;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("TPPCameraController: Target is not assigned!");
            return;
        }
        
        // Initialize rotation based on current camera position
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleRotation();
        HandlePosition();
    }
    
    void HandleRotation()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        
        // Update rotation
        currentX += mouseX;
        currentY -= mouseY;
        
        // Clamp vertical rotation
        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
    }
    
    void HandlePosition()
    {
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 direction = rotation * Vector3.back;
        
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition + direction * distance;
        
        // Collision check
        if (checkCollision)
        {
            RaycastHit hit;
            Vector3 rayDirection = desiredPosition - targetPosition;
            
            if (Physics.SphereCast(targetPosition, collisionRadius, rayDirection.normalized, out hit, distance, collisionLayers))
            {
                // Camera hit something, move closer to target
                desiredPosition = targetPosition + rayDirection.normalized * (hit.distance - collisionRadius);
            }
        }
        
        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Look at target
        transform.LookAt(targetPosition);
    }
}