using System.Collections;
using UnityEngine;

public class DoubleDoor : MonoBehaviour, IInteractable
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    
    [Header("Door Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool canClose = true;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    
    private Quaternion leftClosedRotation;
    private Quaternion leftOpenRotation;
    private Quaternion rightClosedRotation;
    private Quaternion rightOpenRotation;
    
    private AudioSource audioSource;
    private bool isAnimating = false;

    void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("DoubleDoor: Left Door or Right Door reference is missing!");
            return;
        }
        
        leftClosedRotation = leftDoor.rotation;
        leftOpenRotation = Quaternion.Euler(leftDoor.eulerAngles.x, leftDoor.eulerAngles.y - openAngle, leftDoor.eulerAngles.z);
        
        rightClosedRotation = rightDoor.rotation;
        rightOpenRotation = Quaternion.Euler(rightDoor.eulerAngles.x, rightDoor.eulerAngles.y + openAngle, rightDoor.eulerAngles.z);
        
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
        StartCoroutine(ToggleDoubleDoor());
    }

    public string GetInteractPrompt()
    {
        if (isAnimating) return "";
        
        if (isOpen && !canClose) return "";
        
        return isOpen ? "E - close the door" : "E - open the door";
    }

    private IEnumerator ToggleDoubleDoor()
    {
        isAnimating = true;
        
        Quaternion leftTarget = isOpen ? leftOpenRotation : leftClosedRotation;
        Quaternion rightTarget = isOpen ? rightOpenRotation : rightClosedRotation;
        
        if (audioSource != null)
        {
            AudioClip clipToPlay = isOpen ? openSound : closeSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
        
        while (Quaternion.Angle(leftDoor.rotation, leftTarget) > 0.1f || 
               Quaternion.Angle(rightDoor.rotation, rightTarget) > 0.1f)
        {
            leftDoor.rotation = Quaternion.Slerp(leftDoor.rotation, leftTarget, Time.deltaTime * openSpeed);
            rightDoor.rotation = Quaternion.Slerp(rightDoor.rotation, rightTarget, Time.deltaTime * openSpeed);
            yield return null;
        }
        
        leftDoor.rotation = leftTarget;
        rightDoor.rotation = rightTarget;
        isAnimating = false;
    }
}