using UnityEngine;

/// <summary>
/// Component for patrol points that can trigger random room entry
/// Attach this to patrol point GameObjects to enable room exploration behavior
/// </summary>
public class PatrolPoint : MonoBehaviour
{
    /// <summary>
    /// Room entry data with outer (doorway) and inner (exploration) points
    /// </summary>
    [System.Serializable]
    public class RoomEntry
    {
        [Tooltip("Point outside the door (where AI waits for door to open)")]
        public Transform outerPoint;
        
        [Tooltip("Point inside the room (where AI explores)")]
        public Transform innerPoint;
        
        [Tooltip("Time to spend exploring inside the room")]
        public float exploreTime = 3f;
        
        public bool IsValid()
        {
            return outerPoint != null && innerPoint != null;
        }
    }
    
    [Header("Room Entry Settings")]
    [Tooltip("Enable random room entry when AI reaches this patrol point")]
    [SerializeField] private bool canEnterRoom = false;
    
    [Tooltip("Probability of entering a room (0.0 = never, 1.0 = always)")]
    [Range(0f, 1f)]
    [SerializeField] private float roomEntryChance = 0.3f;
    
    [Header("Room Entries (Outer + Inner Points)")]
    [Tooltip("Array of room entries with outer and inner waypoints")]
    [SerializeField] private RoomEntry[] roomEntries;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    /// <summary>
    /// Check if AI should enter a room from this patrol point
    /// </summary>
    /// <returns>True if room entry is enabled and random roll succeeds</returns>
    public bool ShouldEnterRoom()
    {
        if (!canEnterRoom) return false;
        
        // Random roll against room entry chance
        float roll = Random.value;
        bool shouldEnter = roll < roomEntryChance;
        
        if (showDebugLogs)
        {
            Debug.Log($"[PatrolPoint] '{gameObject.name}' room entry roll: {roll:F2} vs {roomEntryChance:F2} = {(shouldEnter ? "ENTER" : "SKIP")}");
        }
        
        return shouldEnter;
    }
    
    /// <summary>
    /// Get a random room entry from the available rooms
    /// </summary>
    /// <returns>Random room entry, or null if no valid rooms available</returns>
    public RoomEntry GetRandomRoomEntry()
    {
        // Check if room entries array exists and has elements
        if (roomEntries == null || roomEntries.Length == 0)
        {
            Debug.LogWarning($"PatrolPoint '{gameObject.name}': No room entries assigned!");
            return null;
        }
        
        // Filter out invalid entries
        int validRoomCount = 0;
        foreach (RoomEntry entry in roomEntries)
        {
            if (entry != null && entry.IsValid()) validRoomCount++;
        }
        
        if (validRoomCount == 0)
        {
            Debug.LogWarning($"PatrolPoint '{gameObject.name}': All room entries are invalid!");
            return null;
        }
        
        // Get random valid room entry
        RoomEntry selectedEntry = null;
        int randomIndex = Random.Range(0, roomEntries.Length);
        int attempts = 0;
        
        // Try to find a valid room entry (max attempts = array length)
        while (selectedEntry == null && attempts < roomEntries.Length)
        {
            if (roomEntries[randomIndex] != null && roomEntries[randomIndex].IsValid())
            {
                selectedEntry = roomEntries[randomIndex];
            }
            else
            {
                randomIndex = (randomIndex + 1) % roomEntries.Length;
                attempts++;
            }
        }
        
        if (showDebugLogs && selectedEntry != null)
        {
            Debug.Log($"[PatrolPoint] '{gameObject.name}' selected room entry - Outer: '{selectedEntry.outerPoint.name}', Inner: '{selectedEntry.innerPoint.name}'");
        }
        
        return selectedEntry;
    }
    
    /// <summary>
    /// LEGACY: Get a random room target (deprecated, use GetRandomRoomEntry instead)
    /// </summary>
    [System.Obsolete("Use GetRandomRoomEntry() for better door handling")]
    public Transform GetRandomRoom()
    {
        var entry = GetRandomRoomEntry();
        return entry != null ? entry.innerPoint : null;
    }
    
    /// <summary>
    /// Draw gizmos in Scene View for visualization
    /// </summary>
    void OnDrawGizmos()
    {
        // Draw sphere at patrol point position
        if (canEnterRoom)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw lines to room entries
        if (roomEntries != null && roomEntries.Length > 0)
        {
            foreach (RoomEntry entry in roomEntries)
            {
                if (entry != null && entry.IsValid())
                {
                    // Draw line from patrol point to outer point (cyan)
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, entry.outerPoint.position);
                    Gizmos.DrawWireSphere(entry.outerPoint.position, 0.3f);
                    
                    // Draw line from outer to inner point (yellow)
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(entry.outerPoint.position, entry.innerPoint.position);
                    Gizmos.DrawWireSphere(entry.innerPoint.position, 0.4f);
                }
            }
        }
    }
    
    /// <summary>
    /// Draw additional gizmos when selected
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw label
        if (canEnterRoom)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }
}
