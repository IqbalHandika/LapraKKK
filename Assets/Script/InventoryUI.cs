using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI display for inventory system
/// Shows item count and updates when items are collected
/// Attach to a Canvas GameObject
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display item count")]
    [SerializeField] private TextMeshProUGUI itemCountText;
    
    [Header("Settings")]
    [Tooltip("Total number of collectibles in the level (for X/Y display)")]
    [SerializeField] private int totalItemsInLevel = 10;
    
    [Tooltip("Text format: {0} = current count, {1} = total")]
    [SerializeField] private string textFormat = "Items: {0}/{1}";
    
    [Header("Optional: Pickup Notification")]
    [Tooltip("Text to show when item is picked up")]
    [SerializeField] private TextMeshProUGUI pickupNotificationText;
    
    [Tooltip("How long to show pickup notification (seconds)")]
    [SerializeField] private float notificationDuration = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // Changed to true for debugging
    
    private Inventory playerInventory;
    private float notificationTimer = 0f;
    
    void Start()
    {
        Debug.Log("[InventoryUI] *** START METHOD CALLED ***");
        
        // Find player's inventory component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            Debug.Log($"[InventoryUI] Found Player GameObject: {player.name}");
            playerInventory = player.GetComponent<Inventory>();
            
            if (playerInventory != null)
            {
                // Subscribe to item collection event
                playerInventory.OnItemCollected.AddListener(OnItemCollected);
                
                Debug.Log("[InventoryUI] ✓ Connected to player inventory and subscribed to OnItemCollected event");
            }
            else
            {
                Debug.LogError("[InventoryUI] Player doesn't have Inventory component!");
            }
        }
        else
        {
            Debug.LogError("[InventoryUI] Player GameObject with 'Player' tag not found!");
        }
        
        // Validate UI references
        if (itemCountText == null)
        {
            Debug.LogError("[InventoryUI] ❌ Item Count Text is NOT assigned in Inspector!");
        }
        else
        {
            Debug.Log($"[InventoryUI] ✓ Item Count Text assigned: {itemCountText.name}");
        }
        
        if (pickupNotificationText != null)
        {
            Debug.Log($"[InventoryUI] ✓ Pickup Notification Text assigned: {pickupNotificationText.name}");
            pickupNotificationText.gameObject.SetActive(false);
        }
        
        // Initial UI update
        UpdateItemCountText();
    }
    
    void Update()
    {
        // Handle pickup notification timer
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            
            if (notificationTimer <= 0f && pickupNotificationText != null)
            {
                pickupNotificationText.gameObject.SetActive(false);
            }
        }
        
        // Force update UI every frame (fallback if event doesn't work)
        UpdateItemCountText();
    }
    
    /// <summary>
    /// Called when player collects an item (via UnityEvent)
    /// </summary>
    /// <param name="item">Collected item</param>
    void OnItemCollected(Collectible item)
    {
        if (item == null) return;
        
        Debug.Log($"[InventoryUI] *** OnItemCollected CALLED - Item: {item.ItemName} ***");
        
        // Update item count display
        UpdateItemCountText();
        
        // Show pickup notification
        ShowPickupNotification(item);
        
        if (showDebugLogs)
        {
            Debug.Log($"[InventoryUI] Displayed collection of '{item.ItemName}'");
        }
    }
    
    /// <summary>
    /// Update the item count text display
    /// </summary>
    void UpdateItemCountText()
    {
        if (itemCountText == null)
        {
            Debug.LogError("[InventoryUI] Cannot update text - itemCountText is NULL!");
            return;
        }
        
        // COUNT ONLY PAPER (BAB), NOT KEYS!
        int currentCount = playerInventory != null ? playerInventory.GetPaperCount() : 0;
        
        // Format: "Kertas: 3/5"
        string newText = string.Format(textFormat, currentCount, totalItemsInLevel);
        itemCountText.text = newText;
        
        Debug.Log($"[InventoryUI] Updated text to: '{newText}' (Papers: {currentCount}/{totalItemsInLevel})");
    }
    
    /// <summary>
    /// Show temporary notification when item is picked up
    /// </summary>
    /// <param name="item">Collected item</param>
    void ShowPickupNotification(Collectible item)
    {
        if (pickupNotificationText == null) return;
        
        // Show notification text
        pickupNotificationText.text = $"{item.ItemName} collected!";
        pickupNotificationText.gameObject.SetActive(true);
        
        // Reset timer
        notificationTimer = notificationDuration;
    }
    
    /// <summary>
    /// Manually update the total items count (useful if items are spawned dynamically)
    /// </summary>
    /// <param name="total">New total count</param>
    public void SetTotalItems(int total)
    {
        totalItemsInLevel = total;
        UpdateItemCountText();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (playerInventory != null)
        {
            playerInventory.OnItemCollected.RemoveListener(OnItemCollected);
        }
    }
}
