using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dialog System - Display text dialogs with Next/Skip functionality
/// Used for Intro/Tutorial and Ending scenes
/// </summary>
public class DialogSystem : MonoBehaviour
{
    [Header("Dialog Content")]
    [Tooltip("List of dialog texts to display")]
    [SerializeField] private List<string> dialogTexts = new List<string>();
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    
    [Header("Scene Settings")]
    [Tooltip("Scene to load after dialog ends (e.g., 'GameScene')")]
    [SerializeField] private string nextSceneName = "";
    [Tooltip("Main menu scene name")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [Tooltip("Game scene name for Play Again")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Animation Settings")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip dialogSound;
    [SerializeField] private AudioClip buttonClickSound;
    private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private int currentDialogIndex = 0;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private bool dialogsComplete = false;
    private TextMeshProUGUI nextButtonText;
    private TextMeshProUGUI skipButtonText;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (dialogSound != null || buttonClickSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Setup buttons
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
            nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            skipButtonText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Validate
        if (dialogTexts.Count == 0)
        {
            Debug.LogWarning("[DialogSystem] No dialog texts assigned!");
        }
        
        // Display first dialog
        DisplayCurrentDialog();
        
        if (showDebugLogs)
        {
            Debug.Log($"[DialogSystem] Initialized with {dialogTexts.Count} dialogs");
        }
    }
    
    void Update()
    {
        // Allow Enter/Space to advance dialog
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                CompleteTypewriter();
            }
            else
            {
                OnNextButtonClicked();
            }
        }
        
        // Allow Escape to skip
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnSkipButtonClicked();
        }
    }
    
    /// <summary>
    /// Display current dialog with optional typewriter effect
    /// </summary>
    void DisplayCurrentDialog()
    {
        if (currentDialogIndex >= dialogTexts.Count)
        {
            OnDialogComplete();
            return;
        }
        
        string text = dialogTexts[currentDialogIndex];
        
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }
        else
        {
            dialogText.text = text;
        }
        
        // Play dialog sound
        if (audioSource != null && dialogSound != null)
        {
            audioSource.PlayOneShot(dialogSound);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[DialogSystem] Displaying dialog {currentDialogIndex + 1}/{dialogTexts.Count}");
        }
    }
    
    /// <summary>
    /// Typewriter effect coroutine
    /// </summary>
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogText.text = "";
        
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    /// <summary>
    /// Complete typewriter effect immediately
    /// </summary>
    void CompleteTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        if (currentDialogIndex < dialogTexts.Count)
        {
            dialogText.text = dialogTexts[currentDialogIndex];
        }
        
        isTyping = false;
    }
    
    /// <summary>
    /// Next button clicked - Advance to next dialog
    /// </summary>
    public void OnNextButtonClicked()
    {
        // If typing, complete typewriter first
        if (isTyping)
        {
            CompleteTypewriter();
            return;
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Advance to next dialog
        currentDialogIndex++;
        DisplayCurrentDialog();
    }
    
    /// <summary>
    /// Skip button clicked - Jump to last dialog immediately
    /// </summary>
    public void OnSkipButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Skip button clicked");
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Stop any typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        isTyping = false;
        
        // Jump to last dialog and show it immediately (no typewriter)
        if (dialogTexts.Count > 0)
        {
            currentDialogIndex = dialogTexts.Count - 1;
            dialogText.text = dialogTexts[currentDialogIndex]; // Full text immediately
            
            // For ending scenes (no nextSceneName), immediately transform buttons
            if (string.IsNullOrEmpty(nextSceneName))
            {
                currentDialogIndex++; // Move past last dialog
                OnDialogComplete();
            }
            // For intro/tutorial scenes, just show last dialog and wait for Next button
        }
    }
    
    /// <summary>
    /// Called when all dialogs are complete
    /// </summary>
    void OnDialogComplete()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] All dialogs complete");
        }
        
        dialogsComplete = true;
        
        // Check if this is an ending scene (no nextSceneName means it's an ending)
        if (string.IsNullOrEmpty(nextSceneName))
        {
            // Transform buttons for ending scene
            TransformButtonsToEnding();
        }
        // Otherwise it's intro/tutorial - transform to Play button and hide Skip
        else
        {
            TransformButtonsForIntro();
        }
    }
    
    /// <summary>
    /// Transform buttons for intro/tutorial scene
    /// </summary>
    void TransformButtonsForIntro()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Transforming buttons to intro mode");
        }
        
        // Transform Next button → Play
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnPlayButtonClicked);
            
            if (nextButtonText != null)
            {
                nextButtonText.text = "Play";
            }
        }
        
        // Hide Skip button
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Transform Next/Skip buttons into Play Again/Main Menu
    /// </summary>
    void TransformButtonsToEnding()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Transforming buttons to ending mode");
        }
        
        // Check current scene to determine button text
        string currentScene = SceneManager.GetActiveScene().name;
        bool isLoseScene = currentScene.Contains("Lose") || currentScene.Contains("lose");
        
        // Transform Next button → Play Again / Try Again
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnPlayAgainButtonClicked);
            
            if (nextButtonText != null)
            {
                // Use "Try Again" for lose scenes, "Play Again" for win scenes
                nextButtonText.text = isLoseScene ? "Try Again" : "Play Again";
            }
        }
        
        // Transform Skip button → Main Menu
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnMainMenuButtonClicked);
            
            if (skipButtonText != null)
            {
                skipButtonText.text = "Main Menu";
            }
        }
    }
    
    /// <summary>
    /// Load next scene after delay
    /// </summary>
    IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (showDebugLogs)
        {
            Debug.Log($"[DialogSystem] Loading next scene: {nextSceneName}");
        }
        
        SceneManager.LoadScene(nextSceneName);
    }
    
    /// <summary>
    /// Play button clicked - Load next scene (for intro/tutorial)
    /// </summary>
    public void OnPlayButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Play button clicked");
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("[DialogSystem] Next scene name not set!");
        }
    }
    
    /// <summary>
    /// Play Again button clicked - Reload game scene
    /// </summary>
    public void OnPlayAgainButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Play Again clicked");
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Load game scene
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("[DialogSystem] Game scene name not set!");
        }
    }
    
    /// <summary>
    /// Main Menu button clicked - Return to main menu
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        if (showDebugLogs)
        {
            Debug.Log("[DialogSystem] Main Menu clicked");
        }
        
        // Play sound
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[DialogSystem] Main menu scene name not set!");
        }
    }
    
    /// <summary>
    /// Add dialog at runtime (for dynamic dialogs)
    /// </summary>
    public void AddDialog(string text)
    {
        dialogTexts.Add(text);
    }
    
    /// <summary>
    /// Clear all dialogs
    /// </summary>
    public void ClearDialogs()
    {
        dialogTexts.Clear();
        currentDialogIndex = 0;
    }
}
