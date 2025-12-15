using UnityEngine;

/// <summary>
/// Win zone trigger - player wins when entering this area
/// </summary>
public class WinZone : MonoBehaviour
{
    [Header("Win Zone Settings")]
    [SerializeField] private bool isActive = true;
    [Tooltip("Delay before triggering win (untuk dramatic effect)")]
    [SerializeField] private float winDelay = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggeredWin = false;
    
    void Start()
    {
        // Setup trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("[WinZone] No Collider found! Add a Box Collider or Capsule Collider.");
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[WinZone] Initialized at {transform.position}. Active: {isActive}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player") && isActive && !hasTriggeredWin)
        {
            if (showDebugLogs)
            {
                Debug.Log("[WinZone] Player entered win zone! Waiting for catch...");
            }
            
            // Play safe zone sound (optional)
            if (winSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(winSound);
            }
            
            // Mark player as safe (won't die even if caught)
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetInWinZone(true);
                
                if (showDebugLogs)
                {
                    Debug.Log("[WinZone] Player marked as safe - will win when caught!");
                }
            }
            
            // DON'T trigger win yet - wait for catch!
        }
    }
    
    /// <summary>
    /// Trigger win condition (called by PlayerHealth when caught in zone)
    /// </summary>
    public void TriggerWin()
    {
        if (hasTriggeredWin) return; // Already won
        
        hasTriggeredWin = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin();
            
            if (showDebugLogs)
            {
                Debug.Log("[WinZone] Win triggered after catch!");
            }
        }
        else
        {
            Debug.LogError("[WinZone] GameManager not found! Cannot trigger win.");
        }
    }
    
    /// <summary>
    /// Activate/deactivate win zone
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (showDebugLogs)
        {
            Debug.Log($"[WinZone] Set active: {active}");
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw win zone area
        Gizmos.color = hasTriggeredWin ? Color.green : (isActive ? Color.cyan : Color.gray);
        Gizmos.matrix = transform.localToWorldMatrix;
        
        // Draw based on collider type
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider boxCol)
        {
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is CapsuleCollider capsuleCol)
        {
            // Draw simplified capsule
            Vector3 center = capsuleCol.center;
            float radius = capsuleCol.radius;
            float height = capsuleCol.height;
            Gizmos.DrawWireSphere(center + Vector3.up * (height * 0.5f - radius), radius);
            Gizmos.DrawWireSphere(center - Vector3.up * (height * 0.5f - radius), radius);
        }
    }
}
