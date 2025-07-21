# Collision Bounce System Setup Guide

## Overview
The **CollisionBounceController** adds realistic collision detection and bounce-back effects to your aircraft without modifying the core UnifiedFlightController. This solves the "ghosting through fuel barges" issue while maintaining all existing flight behavior.

## ✅ What This System Does

### **Collision Detection**
- Detects solid colliders ahead of the aircraft using sphere collision detection
- Only triggers on **non-trigger colliders** (solid objects)
- Ignores trigger colliders (so fuel/damage systems still work normally)
- Has built-in cooldown to prevent rapid collision spam

### **Bounce-Back Effect**
- Calculates realistic bounce direction away from obstacles
- Applies immediate bounce force to push aircraft away
- Gradually reduces bounce intensity over time
- Reduces aircraft speed on impact (realistic collision physics)

### **Damage Integration**
- Automatically applies collision damage when hitting fuel barges
- Works with existing PlayerShipHealth system
- Maintains compatibility with FuelBargeCollision scripts

## 🔧 Setup Instructions

### **Step 1: Add Component to Player Aircraft**

1. **Select your player aircraft** in the hierarchy (the object with UnifiedFlightController)
2. **Add Component** → Search for "CollisionBounceController"
3. **Click Add** - The component will automatically detect required dependencies

### **Step 2: Configure Settings**

#### **Collision Detection Settings:**
- **Detection Distance**: `2.0` - How far ahead to check for obstacles
- **Collider Radius**: `1.0` - Size of collision detection sphere
- **Obstacle Layer Mask**: `Everything` - What layers to consider obstacles

#### **Bounce Settings:**
- **Bounce Force**: `10.0` - How strong the bounce-back effect is
- **Bounce Duration**: `0.5` - How long the bounce effect lasts (seconds)
- **Speed Reduction**: `0.7` - Speed multiplier after collision (0.7 = 30% speed loss)

#### **Debug Settings:**
- **Show Debug Rays**: `✓` - Shows collision detection sphere in Scene view
- **Enable Debug Logging**: `✓` - Logs collision events to console

### **Step 3: Test the System**

1. **Enter Play Mode**
2. **Fly toward a fuel barge**
3. **Observe the behavior:**
   - Yellow sphere shows collision detection area
   - Red sphere indicates active collision
   - Aircraft should bounce away from fuel barge
   - Speed should reduce on impact
   - Damage should be applied

## 🎛️ Tuning Parameters

### **For Gentle Bounces:**
```
Bounce Force: 5.0
Bounce Duration: 0.3
Speed Reduction: 0.9 (10% speed loss)
```

### **For Strong Bounces:**
```
Bounce Force: 20.0
Bounce Duration: 0.8
Speed Reduction: 0.5 (50% speed loss)
```

### **For Realistic Aircraft Collision:**
```
Bounce Force: 15.0
Bounce Duration: 0.6
Speed Reduction: 0.6 (40% speed loss)
```

## 🔍 Visual Debug Features

### **Scene View Gizmos:**
- **Blue Ray**: Forward direction and detection distance
- **Yellow Sphere**: Normal collision detection area
- **Red Sphere**: Active collision detected
- **Red Ray**: Current bounce direction (when bouncing)

### **Console Logging:**
- Collision detection events
- Bounce trigger notifications
- Damage application confirmations
- Bounce completion messages

## 🛠️ Advanced Configuration

### **Layer Mask Setup:**
If you want to exclude certain objects from collision:

1. **Create a new layer** (e.g., "Obstacles")
2. **Assign fuel barges** to this layer
3. **Set Obstacle Layer Mask** to only include "Obstacles" layer

### **Custom Collision Behavior:**
The system provides public methods for external control:

```csharp
// Check if currently bouncing
bool isBouncing = collisionBounceController.IsBouncing();

// Get current bounce direction
Vector3 bounceDir = collisionBounceController.GetBounceDirection();

// Manually trigger bounce
collisionBounceController.TriggerManualBounce(Vector3.back, 15f);
```

## 🚨 Troubleshooting

### **"No collision detected"**
- Check that fuel barge has **non-trigger colliders**
- Verify **Detection Distance** is appropriate for your aircraft speed
- Ensure **Collider Radius** covers the aircraft's width

### **"Bouncing too much/too little"**
- Adjust **Bounce Force** for intensity
- Modify **Bounce Duration** for length of effect
- Change **Speed Reduction** for impact severity

### **"Rapid collision spam"**
- The system has built-in **collision cooldown** (1 second)
- This prevents multiple bounces from the same obstacle
- Cooldown resets when moving away from obstacles

### **"Damage not applying"**
- Ensure your aircraft has **PlayerShipHealth** component
- Verify fuel barge name contains "fuelbarge" (case-sensitive)
- Check that **FuelBargeCollision** script is on the fuel barge

## ✅ System Benefits

### **Non-Invasive Design:**
- ✅ Zero changes to UnifiedFlightController
- ✅ Zero changes to existing flight behavior
- ✅ Can be easily disabled/removed if needed
- ✅ Works alongside all existing systems

### **Realistic Physics:**
- ✅ Proper collision detection for kinematic movement
- ✅ Realistic bounce-back effects
- ✅ Speed reduction on impact
- ✅ Gradual recovery from collisions

### **Game Integration:**
- ✅ Maintains trigger-based fuel/damage systems
- ✅ Applies collision damage automatically
- ✅ Visual feedback with debug gizmos
- ✅ Comprehensive logging for debugging

## 🎮 Expected Behavior

1. **Approach fuel barge** - Yellow detection sphere visible
2. **Contact detected** - Sphere turns red, bounce triggers
3. **Bounce-back effect** - Aircraft pushed away from obstacle
4. **Speed reduction** - Aircraft slows down from impact
5. **Damage applied** - Health reduced from collision
6. **Recovery** - Normal flight resumes after bounce duration

The system provides the realistic collision behavior you wanted while keeping all existing systems intact!
