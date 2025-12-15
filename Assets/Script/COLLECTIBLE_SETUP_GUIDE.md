# Collectible Item System - Setup Guide

Complete guide for implementing the item collection system in your horror game.

## Quick Start (5 Minutes)

### 1. Player Setup
```
1. Select your Player GameObject
2. Add Component → Inventory.cs
3. Configure:
   - Max Capacity: 0 (unlimited) or set limit (e.g., 20)
   - Show Debug Logs: ✓ (for testing)
```

### 2. Create Collectible Prefab

**Step-by-step:**
```
1. Create → 3D Object → Cube (or import your item model)
2. Name it "Collectible_Paper_Bab1"
3. Add Component → Collectible.cs
4. Configure Collectible.cs:
   - Item Type: Paper
   - Item Name: "Bab 1"
   - Item ID: "paper_bab_1"
   - Auto Rotate: ✓
   - Rotation Speed: 50
   
5. Add Box Collider:
   - Is Trigger: ✓ (IMPORTANT!)
   - Size: Adjust to match your model
   
6. Optional: Add Mesh Renderer/Material for visuals

7. Drag to Project → Create Prefab
```

### 3. UI Setup

**Create Canvas:**
```
1. Right-click Hierarchy → UI → Canvas
2. Canvas Scaler → Scale With Screen Size
3. Reference Resolution: 1920x1080
```

**Item Counter (Top-Right):**
```
1. Right-click Canvas → UI → TextMeshPro - Text
2. Name: "ItemCounterText"
3. Position: Anchor to top-right
   - Rect Transform:
     - Anchor Presets: Top-Right
     - Pos X: -100, Pos Y: -50
     - Width: 200, Height: 50
4. Text Settings:
   - Font Size: 24
   - Alignment: Right, Middle
   - Color: White
   - Text: "Items: 0/10"
```

**Pickup Notification (Center):**
```
1. Right-click Canvas → UI → TextMeshPro - Text
2. Name: "PickupNotificationText"
3. Position: Center-bottom
   - Anchor Presets: Bottom-Center
   - Pos Y: 100
   - Width: 400, Height: 60
4. Text Settings:
   - Font Size: 28
   - Alignment: Center, Middle
   - Color: Yellow (#FFFF00)
   - Initially: Disabled (✗)
```

**Add InventoryUI Script:**
```
1. Select Canvas
2. Add Component → InventoryUI.cs
3. Assign:
   - Item Count Text: ItemCounterText
   - Total Items In Level: 10 (or count manually)
   - Pickup Notification Text: PickupNotificationText
   - Notification Duration: 2
```

### 4. Place Collectibles in Scene

```
1. Drag prefab from Project into Scene
2. Position where you want (e.g., on desk, floor, shelf)
3. Duplicate (Ctrl+D) and place around level
4. Rename each instance: "Collectible_Key_Office", etc.
5. Update Item ID for each: "key_office_01", "key_office_02"
```

**IMPORTANT:** Each collectible must have unique `itemID`!

## System Architecture

### Files Created
```
Assets/Script/
├── Collectible.cs        (Item behavior)
├── Inventory.cs          (Player inventory management)
└── InventoryUI.cs        (UI display)
```

### How It Works

**Collection Flow:**
```
1. Player walks into collectible trigger
2. Collectible.OnTriggerEnter() detects "Player" tag
3. Calls inventory.AddItem(this)
4. Inventory checks:
   - Not full?
   - Not duplicate ID?
5. If valid → Adds to list
6. Triggers OnItemCollected event
7. InventoryUI receives event → Updates UI
8. Collectible destroys itself
9. Optional: Plays pickup sound
```

**Inventory Check Flow:**
```
// Example: Door checks if player has key
Inventory playerInv = player.GetComponent<Inventory>();
if (playerInv.HasItem("key_lab_door"))
{
    // Open door
}
```

## Script Reference

### Collectible.cs

**Public Methods:**
- `ItemType Type` - Get item type enum
- `string ItemName` - Get display name
- `string ItemID` - Get unique ID

