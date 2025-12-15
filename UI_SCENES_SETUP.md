# UI Scenes Setup Guide

Complete setup untuk Main Menu, Intro/Tutorial, dan Ending scenes.

## 📋 Overview

**Scenes yang Dibuat:**
1. **MainMenu** - Start game & Quit
2. **IntroTutorial** - Dialog intro dengan Next/Skip
3. **EndingWin** - Win dialog dengan Play Again/Main Menu
4. **EndingLose** - Lose dialog dengan Play Again/Main Menu

**Scripts:**
- `MainMenuManager.cs` - Handle main menu buttons
- `DialogSystem.cs` - Universal dialog system untuk semua dialog scenes

---

## 📷 Camera & Lighting Setup

### Main Camera (REQUIRED)

**All UI scenes NEED Main Camera:**

```
Camera Settings:
  Clear Flags: Solid Color
  Background: Black (0, 0, 0) atau custom color
  Projection: Perspective atau Orthographic
  
  Position: (0, 0, -10)
  Rotation: (0, 0, 0)
```

**Why?**
- Canvas Screen Space - Camera mode perlu camera reference
- Render UI elements properly
- Handle screen transitions

### Directional Light (NOT NEEDED)

**Can be deleted or disabled:**
- UI tidak butuh lighting
- Save performance
- Cleaner hierarchy

**Optional:** Keep untuk background effects (particle, fog, dll)

---

## 🎮 Scene 1: Main Menu

### Hierarchy Setup:

```
MainMenu Scene
├── Main Camera
├── EventSystem
├── Canvas
│   ├── TitleText (TextMeshPro)
│   ├── StartButton (Button)
│   └── QuitButton (Button)
└── MainMenuManager (Empty GameObject)
```

### Step-by-Step:

#### 1. Create Scene
- File → New Scene
- Save as "MainMenu"
- Add to Build Settings (File → Build Settings → Add Open Scenes)

#### 2. Setup Canvas
- Right-click Hierarchy → UI → Canvas
- Canvas settings:
  ```
  Render Mode: Screen Space - Overlay (easiest)
  // OR Screen Space - Camera (assign Main Camera)
  
  Canvas Scaler:
    UI Scale Mode: Scale With Screen Size
    Reference Resolution: 1920 x 1080
    Match: 0.5 (Width/Height balance)
  ```

#### 3. Create Title Text
- Right-click Canvas → UI → Text - TextMeshPro
- Nama: "TitleText"
- Settings:
  ```
  Text: "LAPRAKKKK" (atau judul game kamu)
  Font Size: 72-100
  Alignment: Center, Middle
  Color: White atau custom
  
  RectTransform:
    Anchor: Top Center
    Pos Y: -150
  ```

#### 4. Create Start Button
- Right-click Canvas → UI → Button - TextMeshPro
- Nama: "StartButton"
- Settings:
  ```
  Text: "START GAME"
  Font Size: 36
  
  RectTransform:
    Anchor: Middle Center
    Pos Y: -50
    Width: 300
    Height: 80
  ```
- **Add Component → Button Hover Effect**

#### 5. Create Quit Button
- Duplicate StartButton
- Nama: "QuitButton"
- Settings:
  ```
  Text: "QUIT"
  
  RectTransform:
    Pos Y: -150 (below start button)
  ```
- **Already has ButtonHoverEffect from duplication**

#### 6. Create MainMenuManager GameObject
- Right-click Hierarchy → Create Empty
- Nama: "MainMenuManager"
- Add Component → MainMenuManager script

#### 7. Configure MainMenuManager:
```
UI References:
  Start Button: [Drag StartButton]
  Quit Button: [Drag QuitButton]
  Title Text: [Drag TitleText]

Scene Settings:
  Intro Scene Name: "IntroTutorial"

Animation (Optional):
  Animate Title: ✅ (pulse effect)
  Title Pulse Speed: 1
  Title Pulse Amount: 0.1

Debug:
  Show Debug Logs: ✅
```

### Test Main Menu:
- Play scene
- Click Start → Should try to load IntroTutorial (will error if not created yet)
- Click Quit → Should exit play mode

---

## 📖 Scene 2: Intro & Tutorial

### Hierarchy Setup:

```
IntroTutorial Scene
├── Main Camera
├── EventSystem
├── Canvas
│   ├── DialogPanel
│   │   ├── DialogText (TextMeshPro)
│   │   ├── NextButton (Button)
│   │   └── SkipButton (Button)
│   └── Background (Image - optional)
└── DialogManager (Empty GameObject)
```

### Step-by-Step:

#### 1. Create Scene
- File → New Scene
- Save as "IntroTutorial"
- Add to Build Settings

