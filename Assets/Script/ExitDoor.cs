using UnityEngine;

/// <summary>
/// Exit door that requires exit key + all papers to open
/// Works like a locked door, not auto-unlock
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitDoor : MonoBehaviour, IInteractable
{
    [Header("Door State")]
    [Tooltip("Is the exit door currently open?")]
    [SerializeField] private bool isOpen = false;
    
    [Header("Door Animation")]
    [Tooltip("Door transform to rotate/move")]
    [SerializeField] private Transform doorTransform;
    
    [Tooltip("Open rotation (Euler angles)")]
    [SerializeField] private Vector3 openRotation = new Vector3(0, 90, 0);
    
    [Tooltip("Close rotation (Euler angles)")]
    [SerializeField] private Vector3 closedRotation = new Vector3(0, 0, 0);
    
    [Tooltip("Door animation speed")]
    [SerializeField] private float doorSpeed = 2f;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound when door opens")]
    [SerializeField] private AudioClip openSound;
    
    [Tooltip("Sound when unlocking exit door")]
    [SerializeField] private AudioClip unlockSound;
    
    [Tooltip("Sound when door is locked (denied)")]
    [SerializeField] private AudioClip deniedSound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private bool isAnimating = false;
    private bool isUnlocked = false; // Track if exit door has been unlocked
    
    // Public properties
    public bool IsUnlocked => isUnlocked;
    public bool IsOpen => isOpen;
    
    void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || deniedSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
        
        // Default door transform to self if not assigned
        if (doorTransform == null)
        {
            doorTransform = transform;
        }
    }
    
    /// <summary>
    /// IInteractable - Called when player presses E
    /// </summary>
    public void Interact()
    {
        // Check if player meets requirements
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ExitDoor] GameManager not found!");
            return;
        }
        
        bool canUnlock = GameManager.Instance.CanOpenExitDoor();
        
        // STEP 1: Try to unlock exit door first
        if (!isUnlocked)
        {
            if (!canUnlock)
            {
                // Show why door can't unlock
                bool hasKey = GameManager.Instance.HasExitKey();
                int paperCount = GameManager.Instance.GetPaperCount();
                int required = GameManager.Instance.RequiredPaperCount;
                
                if (!hasKey)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log("[ExitDoor] Missing exit key!");
                    }
                }
                
                if (paperCount < required)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[ExitDoor] Missing papers: {paperCount}/{required}");
                    }
                }
                
                // Play denied sound
                if (audioSource != null && deniedSound != null)
                {
                    audioSource.PlayOneShot(deniedSound);
                }
                
                return;
            }
            else
            {
                // Unlock successful!
                isUnlocked = true;
                
                if (showDebugLogs)
                {
                    Debug.Log("[ExitDoor] Exit door unlocked! Can now open.");
                }
                
                // Play unlock sound
                if (audioSource != null && unlockSound != null)
                {
                    audioSource.PlayOneShot(unlockSound);
                }
                
                return; // Exit - don't open yet
            }
        }
        
        // STEP 2: Door is unlocked - toggle open/close
        if (!isAnimating)
        {
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }
    
    /// <summary>
    /// IInteractable - Get prompt text
    /// </summary>
    public string GetInteractPrompt()
    {
        if (GameManager.Instance == null) return "[E] Exit Door";
        
        // If already unlocked, show open/close prompt
        if (isUnlocked)
        {
            return isOpen ? "E - close exit door" : "E - open exit door";
        }
        
        // Not unlocked yet - show unlock prompt or requirements
        bool canUnlock = GameManager.Instance.CanOpenExitDoor();
        
        if (canUnlock)
        {
            return "E - unlock Exit Door";
        }
        else
        {
            bool hasKey = GameManager.Instance.HasExitKey();
            int paperCount = GameManager.Instance.GetPaperCount();
            int required = GameManager.Instance.RequiredPaperCount;
            
            if (!hasKey && paperCount < required)
            {
                return $"Exit Locked\nNeed: Exit Key + {required - paperCount} more paper(s)";
            }
            else if (!hasKey)
            {
                return "Exit Locked\nNeed: Exit Key";
            }
            else
            {
                return $"Exit Locked\nNeed: {required - paperCount} more paper(s)";
            }
        }
    }
    
    /// <summary>
    /// Open the door
    /// </summary>
    void OpenDoor()
    {
        if (isOpen) return;
        
        isOpen = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[ExitDoor] Door opening... Player can enter hallway.");
        }
        
        // Play open sound
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        // Animate door
        StartCoroutine(AnimateDoor(openRotation));
    }
    
    /// <summary>
    /// Close the door
    /// </summary>
    void CloseDoor()
    {
        if (!isOpen) return;
        
        isOpen = false;
        
        if (showDebugLogs)
        {
            Debug.Log("[ExitDoor] Door closing...");
        }
        
        // Animate door
        StartCoroutine(AnimateDoor(closedRotation));
    }
    
    /// <summary>
    /// Animate door rotation
    /// </summary>
    System.Collections.IEnumerator AnimateDoor(Vector3 targetRotation)
    {
        isAnimating = true;
        
        Quaternion startRotation = doorTransform.localRotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        
        float elapsed = 0f;
        float duration = 1f / doorSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            doorTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        
        doorTransform.localRotation = endRotation;
        isAnimating = false;
    }
    
    /// <summary>
    /// Draw gizmos for exit door
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw exit door location
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.3f);
    }
}


