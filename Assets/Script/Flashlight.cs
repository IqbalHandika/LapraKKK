using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flashlight system with battery management
/// Attach to Player GameObject
/// </summary>
public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("Spotlight component (attach to child object)")]
    [SerializeField] private Light flashlightLight;
    
    [Tooltip("Transform to sync flashlight rotation with (usually Main Camera)")]
    [SerializeField] private Transform cameraTransform;
    
    [Tooltip("Toggle flashlight with this key")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    
    [Tooltip("Is flashlight on by default?")]
    [SerializeField] private bool startOn = false;
    
    [Tooltip("Sync flashlight direction with camera (follows mouse look)")]
    [SerializeField] private bool syncWithCamera = true;
    
    [Header("Battery Settings")]
    [Tooltip("Maximum battery capacity (seconds)")]
    [SerializeField] private float maxBattery = 300f; // 5 minutes
    
    [Tooltip("Battery drain rate per second when ON")]
    [SerializeField] private float drainRate = 1f;
    
    [Tooltip("Battery recharge rate per second when OFF")]
    [SerializeField] private float rechargeRate = 0.5f;
    
    [Tooltip("Minimum battery to turn on (prevent spam at 0%)")]
    [SerializeField] private float minBatteryToTurnOn = 5f;
    
    [Tooltip("Does battery recharge when OFF?")]
    [SerializeField] private bool batteryRecharges = true;
    
    [Header("UI References")]
    [Tooltip("Battery bar fill image (assign Image component)")]
    [SerializeField] private Image batteryBarFill;
    
    [Tooltip("Battery percentage text (optional)")]
    [SerializeField] private Text batteryText;
    
    [Tooltip("Low battery warning icon (optional)")]
    [SerializeField] private GameObject lowBatteryWarning;
    
    [Tooltip("Low battery threshold (%)")]
    [SerializeField] private float lowBatteryThreshold = 20f;
    
    [Header("Visual Effects")]
    [Tooltip("Flicker light when battery is low")]
    [SerializeField] private bool flickerOnLowBattery = true;
    
    [Tooltip("Battery % when flickering starts")]
    [SerializeField] private float flickerThreshold = 10f;
    
    [Tooltip("Flicker frequency")]
    [SerializeField] private float flickerSpeed = 10f;
    
    [Header("Audio")]
    [Tooltip("Sound when turning flashlight ON")]
    [SerializeField] private AudioClip turnOnSound;
    
    [Tooltip("Sound when turning flashlight OFF")]
    [SerializeField] private AudioClip turnOffSound;
    
    [Tooltip("Sound when battery is empty")]
    [SerializeField] private AudioClip batteryEmptySound;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    // State
    private bool isOn = false;
    private float currentBattery;
    private AudioSource audioSource;
    private float originalLightIntensity;
    
    void Start()
    {
        // Initialize battery
        currentBattery = maxBattery;
        
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (cameraTransform == null)
        {
            Debug.LogWarning("[Flashlight] Camera not found! Flashlight won't sync with camera rotation.");
            syncWithCamera = false;
        }
        
        // Validate flashlight
        if (flashlightLight == null)
        {
            Debug.LogError("[Flashlight] Spotlight not assigned! Create a child GameObject with Spotlight component.");
            enabled = false;
            return;
        }
        
        // Store original intensity
        originalLightIntensity = flashlightLight.intensity;
        
        // Set initial state
        isOn = startOn;
        flashlightLight.enabled = isOn;
        
        // Update UI
        UpdateUI();
    }
    
    void Update()
    {
        // Sync flashlight rotation with camera (follows mouse look)
        // Only sync Y rotation (horizontal) to avoid head bob shake
        if (syncWithCamera && cameraTransform != null && flashlightLight != null)
        {
            // Get camera's world rotation
            Vector3 cameraEuler = cameraTransform.eulerAngles;
            
            // Get current flashlight euler
            Vector3 flashlightEuler = flashlightLight.transform.eulerAngles;
            
            // Only update X and Y (pitch and yaw), keep Z to avoid roll shake
            flashlightEuler.x = cameraEuler.x;
            flashlightEuler.y = cameraEuler.y;
            // Z stays the same (no roll from camera shake)
            
            flashlightLight.transform.eulerAngles = flashlightEuler;
        }
        
        // Toggle flashlight
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
        
        // Update battery
        if (isOn)
        {
            // Drain battery
            currentBattery -= drainRate * Time.deltaTime;
            
            if (currentBattery <= 0f)
            {
                // Battery depleted
                currentBattery = 0f;
                TurnOff(true); // Force off, play empty sound
            }
            
            // Flicker effect when low battery
            if (flickerOnLowBattery && GetBatteryPercent() <= flickerThreshold)
            {
                ApplyFlicker();
            }
        }
        else if (batteryRecharges)
        {
            // Recharge battery when OFF
            currentBattery += rechargeRate * Time.deltaTime;
            currentBattery = Mathf.Min(currentBattery, maxBattery);
        }
        
        // Update UI
        UpdateUI();
    }
    
    /// <summary>
    /// Toggle flashlight on/off
    /// </summary>
    void ToggleFlashlight()
    {
        if (isOn)
        {
            TurnOff(false);
        }
        else
        {
            TurnOn();
        }
    }
    
    /// <summary>
    /// Turn flashlight ON
    /// </summary>
    void TurnOn()
    {
        // Check if enough battery
        if (currentBattery < minBatteryToTurnOn)
        {
            if (showDebugLogs)
            {
                Debug.Log("[Flashlight] Not enough battery to turn on!");
            }
            
            // Play empty sound
            if (audioSource != null && batteryEmptySound != null)
            {
                audioSource.PlayOneShot(batteryEmptySound);
            }
            
            return;
        }
        
        isOn = true;
        flashlightLight.enabled = true;
        
        // Play sound
        if (audioSource != null && turnOnSound != null)
        {
            audioSource.PlayOneShot(turnOnSound);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[Flashlight] Turned ON");
        }
    }
    
    /// <summary>
    /// Turn flashlight OFF
    /// </summary>
    void TurnOff(bool batteryEmpty)
    {
        isOn = false;
        flashlightLight.enabled = false;
        flashlightLight.intensity = originalLightIntensity; // Reset flicker
        
        // Play sound
        if (audioSource != null)
        {
            if (batteryEmpty && batteryEmptySound != null)
            {
                audioSource.PlayOneShot(batteryEmptySound);
            }
            else if (turnOffSound != null)
            {
                audioSource.PlayOneShot(turnOffSound);
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[Flashlight] Turned OFF {(batteryEmpty ? "(Battery Empty)" : "")}");
        }
    }
    
    /// <summary>
    /// Apply flicker effect when battery is low
    /// </summary>
    void ApplyFlicker()
    {
        float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        flashlightLight.intensity = originalLightIntensity * Mathf.Lerp(0.3f, 1f, flicker);
    }
    
    /// <summary>
    /// Update UI elements
    /// </summary>
    void UpdateUI()
    {
        float batteryPercent = GetBatteryPercent();
        
        // Update battery bar
        if (batteryBarFill != null)
        {
            batteryBarFill.fillAmount = batteryPercent / 100f;
            
            // Change color based on battery level
            if (batteryPercent <= 10f)
            {
                batteryBarFill.color = Color.red;
            }
            else if (batteryPercent <= 30f)
            {
                batteryBarFill.color = Color.yellow;
            }
            else
            {
                batteryBarFill.color = Color.green;
            }
        }
        
        // Update battery text
        if (batteryText != null)
        {
            batteryText.text = $"{batteryPercent:F0}%";
        }
        
        // Show/hide low battery warning
        if (lowBatteryWarning != null)
        {
            lowBatteryWarning.SetActive(batteryPercent <= lowBatteryThreshold);
        }
    }
    
    /// <summary>
    /// Get battery percentage
    /// </summary>
    public float GetBatteryPercent()
    {
        return (currentBattery / maxBattery) * 100f;
    }
    
    /// <summary>
    /// Add battery (for pickup items)
    /// </summary>
    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, maxBattery);
        
        if (showDebugLogs)
        {
            Debug.Log($"[Flashlight] Added {amount}s battery. Current: {GetBatteryPercent():F1}%");
        }
    }
    
    /// <summary>
    /// Check if flashlight is currently on
    /// </summary>
    public bool IsOn()
    {
        return isOn;
    }
}
