# Fuel Barge Collision Fix Guide

## Problem
Player ship passes through fuel barge instead of colliding with it. Refueling works (trigger), but no solid collision occurs.

## Root Cause
The fuel barge dual-collider system was designed but not fully configured in Unity. You need:
1. **One trigger collider** (for refueling) - "Is Trigger" = true
2. **One solid collider** (for crash damage) - "Is Trigger" = false  
3. **FuelBargeCollision script** attached to handle crash damage

## Quick Fix Steps

### Step 1: Check Current Fuel Barge Setup
1. **Open fuelbarge1 prefab** in Unity
2. **Look at colliders**:
   - Should have 2 Box Colliders
   - One larger (trigger for refueling)
   - One smaller (solid for collision)

### Step 2: Configure Colliders Properly
**Larger Collider (Refueling Zone)**:
- ✅ **Is Trigger**: true
- ✅ **Size**: ~1.66 x 3.27 x 3.08 (larger)
- ✅ **Position**: Higher (y: ~1.87)
- ✅ **Purpose**: Refueling detection

**Smaller Collider (Solid Structure)**:
- ❌ **Is Trigger**: false (THIS IS KEY!)
- ✅ **Size**: ~1.66 x 1.12 x 3.08 (smaller)
- ✅ **Position**: Lower (y: ~-0.36)
- ✅ **Purpose**: Crash damage

### Step 3: Add Missing Script
1. **Select fuelbarge1 prefab**
2. **Add Component** → **FuelBargeCollision**
3. **Configure settings**:
   - Crash Damage: 35
   - Enable Bullet Damage: true

### Step 4: Test
1. **Fly into fuel barge**
2. **Expected results**:
   - Refueling starts (trigger working)
   - Player takes damage and bounces off (solid collision working)
   - Console shows: "COLLISION-BASED DAMAGE: Player crashed into fuel barge!"

## Visual Verification

### In Scene View:
- **Green wireframe** = Trigger collider (larger, higher)
- **Blue wireframe** = Solid collider (smaller, lower)
- **Both visible** = Dual-collider system active

### In Inspector:
```
fuelbarge1
├── Box Collider (Trigger)
│   ├── Is Trigger: ✅ true
│   ├── Size: 1.66, 3.27, 3.08
│   └── Center: 0, 1.87, 0
├── Box Collider (Solid)  
│   ├── Is Trigger: ❌ false
│   ├── Size: 1.66, 1.12, 3.08
│   └── Center: 0, -0.36, 0
└── FuelBargeCollision (Script)
    ├── Crash Damage: 35
    └── Enable Bullet Damage: true
```

## Common Issues & Solutions

### Issue: Still passing through
**Check**: Both colliders set as triggers
**Fix**: Set smaller collider "Is Trigger" = false

### Issue: No damage on collision
**Check**: FuelBargeCollision script missing
**Fix**: Add FuelBargeCollision component

### Issue: Can't refuel
**Check**: Larger collider not set as trigger
**Fix**: Set larger collider "Is Trigger" = true

### Issue: Refueling but no collision
**Check**: Only trigger collider exists
**Fix**: Add second collider with "Is Trigger" = false

## Expected Console Output

### Successful Setup:
```
Refueling started at fuel barge
COLLISION-BASED DAMAGE: Player crashed into fuel barge! Took 35.0 damage
PlayerShipHealth: Health reduced to 65/100
```

### Problem Indicators:
```
Refueling started at fuel barge
(No collision damage message = solid collider not working)
```

## Advanced Configuration

### Damage Scaling:
- **Low speed crash**: ~35 damage
- **High speed crash**: ~70 damage (scales with impact force)

### Visual Effects:
- **Crash Effect**: Assign explosion prefab
- **Bullet Hit Effect**: Assign impact prefab
- **Audio**: Assign crash/hit sound clips

## Physics Layer Setup (Optional)

For bullet filtering (bullets pass through refuel zone):
1. **Create layers**:
   - FuelTrigger (Layer 6)
   - Bullet (Layer 7)
2. **Set trigger collider** to FuelTrigger layer
3. **Configure collision matrix**: Bullet vs FuelTrigger = unchecked

## Testing Checklist

- [ ] Player can refuel (trigger works)
- [ ] Player takes damage on collision (solid works)  
- [ ] Player bounces off barge (physics works)
- [ ] Console shows collision damage message
- [ ] Health bar decreases on impact
- [ ] Both refueling and collision work simultaneously

## Files to Check
- `Assets/Prefabs/fuelbarge1.prefab` - Main prefab configuration
- `Assets/Scripts/FuelBargeCollision.cs` - Collision damage script
- `Assets/Scripts/PlayerShipHealth.cs` - Receives damage
- `Assets/Scripts/PlayerShipFuel.cs` - Handles refueling

Your dual-collider system is designed perfectly - it just needs the Unity configuration completed!
