using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [Tooltip("Radius of interaction sphere (thicker = more forgiving)")]
    [SerializeField] private float interactionRadius = 0.3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Stability (Anti-Flicker)")]
    [Tooltip("Keep interactable locked for this duration to prevent head bob flicker")]
    [SerializeField] private float lockDuration = 0.2f;
    [Tooltip("Extra distance tolerance when checking locked interactable")]
    [SerializeField] private float lockDistanceTolerance = 0.5f;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private IInteractable currentInteractable;
    
    // Stability variables (prevent flicker from head bob)
    private IInteractable lockedInteractable = null;
    private float lockTimer = 0f;
    private Vector3 lockedInteractablePosition;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        CheckForInteractable();
        
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        // STABILITY SYSTEM: If we have a locked interactable, use it for a bit (prevent flicker)
        if (lockedInteractable != null && lockTimer > 0f)
        {
            lockTimer -= Time.deltaTime;
            currentInteractable = lockedInteractable;
            ShowInteractionPrompt(lockedInteractable.GetInteractPrompt());
            
            // Verify locked interactable is still in range (with tolerance)
            float distance = Vector3.Distance(cameraTransform.position, lockedInteractablePosition);
            
            if (distance > interactionDistance + lockDistanceTolerance)
            {
                // Too far, clear lock
                lockedInteractable = null;
                lockTimer = 0f;
                currentInteractable = null;
                HideInteractionPrompt();
            }
            
            return; // Use locked interactable, skip raycast
        }
        
        // Use SphereCast instead of Raycast for thicker detection (more forgiving)
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        
        if (showDebugRay)
        {
            // Draw main ray
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.yellow);
            
            // Draw sphere at end to visualize thickness
            Vector3 endPoint = cameraTransform.position + cameraTransform.forward * interactionDistance;
            Debug.DrawLine(endPoint + Vector3.up * interactionRadius, endPoint - Vector3.up * interactionRadius, Color.green);
            Debug.DrawLine(endPoint + Vector3.right * interactionRadius, endPoint - Vector3.right * interactionRadius, Color.green);
        }
        
        // SphereCast = Raycast with thickness (radius)
        if (Physics.SphereCast(ray, interactionRadius, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            // Cari di GameObject yang kena hit dulu
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            // Kalau gak ada, cari di parent
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }
            
            if (interactable != null)
            {
                currentInteractable = interactable;
                
                // LOCK THIS INTERACTABLE (prevent flicker)
                lockedInteractable = interactable;
                lockTimer = lockDuration;
                lockedInteractablePosition = hit.collider.transform.position;
                
                ShowInteractionPrompt(interactable.GetInteractPrompt());
                return;
            }
        }
        
        // No interactable found, clear everything
        currentInteractable = null;
        lockedInteractable = null;
        lockTimer = 0f;
        HideInteractionPrompt();
    }

    void ShowInteractionPrompt(string promptText)
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
            
            if (interactionPromptText != null && !string.IsNullOrEmpty(promptText))
            {
                interactionPromptText.text = promptText;
            }
        }
    }

    void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }
}