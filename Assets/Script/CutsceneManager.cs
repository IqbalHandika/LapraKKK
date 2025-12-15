using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Cutscene manager for playing win/lose cutscenes
/// Handles video playback with screen transitions
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }
    
    [Header("Cutscene Settings")]
    [Tooltip("VideoPlayer component for playing cutscenes")]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Tooltip("RenderTexture for video display (or use Camera Target)")]
    [SerializeField] private RenderTexture videoRenderTexture;
    
    [Header("Cutscene Clips")]
    [Tooltip("Win cutscene video clip")]
    [SerializeField] private VideoClip winCutscene;
    
    [Tooltip("Lose cutscene video clip")]
    [SerializeField] private VideoClip loseCutscene;
    
    [Header("Transition Settings")]
    [Tooltip("Fade to black duration before cutscene")]
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Header("After Cutscene")]
    [Tooltip("Scene to load after win cutscene (leave empty to restart)")]
    [SerializeField] private string winSceneName = "";
    
    [Tooltip("Scene to load after lose cutscene (leave empty to restart)")]
    [SerializeField] private string loseSceneName = "";
    
    [Tooltip("Auto restart/load scene after cutscene ends")]
    [SerializeField] private bool autoLoadSceneAfterCutscene = true;
    
    [Header("UI")]
    [Tooltip("Canvas to show during cutscene (optional - for skip prompt)")]
    [SerializeField] private GameObject cutsceneUICanvas;
    
    [Header("Audio")]
    [SerializeField] private AudioSource cutsceneAudioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isCutscenePlaying = false;
    private CutsceneType currentCutsceneType = CutsceneType.None;
    
    public enum CutsceneType
    {
        None,
        Win,
        Lose
    }
    
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
        // Validate video player
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("[CutsceneManager] VideoPlayer component not found! Add VideoPlayer component.");
            }
        }
        
        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane; // Or RenderTexture
            
            // Subscribe to end event
            videoPlayer.loopPointReached += OnCutsceneEnd;
        }
        
        // Hide cutscene UI initially
        if (cutsceneUICanvas != null)
        {
            cutsceneUICanvas.SetActive(false);
        }
    }
    
    /// <summary>
    /// Play win cutscene
    /// </summary>
    public void PlayWinCutscene()
    {
        if (isCutscenePlaying)
        {
            Debug.LogWarning("[CutsceneManager] Cutscene already playing!");
            return;
        }
        
        if (winCutscene == null)
        {
            Debug.LogError("[CutsceneManager] Win cutscene not assigned!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[CutsceneManager] Playing WIN cutscene...");
        }
        
        currentCutsceneType = CutsceneType.Win;
        StartCoroutine(PlayCutsceneSequence(winCutscene));
    }
    
    /// <summary>
    /// Play lose cutscene
    /// </summary>
    public void PlayLoseCutscene()
    {
        if (isCutscenePlaying)
        {
            Debug.LogWarning("[CutsceneManager] Cutscene already playing!");
            return;
        }
        
        if (loseCutscene == null)
        {
            Debug.LogError("[CutsceneManager] Lose cutscene not assigned!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[CutsceneManager] Playing LOSE cutscene...");
        }
        
        currentCutsceneType = CutsceneType.Lose;
        StartCoroutine(PlayCutsceneSequence(loseCutscene));
    }
    
    /// <summary>
    /// Cutscene playback sequence: Fade out → Play video → Fade in → Load scene
    /// </summary>
    IEnumerator PlayCutsceneSequence(VideoClip cutscene)
    {
        isCutscenePlaying = true;
        
        // Pause game
        Time.timeScale = 0f;
        
        // 1. Fade to black
        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.FadeOut(fadeOutDuration);
            yield return new WaitForSecondsRealtime(fadeOutDuration);
        }
        
        // 2. Prepare video
        videoPlayer.clip = cutscene;
        videoPlayer.Prepare();
        
        // Wait for video to be ready
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        // 3. Show cutscene UI (optional skip prompt)
        if (cutsceneUICanvas != null)
        {
            cutsceneUICanvas.SetActive(true);
        }
        
        // 4. Play video
        videoPlayer.Play();
        
        if (showDebugLogs)
        {
            Debug.Log($"[CutsceneManager] Cutscene playing... Duration: {videoPlayer.clip.length}s");
        }
        
        // 5. Wait for video to finish (or skip)
        while (videoPlayer.isPlaying)
        {
            // Allow skip with Escape or Space
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                if (showDebugLogs)
                {
                    Debug.Log("[CutsceneManager] Cutscene skipped by player");
                }
                videoPlayer.Stop();
                break;
            }
            
            yield return null;
        }
        
        // 6. Hide cutscene UI
        if (cutsceneUICanvas != null)
        {
            cutsceneUICanvas.SetActive(false);
        }
        
        // 7. Fade out video (optional)
        if (ScreenTransition.Instance != null)
        {
            ScreenTransition.Instance.FadeOut(0.5f);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        // 8. Load scene or restart
        if (autoLoadSceneAfterCutscene)
        {
            LoadSceneAfterCutscene();
        }
        
        isCutscenePlaying = false;
    }
    
    /// <summary>
    /// Called when cutscene video ends
    /// </summary>
    void OnCutsceneEnd(VideoPlayer vp)
    {
        if (showDebugLogs)
        {
            Debug.Log("[CutsceneManager] Cutscene ended");
        }
    }
    
    /// <summary>
    /// Load appropriate scene after cutscene
    /// </summary>
    void LoadSceneAfterCutscene()
    {
        Time.timeScale = 1f; // Resume time
        
        string sceneToLoad = "";
        
        if (currentCutsceneType == CutsceneType.Win)
        {
            sceneToLoad = string.IsNullOrEmpty(winSceneName) ? SceneManager.GetActiveScene().name : winSceneName;
        }
        else if (currentCutsceneType == CutsceneType.Lose)
        {
            sceneToLoad = string.IsNullOrEmpty(loseSceneName) ? SceneManager.GetActiveScene().name : loseSceneName;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[CutsceneManager] Loading scene: {sceneToLoad}");
        }
        
        SceneManager.LoadScene(sceneToLoad);
    }
    
    /// <summary>
    /// Stop cutscene playback
    /// </summary>
    public void StopCutscene()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        StopAllCoroutines();
        isCutscenePlaying = false;
        Time.timeScale = 1f;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnCutsceneEnd;
        }
        
        // Reset time scale
        Time.timeScale = 1f;
    }
    
    public bool IsCutscenePlaying => isCutscenePlaying;
}
