# Exit Chase Sequence Setup Guide

Panduan setup untuk final chase sequence - Dosen spawn di pintu exit, kejar player ke lorong, player menang saat masuk win zone.

## 📋 Overview

**Skenario:**
1. Player buka pintu exit
2. Dosen spawn di depan pintu exit
3. Dosen kejar player
4. Player lari ke ujung lorong (win zone)
5. **Player masuk win zone** - Tidak terjadi apa-apa (immune to death)
6. **Dosen tangkap player** - Trigger win cutscene!

**Flow Detail:**
- Win zone hanya **melindungi** player (tidak langsung menang)
- Player harus **ditangkap** di dalam win zone
- Saat caught → Freeze player → Delay → Win cutscene

**Scripts yang Dibutuhkan:**
- ✅ `WinZone.cs` - Area kemenangan (immunity)
- ✅ `ExitDoorSpawner.cs` - Spawn Dosen saat pintu dibuka
- ✅ `PlayerHealth.cs` (sudah dimodifikasi) - Win trigger saat caught in zone
- ✅ `DosenAI.cs` (sudah dimodifikasi) - Chase behavior

---

## 🎯 Step 1: Setup Win Zone

### 1.1 Buat Win Zone GameObject

1. **Buat Empty GameObject** di ujung lorong exit:
   - Hierarchy → Right-click → Create Empty
   - Nama: `WinZone_Exit`
   - Position: Di ujung lorong exit (tempat player kabur)

2. **Tambahkan Collider:**
   - Add Component → Box Collider (atau Capsule Collider)
   - ✅ **CENTANG** `Is Trigger`
   - Adjust size untuk cover area lorong:
     ```
     Box Collider:
     - Size: (4, 3, 3)  // Lebar x Tinggi x Kedalaman
     - Center: (0, 1.5, 0)
     ```

3. **Tambahkan WinZone Script:**
   - Add Component → Win Zone
   - **Settings:**
     ```
     Win Zone Settings:
       Is Active: ✅ (checked)
       Win Delay: 0.5  // NOT USED - win triggers on catch, not on entry
     
     Audio (optional):
       Win Sound: [Drag safe zone sound - relief/heartbeat slow]
       Audio Source: [Auto-created or drag existing]
     
     Debug:
       Show Debug Logs: ✅ (untuk testing)
     ```

4. **Layer Setup:**
   - Win zone TIDAK perlu layer khusus
   - Collider trigger otomatis detect Player tag

### 1.2 Visualisasi Win Zone

- Win zone tampil sebagai **cyan wire box** di Scene view (Gizmos)
- Jadi **green** saat player masuk
- Pastikan box cover seluruh area lorong exit

---

## 🚪 Step 2: Setup Exit Door Spawner

### 2.1 Buat Spawner GameObject

1. **Buat Empty GameObject** di area pintu exit:
   - Hierarchy → Right-click → Create Empty
   - Nama: `ExitDoorSpawner`
   - Position: Di depan/dekat pintu exit

2. **Tambahkan ExitDoorSpawner Script:**
   - Add Component → Exit Door Spawner

### 2.2 Setup Spawn Point

