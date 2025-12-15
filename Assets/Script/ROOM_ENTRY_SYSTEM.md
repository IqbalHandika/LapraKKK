# Room Entry System Documentation

## Overview
Sequential waypoint system that allows DosenAI to enter rooms through closed doors using A* pathfinding. Solves the issue where A* grid treats closed doors as obstacles, blocking direct paths into rooms.

## Problem Solved
**Original Issue:** "kalo misal dosen dapet patrol masuk ruangan gimana? kan jalurnya ketutup sama pintu"
- A* grid pathfinding marks closed doors as unwalkable obstacles
- Direct path to inner room points blocked by door
- AI couldn't navigate into rooms

**Solution:** Outer + Inner Point System
- Outer Point: Positioned BEFORE the door (outside room, A* can path to it)
- Door Detection: Automatic opening when AI reaches outer point (3m radius)
- Inner Point: Positioned INSIDE the room (AI paths after door opens)
- Sequential navigation ensures door opens before AI attempts to enter

## Architecture

### Components

#### 1. PatrolPoint.cs - RoomEntry Class
```csharp
[System.Serializable]
public class RoomEntry
{
    public Transform outerPoint;   // Point before door (outside room)
    public Transform innerPoint;   // Point inside room
    public float exploreTime = 3f; // Time to stay in room
}
```

**Setup in Unity:**
- Add `PatrolPoint` component to patrol point GameObjects
- Configure room entry:
  - `Room Entry Chance`: 0-1 probability (0.3 = 30%)
  - `Room Entries`: Array of RoomEntry objects
    - Outer Point: Empty GameObject BEFORE door
    - Inner Point: Empty GameObject INSIDE room
    - Explore Time: Seconds to explore room

#### 2. DosenAI.cs - Room Entry State Machine

**State Variables:**
```csharp
private bool isEnteringRoom = false;
private PatrolPoint.RoomEntry currentRoomEntry = null;
private RoomEntryPhase roomEntryPhase = RoomEntryPhase.None;
private float roomExploreTimer = 0f;
```

**Phase Enum:**
```csharp
enum RoomEntryPhase
{
    None,            // Not entering room
    MovingToOuter,   // Navigating to outer point
    WaitingForDoor,  // Waiting 0.8s for door to open
    MovingToInner,   // Navigating to inner point
    Exploring,       // Staying in room for exploreTime
    ReturnToOuter    // Returning to outer point before continuing patrol
}
```

## Execution Flow

### 1. Trigger Room Entry (HandlePatrol)
```
Patrol Point Reached
    ↓
Check PatrolPoint.ShouldEnterRoom() (random chance)
    ↓
Get PatrolPoint.GetRandomRoomEntry()
    ↓
Set: isEnteringRoom = true
     currentRoomEntry = selected entry
     roomEntryPhase = MovingToOuter
```

### 2. MovingToOuter Phase
```
Set Destination: currentRoomEntry.outerPoint.position
    ↓
AI navigates using A* pathfinding (door still closed)
    ↓
Check: agent.reachedEndOfPath
    ↓
Phase: MovingToOuter → WaitingForDoor
```

### 3. WaitingForDoor Phase
```
AI stops at outer point
    ↓
Door.cs CheckForEnemies() detects AI (3m radius)
    ↓
Door automatically opens (coroutine animation)
    ↓
Door calls UpdateAstarGraph() → A* grid updated, door area now WALKABLE
    ↓
Wait 0.8 seconds (roomExploreTimer)
    ↓
Phase: WaitingForDoor → MovingToInner
```

### 4. MovingToInner Phase
```
Set Destination: currentRoomEntry.innerPoint.position
    ↓
AI navigates through NOW OPEN door (A* grid updated)
    ↓
Check: agent.reachedEndOfPath
    ↓
Phase: MovingToInner → Exploring
```

### 5. Exploring Phase
```
AI stays at inner point
    ↓
Wait currentRoomEntry.exploreTime seconds
    ↓
Phase: Exploring → ReturnToOuter
```