#### 2. Setup Canvas (same as Main Menu)

#### 3. Create Dialog Panel (Optional)
- Right-click Canvas → UI → Panel
- Nama: "DialogPanel"
- Settings:
  ```
  Color: Semi-transparent (0, 0, 0, 200)
  RectTransform: Stretch (fill screen)
  ```

#### 4. Create Dialog Text
- Right-click DialogPanel → UI → Text - TextMeshPro
- Nama: "DialogText"
- Settings:
  ```
  Text: (will be set by script)
  Font Size: 32-48
  Alignment: Center, Middle
  Color: White
  
  RectTransform:
    Anchor: Middle Center
    Width: 1200
    Height: 600
  ```

#### 5. Create Next Button
- Right-click DialogPanel → UI → Button - TextMeshPro
- Nama: "NextButton"
- Settings:
  ```
  Text: "NEXT >"
  Font Size: 28
  
  RectTransform:
    Anchor: Bottom Right
    Pos X: -150
    Pos Y: 80
    Width: 200
    Height: 60
  ```
- **Add Component → Button Hover Effect**

#### 6. Create Skip Button
- Right-click DialogPanel → UI → Button - TextMeshPro
- Nama: "SkipButton"
- Settings:
  ```
  Text: "SKIP >>"
  Font Size: 24
  
  RectTransform:
    Anchor: Bottom Right
    Pos X: -150
    Pos Y: 20
    Width: 200
    Height: 50
  ```
- **Add Component → Button Hover Effect**

#### 7. Create DialogManager GameObject
- Create Empty → "DialogManager"
- Add Component → DialogSystem script

#### 8. Configure DialogSystem:
```
Dialog Content:
  Dialog Texts: (click + to add)
    [0]: "Selamat datang di LAPRAKKKK..."
    [1]: "Kamu terjebak di kampus malam hari..."
    [2]: "Hindari Dosen yang berkeliaran..."
    [3]: "Cari kunci untuk keluar..."
    [4]: "Gunakan WASD untuk bergerak..."
    [5]: "Shift untuk sprint..."
    [6]: "E untuk interact..."
    [7]: "Selamat bermain!"

UI References:
  Dialog Text: [Drag DialogText]
  Next Button: [Drag NextButton]
  Skip Button: [Drag SkipButton]
  Ending Buttons Panel: None (leave empty)

Scene Settings:
  Next Scene Name: "GameScene" (your main game scene)
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "GameScene"

Animation Settings:
  Use Typewriter Effect: ✅
  Typewriter Speed: 0.05

Debug:
  Show Debug Logs: ✅
```

### Test Intro:
- Play scene
- Click Next → Dialog advances
- Press Enter/Space → Same as Next
- Click Skip → Jump to end → Auto-load GameScene after 2s

---

## 🏆 Scene 3: Ending Win

### Hierarchy Setup:

```
EndingWin Scene
├── Main Camera
├── EventSystem
├── Canvas
│   ├── DialogPanel
│   │   ├── DialogText (TextMeshPro)
│   │   ├── NextButton (Button)
│   │   └── SkipButton (Button)
│   └── EndingButtonsPanel
│       ├── PlayAgainButton (Button)
│       └── MainMenuButton (Button)
└── DialogManager (Empty GameObject)
```

### Step-by-Step:

#### 1-6: Same as IntroTutorial setup

#### 7. Create EndingButtonsPanel
- Right-click Canvas → UI → Panel
- Nama: "EndingButtonsPanel"
- Settings:
  ```
  Color: Transparent (0, 0, 0, 0)
  RectTransform: Stretch
  Active: ✅ (checked - will be hidden by script)
  ```

#### 8. Create Play Again Button
- Right-click EndingButtonsPanel → UI → Button - TextMeshPro
- Nama: "PlayAgainButton"
- Settings:
  ```
  Text: "PLAY AGAIN"
  Font Size: 36
  
  RectTransform:
    Anchor: Middle Center
    Pos Y: -50
    Width: 300
    Height: 80
  ```
- **Add Component → Button Hover Effect**

#### 9. Create Main Menu Button
- Duplicate PlayAgainButton
- Nama: "MainMenuButton"
- Settings:
  ```
  Text: "MAIN MENU"
  
  RectTransform:
    Pos Y: -150
  ```
- **Already has ButtonHoverEffect from duplication**

