using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Button hover effect - Scale, color, and sound on hover/click
/// Attach to Button GameObject
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private bool enableScaleEffect = true;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressedScale = 0.95f;
    [SerializeField] private float scaleSpeed = 10f;
    
    [Header("Color Animation")]
    [SerializeField] private bool enableColorEffect = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.7f, 1f); // Slightly yellow
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float volume = 1f;
    
    [Header("References (Auto-assigned)")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Color targetColor;
    private AudioSource audioSource;
    private bool isHovering = false;
    private bool isPressed = false;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
        
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hoverSound != null || clickSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Store original values
        originalScale = transform.localScale;
        targetScale = originalScale;
        targetColor = normalColor;
        
        // Set initial color
        if (enableColorEffect && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
    
    void Update()
    {
        // Smooth scale animation
        if (enableScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
        }
        
        // Smooth color animation
        if (enableColorEffect && buttonImage != null)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.unscaledDeltaTime * scaleSpeed);
        }
    }
    
    /// <summary>
    /// Called when pointer enters button
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        
        // Scale effect
        if (enableScaleEffect)
        {
            targetScale = originalScale * hoverScale;
        }
        
        // Color effect
        if (enableColorEffect)
        {
            targetColor = hoverColor;
        }
        
        // Play hover sound
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, volume);
        }
    }
    
    /// <summary>
    /// Called when pointer exits button
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        // Return to normal if not pressed
        if (!isPressed)
        {
            targetScale = originalScale;
            targetColor = normalColor;
        }
    }
    
    /// <summary>
    /// Called when pointer clicks button (mouse down)
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        
        // Scale effect
        if (enableScaleEffect)
        {
            targetScale = originalScale * pressedScale;
        }
        
        // Color effect
        if (enableColorEffect)
        {
            targetColor = pressedColor;
        }
        
        // Play click sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
    
    /// <summary>
    /// Called when pointer releases button (mouse up)
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        
        // Return to hover state if still hovering, otherwise normal
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
            targetColor = hoverColor;
        }
        else
        {
            targetScale = originalScale;
            targetColor = normalColor;
        }
    }
}
