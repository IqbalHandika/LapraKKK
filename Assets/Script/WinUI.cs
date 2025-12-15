using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Win screen UI controller
/// Shows victory message and buttons when player escapes
/// </summary>
public class WinUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main win screen panel (parent of all UI elements)")]
    [SerializeField] private GameObject winPanel;
    
    [Tooltip("Victory message text")]
    [SerializeField] private TextMeshProUGUI victoryText;
    
    [Tooltip("Optional: Statistics text (items collected, time, etc)")]
    [SerializeField] private TextMeshProUGUI statsText;
    
    [Header("Buttons")]
    [Tooltip("Restart game button")]
    [SerializeField] private Button restartButton;
    
    [Tooltip("Main menu button")]
    [SerializeField] private Button mainMenuButton;
    
    [Tooltip("Quit game button")]
    [SerializeField] private Button quitButton;
    
    [Header("Victory Messages")]
    [Tooltip("Main victory message")]
    [SerializeField] private string victoryMessage = "YOU ESCAPED!";
    
    [Tooltip("Subtitle message")]
    [SerializeField] private string subtitleMessage = "Congratulations! You collected all items and escaped.";
    
    [Header("Animation (Optional)")]
    [Tooltip("Fade in duration when showing win screen")]
    [SerializeField] private float fadeInDuration = 1f;
    
    [SerializeField] private bool animateFadeIn = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private CanvasGroup canvasGroup;
    private bool isShowing = false;
    
    void Start()
    {
        // Setup canvas group for fade animation
        canvasGroup = winPanel?.GetComponent<CanvasGroup>();
        if (canvasGroup == null && winPanel != null && animateFadeIn)
        {
            canvasGroup = winPanel.AddComponent<CanvasGroup>();
        }
        
        // Hide win panel initially
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // Validate references
        if (winPanel == null)
        {
            Debug.LogError("[WinUI] Win Panel not assigned! Assign in Inspector.");
        }
    }
    
    /// <summary>
    /// Show the win screen (called by GameManager)
    /// </summary>
    public void ShowWinScreen()
    {
        if (isShowing || winPanel == null) return;
        
        isShowing = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[WinUI] Showing win screen...");
        }
        
        // Set victory text
        if (victoryText != null)
        {
            victoryText.text = victoryMessage;
        }
        
        // Set stats text
        UpdateStatsText();
        
        // Show panel
        winPanel.SetActive(true);
        
        // Fade in animation
        if (animateFadeIn && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Update statistics text (items collected, time, etc)
    /// </summary>
    void UpdateStatsText()
    {
        if (statsText == null) return;
        
        string stats = subtitleMessage + "\n\n";
        
        // Get stats from GameManager
        if (GameManager.Instance != null)
        {
            int requiredPapers = GameManager.Instance.RequiredPaperCount;
            stats += $"Papers Collected: {requiredPapers}/{requiredPapers}\n";
            stats += "Exit Key: Found\n";
        }
        
        // Optional: Add playtime
        float playTime = Time.timeSinceLevelLoad;
        int minutes = Mathf.FloorToInt(playTime / 60f);
        int seconds = Mathf.FloorToInt(playTime % 60f);
        stats += $"Time: {minutes:00}:{seconds:00}";
        
        statsText.text = stats;
    }
    
    /// <summary>
    /// Fade in animation
    /// </summary>
    System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Hide win screen
    /// </summary>
    public void HideWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        isShowing = false;
    }
    
    /// <summary>
    /// Restart button clicked
    /// </summary>
    void OnRestartClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[WinUI] Restart button clicked");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    /// <summary>
    /// Main menu button clicked
    /// </summary>
    void OnMainMenuClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[WinUI] Main menu button clicked");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
    
    /// <summary>
    /// Quit button clicked
    /// </summary>
    void OnQuitClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[WinUI] Quit button clicked");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}
