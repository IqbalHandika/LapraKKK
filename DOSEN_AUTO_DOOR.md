# Dosen Auto-Open Locked Doors System

Sistem untuk Dosen membuka pintu terkunci (single & double door) secara otomatis dan auto-close setelah pergi.

## 🎯 Fitur

1. **Dosen bisa buka pintu terkunci** - Bypass key requirement
2. **Support Single Door & Double Door** - Kedua jenis pintu
3. **Auto-close setelah Dosen pergi** - Pintu tutup dan terkunci lagi
4. **Player protection** - Jika player sudah buka pintu, TIDAK akan auto-close
5. **Smart detection** - Track siapa yang buka pintu (Dosen vs Player)

---

## 📋 Cara Kerja

### Scenario 1: Dosen Membuka Pintu Terkunci

```
1. Dosen mendekati pintu terkunci (locked + key required)
2. Door detect Dosen dalam radius (enemy detection)
3. Door.DosenRequestOpen() - Bypass lock, buka pintu
4. Dosen masuk ke kelas
5. Dosen keluar dan pergi (out of detection radius)
6. Auto-close timer start (default 2 detik)
7. Pintu tertutup dan terkunci lagi
```

**Result:** Pintu kembali ke state locked, seolah tidak pernah dibuka.

### Scenario 2: Player Sudah Buka Pintu Dulu

```
1. Player unlock pintu dengan key
2. Player buka pintu (wasOpenedByPlayer = true)
3. Dosen datang dan masuk
4. Dosen keluar dan pergi
5. Check: wasOpenedByPlayer == true
6. SKIP auto-close - pintu tetap terbuka!
```

**Result:** Pintu tetap terbuka karena player yang buka.

### Scenario 3: Dosen Buka, Lalu Player Buka

```
1. Dosen buka pintu terkunci (dosenRequestedOpen = true)
2. Player datang dan interact dengan pintu
3. wasOpenedByPlayer = true (override Dosen)
4. Auto-close timer di-cancel
5. Dosen pergi - pintu TIDAK auto-close
```

**Result:** Player action override Dosen action.

---

## ⚙️ Setup di Inspector

### Single Door Component Settings:

```
Lock Settings:
  Is Locked: ✅ (checked)
  Required Key ID: "classroom_key"
  Door Name: "Classroom Door"

Dosen Auto-Open/Close:
  Dosen Can Open Locked: ✅ (checked)
  Auto Close After Dosen: ✅ (checked)
  Auto Close Delay: 2  // Seconds after Dosen leaves

Enemy Detection (Auto-Open):
  Enable Enemy Detection: ✅ (checked)
  Detection Radius: 3
  Enemy Layer: Enemy (layer mask)
  Check Interval: 0.2

Debug:
  Show Debug Logs: ✅ (untuk testing)
```

### Double Door Component Settings:

```
Door References:
  Left Door: [Drag left door Transform]
  Right Door: [Drag right door Transform]

Lock Settings:
  Is Locked: ✅ (checked)
  Required Key ID: "main_hall_key"
  Door Name: "Main Hall Double Door"

Dosen Auto-Open/Close:
  Dosen Can Open Locked: ✅ (checked)
  Auto Close After Dosen: ✅ (checked)
  Auto Close Delay: 2

Enemy Detection (Auto-Open):
  Enable Enemy Detection: ✅ (checked)
  Detection Radius: 3
  Enemy Layer: Enemy

Debug:
  Show Debug Logs: ✅
```

**Note:** Double door settings identik dengan single door, sistem kerja sama persis!

---

## 🔍 Debug Console Output

### Dosen Opens Locked Door:
```
[Door] 'Classroom Door' detected enemy 'Dosen' - Opening door
[Door] Dosen opening 'Classroom Door' (locked: True)
[Door] 'Classroom Door' is now OPEN (walkable)

// OR for double door:
[DoubleDoor] 'Main Hall' detected enemy 'Dosen' - Opening doors
[DoubleDoor] Dosen opening 'Main Hall' (locked: True)
[DoubleDoor] Both doors opening...
```

### Dosen Leaves (Auto-Close):
```
[Door] Dosen left - auto-closing 'Classroom Door' in 2s
[Door] Auto-closing and locking 'Classroom Door'
[Door] 'Classroom Door' is now CLOSED (unwalkable)
```

