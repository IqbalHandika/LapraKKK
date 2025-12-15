using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu Manager - Handle Start and Quit buttons
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the intro/tutorial scene to load")]
    [SerializeField] private string introSceneName = "IntroTutorial";
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip startGameSound;
    private AudioSource audioSource;
    
    [Header("Animation (Optional)")]
    [SerializeField] private bool animateTitle = true;
    [SerializeField] private float titlePulseSpeed = 1f;
    [SerializeField] private float titlePulseAmount = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Vector3 originalTitleScale;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (buttonClickSound != null || startGameSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("[MainMenu] Start Button not assigned!");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        else
        {
            Debug.LogError("[MainMenu] Quit Button not assigned!");
        }
        
        // Store original title scale for animation
        if (titleText != null && animateTitle)
        {
            originalTitleScale = titleText.transform.localScale;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[MainMenu] Main Menu initialized");
        }
    }
    
    void Update()
    {
        // Animate title (pulse effect)
        if (animateTitle && titleText != null)
        {
            float scale = 1f + Mathf.Sin(Time.time * titlePulseSpeed) * titlePulseAmount;
            titleText.transform.localScale = originalTitleScale * scale;
        }
    }
    
    /// <summary>
    /// Start button clicked - Load intro/tutorial scene
    /// </summary>
    public void OnStartButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[MainMenu] Start button clicked - Loading intro scene");
        }
        
        // Play sound
        if (audioSource != null && startGameSound != null)
        {
            audioSource.PlayOneShot(startGameSound);
        }
        else if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Load intro scene
        LoadIntroScene();
    }
    
    /// <summary>
    /// Quit button clicked - Exit game
    /// </summary>
    public void OnQuitButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[MainMenu] Quit button clicked - Exiting game");
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Quit game
        QuitGame();
    }
    
    /// <summary>
    /// Load intro/tutorial scene
    /// </summary>
    void LoadIntroScene()
    {
        if (string.IsNullOrEmpty(introSceneName))
        {
            Debug.LogError("[MainMenu] Intro scene name not set!");
            return;
        }
        
        SceneManager.LoadScene(introSceneName);
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