### 6. ReturnToOuter Phase
```
Set Destination: currentRoomEntry.outerPoint.position
    ↓
AI navigates back to outer point
    ↓
Check: agent.reachedEndOfPath
    ↓
Return to patrol:
    - isEnteringRoom = false
    - roomEntryPhase = None
    - currentRoomEntry = null
    - Continue to NEXT patrol point (sequential)
```

## Integration with Existing Systems

### Door Auto-Opening
- **Door.cs** has enemy detection (layer mask "Enemy")
- Radius: 3m (configurable `enemyDetectionRadius`)
- Check interval: 0.2 seconds (optimized)
- Works automatically when AI enters detection range

### A* Pathfinding Integration
- **Door.cs** and **DoubleDoor.cs** now update A* grid when opened/closed
- `UpdateAstarGraph()` method called after door animation completes
- **GraphUpdateObject** with configurable bounds (default 2x3x0.5m)
- Door area nodes recalculated: CLOSED = unwalkable, OPEN = walkable
- Enables AI to path through doors dynamically

**Configuration:**
- `Update Astar Graph`: Enable/disable graph updates (default: true)
- `Graph Update Bounds`: Size of area to update (adjust to cover door fully)
- Visualized as green wire cube in Scene View when door selected

**How It Works:**
1. Door opens → Animation completes
2. `UpdateAstarGraph()` called
3. A* recalculates nodes in `graphUpdateBounds`
4. Grid graph updates walkability based on door collider state
5. AI can now path through open door

**Important:** 
- Door GameObject must have collider for A* obstacle detection
- Grid graph must be set to scan for colliders
- Graph update happens AFTER animation, so 0.8s wait time ensures door is fully open

### Player Detection Override
- `CanSeePlayer()` checked in `HandleRoomEntry()`
- If player spotted during room entry:
  - Abort room entry sequence
  - Clear state (`isEnteringRoom = false`)
  - Switch to Chase state immediately

## Configuration Guide

### Setting Up Room Entry Points

1. **Create Outer Point:**
   ```
   GameObject → Create Empty → Name: "Room1_OuterPoint"
   Position: Just BEFORE the door (outside room)
   Distance: 1-2m from door
   ```

2. **Create Inner Point:**
   ```
   GameObject → Create Empty → Name: "Room1_InnerPoint"
   Position: Inside the room (center or strategic location)
   Distance: 2-3m past door threshold
   ```

3. **Configure PatrolPoint:**
   ```
   Select patrol point GameObject
   Add Component → PatrolPoint
   Room Entry Chance: 0.3 (30%)
   Room Entries:
     [0] Outer Point: Room1_OuterPoint
         Inner Point: Room1_InnerPoint
         Explore Time: 3
   ```

### Best Practices

**Outer Point Placement:**
- ✅ Must be pathable by A* (not blocked)
- ✅ Within 3m of door (auto-open range)
- ✅ Aligned with door direction
- ❌ Don't place inside door frame
- ❌ Don't place behind obstacles

**Inner Point Placement:**
- ✅ Clear area inside room
- ✅ Strategic position (cover, corner, center)
- ✅ Account for furniture/obstacles
- ❌ Don't place too close to door (AI might clip)
- ❌ Don't place in unreachable areas

**Timing Configuration:**
- `Wait Time`: 0.8s (door open animation)
- `Explore Time`: 3-5s (room exploration)
- Adjust based on door animation speed

## Debugging

### Debug Logs
Enable in DosenAI inspector: `Show Debug Logs`

**Key Messages:**
```
[DosenAI] Starting room entry sequence from patrol point 'Patrol1' -> Outer: 'Room1_Outer' -> Inner: 'Room1_Inner'
[DosenAI] Room Entry Phase: MovingToOuter -> 'Room1_OuterPoint'
[DosenAI] Room Entry Phase: Reached outer point, waiting for door to open
[Door] A* graph updated at (10, 0, 5) - Door is now OPEN (walkable)
[DosenAI] Room Entry Phase: Door should be open, proceeding to inner point
[DosenAI] Room Entry Phase: MovingToInner -> 'Room1_InnerPoint'
[DosenAI] Room Entry Phase: Reached inner point, exploring for 3s
[DosenAI] Room Entry Phase: Finished exploring, returning to outer point
[DosenAI] Room Entry Phase: ReturnToOuter -> 'Room1_OuterPoint'
[DosenAI] Room Entry Phase: Back at outer point, continuing patrol
[DosenAI] Moving to next patrol point: 'Patrol2'
```