### Player Opens Door (Prevents Auto-Close):
```
[PlayerInteraction] Interacting with: Classroom Door
[Door] 'Classroom Door' unlocked with key 'classroom_key'!
[Door] Player opened - auto-close cancelled
[Door] 'Classroom Door' is now OPEN (walkable)
```

---

## 🧪 Testing Checklist

### Test 1: Dosen Auto-Open Locked Door
- [ ] Pintu terkunci (player tidak punya key)
- [ ] Dosen mendekati pintu
- [ ] Pintu terbuka otomatis untuk Dosen
- [ ] Dosen masuk kelas
- [ ] Console: "Dosen opening 'Door' (locked: True)"

### Test 2: Auto-Close After Dosen Leaves
- [ ] Dosen buka pintu terkunci
- [ ] Dosen masuk dan eksplorasi kelas
- [ ] Dosen keluar dan pergi jauh
- [ ] Wait 2 detik
- [ ] Pintu tertutup otomatis
- [ ] Pintu kembali terkunci (player tidak bisa buka tanpa key)
- [ ] Console: "Auto-closing and locking 'Door'"

### Test 3: Player Opens - No Auto-Close
- [ ] Player unlock dan buka pintu dengan key
- [ ] Dosen datang dan masuk
- [ ] Dosen keluar dan pergi
- [ ] Wait 5+ detik
- [ ] Pintu TETAP TERBUKA (no auto-close)
- [ ] Console: "Player opened - auto-close cancelled"

### Test 4: Dosen Opens, Player Overrides
- [ ] Dosen buka pintu terkunci (auto-close scheduled)
- [ ] Player datang dan interact pintu
- [ ] Player buka/tutup pintu manually
- [ ] Dosen pergi
- [ ] Pintu TIDAK auto-close (player override)

---

## 🛠️ Troubleshooting

### Dosen tidak bisa buka pintu terkunci:
- Check "Dosen Can Open Locked" = ✅
- Check "Enable Enemy Detection" = ✅
- Check Dosen punya tag "Enemy"
- Check "Enemy Layer" mask include Dosen layer
- Check Detection Radius cukup besar (min 3)

### Pintu tidak auto-close setelah Dosen pergi:
- Check "Auto Close After Dosen" = ✅
- Check Console untuk "wasOpenedByPlayer" status
- Pastikan player BELUM interact dengan pintu
- Check Auto Close Delay (coba naikkan ke 5 detik)

### Pintu auto-close meskipun player sudah buka:
- **BUG!** Check code - wasOpenedByPlayer harus set di Interact()
- Pastikan player pakai Interact() method (bukan DosenRequestOpen)
- Check Console log untuk track siapa yang buka

### Auto-close terlalu cepat (Dosen masih di dalam):
- Naikkan Auto Close Delay (misal 5 detik)
- Check Detection Radius - mungkin terlalu kecil
- Dosen keluar dari radius = trigger auto-close

---

## 💡 Advanced Tips

### 1. Adjust Auto-Close Timing

**Fast Close (Secure Room):**
```csharp
Auto Close Delay: 1  // 1 second after Dosen leaves
// Pintu cepat tertutup, ruangan secure lagi
```

**Delayed Close (Grace Period):**
```csharp
Auto Close Delay: 5  // 5 seconds
// Kasih waktu untuk Dosen keluar sepenuhnya
```

### 2. Detection Radius Tuning

**Tight Detection (Door Only):**
```csharp
Detection Radius: 2
Detection Center Offset: (0, 0, 1)  // Slightly in front of door
// Dosen harus sangat dekat untuk trigger
```

**Wide Detection (Room Entry):**
```csharp
Detection Radius: 5
// Deteksi Dosen saat masih di koridor
```

### 3. Multiple Classroom Support

Setup beberapa kelas dengan sistem yang sama:

```
Classroom_A_Door (Single):
  Required Key ID: "classroom_a_key"
  Dosen Can Open Locked: ✅
  Auto Close After Dosen: ✅

Classroom_B_Door (Double):
  Left Door: [Left Transform]
  Right Door: [Right Transform]
  Required Key ID: "classroom_b_key"
  Dosen Can Open Locked: ✅
  Auto Close After Dosen: ✅

Main_Hall_Door (Double):
  Required Key ID: "main_hall_key"
  Dosen Can Open Locked: ✅
  Auto Close After Dosen: ✅
```

