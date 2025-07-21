# Fuel Barge Dual Collider Setup Instructions

## Current Issue
Your fuel barge currently has:
- ✅ **1 Trigger Collider** (refueling zone) - Working correctly
- ✅ **1 Solid Collider** (physical collision) - Prevents plane from passing through
- ❌ **Missing: Small Damage Trigger** - No crash damage zone

## Required Setup

You need to add a **second trigger collider** for the damage zone.

### Step 1: Open the Fuel Barge Prefab
1. In Unity, navigate to `Assets/Prefabs/fuelbarge1.prefab`
2. Double-click to open the prefab for editing
3. Select the main `fuelbarge1` GameObject

### Step 2: Add Second Box Collider
1. In the Inspector, click **"Add Component"**
2. Search for **"Box Collider"**
3. Add another Box Collider component

### Step 3: Configure the New Damage Collider
Set the new Box Collider properties:

**✅ Check "Is Trigger"** (very important!)

**Size Settings:**
- X: `1.0` (smaller than refuel zone)
- Y: `1.5` (smaller than refuel zone) 
- Z: `2.0` (smaller than refuel zone)

**Center Settings:**
- X: `0`
- Y: `0.5` (lower than refuel zone)
- Z: `0`

### Step 4: Verify Your Setup
After adding the second collider, you should have:

```
fuelbarge1 (GameObject)
├── Box Collider #1 (IsTrigger: true)  - REFUEL ZONE (Large)
│   └── Size: (1.66, 3.27, 3.08)
│   └── Center: (0, 1.87, 0)
├── Box Collider #2 (IsTrigger: true)  - DAMAGE ZONE (Small) ← NEW
│   └── Size: (1.0, 1.5, 2.0)
│   └── Center: (0, 0.5, 0)
└── Box Collider #3 (IsTrigger: false) - SOLID COLLISION
    └── Size: (1.66, 1.12, 3.08)
    └── Center: (0, -0.36, 0)
```

### Step 5: Test the System
1. **Save the prefab** (Ctrl+S)
2. **Play the game**
3. **Fly into the large area** - should refuel safely
4. **Fly into the small/lower area** - should take 35 damage

### Expected Console Output
After setup, you should see:
- `"Found 2 trigger colliders total"`
- `"Collider 1 volume: X.XX, Collider 2 volume: Y.YY"`
- `"Current collider identified as DAMAGE collider"` (for small one)
- `"Current collider identified as REFUEL collider"` (for large one)

## Troubleshooting

### If you still see "Found 1 trigger colliders total":
- Make sure you checked **"Is Trigger"** on the new Box Collider
- Save the prefab and restart Play mode

### If damage is applied in the wrong zone:
- The script automatically identifies the smaller collider as the damage zone
- Make sure your new collider is smaller than the existing refuel collider

### If the plane still passes through:
- The solid collider (IsTrigger: false) should prevent this
- Make sure the solid collider is properly positioned and sized

## Visual Layout
```
     [Large Refuel Trigger - Safe Zone]
           [Small Damage Trigger]
              [Solid Collider]
                 [Barge Mesh]
```

The player should be able to refuel safely in the large trigger area while avoiding the smaller damage trigger area near the barge structure.