### Gizmos Visualization
**PatrolPoint Gizmos (Scene View):**
- **Cyan Line**: Patrol Point → Outer Point
- **Yellow Line**: Outer Point → Inner Point
- Shows complete path sequence visually

**Door Gizmos:**
- **Green Sphere**: Enemy detection radius (3m)

### Common Issues

**Problem:** AI doesn't enter room
- Check: `Room Entry Chance` > 0
- Check: `Room Entries` array not empty
- Check: Outer/Inner points assigned

**Problem:** AI stuck at outer point
- Check: Door `enemyDetectionRadius` >= distance to outer point
- Check: Door has "Enemy" layer detection enabled
- Check: AI GameObject has "Enemy" layer

**Problem:** AI paths through closed door
- Check: A* graph updated (scan at runtime if dynamic)
- Check: Door collider properly configured
- Check: A* graph obstacles include door
- Check: `Update Astar Graph` enabled on Door component
- Check: `Graph Update Bounds` covers door area completely

**Problem:** Graph update not working
- Check: `AstarPath` component exists in scene
- Check: Door has `using Pathfinding;` directive
- Enable `Show Debug Logs` on Door to see update messages
- Verify bounds size in Scene View (green wire cube)
- Grid graph must have "Collision Testing" enabled

**Problem:** Door doesn't open
- Check: Door.cs `detectEnemies` enabled
- Check: `enemyDetectionRadius` = 3m minimum
- Check: `enemyLayer` matches AI layer

## Performance

**Room Entry System:**
- No per-frame overhead when not entering room
- State machine only active during room entry
- Timer-based (no expensive checks)

**Overall Cost:**
- Phase transitions: Negligible (state enum)
- Pathfinding: Standard A* cost (2 paths per room entry)
- Door detection: 5 checks/second (optimized in Door.cs)

## Legacy System

**Old System (Deprecated):**
```csharp
Transform roomTarget = patrolPoint.GetRandomRoom(); // OLD
agent.destination = roomTarget.position;             // Direct path (blocked by door)
```

**Why It Failed:**
- Direct path to inner point
- A* couldn't path through closed door
- No sequential waypoints

**New System:**
```csharp
RoomEntry entry = patrolPoint.GetRandomRoomEntry(); // NEW
// Sequential: outer → wait → inner
```

**Backward Compatibility:**
- Old `roomPoints` array still supported
- Triggers warning in console
- Recommend migrating to RoomEntry system

## Summary

The room entry system elegantly solves A* pathfinding limitations by:
1. **Breaking room navigation into reachable segments** (outer → inner → outer → next patrol)
2. **Leveraging automatic door opening mechanics** (3m radius detection)
3. **Using state machine for reliable sequencing** (6-phase system)
4. **Dynamically updating A* grid when doors open/close** (GraphUpdateObject)
5. **Providing visual tools for level designers** (gizmos for points and bounds)
6. **Sequential patrol flow** (returns to outer point before continuing to next patrol point)

This allows AI to explore rooms naturally while respecting A* grid obstacles and maintaining predictable patrol behavior.

### Key Improvements from Legacy System
- ❌ **OLD:** Direct path to room point → blocked by closed door
- ✅ **NEW:** Outer point → door opens → inner point → outer point → continue patrol
- ❌ **OLD:** Static grid, doors always unwalkable
- ✅ **NEW:** Dynamic grid updates, doors become walkable when open
- ❌ **OLD:** Random room selection with deprecated `roomPoints` array
- ✅ **NEW:** PatrolPoint component with RoomEntry system (outer + inner points)