1. **Buat Child GameObject untuk spawn point:**
   - Right-click `ExitDoorSpawner` → Create Empty
   - Nama: `DosenSpawnPoint`
   - **Position:** Di DEPAN pintu exit (blocking player's way)
     - Example: Jika pintu di (0, 0, 10), spawn point di (0, 0, 12)
   - **Rotation:** Hadap ke arah player akan lari
     - Example: Y rotation = 180° (facing back into corridor)

2. **Visualisasi:**
   - Spawn point tampil sebagai **yellow sphere** + forward arrow (Gizmos)
   - Arrow menunjukkan arah Dosen akan menghadap
   - Cyan line menunjukkan target chase (Player)

### 2.3 Configure Spawner Settings

```
Spawn Settings:
  Dosen Prefab: [Drag Dosen prefab dari Assets]
  Spawn Point: [Drag DosenSpawnPoint child object]
  Spawn Delay: 0.5  // Delay setelah pintu dibuka

Dosen Behavior:
  Chase Target: [Drag Player GameObject]
  Dosen Chases Immediately: ✅ (checked)

Door Reference:
  Exit Door: [Drag exit door GameObject]
  // Script auto-detect saat door.IsOpen() == true

Audio (optional):
  Spawn Sound: [Drag dramatic spawn/door slam sound]
  Audio Source: [Auto-created]

Debug:
  Show Debug Logs: ✅ (untuk testing)
```

**Cara Kerja:**
- Spawner cek `exitDoor.IsOpen()` setiap frame
- Saat pintu buka → Delay 0.5s → Spawn Dosen
- Dosen langsung chase Player

---

## 🧟 Step 3: Setup Dosen Prefab

### 3.1 Pastikan Dosen Prefab Sudah Benar

Buka Dosen prefab dan pastikan ada:

1. **DosenAI Component** dengan settings:
   ```
   AI Settings:
     Starting State: Patrol (akan auto-override ke Chase)
   
   Movement:
     Chase Speed: 6-8  // Harus lebih cepat dari player!
   
   Detection:
     Player: [Auto-assigned by spawner]
     Detection Range: 5
     Vision Range: 15
   
   Kill System:
     Kill Radius: 2
     Kill Angle: 90
   
   Debug:
     Show Debug Logs: ✅
   ```

2. **Collider:** Capsule Collider (untuk collision dengan player)
3. **Rigidbody:** TIDAK perlu (DosenAI pakai A* Pathfinding)
4. **Animator:** Dengan Speed parameter

### 3.2 Test Dosen Spawn

1. Play mode
2. Buka exit door
3. Dosen harus spawn di spawn point setelah 0.5s
4. Check Console:
   ```
   [ExitDoorSpawner] Spawned Dosen at (0, 0, 12)
   [DosenAI] Force chase target set to: Player
   ```

---

## 👤 Step 4: Player Setup

### 4.1 Tag Player GameObject

Pastikan Player punya tag `Player`:
- Select Player GameObject
- Inspector → Tag → Player

### 4.2 PlayerHealth Component

PlayerHealth sudah dimodifikasi dengan win zone catch system:

```csharp
private bool isInWinZone = false;  // NEW

public void Die(Transform killer, float animationDuration)
{
    // Check if in win zone - trigger WIN instead of death
    if (isInWinZone)
    {
        Debug.Log("Player caught in win zone - triggering WIN!");
        
        // Freeze player
        movementController.enabled = false;
        
        // Trigger win after delay (dramatic effect)
        Invoke(nameof(TriggerWinCondition), animationDuration * 0.5f);
        return;  // Don't die!
    }
    // ... normal death code
}

void TriggerWinCondition()
{
    GameManager.Instance.TriggerWin();
}
```

**Cara Kerja:**
1. Player masuk win zone → `isInWinZone = true`
2. Dosen tangkap player → Call `Die()`
3. Check `isInWinZone` → True
4. Freeze player + delay
5. Trigger win cutscene

**Tidak perlu setup apapun** - otomatis berfungsi!

---

## 🎬 Step 5: Testing Complete Sequence

### 5.1 Test Flow

1. **Start game** di area sebelum exit door
2. **Buka exit door** (E key)
3. **Observe:**
   - [ ] Dosen spawn setelah 0.5s
   - [ ] Dosen langsung chase Player
   - [ ] Dosen lebih cepat dari player walk (tapi player bisa sprint)
4. **Sprint ke win zone** (Shift + W)
5. **Masuk win zone:**
   - [ ] Console: `[WinZone] Player entered win zone! Waiting for catch...`
   - [ ] Console: `[PlayerHealth] Win zone immunity: True`
   - [ ] **TIDAK** langsung trigger win - player masih bisa gerak
6. **Keep running** - Dosen masih kejar
7. **Dosen tangkap player:**
   - [ ] Console: `[PlayerHealth] Player caught in win zone - triggering WIN sequence!`
   - [ ] Player freeze
   - [ ] Delay ~1 second
8. **Win triggered:**
   - [ ] Console: `[PlayerHealth] Triggering WIN after catch sequence!`
   - [ ] GameManager.TriggerWin() dipanggil
   - [ ] Transisi ke cutscene/win screen

### 5.2 Debug Console Output (Expected)

```
[ExitDoorSpawner] Spawned Dosen at (0, 0, 12)
[DosenAI] Force chase target set to: Player
[WinZone] Player entered win zone! Waiting for catch...
[PlayerHealth] Win zone immunity: True
[PlayerHealth] Player caught in win zone - triggering WIN sequence!
[PlayerHealth] Triggering WIN after catch sequence!
[GameManager] TriggerWin() called
```

### 5.3 Troubleshooting

**Dosen tidak spawn:**
- Check exitDoor reference di ExitDoorSpawner
- Check Console untuk error
- Pastikan door script punya method `IsOpen()`

**Dosen spawn tapi tidak chase:**
- Check DosenAI → Player reference
- Check "Dosen Chases Immediately" di spawner
- Pastikan DosenAI starting state = Patrol/Idle (bukan disabled)

**Player tetap mati di win zone:**
- Check Player tag (harus `Player`)
- Check WinZone → Is Active
- Check Console untuk "[PlayerHealth] Win zone immunity: True"
- Pastikan WinZone collider IS TRIGGER

**Win tidak trigger:**
- Pastikan Dosen TANGKAP player dulu (masuk kill radius)
- Check Console untuk "Player caught in win zone"
- Check GameManager.Instance ada di scene
- Check GameManager punya method TriggerWin()

**Win trigger terlalu cepat (sebelum caught):**
- Ini SALAH - win harus trigger setelah caught
- Check PlayerHealth.Die() - harus ada check isInWinZone
- Win delay = animationDuration * 0.5f (~1 second)

---

## ⚙️ Step 6: Fine-Tuning

### 6.1 Timing Adjustments

**Spawn Delay:**
```csharp
// ExitDoorSpawner
Spawn Delay: 0.5f  // Cepat = dramatic
Spawn Delay: 2.0f  // Kasih waktu player lari dulu
```

**Win Delay (After Catch):**
```csharp
// PlayerHealth.Die()
Delay: animationDuration * 0.5f  // Default ~1 second
// Cukup waktu untuk show "caught" moment
// Tapi tidak terlalu lama
```

### 6.2 Chase Balance

**Dosen Speed vs Player Speed:**
- Player walk: 10 m/s
- Player sprint: 16 m/s
- **Dosen chase:** 6-8 m/s (lebih lambat dari sprint)

**Important:** Player HARUS ditangkap di win zone!

**Scenario Options:**

**Option A: Guaranteed Catch (Recommended)**
```
Dosen Chase Speed: 8-10
Win Zone: Di ujung lorong (dead end)
Result: Player terpojok, PASTI ditangkap
```

**Option B: Slower Chase (Dramatic)**
```
Dosen Chase Speed: 7
Win Zone: Ujung lorong
Result: Player sampai ujung dulu, baru Dosen datang
```

**Option C: Fast Chase (Intense)**
```
Dosen Chase Speed: 12
Win Zone: Ujung lorong
Result: Dosen hampir tangkap sebelum player sampai ujung
```

**Key Point:** Karena lorong BUNTU, player tidak punya escape route. Pasti ditangkap di ujung!

### 6.3 Win Zone Size

**Important:** Karena lorong EXIT adalah **DEAD END (buntu)**, player bakal terpojok di ujung!

**Standard Win Zone (Recommended):**
```
Box Collider Size: (6, 3, 5)  // Cukup untuk cover ujung lorong
Player lari sampai ujung → Terpojok → Dosen tangkap
```

**Tight Zone (Minimal):**
```
Box Collider Size: (4, 3, 3)  // Kecil, tepat di ujung
Player mentok ke tembok → Nowhere to run → Caught
```

**Wide Zone (Forgiving):**
```
Box Collider Size: (8, 3, 7)  // Lebih besar
Cover area lebih luas di ujung lorong
```

**Design Note:** 
- Lorong buntu = Player PASTI ditangkap
- Win zone hanya perlu cover area "terpojok"
- Tidak perlu panjang karena player tidak bisa lari lebih jauh

---

## 🎮 Step 7: Polish

### 7.1 Audio Enhancement

**WinZone:**
- Win Sound: Escape/relief sound (heartbeat slow down, victory music)

**ExitDoorSpawner:**
- Spawn Sound: Dramatic slam, monster roar, atau sudden music sting

**Player:**
- Heavy breathing saat sprint di lorong

### 7.2 Visual Effects (Optional)

**Particle effects saat Dosen spawn:**
```csharp
// Add to ExitDoorSpawner.ExecuteSpawn()
ParticleSystem spawnEffect = Instantiate(spawnEffectPrefab, spawnPoint.position, Quaternion.identity);
```

**Screen shake saat Dosen spawn:**
```csharp
// Add camera shake script
CameraShake.Instance.Shake(0.5f, 0.3f);
```

**Slow motion di win zone:**
```csharp
// Add to WinZone.OnTriggerEnter()
Time.timeScale = 0.3f;  // Slow motion effect
```

### 7.3 Camera Cinematic

**Option: Kamera look back saat lari:**
```csharp
// PlayerHealth atau custom script
void Update()
{
    if (Input.GetKey(KeyCode.C))
    {
        // Rotate camera to look behind (see Dosen chasing)
        playerCamera.transform.localEulerAngles = new Vector3(0, 180, 0);
    }
}
```

---

## 📐 Recommended Layout

```
Scene Hierarchy:
├── Player
│   └── Camera
├── ExitDoor (with Door script)
├── ExitDoorSpawner
│   └── DosenSpawnPoint (Transform)
├── ExitCorridor (geometry - DEAD END!)
│   └── Wall_End (collision wall at end)
└── WinZone_Exit (Box Collider trigger at dead end)

Positions (Example - Dead End Corridor):
- ExitDoor: (0, 0, 10)
- DosenSpawnPoint: (0, 0, 12) facing (0, 0, -1)
- Corridor End Wall: (0, 0, 30)
- Win Zone: (0, 1.5, 28) size (6, 3, 5)
- Total corridor length: 20 meters
- Dead end: Player trapped at end
```

**Layout Notes:**
- Corridor MUST have wall/collision at end
- Win zone positioned AT the dead end
- Player akan lari → Mentok tembok → Trapped → Caught
- No escape routes = guaranteed catch

---

## 🎯 Summary Checklist

### Setup:
- [ ] WinZone GameObject dengan Box Collider (trigger)
- [ ] WinZone script configured
- [ ] ExitDoorSpawner GameObject
- [ ] DosenSpawnPoint child dengan position/rotation
- [ ] ExitDoorSpawner script dengan Dosen prefab reference
- [ ] Exit door reference assigned
- [ ] Player tagged as "Player"

### Testing:
- [ ] Door buka → Dosen spawn
- [ ] Dosen chase player immediately
- [ ] Player sprint ke win zone
- [ ] Player masuk win zone → Safe (no instant win)
- [ ] Dosen masih chase DI DALAM win zone
- [ ] Dosen tangkap player → Freeze → Win triggered
- [ ] Transition ke cutscene

### Polish:
- [ ] Audio effects (spawn, win)
- [ ] Timing balance (spawn delay, win delay, chase speed)
- [ ] Win zone size appropriate
- [ ] Debug logs disabled di final build

---

## 🔧 Advanced: Alternative Triggers

### Manual Trigger (Cutscene Control)

Jika tidak pakai auto-detect door:

```csharp
// Di cutscene atau custom script
public ExitDoorSpawner spawner;

void OnExitDoorOpened()
{
    spawner.TriggerSpawn();  // Manual spawn
}
```

### Multiple Win Zones

Setup multiple win areas:

```csharp
// Scene setup:
WinZone_Exit1 (primary)
WinZone_Exit2 (alternative route)
WinZone_SafeRoom (backup)

// Semua pakai WinZone script yang sama
```

---

## 💡 Tips

1. **Test spawn point position:**
   - Jangan terlalu dekat pintu (player bisa stuck)
   - Jangan terlalu jauh (lose dramatic effect)
   - Ideal: 2-3 meters dari pintu

2. **Win zone placement:**
   - Buat visible cue (cahaya, particle effect, door)
   - Di ujung lorong buntu (dead end)
   - Player tahu "ini ujung jalan, bakal ketangkep di sini"

3. **Speed balance:**
   - Karena lorong buntu, focus on dramatic timing
   - Terlalu cepat = caught sebelum sampai ujung
   - Terlalu lambat = player tunggu terlalu lama di ujung
   - Sweet spot: Player sampai ujung → Turn around → See Dosen → Caught

4. **Audio timing:**
   - Spawn sound INSTANT saat spawn
   - Win sound delayed 0.5s untuk dramatic build

5. **Multiple playtests:**
   - Coba beberapa kali untuk consistency
   - Pastikan SELALU bisa menang
   - Check tidak ada bug physics/collision

---

**Setup Complete!** 🎉

Player sekarang punya dramatic final escape sequence dengan Dosen chase yang intens!