#### 10. Configure DialogSystem:
```
Dialog Content:
  Dialog Texts:
    [0]: "Selamat! Kamu berhasil kabur!"
    [1]: "Dosen hampir menangkapmu..."
    [2]: "Tapi kamu lebih cepat!"
    [3]: "KAMU MENANG!"

UI References:
  Dialog Text: [Drag DialogText]
  Next Button: [Drag NextButton]
  Skip Button: [Drag SkipButton]
  Ending Buttons Panel: [Drag EndingButtonsPanel]
  Play Again Button: [Drag PlayAgainButton]
  Main Menu Button: [Drag MainMenuButton]

Scene Settings:
  Next Scene Name: (leave empty - uses buttons instead)
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "GameScene"

Animation Settings:
  Use Typewriter Effect: ✅
  Typewriter Speed: 0.05
```

### Test Ending Win:
- Play scene
- Next through dialogs
- After last dialog → EndingButtonsPanel appears
- Click Play Again → Reload GameScene
- Click Main Menu → Back to MainMenu

---

## 💀 Scene 4: Ending Lose

### Setup:

**IDENTIK dengan EndingWin!**

Just change dialog texts:

```
Dialog Content:
  Dialog Texts:
    [0]: "Kamu tertangkap oleh Dosen..."
    [1]: "Suara langkah semakin dekat..."
    [2]: "Terlalu lambat untuk kabur..."
    [3]: "GAME OVER"

// Everything else SAMA seperti EndingWin
```

**Quick Setup:**
1. Duplicate EndingWin scene
2. Save as "EndingLose"
3. Ubah dialog texts di DialogSystem
4. Done!

---

## 🔗 Scene Flow Integration

### GameManager Integration:

Update GameManager untuk load ending scenes:

```csharp
// In GameManager.cs

public void TriggerWin()
{
    SceneManager.LoadScene("EndingWin");
}

public void TriggerLose()
{
    SceneManager.LoadScene("EndingLose");
}
```

### Build Settings:

Add all scenes in order:
```
0. MainMenu
1. IntroTutorial
2. GameScene (your main game)
3. EndingWin
4. EndingLose
```

File → Build Settings → Add Open Scenes (untuk setiap scene)

---

## 🎨 Visual Polish (Optional)

### Background Images:

**MainMenu:**
```
Canvas → UI → Image
Name: "BackgroundImage"
Source Image: [Your menu background]
RectTransform: Stretch (fill screen)
Move to top of Canvas (render behind everything)
```

**Intro/Endings:**
```
DialogPanel → Background Image
Add: Gradient, Logo, atau screenshots
```

### Button Styling:

**Hover Effect (INCLUDED - ButtonHoverEffect.cs):**

All buttons should have this component!

```
Select Button GameObject
Add Component → Button Hover Effect

Scale Animation:
  Enable Scale Effect: ✅
  Hover Scale: 1.1  // 10% bigger on hover
  Pressed Scale: 0.95  // Slightly smaller on click
  Scale Speed: 10

Color Animation:
  Enable Color Effect: ✅
  Normal Color: White (255, 255, 255)
  Hover Color: Light Yellow (255, 255, 180)
  Pressed Color: Light Gray (200, 200, 200)

Audio:
  Hover Sound: [UI hover sound]
  Click Sound: [UI click sound]
  Volume: 1

References: (Auto-assigned, leave empty)
```

**Built-in Unity Transition (Alternative - Simpler):**
- Select Button → Transition: Color Tint
- Highlighted Color: Lighter color
- Pressed Color: Darker color
- **Note:** Not as smooth as ButtonHoverEffect!

**Custom Graphics:**
- Button → Image (Source Image): Custom button sprite
- TextMeshPro → Font Asset: Custom font

### Animations:

**Fade In:**
```csharp
// Add to DialogSystem Start():
CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
canvasGroup.alpha = 0;
LeanTween.alphaCanvas(canvasGroup, 1f, 1f);
```

**Button Animations:**
- Window → Animation
- Create animations for buttons (scale, color, etc.)

---

## 🎵 Audio Setup

### MainMenu Audio:
```
MainMenuManager GameObject:
  Add Component → Audio Source
  
MainMenuManager Script:
  Button Click Sound: [UI click sound]
  Start Game Sound: [Game start sound]
```

### Dialog Audio:
```
DialogManager GameObject:
  Add Component → Audio Source
  
DialogSystem Script:
  Dialog Sound: [Text appear sound]
  Button Click Sound: [UI click sound]
```

### Background Music (Optional):
```
Create Empty GameObject: "BGM"
Add Component → Audio Source:
  Audio Clip: [Background music]
  Play On Awake: ✅
  Loop: ✅
  Volume: 0.3
```

---

## ⌨️ Keyboard Controls

**Intro/Tutorial & Endings:**
- `Enter` or `Space` - Next dialog
- `Escape` - Skip all dialogs

