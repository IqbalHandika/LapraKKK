using UnityEngine;

/// <summary>
/// Data structure for storing collected item information
/// This survives after the GameObject is destroyed
/// </summary>
[System.Serializable]
public class ItemData
{
    public Collectible.ItemType type;
    public string itemName;
    public string itemID;
    
    public ItemData(Collectible.ItemType type, string itemName, string itemID)
    {
        this.type = type;
        this.itemName = itemName;
        this.itemID = itemID;
    }
}

/// <summary>
/// Collectible item that can be picked up by the player via interaction (Press E)
/// Implements IInteractable interface for consistent interaction system
/// </summary>
[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Types of collectible items in the game
    /// </summary>
    public enum ItemType
    {
        Paper,       // Bab 1-5 papers (required to complete game)
        Key          // Keys for opening doors
    }
    
    [Header("Item Information")]
    [Tooltip("Type of this collectible item")]
    [SerializeField] private ItemType itemType = ItemType.Paper;
    
    [Tooltip("Display name of the item")]
    [SerializeField] private string itemName = "Item";
    
    [Tooltip("Unique identifier for this item (e.g., 'paper_bab_1', 'key_exit')")]
    [SerializeField] private string itemID = "";
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound played when item is collected")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Settings")]
    [Tooltip("Rotate the item slowly for visual effect")]
    [SerializeField] private bool autoRotate = true;
    
    [Tooltip("Rotation speed (degrees per second)")]
    [SerializeField] private float rotationSpeed = 50f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // Public getters
    public ItemType Type => itemType;
    public string ItemName => itemName;
    public string ItemID => itemID;
    
    void Start()
    {
        // Validate setup - Collider doesn't need to be trigger for raycast interaction
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"Collectible '{itemName}': No Collider found! Adding BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Auto-generate ID if empty
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = $"{itemType}_{gameObject.name}_{GetInstanceID()}";
            Debug.LogWarning($"Collectible '{itemName}': No itemID set! Auto-generated: {itemID}");
        }
    }
    
    void Update()
    {
        // Auto-rotate for visual effect
        if (autoRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    
    /// <summary>
    /// IInteractable implementation - Called when player presses E
    /// </summary>
    public void Interact()
    {
        // Find player inventory
        Inventory inventory = FindObjectOfType<Inventory>();
        
        if (inventory == null)
        {
            Debug.LogError($"Collectible '{itemName}': No Inventory found in scene!");
            return;
        }
        
        // Add item to inventory
        bool added = inventory.AddItem(this);
        
        if (added)
        {
            // Play pickup sound if available
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"[Collectible] Player picked up '{itemName}' (ID: {itemID})");
            }
            
            // Destroy the collectible GameObject
            Destroy(gameObject);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[Collectible] Failed to add '{itemName}' - inventory full or duplicate");
            }
        }
    }
    
    /// <summary>
    /// IInteractable implementation - Returns prompt text shown to player
    /// </summary>
    public string GetInteractPrompt()
    {
        // Return different prompts based on item type
        switch (itemType)
        {
            case ItemType.Paper:
                return $"[E] Collect {itemName}";
            case ItemType.Key:
                return $"[E] Collect {itemName}";
            default:
                return $"[E] Pick up {itemName}";
        }
    }
    
    /// <summary>
    /// Draw gizmos in Scene View for easy identification
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw yellow sphere at item position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw icon based on type
        switch (itemType)
        {
            case ItemType.Paper:
                Gizmos.color = Color.white; // White for papers
                break;
            case ItemType.Key:
                Gizmos.color = Color.cyan; // Cyan for keys
                break;
        }
        
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
    
    /// <summary>
    /// Draw additional info when selected
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw trigger radius (approximate)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
