using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private IInteractable currentInteractable;

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
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        
        if (showDebugRay)
        {
            Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.yellow);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
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
                ShowInteractionPrompt(interactable.GetInteractPrompt());
                return;
            }
        }
        
        currentInteractable = null;
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