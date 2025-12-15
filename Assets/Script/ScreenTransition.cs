using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Screen transition effects (fade to black, fade in, etc)
/// Used before playing cutscenes or scene transitions
/// </summary>
public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance { get; private set; }
    
    [Header("Transition Settings")]
    [Tooltip("UI Image for fade overlay (full screen black image)")]
    [SerializeField] private Image fadeImage;
    
    [Tooltip("Default fade duration in seconds")]
    [SerializeField] private float defaultFadeDuration = 1f;
    
    [Tooltip("Color to fade to (usually black)")]
    [SerializeField] private Color fadeColor = Color.black;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isTransitioning = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }
    
    void Start()
    {
        // Validate fade image
        if (fadeImage == null)
        {
            Debug.LogError("[ScreenTransition] Fade Image not assigned! Create a full-screen black Image in Canvas.");
            return;
        }
        
        // Start with transparent (no fade)
        SetAlpha(0f);
        fadeImage.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Fade to black (or specified color)
    /// </summary>
    public void FadeOut(float duration = -1f, System.Action onComplete = null)
    {
        if (duration < 0) duration = defaultFadeDuration;
        
        if (showDebugLogs)
        {
            Debug.Log($"[ScreenTransition] Fading out to black ({duration}s)");
        }
        
        StartCoroutine(FadeCoroutine(0f, 1f, duration, onComplete));
    }
    
    /// <summary>
    /// Fade from black to clear
    /// </summary>
    public void FadeIn(float duration = -1f, System.Action onComplete = null)
    {
        if (duration < 0) duration = defaultFadeDuration;
        
        if (showDebugLogs)
        {
            Debug.Log($"[ScreenTransition] Fading in from black ({duration}s)");
        }
        
        StartCoroutine(FadeCoroutine(1f, 0f, duration, onComplete));
    }
    
    /// <summary>
    /// Instant fade to black (no animation)
    /// </summary>
    public void SetBlack()
    {
        SetAlpha(1f);
    }
    
    /// <summary>
    /// Instant fade to clear (no animation)
    /// </summary>
    public void SetClear()
    {
        SetAlpha(0f);
    }
    
    /// <summary>
    /// Fade coroutine
    /// </summary>
    IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        isTransitioning = true;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for paused games
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetAlpha(alpha);
            yield return null;
        }
        
        SetAlpha(endAlpha);
        isTransitioning = false;
        
        // Call completion callback
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Set fade image alpha
    /// </summary>
    void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        
        Color color = fadeColor;
        color.a = alpha;
        fadeImage.color = color;
    }
    
    public bool IsTransitioning => isTransitioning;
}
