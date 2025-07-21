# Dual-Collider Fuel Barge Setup Guide

## Overview
This guide explains how to set up the fuelbarge1 prefab with dual colliders for both refueling and collision damage functionality, while preventing bullets from hitting the refueling trigger zone.

## System Components

### 1. FuelBargeCollision.cs Script
- **Purpose**: Handles collision damage for the solid collider
- **Features**: 
  - Player crash damage with impact force scaling
  - Bullet hit detection (solid collider only)
  - Visual and audio effects support
  - Configurable damage values

### 2. Physics Layers
- **FuelTrigger Layer (Layer 6)**: For refueling trigger collider
- **Bullet Layer (Layer 7)**: For bullet objects
- **Default Layer**: For solid collider and player

## Setup Instructions

### Step 1: Configure the Fuel Barge Prefab

1. **Open the fuelbarge1 prefab** in the Unity editor
2. **Add the FuelBargeCollision script** to the main GameObject
3. **Configure the two colliders**:

   **Trigger Collider (Refueling Zone)**:
   - Set Layer to "FuelTrigger" (Layer 6)
   - Keep "Is Trigger" checked
   - Size: Larger area for easy refueling access
   - This collider handles fuel refilling via PlayerShipFuel.cs

   **Solid Collider (Crash Zone)**:
   - Keep on "Default" layer (Layer 0)
   - Uncheck "Is Trigger"
   - Size: Smaller, representing the solid hull
   - This collider handles crashes via FuelBargeCollision.cs

### Step 2: Configure Physics Layer Collision Matrix

1. **Open Project Settings > Physics**
2. **Expand the Layer Collision Matrix**
3. **Uncheck the intersection** between:
   - "Bullet" layer and "FuelTrigger" layer
   - This prevents bullets from hitting the refueling trigger

### Step 3: Configure Bullet Objects

1. **Set all bullet prefabs** to the "Bullet" layer (Layer 7)
2. **Update Bullet.cs script** if needed to respect layer masks
3. **Verify bullets can still hit**:
   - Player (Default layer)
   - Enemies (Default layer)  
   - Solid colliders (Default layer)
   - But NOT trigger colliders (FuelTrigger layer)

### Step 4: Configure FuelBargeCollision Settings

In the Inspector for fuelbarge1:

**Collision Damage**:
- `Crash Damage`: 35f (adjust as needed)
- `Enable Bullet Damage`: true

**Visual Effects** (Optional):
- `Crash Effect`: Assign explosion/sparks prefab
- `Bullet Hit Effect`: Assign bullet impact prefab

**Audio** (Optional):
- `Crash Sound`: Assign crash audio clip
- `Bullet Hit Sound`: Assign bullet impact audio clip

## How It Works

### Refueling Process
1. Player enters trigger collider (FuelTrigger layer)
2. `PlayerShipFuel.OnTriggerEnter()` detects "FuelBarge" tag
3. Refueling begins automatically
4. Bullets pass through trigger (no collision due to layer matrix)

### Collision Damage Process
1. Player hits solid collider (Default layer)
2. `FuelBargeCollision.OnCollisionEnter()` detects "Player" tag
3. Damage calculated based on impact force
4. Visual/audio effects triggered
5. Player health reduced via PlayerShipHealth.TakeDamage()

### Bullet Interactions
1. Bullets hit solid collider (Default layer) → Impact effects
2. Bullets pass through trigger collider (FuelTrigger layer) → No interaction

## Testing Checklist

- [ ] Player can refuel when near the barge
- [ ] Player takes damage when crashing into solid part
- [ ] Bullets hit the solid collider and create effects
- [ ] Bullets pass through the refueling trigger zone
- [ ] Refueling works while bullets are passing through
- [ ] Visual and audio effects work correctly

## Troubleshooting

### Bullets Still Hit Trigger Collider
- Check Layer Collision Matrix in Project Settings > Physics
- Ensure bullets are on "Bullet" layer
- Verify trigger collider is on "FuelTrigger" layer

### Player Not Taking Crash Damage
- Ensure FuelBargeCollision script is attached
- Check that solid collider is NOT a trigger
- Verify player has PlayerShipHealth component

### Refueling Not Working
- Check that trigger collider "Is Trigger" is enabled
- Verify fuelbarge1 has "FuelBarge" tag
- Ensure PlayerShipFuel script is on player

### No Visual/Audio Effects
- Assign effect prefabs in FuelBargeCollision inspector
- Check that AudioSource component exists
- Verify effect prefabs are properly configured

## Advanced Configuration

### Speed-Based Damage
The crash damage automatically scales with impact force:
```csharp
if (impactForce > 10f)
{
    finalDamage *= Mathf.Clamp(impactForce / 20f, 1f, 2f);
}
```

### Custom Damage Values
Use the public methods to adjust damage at runtime:
```csharp
fuelBarge.GetComponent<FuelBargeCollision>().SetCrashDamage(50f);
```

### Barge Destruction
Add barge health system by extending FuelBargeCollision:
- Track barge health
- Handle bullet damage accumulation
- Destroy/disable barge when health reaches zero

## Integration Notes

- Works seamlessly with existing PlayerShipFuel system
- Compatible with PlayerShipHealth damage system
- Uses centralized FlightData for health management
- Supports existing bullet and weapon systems
- No conflicts with other collision systems

## Performance Considerations

- Minimal performance impact (only collision events)
- Layer-based filtering reduces unnecessary collision checks
- Optional effects can be disabled for better performance
- Audio clips use PlayOneShot for efficiency
