using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Loading Screen - Display "Loading..." text and load next scene
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [Header("Scene Settings")]
    [Tooltip("Scene to load after loading screen")]
    [SerializeField] private string nextSceneName = "GameScene";
    
    [Header("Animation Settings")]
    [SerializeField] private bool animateDots = true;
    [SerializeField] private float dotAnimationSpeed = 0.5f;
    [SerializeField] private int maxDots = 3;
    
    [Header("Timing")]
    [Tooltip("Minimum time to show loading screen (seconds)")]
    [SerializeField] private float minimumLoadTime = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private int currentDots = 0;
    private float dotTimer = 0f;
    
    void Start()
    {
        if (loadingText == null)
        {
            Debug.LogError("[LoadingScreen] Loading Text not assigned!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[LoadingScreen] Loading screen started. Will load: {nextSceneName}");
        }
        
        // Start loading
        StartCoroutine(LoadSceneAsync());
    }
    
    void Update()
    {
        if (!animateDots || loadingText == null) return;
        
        // Animate dots
        dotTimer += Time.deltaTime;
        
        if (dotTimer >= dotAnimationSpeed)
        {
            dotTimer = 0f;
            currentDots = (currentDots + 1) % (maxDots + 1);
            
            // Update text
            string dots = new string('.', currentDots);
            loadingText.text = "Loading" + dots;
        }
    }
    
    /// <summary>
    /// Load next scene asynchronously
    /// </summary>
    IEnumerator LoadSceneAsync()
    {
        // Wait minimum time
        float startTime = Time.time;
        
        // Start loading scene in background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;
        
        if (showDebugLogs)
        {
            Debug.Log($"[LoadingScreen] Loading {nextSceneName}...");
        }
        
        // Wait until scene is loaded and minimum time has passed
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (showDebugLogs)
            {
                Debug.Log($"[LoadingScreen] Progress: {progress * 100}%");
            }
            
            // Check if loading is done (progress reaches 0.9 when ready)
            if (asyncLoad.progress >= 0.9f)
            {
                // Wait for minimum time
                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= minimumLoadTime)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"[LoadingScreen] Loading complete! Activating scene...");
                    }
                    
                    asyncLoad.allowSceneActivation = true;
                }
            }
            
            yield return null;
        }
    }
}
