using UnityEngine;

public class DoorTriggerZone : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private MonoBehaviour doorScript;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool triggerForPlayer = true;
    [SerializeField] private bool triggerForEnemy = true;
    [SerializeField] private float autoCloseDelay = 3f;
    
    private IInteractable door;
    private bool isDoorOpen = false;
    private float closeTimer = 0f;
    
    void Start()
    {
        // Get door interface
        if (doorScript != null)
        {
            door = doorScript as IInteractable;
        }
        
        if (door == null)
        {
            Debug.LogError("DoorTriggerZone: Door script must implement IInteractable!");
        }
        
        // Ensure trigger is enabled
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        // Auto-close door after delay
        if (isDoorOpen && autoCloseDelay > 0f)
        {
            closeTimer += Time.deltaTime;
            
            if (closeTimer >= autoCloseDelay)
            {
                // Check if door is open before closing
                Door doorComponent = doorScript as Door;
                if (doorComponent != null && doorComponent.IsOpen())
                {
                    door.Interact(); // Close door
                    isDoorOpen = false;
                }
                
                DoubleDoor doubleDoor = doorScript as DoubleDoor;
                if (doubleDoor != null && doubleDoor.IsOpen())
                {
                    door.Interact(); // Close door
                    isDoorOpen = false;
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        bool shouldOpen = false;
        
        if (triggerForPlayer && other.CompareTag("Player"))
        {
            shouldOpen = true;
        }
        
        if (triggerForEnemy && (other.CompareTag("Enemy") || other.GetComponent<DosenAI>() != null))
        {
            shouldOpen = true;
        }
        
        if (shouldOpen && door != null)
        {
            // Check if door is closed
            Door doorComponent = doorScript as Door;
            if (doorComponent != null && !doorComponent.IsOpen())
            {
                door.Interact(); // Open door
                isDoorOpen = true;
                closeTimer = 0f;
            }
            
            DoubleDoor doubleDoor = doorScript as DoubleDoor;
            if (doubleDoor != null && !doubleDoor.IsOpen())
            {
                door.Interact(); // Open door
                isDoorOpen = true;
                closeTimer = 0f;
            }
        }
    }
}   