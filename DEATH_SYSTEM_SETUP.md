# Player Death System - Setup Guide

## Overview
Complete player death system with scream animation, death camera, and lose condition.

---

## Components Created

### 1. **PlayerHealth.cs**
Manages player death state and triggers lose sequence.

**Features:**
- `isAlive` state tracking
- `Die(Transform killer)` - triggers death sequence
- Camera faces enemy on death (optional)
- Collision/trigger detection with "Enemy" tag
- Audio support (death sound)
- Delays GameManager.TriggerLose() for death animation

### 2. **DosenAI.cs Updates**
Enemy scream animation and kill sequence.

**New Features:**
- Scream animation trigger (`Scream` parameter)
- Scream audio playback
- Coroutine delay before killing player
- Calls `PlayerHealth.Die(transform)` after scream

### 3. **GameManager.cs Updates**
Lose state management and player input disabling.

**New Features:**
- `hasLost` flag (prevents multiple lose triggers)
- `DisablePlayerInput()` - disables movement and interaction
- `HasLost` public property

---

## Setup Instructions

### **Step 1: Player Setup**

1. **Add PlayerHealth component to Player GameObject:**
   - Select Player in Hierarchy
   - Add Component → Scripts → PlayerHealth

2. **Configure PlayerHealth Inspector:**
   ```
   ✓ Is Alive: true
   Movement Controller: [Auto-detected]
   Death Camera: [Optional - assign empty GameObject for death cam]
   Player Camera: [Auto-detected - Main Camera]
   Death Delay: 2.0 (seconds before lose screen)
   Death Sound: [Optional - assign AudioClip]
   Show Debug Logs: false
   ```

3. **Ensure Player has these tags/components:**
   - Tag: `Player`
   - Components: `TPPMovementController`, `CharacterController`
   - Collider: `CapsuleCollider` (NOT trigger)

---

### **Step 2: Enemy Setup (DosenAI)**

1. **Configure DosenAI Inspector:**
   ```
   Kill Sequence:
   - Scream Sound: [Assign scream AudioClip]
   - Scream Animation Duration: 2.0 (match animation length)
   ```

2. **Setup Animator Controller:**
   - Open DosenAI's Animator Controller
   - Add new **Trigger parameter**: `Scream`
   - Create transition: Any State → Scream Animation
     - Condition: Scream (trigger)
     - Has Exit Time: false
     - Transition Duration: 0.1s
   - Scream animation should be ~2 seconds long

3. **Ensure Enemy has:**
   - Tag: `Enemy`
   - Collider: NOT a trigger (for OnCollisionEnter)
   - Rigidbody: Is Kinematic = true (prevents physics push)

---

### **Step 3: Animation Setup**

**Animator Controller Setup:**

```
Parameters:
- Speed (Float) - existing
- Scream (Trigger) - NEW

States:
- Idle
- Walk
- Run
- Scream (NEW)
  - Motion: ScreamAnimation clip
  - Loop: false
  - Duration: ~2 seconds

Transitions:
- Any State → Scream
  - Condition: Scream (trigger)
  - Has Exit Time: false
  - Interruption Source: None
- Scream → Idle (automatic after animation finishes)
  - Has Exit Time: true
  - Exit Time: 0.95
```

**If you don't have scream animation yet:**
- Use placeholder: duplicate "Idle" animation
- Or use existing attack/death animation
- Adjust `Scream Animation Duration` in DosenAI Inspector to match

---

### **Step 4: Death Camera (Optional)**

**For cinematic death camera facing enemy:**

1. Create empty GameObject: `DeathCamera`
2. Add component: `Camera`
3. Configure:
   - Enabled: false (initially)
   - Field of View: 60
   - Near Clipping Plane: 0.3
4. Position: At player's head height
5. Assign to PlayerHealth → Death Camera field

**Automatic Behavior:**
- When player dies, DeathCamera activates
- Camera rotates to look at killer enemy
- Main camera disables

**Alternative (Simple):**
- Leave Death Camera empty
- Main camera will just rotate toward enemy (no camera switch)

---

### **Step 5: Audio Setup**

**DosenAI Scream Sound:**
- Assign scream AudioClip to DosenAI → Kill Sequence → Scream Sound
- Will play via AudioSource component (auto-created if missing)

**PlayerHealth Death Sound (Optional):**
- Assign death gasp/grunt AudioClip to PlayerHealth → Death Sound
- Plays when Die() is called

---

## Testing Checklist

### **Basic Death Test:**
1. ✅ Start game
2. ✅ Let enemy catch player (collision)
3. ✅ Enemy plays scream animation
4. ✅ Scream audio plays
5. ✅ After 2 seconds, player movement disabled
6. ✅ Camera faces enemy (if death camera setup)
7. ✅ GameManager.TriggerLose() called
8. ✅ Lose cutscene plays

### **Edge Cases:**
- ✅ Player dies only once (no duplicate deaths)
- ✅ Lose sequence only triggers once (hasLost flag)
- ✅ Player input disabled after death
- ✅ Cursor unlocks for cutscene

---

## Code Reference

