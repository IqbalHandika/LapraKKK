# Win/Lose Cutscene System - Setup Guide

Complete guide untuk setup cutscene win/lose condition dengan screen transition.

---

## **Quick Setup (10 Menit)**

### **1. ScreenTransition Setup (Fade to Black)**

**Create Fade Canvas:**
```
1. Right-click Hierarchy → UI → Canvas
2. Name: "TransitionCanvas"
3. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
4. Canvas:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 999 (di atas semua UI)
```

**Create Fade Image:**
```
1. Right-click TransitionCanvas → UI → Image
2. Name: "FadeImage"
3. Rect Transform:
   - Anchor: Stretch (full screen)
   - Left: 0, Top: 0, Right: 0, Bottom: 0
4. Image Component:
   - Color: Black (RGB: 0,0,0)
   - Alpha: 0 (transparent - akan dikontrol script)
5. Raycast Target: ✓ (block input during fade)
```

**Add ScreenTransition Script:**
```
1. Select TransitionCanvas
2. Add Component → ScreenTransition.cs
3. Assign:
   - Fade Image: FadeImage
   - Default Fade Duration: 1
   - Fade Color: Black (0,0,0,255)
   - Show Debug Logs: ✓
```

---

### **2. CutsceneManager Setup**

**Create Cutscene GameObject:**
```
1. Create Empty GameObject → Name: "CutsceneManager"
2. Add Component → CutsceneManager.cs
3. Add Component → Video Player
4. Add Component → Audio Source (optional untuk cutscene audio)
```

**Configure CutsceneManager:**
```
1. Video Player:
   - Play On Awake: ✗ (unchecked)
   - Loop: ✗
   - Render Mode: Camera Far Plane (atau RenderTexture)
   - Target Camera: Main Camera

2. CutsceneManager Script:
   - Video Player: Auto-assigned
   - Win Cutscene: Drag video file (WinCutscene.mp4)
   - Lose Cutscene: Drag video file (LoseCutscene.mp4)
   - Fade Out Duration: 1.5
   - Fade In Duration: 1
   - Win Scene Name: "MainMenu" (atau kosongkan untuk restart)
   - Lose Scene Name: "" (kosong = restart level)
   - Auto Load Scene After Cutscene: ✓
   - Show Debug Logs: ✓
```

**Import Video Files:**
```
1. Drag video files ke Assets/Videos/
2. Select video file → Inspector:
   - Video Codec: H.264
   - Transcode: ✓ (jika perlu)
```

---

### **3. GameManager Integration**

GameManager sudah otomatis terintegrasi:
- `TriggerWin()` → Fade out → Play win cutscene → Restart/Load scene
- `TriggerLose()` → Fade out → Play lose cutscene → Restart

**No additional setup needed!**

---

## **Win/Lose Flow:**

### **Win Condition Flow:**
```
1. Player kumpulin 5 Bab papers
   ↓
2. Exit door unlocked (hijau)
   ↓
3. Player ambil exit key
   ↓
4. Player ke exit door, press E
   ↓
5. ExitDoor.TriggerExit() → GameManager.TriggerWin()
   ↓
6. ScreenTransition.FadeOut(1.5s) → Layar hitam
   ↓
7. CutsceneManager.PlayWinCutscene()
   ↓
8. Video plays (player bisa skip: Space/Escape)
   ↓
9. Video selesai → Fade out → Load scene/restart
```

### **Lose Condition Flow:**
```
1. Player tertangkap dosen (or dies)
   ↓
2. DosenAI calls GameManager.TriggerLose()
   ↓
3. ScreenTransition.FadeOut(1s) → Layar hitam
   ↓
4. CutsceneManager.PlayLoseCutscene()
   ↓
5. Video plays (player bisa skip: Space/Escape)
   ↓
6. Video selesai → Restart level
```

---

## **DosenAI Integration (Lose Condition)**