**Main Menu:**
- No keyboard controls (button only)
- Can add: Up/Down arrows + Enter to select

---

## 🧪 Testing Checklist

### MainMenu:
- [ ] Title text animates (pulse)
- [ ] Start button → Load IntroTutorial
- [ ] Quit button → Exit game
- [ ] **Button hover effect works (scale + color)**
- [ ] Audio plays on button clicks

### IntroTutorial:
- [ ] Dialogs display in order
- [ ] Typewriter effect works
- [ ] Next button advances dialog
- [ ] Skip button jumps to end
- [ ] After last dialog → Auto-load GameScene
- [ ] Enter/Space works as Next
- [ ] Escape works as Skip

### EndingWin:
- [ ] Win dialogs display
- [ ] After last dialog → Ending buttons appear
- [ ] Play Again → Reload GameScene
- [ ] Main Menu → Back to MainMenu

### EndingLose:
- [ ] Lose dialogs display
- [ ] Ending buttons work same as Win

### Scene Flow:
- [ ] MainMenu → IntroTutorial → GameScene
- [ ] GameScene (win) → EndingWin
- [ ] GameScene (lose) → EndingLose
- [ ] EndingWin → MainMenu or Replay
- [ ] EndingLose → MainMenu or Replay

---

## 🛠️ Troubleshooting

### "Scene not found" error:
- Check scene names match exactly (case-sensitive)
- Verify scenes added to Build Settings
- Check spelling in script Inspector fields

### Buttons not working:
- Check EventSystem exists in scene
- Verify Button OnClick listeners assigned
- Check Button Interactable = ✅
- Check ButtonHoverEffect script attached

### Hover effect not working:
- Check ButtonHoverEffect component added to button
- Verify Enable Scale/Color Effect = ✅
- Check EventSystem exists in scene
- Test in Play mode (not Editor scene view)

### Dialog not appearing:
- Check Dialog Texts list not empty
- Verify DialogText reference assigned
- Check Canvas/Panel visible

### Auto-load not working:
- Check "Next Scene Name" field filled
- Verify scene added to Build Settings
- Check Console for errors

### Typewriter too fast/slow:
- Adjust "Typewriter Speed" (0.05 = default)
- Lower = slower, higher = faster

---

## 💡 Advanced Features

### Save/Load Progress:

```csharp
// In MainMenuManager:
void Start()
{
    if (PlayerPrefs.HasKey("HasSeenIntro"))
    {
        // Skip intro, load game directly
        introSceneName = "GameScene";
    }
}

// In DialogSystem (Intro end):
PlayerPrefs.SetInt("HasSeenIntro", 1);
```

### Multiple Endings:

```csharp
// Pass ending type to EndingWin scene:
public static string endingType = "good";

// In DialogSystem, load different dialogs:
if (endingType == "good")
{
    dialogTexts = goodEndingDialogs;
}
else if (endingType == "bad")
{
    dialogTexts = badEndingDialogs;
}
```

### Localization:

```csharp
// Use TextMeshPro localization or:
Dictionary<string, List<string>> localizedDialogs;
string currentLanguage = "EN";

void Start()
{
    dialogTexts = localizedDialogs[currentLanguage];
}
```

---

## 📐 Canvas Resolution Guide

### Common Resolutions:

```
Reference Resolution: 1920 x 1080 (Full HD)
Match: 0.5 (balanced)

For 16:9 games:
  1280 x 720 (HD)
  1920 x 1080 (Full HD)
  2560 x 1440 (2K)

For mobile:
  1080 x 1920 (portrait)
  Reference Resolution: 1080 x 1920
  Match: 0 (width priority)
```

### Safe Area (for mobile):

```csharp
// Adjust UI for notches/rounded corners:
RectTransform rectTransform = GetComponent<RectTransform>();
Rect safeArea = Screen.safeArea;
rectTransform.anchorMin = safeArea.position;
rectTransform.anchorMax = safeArea.position + safeArea.size;
```

---

## ✅ Summary

**Created:**
- ✅ MainMenuManager.cs - Main menu control
- ✅ DialogSystem.cs - Universal dialog system
- ✅ Complete setup guide for 4 scenes

**Scenes:**
1. MainMenu - Start/Quit
2. IntroTutorial - Dialog + Auto-load
3. EndingWin - Dialog + Buttons
4. EndingLose - Dialog + Buttons

**Camera & Light:**
- ✅ Main Camera REQUIRED (for Canvas)
- ❌ Directional Light NOT needed (UI only)

**Flow:**
MainMenu → Intro → Game → Ending (Win/Lose) → Replay/Menu

---

**Setup Complete!** 🎉

Tinggal buat UI visual design dan integrate dengan GameManager!
