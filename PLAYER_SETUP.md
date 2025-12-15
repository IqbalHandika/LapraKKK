# 🎮 Player Setup Guide - LapraKKK Horror Game

Complete setup guide for player character with human model, animations, and flashlight system.

---

## 📋 Table of Contents
1. [Player Model Setup](#1-player-model-setup)
2. [Animation Controller](#2-animation-controller)
3. [Flashlight System](#3-flashlight-system)
4. [Lighting Environment](#4-lighting-environment)
5. [Component Migration](#5-component-migration)

---

## 1. Player Model Setup

### Step 1: Import Human Model
1. Import your human character model to `Assets/Models/Player/`
2. Select the model in Project window
3. In Inspector → **Rig** tab:
   - Animation Type: **Humanoid**
   - Avatar Definition: **Create From This Model**
   - Click **Apply**

### Step 2: Create Player GameObject
1. Create new GameObject: `Player`
2. Add Components:
   - **Character Controller** (adjust size to match model)
     - Height: `1.8` (adjust to model)
     - Radius: `0.3`
     - Center: `(0, 0.9, 0)`
   - **SimpleMovement** script (from old player)
   - **PlayerHealth** script
   - **PlayerInteraction** script
   - **Inventory** script
   - **FootstepAudio** script
   - **Flashlight** script (NEW)

3. Drag your human model as **child** of Player GameObject
   - Position: `(0, 0, 0)` (at player's feet)
   - Scale: Adjust if needed

### Step 3: Setup Camera
1. Create child GameObject under Player: `Main Camera`
2. Position camera at head height:
   - Position: `(0, 1.6, 0)` (adjust to model's eye level)
3. Add **CameraHeadBob** script to camera
4. In Player's `SimpleMovement` script:
   - Assign `Main Camera` to **playerCamera** field

---

## 2. Animation Controller

### Step 1: Create Animator Controller
1. Right-click in Project → **Create → Animator Controller**
2. Name it: `PlayerAnimator`
3. Save to: `Assets/Animations/Player/`

### Step 2: Setup Animation States
1. Double-click `PlayerAnimator` to open Animator window
2. Add your 3 animations:
   - Right-click in graph → **Create State → Empty**
   - Rename to: `Idle`
   - In Inspector, assign `Idle` animation clip to **Motion** field
   - Repeat for `Walking` and `Running`

3. Set **Idle** as default (right-click → **Set as Layer Default State**)

### Step 3: Create Parameters
In Animator window → **Parameters** tab, add:
- **Float**: `Speed` (default: 0)

### Step 4: Create Transitions

**Idle → Walking:**
- Click Idle state → Right-click → **Make Transition** → Click Walking
- Select transition arrow → Inspector:
  - **Conditions**: Add `Speed` Greater `0.1`
  - Has Exit Time: ☐ (unchecked)
  - Transition Duration: `0.1`

**Walking → Idle:**
- Walking → Idle transition
- **Conditions**: `Speed` Less `0.1`
- Has Exit Time: ☐

**Walking → Running:**
- Walking → Running transition
- **Conditions**: `Speed` Greater `7`
- Has Exit Time: ☐
- Transition Duration: `0.2`

**Running → Walking:**
- Running → Walking transition
- **Conditions**: `Speed` Less `7`
- Has Exit Time: ☐
- Transition Duration: `0.2`

### Step 5: Attach to Model
1. Select your **human model** (child of Player)
2. Add **Animator** component
3. Assign `PlayerAnimator` to **Controller** field

### Step 6: Create Animation Sync Script

Create new script `PlayerAnimator.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Syncs player movement speed with animator parameters
/// Attach to Player GameObject (same object as SimpleMovement)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator component on player model")]
    [SerializeField] private Animator animator;
    
    [Header("Settings")]
    [Tooltip("Smoothing speed for animation transitions")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    
    private CharacterController controller;
    private readonly int speedHash = Animator.StringToHash("Speed");
    private float currentAnimSpeed = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("[PlayerAnimator] Animator not found! Assign manually or add to child model.");
            enabled = false;
        }
    }
    
    void Update()
    {
        // Get horizontal velocity
        Vector3 velocity = controller.velocity;
        float speed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        
        // Smooth transition
        currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, speed, Time.deltaTime / animationSmoothTime);
        
        // Update animator
        animator.SetFloat(speedHash, currentAnimSpeed);
    }
}
```

**Attach `PlayerAnimator.cs` to Player GameObject**

---

## 3. Flashlight System

### Step 1: Create Flashlight Light
1. Select **Player** GameObject
2. Right-click → **3D Object → Create Empty**
3. Rename to: `Flashlight`
4. Position at camera level (slightly offset):
   - Position: `(0.3, 1.5, 0.4)` (right side, near face)
   - Rotation: `(0, 0, 0)`

5. Add **Light** component to Flashlight:
   - Type: **Spot**
   - Color: Warm white `(255, 245, 220)` or yellowish
   - Intensity: `2-3`
   - Range: `15-20`
   - Spot Angle: `60-80`
   - Shadows: **Soft Shadows** (if performance allows)

### Step 2: Create Battery UI

**A. Canvas Setup:**
1. Right-click Hierarchy → **UI → Canvas**
2. Canvas settings:
   - Render Mode: **Screen Space - Overlay**
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: `1920 x 1080`

**B. Battery Panel (Bottom-Right):**
1. Right-click Canvas → **UI → Panel**
2. Rename: `BatteryUI_Panel`
3. Anchor preset: **Bottom-Right** (shift+click for position too)
4. Position offset: `(-150, 80)` from bottom-right
5. Size: `(250, 80)`
6. Color: Semi-transparent dark `(0, 0, 0, 180)`

**C. Battery Icon (Optional):**
1. Right-click BatteryUI_Panel → **UI → Image**
2. Rename: `Battery_Icon`
3. Assign battery sprite (or use white square temporarily)
4. Rect Transform:
   - Anchor: Left
   - Position: `(30, 0)`
   - Size: `(50, 50)`

**D. Battery Bar Background:**
1. Right-click BatteryUI_Panel → **UI → Image**
2. Rename: `BatteryBar_Background`
3. Color: Dark gray `(50, 50, 50)`
4. Rect Transform:
   - Anchor: Middle-Right area
   - Position: `(150, 0)`
   - Size: `(180, 25)`

**E. Battery Bar Fill:**
1. Right-click BatteryBar_Background → **UI → Image**
2. Rename: `BatteryBar_Fill`
3. Image component:
   - Color: Green `(0, 255, 0)`
   - Image Type: **Filled**
   - Fill Method: **Horizontal**
   - Fill Origin: **Left**
   - Fill Amount: `1`
4. Rect Transform:
   - Stretch to fill parent (anchor all sides)
   - Offsets: `(0, 0, 0, 0)`

**F. Battery Percentage Text:**
1. Right-click BatteryUI_Panel → **UI → Text**
2. Rename: `Battery_Text`
3. Text settings:
   - Text: `100%`
   - Font Size: `20`
   - Alignment: Center
   - Color: White
4. Position: Center-right area of panel

**G. Low Battery Warning (Optional):**
1. Right-click BatteryUI_Panel → **UI → Image**
2. Rename: `LowBatteryWarning`
3. Assign warning icon (red exclamation mark sprite)
4. Rect Transform: Position near battery
5. **Disable** GameObject by default (script will enable when low)

### Step 3: Connect Flashlight Script
1. Select **Player** GameObject
2. Flashlight script Inspector:
   - **Flashlight Light**: Drag `Flashlight` child object
   - **Toggle Key**: `F`
   - **Battery Bar Fill**: Drag `BatteryBar_Fill` Image
   - **Battery Text**: Drag `Battery_Text`
   - **Low Battery Warning**: Drag warning icon object (optional)
   
3. Adjust settings:
   - Max Battery: `300` (5 minutes)
   - Drain Rate: `1` (1 second per second)
   - Recharge Rate: `0.5` (or 0 for no recharge)
   - Low Battery Threshold: `20%`
   - Flicker Threshold: `10%`

### Step 4: Audio (Optional)
Assign AudioClips in Flashlight script:
- Turn On Sound: Click/switch sound
- Turn Off Sound: Click sound
- Battery Empty Sound: Beep/error sound

---

## 4. Lighting Environment

### Global Darkness Setup

**Option A: Directional Light (Recommended for outdoor scenes)**
1. Select **Directional Light** in scene
2. Settings:
   - Intensity: `0.1 - 0.3` (very dim)
   - Color: Dark blue/purple `(20, 20, 40)` for night
   - Shadows: **Soft Shadows**
   
3. Window → Rendering → **Lighting**
   - Environment:
     - Skybox Material: Dark night skybox (or solid color)
     - Sun Source: Directional Light
     - Environment Lighting: `(10, 10, 15)` (very dark ambient)
     - Environment Reflections: None or dark cubemap
   - Realtime Lighting:
     - Realtime Global Illumination: ☑ (if using baked lights)
   - Baked GI:
     - ☐ Uncheck if not using lightmaps

**Option B: No Directional Light (Total darkness)**
1. Delete or disable Directional Light
2. Set ambient lighting to black:
   - Window → Rendering → Lighting
   - Environment Lighting → Color: `(0, 0, 0)`

**Option C: Fog for Atmosphere**
1. Window → Rendering → Lighting → **Other Settings**
2. Fog:
   - ☑ Enable Fog
   - Color: Dark gray/blue
   - Mode: Exponential Squared
   - Density: `0.01 - 0.05` (adjust for visibility)

### Indoor Lights Setup
For each room light:
1. Create **Point Light** or **Spot Light**
2. Settings:
   - Type: **Point** (omnidirectional) or **Spot** (focused)
   - Color: Warm yellow `(255, 200, 100)` for incandescent
   - Intensity: `1-2`
   - Range: `5-10`
   - Shadows: **Soft** (if needed, expensive)

**Light Prefab (Recommended):**
1. Create prefab structure:
   ```
   RoomLight (Empty GameObject)
   └── PointLight (Light component)
   └── LightBulb (Mesh - visual bulb model)
   ```
2. Save as prefab: `Assets/Prefabs/RoomLight.prefab`
3. Duplicate and place throughout level

---

## 5. Component Migration

### From Old Cylinder Player to New Human Model

**Step-by-Step:**

1. **Create New Player:**
   - Follow Section 1 to create new Player with human model

2. **Copy Component Values:**
   - Open both players side-by-side in Inspector
   - In **SimpleMovement**:
     - Copy `kecepatanJalan`, `kecepatanLari`, `mouseSensitivity` values
   - In **PlayerHealth**:
     - Copy all death settings, camera zoom values
   - In **Inventory**:
     - No need to copy (will be empty anyway)
   - In **PlayerInteraction**:
     - Copy `interactionRange`, UI Text reference

3. **Update References:**
   - Find all scene objects referencing old player:
     - GameManager → Player Inventory
     - DosenAI → Player transform
     - CutsceneManager → Player references
   - Replace with new Player GameObject

4. **Camera Setup:**
   - Delete old Main Camera if still in scene
   - New camera is child of new Player
   - Ensure `MainCamera` tag is set

5. **Test Movement:**
   - Play scene
   - Test: WASD movement, mouse look, sprint (Shift), crouch if implemented
   - Check head bob, footsteps

6. **Delete Old Player:**
   - Once everything works, delete old cylinder player GameObject

---

## ✅ Verification Checklist

After setup, verify:

- [ ] Player model visible and scaled correctly
- [ ] Animations play (Idle → Walk → Run transitions)
- [ ] Movement controls work (WASD, mouse, sprint)
- [ ] Camera positioned at eye level
- [ ] Head bob feels natural
- [ ] Footstep sounds play and sync with steps
- [ ] Flashlight toggles with F key
- [ ] Battery UI visible in bottom-right
- [ ] Battery drains when flashlight on
- [ ] Battery recharges when flashlight off (if enabled)
- [ ] Low battery warning shows at 20%
- [ ] Flashlight flickers at 10% battery
- [ ] Environment is dark (only flashlight illuminates)
- [ ] Room lights work if placed
- [ ] Inventory system still works
- [ ] DosenAI still chases player
- [ ] Death system works with new player

---

## 🎨 Recommended Values Summary

**SimpleMovement:**
- Walk Speed: `5`
- Sprint Speed: `8`
- Mouse Sensitivity: `100`

**CameraHeadBob:**
- Walk Bob Speed: `10`
- Walk Bob Amount: `0.15`
- Run Bob Speed: `14`
- Run Bob Amount: `0.25`
- Sprint FOV: `70`
- Normal FOV: `60`

**Flashlight:**
- Max Battery: `300s` (5 min)
- Drain Rate: `1.0`
- Recharge Rate: `0.5`
- Light Intensity: `2-3`
- Light Range: `15-20`
- Spot Angle: `60-80°`

**Environment Lighting:**
- Directional Light Intensity: `0.1-0.3`
- Ambient Color: `(10, 10, 15)` RGB
- Fog Density: `0.02` (optional)

---

## 🐛 Troubleshooting

**Animations not playing:**
- Check Animator component is on model child (not Player root)
- Verify animation clips are assigned to states
- Check parameter name is exactly `Speed` (case-sensitive)
- Ensure PlayerAnimator script is on Player root

**Flashlight not visible:**
- Check Light component is enabled
- Verify intensity > 0
- Check culling mask includes your objects
- Position light in front of player

**Battery UI not updating:**
- Verify Image component's Image Type is **Filled**
- Check references in Flashlight script are assigned
- Ensure Canvas is set to Screen Space Overlay

**Head bob not working:**
- CameraHeadBob must be on Camera (child), not Player (root)
- Check Player Transform field is assigned to Player root
- Verify minSpeedThreshold < walk speed

**Player falls through floor:**
- Check Character Controller collider size
- Ensure floor has Collider component
- Verify player is above ground at start

---

## 📝 Notes

- **Performance:** Soft shadows are expensive. Use hard shadows or no shadows for better FPS.
- **Battery Recharge:** Disable `batteryRecharges` for hardcore mode.
- **Flashlight Positioning:** Adjust offset to match model's hand if holding flashlight.
- **Animation Speed:** If walk/run animations play too fast/slow, adjust animation clip speed in Import Settings.
- **Lighting Baking:** For better performance in large scenes, consider baking lightmaps (Window → Rendering → Lighting → Generate Lighting).

---

**Setup complete!** 🎉 You now have a fully functional first-person horror game player with animations, flashlight, and dark atmosphere.