### **PlayerHealth Public Methods:**
```csharp
// Kill player (called by enemy or hazard)
public void Die(Transform killer = null)

// Check if player is alive
public bool IsAlive => isAlive

// Revive player (for debugging/respawn)
public void Revive()
```

### **DosenAI Kill Sequence:**
```csharp
// In OnCollisionEnter with Player:
1. Stop AI movement (agent.isStopped = true)
2. Trigger scream animation (animator.SetTrigger("Scream"))
3. Play scream audio (audioSource.PlayOneShot(screamSound))
4. Wait screamAnimationDuration (2 seconds)
5. Call playerHealth.Die(transform)
```

### **GameManager Lose Sequence:**
```csharp
// Called by PlayerHealth.Die():
1. Set hasLost = true
2. DisablePlayerInput() - disable movement + interaction
3. CutsceneManager.PlayLoseCutscene()
4. Unlock cursor
```

---

## Animation Parameter Names

**DosenAI Animator:**
- `Speed` (Float) - 0 = Idle, 1 = Walk, 2 = Run
- `Scream` (Trigger) - **NEW** - Triggers scream animation

**Example Animator Setup Code (already in DosenAI.cs):**
```csharp
// Animation parameters
private readonly int speedHash = Animator.StringToHash("Speed");
private readonly int screamHash = Animator.StringToHash("Scream");

// Trigger scream
animator.SetTrigger(screamHash);
```

---

## Collision Detection Flow

```
Player collides with Enemy
    ↓
DosenAI.OnCollisionEnter (Enemy side)
    ↓
Trigger scream animation + audio
    ↓
Wait 2 seconds (screamAnimationDuration)
    ↓
PlayerHealth.Die(enemy.transform)
    ↓
- Disable player movement
- Face camera at enemy
- Play death sound
    ↓
Wait 2 seconds (deathDelay)
    ↓
GameManager.TriggerLose()
    ↓
- Set hasLost = true
- Disable player input
- Play lose cutscene
    ↓
Fade to black → Video cutscene → Restart/Menu
```

---

## Troubleshooting

### **Player doesn't die on collision:**
- Check Player tag is set to "Player"
- Check Enemy tag is set to "Enemy"
- Ensure colliders are NOT triggers
- Ensure Player has CharacterController or Rigidbody

### **Scream animation doesn't play:**
- Check Animator has "Scream" trigger parameter
- Check transition from Any State → Scream exists
- Check DosenAI has Animator component assigned

### **Death happens twice:**
- Check `isAlive` flag in PlayerHealth (should become false)
- Check `hasLost` flag in GameManager (should become true)
- Both prevent duplicate triggers

### **Camera doesn't face enemy:**
- Assign Death Camera GameObject to PlayerHealth
- Or check that Player Camera is assigned (Main Camera)

### **Player can still move after death:**
- Check TPPMovementController.enabled becomes false
- Check GameManager.DisablePlayerInput() is called

---

## Optional Enhancements

### **1. Ragdoll on Death:**
```csharp
// In PlayerHealth.Die():
Animator animator = GetComponent<Animator>();
if (animator != null)
{
    animator.enabled = false; // Disable animator
}

// Enable ragdoll physics
Rigidbody[] ragdollBodies = GetComponentsInChildren<Rigidbody>();
foreach (var rb in ragdollBodies)
{
    rb.isKinematic = false;
}
```

### **2. Screen Shake on Death:**
```csharp
// Call CameraShake component:
CameraShake shake = Camera.main.GetComponent<CameraShake>();
if (shake != null)
{
    shake.Shake(0.5f, 0.3f); // duration, intensity
}
```

### **3. Slow Motion Death:**
```csharp
// In PlayerHealth.Die():
Time.timeScale = 0.3f; // Slow motion
StartCoroutine(ResetTimeScale());

IEnumerator ResetTimeScale()
{
    yield return new WaitForSecondsRealtime(2f);
    Time.timeScale = 1f;
}
```

---

## Final Checklist

**Player:**
- ✅ PlayerHealth.cs attached
- ✅ Tag: "Player"
- ✅ TPPMovementController component
- ✅ CapsuleCollider (NOT trigger)

**Enemy (DosenAI):**
- ✅ Tag: "Enemy"  
- ✅ Scream Sound assigned
- ✅ Scream Animation Duration set (2.0)
- ✅ Animator has "Scream" trigger parameter
- ✅ Scream animation in Animator Controller
- ✅ Collider (NOT trigger)

**GameManager:**
- ✅ Singleton instance exists
- ✅ CutsceneManager referenced
- ✅ Lose cutscene video assigned

**Testing:**
- ✅ Collision triggers scream
- ✅ Death sequence works
- ✅ Lose cutscene plays
- ✅ Can restart game

---

## Summary

**System Flow:**
1. Enemy catches player → Scream animation (2s)
2. Player dies → Camera faces enemy → Death delay (2s)  
3. GameManager triggers lose → Cutscene plays
4. Restart/Menu options

**Key Scripts:**
- `PlayerHealth.cs` - Death management
- `DosenAI.cs` - Kill sequence with scream
- `GameManager.cs` - Lose state + player input disable

All systems integrated with existing cutscene/win condition framework! 🎮