Update DosenAI untuk trigger lose saat player tertangkap:

```csharp
// Di DosenAI.cs, tambahkan method:
void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Player") && currentState == AIState.Chase)
    {
        // Player tertangkap!
        Debug.Log("[DosenAI] Player caught!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerLose();
        }
    }
}
```

**Setup DosenAI Collider:**
```
1. Select DosenAI GameObject
2. Add Collider (if not exists):
   - Capsule Collider
   - Is Trigger: ✗ (UNCHECK - use collision, not trigger)
   - Radius: 0.5
   - Height: 2
3. Rigidbody (if needed):
   - Is Kinematic: ✓
```

---

## **Cutscene UI (Optional - Skip Prompt)**

**Create Skip Prompt:**
```
1. Right-click Hierarchy → UI → Canvas
2. Name: "CutsceneUICanvas"
3. Add child → UI → TextMeshPro - Text
4. Name: "SkipText"
5. Text: "Press SPACE or ESC to skip"
6. Position: Bottom-right
7. Font Size: 18
8. Color: White with shadow

Assign di CutsceneManager:
- Cutscene UI Canvas: CutsceneUICanvas
```

---

## **Video File Requirements:**

**Recommended Settings:**
```
Format: MP4 (H.264)
Resolution: 1920x1080 atau 1280x720
Frame Rate: 30 FPS
Codec: H.264
Audio: AAC
Duration: 5-30 seconds (jangan terlalu panjang)
```

**Import Settings (Unity):**
```
1. Select video in Project
2. Inspector:
   - Transcode: ✓
   - Codec: H.264
   - Quality: Medium-High
```

---

## **Testing:**

### **Test Win Cutscene:**
```
1. Play game
2. Kumpulin 5 papers
3. Ambil exit key
4. Ke exit door, press E
5. Harus: Fade to black → Video plays → Scene restarts
```

### **Test Lose Cutscene:**
```
1. Play game
2. Kena tangkap dosen
3. Harus: Fade to black → Video plays → Scene restarts
```

### **Test Skip:**
```
1. Saat cutscene playing
2. Press Space atau Escape
3. Video stops → Load scene
```

---

## **Debug Logs:**

Console akan show:
```
[ScreenTransition] Fading out to black (1.5s)
[CutsceneManager] Playing WIN cutscene...
[CutsceneManager] Cutscene playing... Duration: 10s
[CutsceneManager] Cutscene ended
[CutsceneManager] Loading scene: MainMenu
```

---

## **Troubleshooting:**

**Issue: Video tidak muncul**
- Check Video Player Render Mode = Camera Far Plane
- Check Target Camera assigned
- Check video file imported dengan benar

**Issue: Fade tidak smooth**
- Check Fade Image alpha = 0 di start
- Check Canvas Render Mode = Screen Space Overlay
- Check Sort Order = 999

**Issue: Cutscene tidak skip**
- Check Input di CutsceneManager coroutine
- Press Space atau Escape saat video playing

**Issue: Scene tidak load setelah cutscene**
- Check Auto Load Scene After Cutscene = ✓
- Check scene name di Build Settings
- Check console untuk error

---

## **File Structure:**

```
Assets/
├── Videos/
│   ├── WinCutscene.mp4
│   └── LoseCutscene.mp4
├── Script/
│   ├── ScreenTransition.cs
│   ├── CutsceneManager.cs
│   ├── GameManager.cs (updated)
│   └── DosenAI.cs (update untuk lose condition)
└── Scenes/
    ├── MainMenu
    └── GameLevel
```

---

## **Next Steps:**

1. ✅ Buat video cutscene win/lose (atau pakai placeholder)
2. ✅ Setup DosenAI collision untuk trigger lose
3. ✅ Test win condition (kumpulin items → exit)
4. ✅ Test lose condition (kena tangkap)
5. ✅ Polish: Add sound effects, better transitions

Selamat! Cutscene system ready! 🎬
