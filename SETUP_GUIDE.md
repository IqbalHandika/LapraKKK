# Setup Guide - Door Detection & Patrol Point Systems

This guide covers the complete setup for the automatic door detection system and patrol point room entry mechanics.

---

## Table of Contents
1. [Door Radius Detection Setup](#door-radius-detection-setup)
2. [Patrol Point Room Entry Setup](#patrol-point-room-entry-setup)
3. [Testing Checklist](#testing-checklist)
4. [Inspector Settings Reference](#inspector-settings-reference)
5. [Troubleshooting](#troubleshooting)

---

## Door Radius Detection Setup

### Overview
Doors automatically detect enemies (tagged "Enemy") within a configurable radius and open/close accordingly. This system works with both NavMesh Agent and A* Pathfinding (AIPath).

### 1. Configure Door Component

#### For Single Doors (Door.cs)
1. Select your door GameObject in the Hierarchy
2. In the Inspector, locate the **Door** component
3. Configure the following settings:

**Door Settings:**
- `Is Open`: Initial state (usually false)
- `Open Angle`: Rotation angle when open (default: 90°)
- `Open Speed`: Animation speed (default: 2)
- `Can Close`: Allow manual closing (true/false)

**Enemy Detection (Auto-Open):**
- `Enable Enemy Detection`: ✅ Check this to enable auto-open
- `Detection Radius`: Distance for enemy detection (see recommended values below)
- `Auto Close Delay`: Seconds before auto-closing after enemy leaves (default: 2s)
- `Action Cooldown`: Prevents rapid toggling (default: 0.5s)

**Debug:**
- `Show Debug Logs`: ✅ Check to see console logs when door opens/closes

#### For Double Doors (DoubleDoor.cs)
Same settings as single door, but requires:
- `Left Door`: Drag the left door Transform
- `Right Door`: Drag the right door Transform

### 2. Test in Scene View (Gizmos Visualization)

**To see the detection radius:**
1. Select your door GameObject
2. Look in Scene View for a **yellow wire sphere** around the door
3. This sphere represents the detection radius
4. Adjust `Detection Radius` value to resize the sphere

**Visual Indicators:**
- 🟡 **Yellow Wire Sphere** = Detection radius (visible when door is selected)
- The radius extends from the door's pivot point (center of GameObject)

### 3. Recommended Radius Values

Choose radius based on door type and hallway width:

| Door Type | Recommended Radius | Use Case |
|-----------|-------------------|----------|
| **Narrow Door** | 2.0 - 2.5m | Small rooms, closets |
| **Standard Door** | 3.0 - 3.5m | Normal rooms, offices |
| **Wide Door** | 4.0 - 5.0m | Large rooms, double doors |
| **Hallway Door** | 3.5 - 4.5m | Corridor intersections |
| **Automatic Sliding Door** | 3.0 - 4.0m | Modern buildings |

**Tips:**
- Larger radius = Door opens earlier (more time for animation)
- Smaller radius = Door opens late (enemy might bump into it)
- Test with enemy speed: Fast enemies need larger radius
- Hallways: Match radius to hallway width

### 4. Troubleshooting Door Detection

#### Enemy Not Detected

**Problem**: Door doesn't open when enemy approaches

**Solutions:**
1. **Check Enemy Tag**
   ```
   - Select enemy GameObject
   - In Inspector, check Tag dropdown (top)
   - Must be set to "Enemy"
   ```

2. **Check Detection Radius**
   - Is the yellow gizmo visible in Scene View?
   - Is the enemy actually entering the sphere?
   - Increase radius if needed

3. **Check Component Setup**
   - Door component attached? ✅
   - `Enable Enemy Detection` checked? ✅
   - Enemy has a Collider component? ✅

4. **Enable Debug Logs**
   - Check `Show Debug Logs` on Door component
   - Play the game
   - Watch Console for: `[Door] 'DoorName' detected enemy 'EnemyName' - Opening door`
   - If no log appears, enemy is not entering radius

#### Door Not Opening (Enemy Detected)

**Problem**: Console shows detection but door doesn't open

**Solutions:**
1. **Check Door State**
   - Is door already open? (`Is Open` = true)
   - Is door animating? (wait for animation to finish)

2. **Check Cooldown**
   - `Action Cooldown` might be too high
   - Reduce to 0.5s or lower

3. **Check Door Script**
   - Is `ToggleDoor()` coroutine working?
   - Check for errors in Console

#### Door Opens/Closes Rapidly

**Problem**: Door toggles multiple times quickly

**Solutions:**
1. **Increase Action Cooldown**
   - Set `Action Cooldown` to 0.5s - 1.0s
   - Prevents rapid toggling

2. **Increase Auto Close Delay**
   - Set `Auto Close Delay` to 2s - 3s
   - Gives enemy more time to pass

3. **Check Radius Edge**
   - Enemy might be walking along radius edge
   - Increase radius slightly to avoid edge cases

---

## Patrol Point Room Entry Setup

### Overview
Patrol points can trigger random room exploration when the AI reaches them. Each patrol point can have different room targets and entry chances.

### 1. Create Patrol Points with Room Entry

#### Basic Setup
1. Create empty GameObjects for patrol route
   ```
   GameObject > Create Empty
   Name: "PatrolPoint_1", "PatrolPoint_2", etc.
   ```

2. Position patrol points along the route
   - Place in hallways, intersections, key locations
   - Standard patrol route: 4-8 points in a loop

3. Add PatrolPoint component (for points that can enter rooms)
   ```
   Select patrol point > Add Component > PatrolPoint
   ```

#### Configure PatrolPoint Component

**Room Entry Settings:**
- `Can Enter Room`: ✅ Check to enable room entry from this point
- `Room Entry Chance`: 0.0 - 1.0 (0.3 = 30% chance)

**Room Targets:**
- `Room Targets`: Array of Transform references to room positions
- Click the `+` button to add slots
- Drag room target GameObjects into each slot

### 2. Assign Room Targets

#### Create Room Target Points
1. Inside each explorable room, create an empty GameObject
   ```
   GameObject > Create Empty
   Name: "RoomTarget_ClassroomA", etc.
   Position: Center of room or desired exploration point
   ```

2. Assign to patrol points
   - Select a patrol point with PatrolPoint component
   - In `Room Targets` array, set size (e.g., Size: 3)
   - Drag room target GameObjects into each slot

#### Strategic Room Assignment

**Method 1: Nearest Rooms**
- Assign rooms that are close to the patrol point
- AI will naturally path to nearby rooms

**Method 2: Thematic Grouping**
- PatrolPoint near classrooms → Assign classroom rooms
- PatrolPoint near offices → Assign office rooms

**Method 3: Mixed Variety**
- Mix different room types for unpredictable behavior
- PatrolPoint_1: [ClassA, Office, Storage]

### 3. Balance Room Entry Chance

#### Recommended Values

| Scenario | Entry Chance | Behavior |
|----------|--------------|----------|
| **Rare Exploration** | 0.1 - 0.2 | 10-20% chance, mostly patrol |
| **Balanced** | 0.3 - 0.4 | 30-40% chance, good variety |
| **Frequent Exploration** | 0.5 - 0.6 | 50-60% chance, often enters rooms |
| **Almost Always** | 0.7 - 0.9 | 70-90% chance, mostly explores |
| **Guaranteed** | 1.0 | 100% chance, always enters room |

#### Balancing Tips

**For Horror Games:**
- Use lower chances (0.2 - 0.3)
- Creates unpredictable behavior
- Player can't predict AI path

**For Search/Hunt Behavior:**
- Use higher chances (0.5 - 0.7)
- AI actively searches rooms
- More thorough coverage

**For Patrol Games:**
- Use moderate chances (0.3 - 0.4)
- Mix of patrol and exploration
- Natural feeling behavior

### 4. Strategic Patrol Point Placement

#### Which Points Should Have Room Entry?

**✅ Enable Room Entry On:**
1. **Hallway Intersections**
   - Natural decision points
   - Multiple rooms accessible
   - Example: T-junction near 3 classrooms

2. **Door Entrances**
   - Points directly outside room doors
   - Easy room access
   - Example: Point at doorway → Room inside

3. **Central Locations**
   - Hub areas connecting multiple rooms
   - Example: Main lobby with 4 room doors

**❌ Disable Room Entry On:**
1. **Mid-Hallway Points**
   - No rooms nearby
   - Would cause awkward pathing
   - Keep as pure patrol points

2. **Dead Ends**
   - Limited room access
   - AI might get stuck
   - Use regular patrol logic

3. **Staircase/Transition Points**
   - Focus on navigation
   - Avoid room entry distractions

#### Example Patrol Route Setup

```
Patrol Route: Office Floor (8 points)

PatrolPoint_1 (Entrance) - Room Entry: ❌ (just entered, keep patrolling)
PatrolPoint_2 (Main Hall) - Room Entry: ✅ Chance: 0.3, Rooms: [Office1, Office2]
PatrolPoint_3 (Intersection) - Room Entry: ✅ Chance: 0.4, Rooms: [MeetingRoom, Storage]
PatrolPoint_4 (Hallway Mid) - Room Entry: ❌ (no rooms nearby)
PatrolPoint_5 (Break Room Door) - Room Entry: ✅ Chance: 0.5, Rooms: [BreakRoom]
PatrolPoint_6 (Far Corridor) - Room Entry: ❌ (transition point)
PatrolPoint_7 (Admin Area) - Room Entry: ✅ Chance: 0.3, Rooms: [Admin1, Admin2, Admin3]
PatrolPoint_8 (Loop Back) - Room Entry: ❌ (returning to start)
```

### 5. Visualize in Scene View (Gizmos)

**Gizmos Indicators:**

🟢 **Green Wire Sphere** = Room entry enabled (`Can Enter Room` = true)
🔴 **Red Wire Sphere** = Room entry disabled (`Can Enter Room` = false)
🔵 **Cyan Lines** = Lines connecting patrol point to each room target

**How to View:**
1. Select patrol point in Hierarchy
2. Look in Scene View
3. See sphere color and lines to rooms

**Tips:**
- Green points stand out → Shows which points trigger rooms
- Cyan lines help visualize room accessibility
- Check lines don't pass through walls (bad room placement)

---

## Testing Checklist

### Pre-Test Setup
- [ ] All doors have Door/DoubleDoor component
- [ ] Enemy GameObject has tag "Enemy"
- [ ] Enemy has AIPath component (A* Pathfinding)
- [ ] Enemy has Seeker component (required by AIPath)
- [ ] Enemy has DosenAI component with patrol points assigned
- [ ] Patrol points created and positioned
- [ ] At least one patrol point has PatrolPoint component
- [ ] Room targets created inside explorable rooms

### Door Detection Tests

#### Test 1: Basic Door Opening
1. Enable `Show Debug Logs` on Door component
2. Play the game
3. Move enemy toward door
4. **Expected**: Door opens when enemy enters yellow sphere radius
5. **Console**: `[Door] 'DoorName' detected enemy 'EnemyName' - Opening door`

#### Test 2: Auto-Close
1. Continue from Test 1
2. Move enemy away from door
3. Wait for `Auto Close Delay` seconds (default 2s)
4. **Expected**: Door closes automatically
5. **Console**: `[Door] 'DoorName' auto-closing after 2s delay`

#### Test 3: Multiple Doors
1. Place 2-3 doors near each other
2. Move enemy through all doors
3. **Expected**: Each door operates independently
4. **Expected**: No interference between doors

#### Test 4: Rapid Movement (Cooldown)
1. Move enemy back and forth at radius edge
2. **Expected**: Door doesn't toggle rapidly
3. **Expected**: Cooldown prevents spam opening/closing

#### Test 5: DoorTriggerZone Compatibility
1. Add DoorTriggerZone component to a trigger zone
2. Assign door to DoorTriggerZone
3. Move enemy through zone
4. **Expected**: Both radius detection AND trigger zone work
5. **Expected**: Door opens from either system

### Patrol Point Room Entry Tests

#### Test 6: Room Entry Trigger
1. Enable `Show Debug Logs` on PatrolPoint component
2. Enable `Show Debug Logs` on DosenAI component
3. Play the game
4. Wait for enemy to reach patrol point with `Can Enter Room` = true
5. **Expected**: Random roll occurs (see Console)
6. **Console**: `[PatrolPoint] 'PointName' room entry roll: 0.25 vs 0.30 = ENTER`
7. **Console**: `[PatrolPoint] 'PointName' selected room: 'RoomName'`
8. **Console**: `[DosenAI] Entering room 'RoomName' from patrol point 'PointName'`

#### Test 7: Room Entry Disabled
1. Use patrol point with `Can Enter Room` = false (red sphere)
2. Wait for enemy to reach this point
3. **Expected**: Enemy continues to next patrol point
4. **Console**: `[DosenAI] Moving to next patrol point: 'NextPointName'`

#### Test 8: Room Entry Failure (No Rooms)
1. Use patrol point with `Can Enter Room` = true but empty `Room Targets` array
2. Wait for enemy to reach this point
3. **Expected**: Enemy skips room entry, continues patrol
4. **Console**: `[PatrolPoint] 'PointName' No room targets assigned!`

#### Test 9: Room Entry Success Rate
1. Set `Room Entry Chance` = 0.5 (50%)
2. Let enemy loop patrol route 10 times
3. Count how many times enemy enters rooms
4. **Expected**: Approximately 5 out of 10 times (may vary due to randomness)

#### Test 10: Multiple Room Targets
1. Assign 3-5 different rooms to one patrol point
2. Let enemy reach that point multiple times
3. **Expected**: Different rooms selected each time (random)
4. **Console**: Shows different room names

### Integration Tests

#### Test 11: Chase Mode During Room Exploration
1. Enemy enters room via patrol point
2. Player enters enemy's detection range
3. **Expected**: Enemy switches to Chase state
4. **Expected**: Enemy abandons room exploration
5. **Expected**: Enemy chases player

#### Test 12: Return to Patrol After Chase
1. Continue from Test 11
2. Player escapes (leaves detection range)
3. Wait for lose player timer (default 5s)
4. **Expected**: Enemy enters Search state
5. After search timer (10s), enemy returns to Patrol state
6. **Expected**: Enemy resumes normal patrol route

### Performance Considerations

#### Frame Rate Test
1. Place 10+ doors in scene with radius detection enabled
2. Spawn 3-5 enemies patrolling
3. Monitor frame rate (FPS)
4. **Expected**: No significant FPS drop
5. **If FPS drops**: 
   - Reduce detection radius on doors
   - Disable detection on rarely-used doors
   - Optimize Physics.OverlapSphere calls

#### Memory Test
1. Play for 5-10 minutes
2. Monitor memory usage in Profiler
3. **Expected**: No memory leaks
4. **Expected**: Stable memory usage

---

## Inspector Settings Reference

### Door Component Layout

```
┌─────────────────────────────────────┐
│ Door (Script)                       │
├─────────────────────────────────────┤
│ ▼ Door Settings                     │
│   ☐ Is Open                         │
│   Open Angle          90            │
│   Open Speed          2             │
│   ☑ Can Close                       │
│                                     │
│ ▼ Audio (Optional)                  │
│   Open Sound         [None]         │
│   Close Sound        [None]         │
│                                     │
│ ▼ Enemy Detection (Auto-Open)       │
│   ☑ Enable Enemy Detection          │
│   Detection Radius    3             │
│   Auto Close Delay    2             │
│   Action Cooldown     0.5           │
│                                     │
│ ▼ Debug                             │
│   ☑ Show Debug Logs                 │
└─────────────────────────────────────┘

Gizmos (Scene View when selected):
🟡 Yellow wire sphere = Detection radius
```

### DoubleDoor Component Layout

```
┌─────────────────────────────────────┐
│ Double Door (Script)                │
├─────────────────────────────────────┤
│ ▼ Door References                   │
│   Left Door          [LeftDoor]     │
│   Right Door         [RightDoor]    │
│                                     │
│ ▼ Door Settings                     │
│   ☐ Is Open                         │
│   Open Angle          90            │
│   Open Speed          2             │
│   ☑ Can Close                       │
│                                     │
│ ▼ Audio (Optional)                  │
│   Open Sound         [None]         │
│   Close Sound        [None]         │
│                                     │
│ ▼ Enemy Detection (Auto-Open)       │
│   ☑ Enable Enemy Detection          │
│   Detection Radius    4             │
│   Auto Close Delay    2             │
│   Action Cooldown     0.5           │
│                                     │
│ ▼ Debug                             │
│   ☑ Show Debug Logs                 │
└─────────────────────────────────────┘

Note: Double doors typically need larger radius (4-5m)
```

### PatrolPoint Component Layout

```
┌─────────────────────────────────────┐
│ Patrol Point (Script)               │
├─────────────────────────────────────┤
│ ▼ Room Entry Settings               │
│   ☑ Can Enter Room                  │
│   Room Entry Chance  [▓▓▓░░] 0.3    │
│                      (0.0 - 1.0)    │
│                                     │
│ ▼ Room Targets                      │
│   Size              3               │
│   Element 0         [ClassroomA]    │
│   Element 1         [Office]        │
│   Element 2         [Storage]       │
│                                     │
│ ▼ Debug                             │
│   ☑ Show Debug Logs                 │
└─────────────────────────────────────┘

Gizmos (Scene View):
🟢 Green wire sphere = Can enter room (enabled)
🔴 Red wire sphere = Cannot enter room (disabled)
🔵 Cyan lines = Lines to each room target
```

### DosenAI Component Layout

```
┌─────────────────────────────────────┐
│ Dosen AI (Script)                   │
├─────────────────────────────────────┤
│ ▼ AI Settings                       │
│   Starting State     Patrol         │
│                                     │
│ ▼ Movement                          │
│   Patrol Speed       3              │
│   Chase Speed        6              │
│                                     │
│ ▼ Patrol Settings                   │
│   Patrol Points                     │
│     Size            8               │
│     Element 0       [Point_1]       │
│     Element 1       [Point_2]       │
│     ...                             │
│   Patrol Wait Time   2              │
│   ☐ Random Patrol                   │
│   Room Entry Chance  0.3 (Legacy)   │
│   Room Points       [Empty/Legacy]  │
│                                     │
│ ▼ Detection                         │
│   Player            [Player]        │
│   Detection Range    15             │
│   Field Of View      120            │
│   Obstacle Mask      [Default]      │
│                                     │
│ ▼ Chase Settings                    │
│   Lose Player Time   5              │
│                                     │
│ ▼ Door Detection                    │
│   Door Detection Range  3           │
│   Door Layer         [Door]         │
│                                     │
│ ▼ Animation                         │
│   Animator          [Animator]      │
│                                     │
│ ▼ Debug                             │
│   ☑ Show Debug Logs                 │
└─────────────────────────────────────┘

Note: Requires AIPath and Seeker components
Tag must be set to "Enemy"
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: "Door doesn't detect enemy"

**Diagnosis:**
1. Check Console with `Show Debug Logs` enabled
2. If no detection message appears:

**Solutions:**
- ✅ Enemy has tag "Enemy" (case-sensitive)
- ✅ Enemy has Collider component
- ✅ Door has `Enable Enemy Detection` checked
- ✅ Enemy is actually entering detection radius (check yellow gizmo)
- ✅ No errors in Console

---

#### Issue: "Patrol point room entry not working"

**Diagnosis:**
1. Enable debug logs on PatrolPoint and DosenAI
2. Check Console when enemy reaches patrol point

**Solutions:**
- ✅ PatrolPoint component added to patrol point GameObject
- ✅ `Can Enter Room` is checked
- ✅ `Room Targets` array has at least one room assigned
- ✅ Room target Transform references are not null
- ✅ `Room Entry Chance` > 0
- ✅ DosenAI patrol points array includes this patrol point

---

#### Issue: "Enemy ignores patrol route"

**Solutions:**
- ✅ DosenAI component attached to enemy
- ✅ AIPath component attached to enemy
- ✅ Seeker component attached to enemy
- ✅ Patrol points array is populated
- ✅ Patrol points have valid positions
- ✅ NavMesh or A* Grid is baked
- ✅ `Starting State` is set to "Patrol"

---

#### Issue: "Door opens and closes rapidly"

**Solutions:**
- ✅ Increase `Action Cooldown` (0.5s - 1.0s)
- ✅ Increase `Auto Close Delay` (2s - 3s)
- ✅ Increase `Detection Radius` slightly (avoid edge detection)
- ✅ Check enemy isn't stuck at radius boundary

---

#### Issue: "Enemy enters wrong rooms"

**Diagnosis:**
Room selection is random - this is intended behavior

**Solutions (if truly wrong):**
- ✅ Check room targets assigned to patrol point
- ✅ Verify Transform references point to correct rooms
- ✅ Room targets are positioned correctly inside rooms
- ✅ No null entries in room targets array

---

#### Issue: "Performance drops with many doors"

**Solutions:**
- Reduce number of doors with detection enabled
- Increase `Action Cooldown` to reduce check frequency
- Disable detection on rarely-used doors
- Use DoorTriggerZone instead for some doors
- Consider pooling/LOD for distant doors

---

#### Issue: "Debug logs not showing"

**Solutions:**
- ✅ `Show Debug Logs` is checked on component
- ✅ Console window is visible (Window > General > Console)
- ✅ Console is not paused
- ✅ Log filters are not hiding Debug messages
- ✅ Game is actually running (Play mode)

---

## Quick Setup Checklist

### For New Door
1. [ ] Add Door or DoubleDoor component
2. [ ] Set `Open Angle` (90° typical)
3. [ ] Enable `Enemy Detection`
4. [ ] Set `Detection Radius` (3-4m typical)
5. [ ] Test with enemy approach
6. [ ] Adjust radius if needed

### For New Patrol Point (with room entry)
1. [ ] Create empty GameObject at patrol position
2. [ ] Add to DosenAI patrol points array
3. [ ] Add PatrolPoint component
4. [ ] Check `Can Enter Room`
5. [ ] Set `Room Entry Chance` (0.3 recommended)
6. [ ] Create room target GameObjects inside rooms
7. [ ] Assign room targets to patrol point
8. [ ] Test with enemy patrol
9. [ ] Verify gizmos (green sphere, cyan lines)

### For New Enemy
1. [ ] Add AIPath component
2. [ ] Add Seeker component
3. [ ] Add DosenAI component
4. [ ] Set tag to "Enemy"
5. [ ] Assign patrol points
6. [ ] Set starting state to Patrol
7. [ ] Test patrol route

---

## Advanced Tips

### Optimizing Room Entry Behavior

**Create Zones:**
Group patrol points by area for thematic room entry:
- Library zone points → Library rooms
- Lab zone points → Lab rooms

**Probability Curves:**
Use different chances based on AI state:
- Normal patrol: 30% chance
- Alert state: 60% chance (searches more)

**Dynamic Adjustment:**
Adjust room entry chance based on player behavior:
```csharp
// Example: Increase search intensity if player is hiding
if (playerIsHiding)
{
    patrolPoint.roomEntryChance = 0.7f;
}
```

### Door Detection Optimization

**Layer Masks:**
Create a "Detectable" layer for enemies to optimize Physics.OverlapSphere:
```csharp
// In Door.cs, add layer mask parameter
[SerializeField] private LayerMask enemyLayer;

// In CheckForEnemies()
Collider[] hitColliders = Physics.OverlapSphere(
    transform.position, 
    detectionRadius, 
    enemyLayer
);
```

**Distance-Based Detection:**
Disable far doors to save performance:
```csharp
// Disable detection if player is far away
float distanceToPlayer = Vector3.Distance(transform.position, player.position);
if (distanceToPlayer > 50f)
{
    enableEnemyDetection = false;
}
```

---

## Support

If you encounter issues not covered in this guide:
1. Enable all debug logs
2. Check Unity Console for errors
3. Verify all components are attached
4. Check Gizmos in Scene View
5. Test with a minimal scene first

**Additional Resources:**
- A* Pathfinding Documentation: [https://arongranberg.com/astar/docs/](https://arongranberg.com/astar/docs/)
- Unity Physics.OverlapSphere: [Unity Manual](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html)
- Unity Gizmos: [Unity Manual](https://docs.unity3d.com/ScriptReference/Gizmos.html)

---

**Last Updated:** December 11, 2025
**Compatible With:** Unity 2021.3+, A* Pathfinding Project 4.x+