Setiap pintu track sendiri-sendiri (independent), termasuk mix single & double door!

### 4. One-Way Doors

Untuk pintu yang hanya auto-close dari satu sisi:

```csharp
Detection Center Offset: (0, 0, -1.5)  // Detect behind door
// Hanya detect Dosen saat DI DALAM kelas
// Tidak detect saat mendekati dari luar
```

### 5. Audio Feedback

Tambahkan audio untuk Dosen opening locked door:

```csharp
// Di DosenRequestOpen():
if (audioSource != null && unlockSound != null)
{
    audioSource.PlayOneShot(unlockSound);
}
```

---

## 🎮 Gameplay Flow Example

**Player Experience:**

```
Player: "Aku butuh masuk kelas A untuk ambil item"
Player: *Cari key untuk classroom A*
Player: *Unlock dan buka pintu*
Player: *Masuk dan eksplorasi*

Dosen: *Patrol lewat koridor*
Dosen: *Detect player di kelas A*
Dosen: *Buka pintu kelas A (yang player sudah buka)*
Dosen: *Masuk dan chase player*

Player: *Kabur dari kelas*
Dosen: *Keluar dari kelas dan patrol lagi*

Result: Pintu kelas A TETAP TERBUKA (player yang buka)
```

**Alternative Flow:**

```
Player: *Sembunyi di koridor, belum unlock kelas B*

Dosen: *Patrol dan mendekati kelas B (locked)*
Dosen: *Auto-open kelas B (bypass lock)*
Dosen: *Masuk dan eksplorasi*
Dosen: *Keluar dan lanjut patrol*

Auto-Close: *2 detik delay*
Pintu kelas B: *Tertutup dan terkunci lagi*

Player: "Wah, Dosen bisa buka pintu terkunci!"
Player: "Tapi pintunya udah tertutup lagi, aku masih butuh key"
```

---

## 📊 State Tracking

Door internal states:

| State | Description | Auto-Close? |
|-------|-------------|-------------|
| `isLocked = true` | Pintu terkunci, butuh key | N/A |
| `isUnlocked = true` | Player sudah unlock dengan key | No |
| `dosenRequestedOpen = true` | Dosen yang buka (bypass lock) | Yes |
| `wasOpenedByPlayer = true` | Player pernah interact | No |
| `autoCloseCoroutine != null` | Timer sedang jalan | Pending |

**Priority:**
- `wasOpenedByPlayer` > `dosenRequestedOpen`
- Player action always overrides Dosen action

---

## 🔒 Security Features

1. **Double-Check Before Close:**
   ```csharp
   // Only close if:
   // - Door is open
   // - Player hasn't opened it
   // - Dosen was the one who opened it
   if (isOpen && !wasOpenedByPlayer && dosenRequestedOpen)
   ```

2. **Cancel Timer on Player Interact:**
   ```csharp
   if (autoCloseCoroutine != null)
   {
       StopCoroutine(autoCloseCoroutine);
       autoCloseCoroutine = null;
   }
   ```

3. **Locked State Preserved:**
   - `isLocked` never changes (always true)
   - `isUnlocked` only true if player used key
   - Dosen opening doesn't unlock door permanently

---

## ✅ Summary

**What This System Does:**
- ✅ Dosen bisa buka pintu terkunci (special ability)
- ✅ Support Single Door & Double Door
- ✅ Auto-close setelah Dosen pergi (restore locked state)
- ✅ Player-opened doors NEVER auto-close (safe)
- ✅ Smart detection untuk track siapa yang buka
- ✅ Independent per-door (multi-room support)

**What This System Prevents:**
- ❌ Pintu terkunci jika player sudah buka
- ❌ Auto-close saat Dosen masih di dalam
- ❌ Conflict antara player dan Dosen actions
- ❌ Pintu stuck terbuka selamanya (jika Dosen yang buka)

**Perfect For:**
- Locked classroom scenarios (single & double doors)
- Dosen patrol dengan akses khusus
- Dynamic room access
- Horror game dengan smart AI

**Scripts Modified:**
- ✅ Door.cs - Single door support
- ✅ DoubleDoor.cs - Double door support (identical logic)

---

**Setup Complete!** 🎉

Dosen sekarang punya akses khusus ke ruangan terkunci, tapi player control tetap prioritas!
