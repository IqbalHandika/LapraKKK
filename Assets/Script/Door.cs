using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool canClose = true;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;
    private bool isAnimating = false;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + openAngle, transform.eulerAngles.z);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void Interact()
    {
        if (isAnimating) return;
        
        if (isOpen && !canClose) return;
        
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(ToggleDoor());
    }

    public string GetInteractPrompt()
    {
        if (isAnimating) return "";
        
        if (isOpen && !canClose) return "";
        
        return isOpen ? "E - close the door" : "E - open the door";
    }

    private IEnumerator ToggleDoor()
    {
        isAnimating = true;
        
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        
        if (audioSource != null)
        {
            AudioClip clipToPlay = isOpen ? openSound : closeSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isAnimating = false;
    }
}