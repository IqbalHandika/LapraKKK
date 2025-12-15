using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Player inventory system for managing collected items
/// Attach this to the Player GameObject
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Maximum number of items that can be stored (0 = unlimited)")]
    [SerializeField] private int maxCapacity = 0; // 0 means unlimited
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showInventoryInInspector = true;
    
    // List of collected items (stores data, not component references)
    private List<ItemData> collectedItems = new List<ItemData>();
    
    // Event triggered when an item is collected (for UI updates)
    public UnityEvent<Collectible> OnItemCollected = new UnityEvent<Collectible>();
    
    // Public properties
    public int ItemCount => collectedItems.Count;
    public int MaxCapacity => maxCapacity;
    public bool IsFull => maxCapacity > 0 && collectedItems.Count >= maxCapacity;
    
    /// <summary>
    /// Add an item to the inventory
    /// </summary>
    /// <param name="item">Collectible item to add</param>
    /// <returns>True if item was added successfully, false otherwise</returns>
    public bool AddItem(Collectible item)
    {
        if (item == null)
        {
            Debug.LogError("[Inventory] Attempted to add null item!");
            return false;
        }
        
        // Check if inventory is full
        if (IsFull)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[Inventory] Cannot add '{item.ItemName}' - Inventory is full ({collectedItems.Count}/{maxCapacity})");
            }
            return false;
        }
        
        // Check for duplicate items (by ID)
        if (HasItem(item.ItemID))
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[Inventory] Item '{item.ItemName}' (ID: {item.ItemID}) already exists in inventory!");
            }
            return false;
        }
        
        // Store item data (not component reference) so it survives GameObject destruction
        ItemData itemData = new ItemData(item.Type, item.ItemName, item.ItemID);
        collectedItems.Add(itemData);
        
        if (showDebugLogs)
        {
            Debug.Log($"[Inventory] Added '{item.ItemName}' (Type: {item.Type}, ID: {item.ItemID}) - Total items: {collectedItems.Count} | Papers: {GetPaperCount()}");
        }
        
        // Trigger event for UI updates (pass original Collectible for sound/effects)
        OnItemCollected?.Invoke(item);
        
        return true;
    }
    
    /// <summary>
    /// Check if inventory contains an item with the specified ID
    /// </summary>
    /// <param name="itemID">Unique item identifier</param>
    /// <returns>True if item exists in inventory</returns>
    public bool HasItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogWarning("[Inventory] HasItem called with null or empty itemID!");
            return false;
        }
        
        foreach (ItemData item in collectedItems)
        {
            if (item != null && item.itemID == itemID)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if inventory contains an item of the specified type
    /// </summary>
    /// <param name="itemType">Type of item to search for</param>
    /// <returns>True if at least one item of that type exists</returns>
    public bool HasItemOfType(Collectible.ItemType itemType)
    {
        foreach (ItemData item in collectedItems)
        {
            if (item != null && item.type == itemType)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Get a specific item by ID
    /// </summary>
    /// <param name="itemID">Unique item identifier</param>
    /// <returns>ItemData or null if not found</returns>
    public ItemData GetItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            return null;
        }
        
        foreach (ItemData item in collectedItems)
        {
            if (item != null && item.itemID == itemID)
            {
                return item;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all items of a specific type
    /// </summary>
    /// <param name="itemType">Type of items to retrieve</param>
    /// <returns>List of matching items</returns>
    public List<ItemData> GetItemsOfType(Collectible.ItemType itemType)
    {
        List<ItemData> matchingItems = new List<ItemData>();
        
        foreach (ItemData item in collectedItems)
        {
            if (item != null && item.type == itemType)
            {
                matchingItems.Add(item);
            }
        }
        
        return matchingItems;
    }
    
    /// <summary>
    /// Remove an item from inventory by ID
    /// </summary>
    /// <param name="itemID">Unique item identifier</param>
    /// <returns>True if item was removed</returns>
    public bool RemoveItem(string itemID)
    {
        ItemData itemToRemove = GetItem(itemID);
        
        if (itemToRemove != null)
        {
            collectedItems.Remove(itemToRemove);
            
            if (showDebugLogs)
            {
                Debug.Log($"[Inventory] Removed '{itemToRemove.itemName}' (ID: {itemID})");
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Clear all items from inventory
    /// </summary>
    public void ClearInventory()
    {
        int count = collectedItems.Count;
        collectedItems.Clear();
        
        if (showDebugLogs)
        {
            Debug.Log($"[Inventory] Cleared {count} items from inventory");
        }
    }
    
    /// <summary>
    /// Get total count of items in inventory
    /// </summary>
    /// <returns>Number of items</returns>
    public int GetItemCount()
    {
        return collectedItems.Count;
    }
    
    /// <summary>
    /// Get count of Paper items only (Bab 1-5) - excludes Keys
    /// </summary>
    /// <returns>Number of Paper items</returns>
    public int GetPaperCount()
    {
        int count = 0;
        
        if (showDebugLogs)
        {
            Debug.Log($"[Inventory] GetPaperCount() called - Total items in list: {collectedItems.Count}");
        }
        
        foreach (ItemData item in collectedItems)
        {
            if (item == null)
            {
                if (showDebugLogs) Debug.LogWarning($"[Inventory] Found NULL item in collectedItems!");
                continue;
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"[Inventory] Checking item: '{item.itemName}' | Type: {item.type} | ID: {item.itemID}");
            }
            
            if (item.type == Collectible.ItemType.Paper)
            {
                count++;
                if (showDebugLogs)
                {
                    Debug.Log($"[Inventory] ✓ Counted Paper #{count}: '{item.itemName}'");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[Inventory] GetPaperCount() result: {count} papers found");
        }
        
        return count;
    }
    
    /// <summary>
    /// Get read-only list of all collected items
    /// </summary>
    /// <returns>List of item data</returns>
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(collectedItems); // Return a copy
    }
    
    // Debug: Show inventory contents in inspector (Editor only)
    void OnValidate()
    {
        #if UNITY_EDITOR
        if (showInventoryInInspector && Application.isPlaying)
        {
            // This will show in inspector during play mode
        }
        #endif
    }
}
