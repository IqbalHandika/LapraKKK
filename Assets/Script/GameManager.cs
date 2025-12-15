using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game manager (Singleton pattern)
/// Handles win condition, item collection tracking, and game state
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Win Condition Settings")]
    [Tooltip("Number of papers required (Bab 1-5)")]
    [SerializeField] private int requiredPaperCount = 5;
    
    [Tooltip("Item ID for exit door key")]
    [SerializeField] private string exitKeyID = "key_exit";
    
    [Tooltip("Exit door that unlocks when items collected")]
    [SerializeField] private ExitDoor exitDoor;
    
    [Header("Scene Names")]
    [Tooltip("Name of win scene to load")]
    [SerializeField] private string winSceneName = "EndingWin";
    
    [Tooltip("Name of lose scene to load")]
    [SerializeField] private string loseSceneName = "EndingLose";
    
    [Header("References")]
    [Tooltip("Player inventory (auto-detected if null)")]
    [SerializeField] private Inventory playerInventory;
    
    [Header("Audio (Optional)")]
    [Tooltip("Sound played when exit unlocks")]
    [SerializeField] private AudioClip exitUnlockedSound;
    
    [Tooltip("Sound played when player wins")]
    [SerializeField] private AudioClip victorySound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // State tracking
    private bool hasWon = false;
    private bool hasLost = false;
    private bool exitUnlocked = false;
    
    // Public properties
    public bool HasWon => hasWon;
    public bool HasLost => hasLost;
    public bool ExitUnlocked => exitUnlocked;
    public int RequiredPaperCount => requiredPaperCount;
    public string ExitKeyID => exitKeyID;
    public Inventory PlayerInventory => playerInventory;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    void Start()
    {
        // Auto-detect player inventory if not assigned
        if (playerInventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
            }
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("[GameManager] Player Inventory not found! Assign manually or ensure Player has Inventory component.");
            return;
        }
        
        // Validate exit door
        if (exitDoor == null)
        {
            Debug.LogWarning("[GameManager] Exit Door not assigned! Assign in Inspector.");
        }
        
        // Subscribe to item collection event
        playerInventory.OnItemCollected.AddListener(OnItemCollected);
        
        if (showDebugLogs)
        {
            Debug.Log($"[GameManager] Initialized. Required: {requiredPaperCount} papers + exit key (ID: {exitKeyID})");
        }
    }
    
    void Update()
    {
        // No longer auto-unlock exit - player must manually open with key + all papers
    }
    
    /// <summary>
    /// Called when player collects an item (via UnityEvent)
    /// </summary>
    void OnItemCollected(Collectible item)
    {
        if (item == null) return;
        
        // Only count papers for progress tracking
        if (item.Type == Collectible.ItemType.Paper)
        {
            int paperCount = playerInventory.GetPaperCount();
            int remaining = requiredPaperCount - paperCount;
            
            if (showDebugLogs)
            {
                Debug.Log($"[GameManager] Paper collected: '{item.ItemName}' | Progress: {paperCount}/{requiredPaperCount} | Remaining: {remaining}");
            }
            
            // Just log progress, no auto-unlock
            if (paperCount >= requiredPaperCount)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[GameManager] All {requiredPaperCount} papers collected! Find exit key to escape.");
                }
            }
        }
        else if (item.Type == Collectible.ItemType.Key)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[GameManager] Key collected: '{item.ItemName}' (ID: {item.ItemID})");
            }
            
            // Check if it's the exit key
            if (item.ItemID == exitKeyID)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[GameManager] EXIT KEY OBTAINED! Collect all papers then go to exit door.");
                }
            }
        }
    }
    
    /// <summary>
    /// Trigger win state (called by WinZone when player caught inside)
    /// </summary>
    public void TriggerWin()
    {
        if (hasWon) return;
        
        hasWon = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[GameManager] PLAYER WINS! Loading win scene...");
        }
        
        // Play victory sound (optional)
        if (victorySound != null)
        {
            AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position);
        }
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Resume time
        Time.timeScale = 1f;
        
        // Load win scene
        SceneManager.LoadScene(winSceneName);
    }
    
    /// <summary>
    /// Trigger lose state (called when player is caught/dies)
    /// </summary>
    public void TriggerLose()
    {
        if (hasWon) return; // Can't lose if already won
        if (hasLost) return; // Already lost
        
        hasLost = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[GameManager] PLAYER CAUGHT! Loading lose scene...");
        }
        
        // Disable player input
        DisablePlayerInput();
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Resume time
        Time.timeScale = 1f;
        
        // Load lose scene
        SceneManager.LoadScene(loseSceneName);
    }
    
    /// <summary>
    /// Restart current scene
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// Load main menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene("MainMenu"); // Load by name
    }
    
    /// <summary>
    /// Quit application
    /// </summary>
    public void QuitGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("[GameManager] Quitting game...");
        }
        
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    /// <summary>
    /// Get current collection progress (papers only)
    /// </summary>
    public float GetProgress()
    {
        if (playerInventory == null) return 0f;
        int paperCount = playerInventory.GetPaperCount();
        return (float)paperCount / requiredPaperCount;
    }
    
    /// <summary>
    /// Get remaining papers needed
    /// </summary>
    public int GetRemainingPapers()
    {
        if (playerInventory == null) return requiredPaperCount;
        int paperCount = playerInventory.GetPaperCount();
        return Mathf.Max(0, requiredPaperCount - paperCount);
    }
    
    /// <summary>
    /// Check if player has exit key
    /// </summary>
    public bool HasExitKey()
    {
        if (playerInventory == null) return false;
        return playerInventory.HasItem(exitKeyID);
    }
    
    /// <summary>
    /// Check if player has a specific key by itemID
    /// </summary>
    public bool HasKey(string keyID)
    {
        if (playerInventory == null) return false;
        if (string.IsNullOrEmpty(keyID)) return false;
        return playerInventory.HasItem(keyID);
    }
    
    /// <summary>
    /// Check if player can open exit door (has key + all papers)
    /// </summary>
    public bool CanOpenExitDoor()
    {
        if (playerInventory == null) return false;
        
        bool hasKey = playerInventory.HasItem(exitKeyID);
        int paperCount = playerInventory.GetPaperCount();
        bool hasAllPapers = paperCount >= requiredPaperCount;
        
        return hasKey && hasAllPapers;
    }
    
    /// <summary>
    /// Get current paper count
    /// </summary>
    public int GetPaperCount()
    {
        if (playerInventory == null) return 0;
        return playerInventory.GetPaperCount();
    }
    
    /// <summary>
    /// Disable player input (movement, interaction, etc.)
    /// </summary>
    void DisablePlayerInput()
    {
        // Disable player movement controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Try SimpleMovement (FPP) first
            SimpleMovement simpleMovement = player.GetComponent<SimpleMovement>();
            if (simpleMovement != null)
            {
                simpleMovement.enabled = false;
                
                if (showDebugLogs)
                {
                    Debug.Log("[GameManager] Player movement (SimpleMovement) disabled");
                }
            }
            else
            {
                // Fallback: Try TPPMovementController (Third Person)
                TPPMovementController tppMovement = player.GetComponent<TPPMovementController>();
                if (tppMovement != null)
                {
                    tppMovement.enabled = false;
                    
                    if (showDebugLogs)
                    {
                        Debug.Log("[GameManager] Player movement (TPPMovementController) disabled");
                    }
                }
            }
            
            // Disable player interaction
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction != null)
            {
                interaction.enabled = false;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerInventory != null)
        {
            playerInventory.OnItemCollected.RemoveListener(OnItemCollected);
        }
        
        // Reset time scale
        Time.timeScale = 1f;
    }
}