**Inspector Fields:**
- `Item Type` - Enum: Document/Key/Tool/Evidence/Health/Battery/Misc
- `Item Name` - Display name (shown in UI)
- `Item ID` - MUST BE UNIQUE per item
- `Pickup Sound` - AudioClip (optional)
- `Auto Rotate` - Slow Y-axis spin
- `Rotation Speed` - Degrees/second

### Inventory.cs

**Public Methods:**
```csharp
bool AddItem(Collectible item)                    // Add to inventory
bool HasItem(string itemID)                       // Check if item exists
bool HasItemOfType(Collectible.ItemType type)     // Check by type
Collectible GetItem(string itemID)                // Get specific item
List<Collectible> GetItemsOfType(ItemType type)   // Get all of type
bool RemoveItem(string itemID)                    // Remove item
int GetItemCount()                                // Total items
List<Collectible> GetAllItems()                   // Get all items
void ClearInventory()                             // Remove all
```

**Events:**
```csharp
OnItemCollected.AddListener((Collectible item) => {
    Debug.Log($"Collected: {item.ItemName}");
});
```

**Inspector Fields:**
- `Max Capacity` - 0 = unlimited, or set limit (e.g., 20)
- `Show Debug Logs` - Enable console logs

### InventoryUI.cs

**Public Methods:**
```csharp
void SetTotalItems(int total)  // Update total item count
```

**Inspector Fields:**
- `Item Count Text` - TextMeshProUGUI reference
- `Total Items In Level` - Max collectibles (for X/Y display)
- `Text Format` - "{0}/{1}" format string
- `Pickup Notification Text` - Temporary message
- `Notification Duration` - Seconds to show notification

## Advanced Usage

### 1. Locked Door Example

```csharp
// DoorLock.cs
public class DoorLock : MonoBehaviour
{
    [SerializeField] private string requiredKeyID = "key_lab_01";
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inv = other.GetComponent<Inventory>();
            
            if (inv.HasItem(requiredKeyID))
            {
                // Open door
                Debug.Log("Door unlocked!");
                // Optional: Remove key from inventory
                inv.RemoveItem(requiredKeyID);
            }
            else
            {
                Debug.Log("Door locked! Need key.");
            }
        }
    }
}
```

### 2. Count Specific Item Type

```csharp
// Check how many papers collected
Inventory inv = player.GetComponent<Inventory>();
List<Collectible> papers = inv.GetItemsOfType(Collectible.ItemType.Paper);
Debug.Log($"Collected {papers.Count}/5 papers");
```

### 3. Game Completion Check (Collect All 5 Papers)

```csharp
// Collect all 5 papers (Bab 1-5) to complete the game
Inventory inv = player.GetComponent<Inventory>();

if (inv.GetItemsOfType(Collectible.ItemType.Paper).Count >= 5)
{
    // Check if player has all specific papers
    bool hasAllPapers = inv.HasItem("paper_bab_1") &&
                        inv.HasItem("paper_bab_2") &&
                        inv.HasItem("paper_bab_3") &&
                        inv.HasItem("paper_bab_4") &&
                        inv.HasItem("paper_bab_5");
    
    if (hasAllPapers)
    {
        // Trigger victory/ending
        Debug.Log("All papers collected! Game completed!");
        // Load ending scene or show victory screen
    }
}
```

### 4. Custom Pickup Sound per Item

```
1. Import audio file (e.g., key_pickup.wav)
2. Select Collectible prefab
3. Assign audio to "Pickup Sound" field
4. Plays automatically on collection via AudioSource.PlayClipAtPoint()
```

### 5. Dynamic Total Items Count

```csharp
// In GameManager.cs Start()
void Start()
{
    // Count all collectibles in scene
    int total = FindObjectsOfType<Collectible>().Length;
    
    // Update UI
    InventoryUI ui = FindObjectOfType<InventoryUI>();
    ui.SetTotalItems(total);
}
```

## Gizmos Visualization

**In Scene View:**
- **Yellow Wire Sphere** - Collectible location
- **Colored Sphere** - Item type indicator:
  - White = Paper (Bab 1-5)
  - Cyan = Key
- **Selected**: Shows trigger radius (yellow transparent sphere)

## Common Issues & Solutions

### Issue: "Player doesn't have Inventory component!"
**Solution:** Add `Inventory.cs` to Player GameObject

### Issue: Items not collecting
**Solution:** 
1. Check Player has "Player" tag
2. Check Collider "Is Trigger" = ✓
3. Check Collectible has unique Item ID

### Issue: UI not updating
**Solution:**
1. Check InventoryUI references are assigned
2. Check Player has Inventory component
3. Check OnItemCollected event is connected (should be automatic)

### Issue: Duplicate item IDs
**Solution:** Each collectible must have unique ID:
- ✅ Good: "key_lab_01", "key_lab_02", "key_office_01"
- ❌ Bad: "key", "key", "key"

### Issue: Sound not playing
**Solution:**
1. Import audio as AudioClip
2. Assign to Collectible "Pickup Sound" field
3. Check audio import settings (3D Sound = ✓)

## Testing Checklist

```
✓ Player has Inventory.cs component
✓ Collectibles have trigger colliders
✓ Each collectible has unique Item ID
✓ Canvas has InventoryUI.cs
✓ UI text references assigned
✓ Player tag is set correctly
✓ Test collection: walk into item → UI updates
✓ Test HasItem(): Check door unlock logic
✓ Test inventory limit (if Max Capacity > 0)
```

## Performance Notes

- ✅ **Efficient**: Uses UnityEvents (no Update loops)
- ✅ **Optimized**: Items destroy after collection (no inactive GameObjects)
- ✅ **Safe**: Null checks prevent errors
- ✅ **Scalable**: Handles 100+ items without lag

## Game-Specific Setup (Bab 1-5 Papers + Keys)

### Create All 5 Paper Collectibles

```
1. Create prefab: "Paper_Bab1"
   - Item Type: Paper
   - Item Name: "Bab 1"
   - Item ID: "paper_bab_1"
   
2. Duplicate and rename for Bab 2-5:
   - Paper_Bab2: ID "paper_bab_2", Name "Bab 2"
   - Paper_Bab3: ID "paper_bab_3", Name "Bab 3"
   - Paper_Bab4: ID "paper_bab_4", Name "Bab 4"
   - Paper_Bab5: ID "paper_bab_5", Name "Bab 5"
```

### Create Key Collectibles

```
1. Create prefab: "Key_Lab"
   - Item Type: Key
   - Item Name: "Kunci Lab"
   - Item ID: "key_lab"
   
2. Create more keys as needed:
   - Key_Office: ID "key_office", Name "Kunci Kantor"
   - Key_Storage: ID "key_storage", Name "Kunci Gudang"
```

### Victory Condition Script

```csharp
// GameManager.cs - Add this to check for game completion
public class GameManager : MonoBehaviour
{
    private Inventory playerInventory;
    
    void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        
        // Subscribe to collection event
        playerInventory.OnItemCollected.AddListener(CheckVictoryCondition);
    }
    
    void CheckVictoryCondition(Collectible item)
    {
        // Only check when paper is collected
        if (item.Type != Collectible.ItemType.Paper) return;
        
        // Check if all 5 papers collected
        List<Collectible> papers = playerInventory.GetItemsOfType(Collectible.ItemType.Paper);
        
        if (papers.Count >= 5)
        {
            Debug.Log("Semua Bab terkumpul! Game selesai!");
            // Load ending scene
            // SceneManager.LoadScene("EndingScene");
        }
        else
        {
            Debug.Log($"Kertas terkumpul: {papers.Count}/5");
        }
    }
}
```

### Update UI Text Format

```
1. Select Canvas → InventoryUI component
2. Change "Text Format" to: "Kertas: {0}/5"
3. Set "Total Items In Level": 5
```

### Placement Recommendations

```
- Bab 1: Easy to find (tutorial area)
- Bab 2-3: Require keys to access rooms
- Bab 4: Hidden in hard-to-reach area
- Bab 5: Guarded by enemy patrol route

- Keys: Place before locked doors they open
```

## Next Steps

1. **Add Item Descriptions:**
   - Add `[TextArea] string description` to Collectible.cs
   - Show paper content when collected

2. **Add Icons:**
   - Paper icon for Bab 1-5
   - Key icon for keys

3. **Save System:**
   - Serialize collected paper IDs
   - Persist between game sessions

4. **Victory Screen:**
   - Show all collected Bab papers
   - Display completion message

Selamat membuat game! 🎮
