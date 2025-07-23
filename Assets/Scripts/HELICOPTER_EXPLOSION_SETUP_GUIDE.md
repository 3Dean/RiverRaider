# Helicopter Explosion System - Complete Setup Guide

## Overview
This guide provides step-by-step instructions for setting up the realistic helicopter explosion system with physics-based shards in Unity.

## Phase 1: Prefab Setup (COMPLETED)
✅ You have already completed this phase by:
- Importing `enemyHelicopterExplode.fbx` 
- Adding Rigidbody and Collider components to all 51 shards
- Setting up physics materials and properties

## Phase 2: Scripts Created
✅ The following scripts have been created:

### Core Scripts:
1. **`HelicopterExplosion.cs`** - Main explosion controller
   - Location: `Assets/Scripts/Effects/HelicopterExplosion.cs`
   - Manages all 51 shards and explosion physics
   - Handles visual effects, audio, and cleanup

2. **`ExplosionShard.cs`** - Individual shard behavior
   - Location: `Assets/Scripts/Effects/ExplosionShard.cs`
   - Controls collision effects, lifetime, and physics interactions
   - Handles sparks, sounds, and terrain sticking

3. **`ExplosionForceCalculator.cs`** - Physics helper
   - Location: `Assets/Scripts/Effects/ExplosionForceCalculator.cs`
   - Provides realistic force calculations
   - Supports different explosion types and patterns

### Integration:
4. **`EnemyAI.cs`** - Updated with explosion integration
   - Location: `Assets/Scripts/EnemyAI.cs`
   - Now tries realistic explosion first, then fallbacks
   - Passes damage direction for directional forces

## Phase 3: Unity Setup Instructions

### Step 1: Create the Explosion Prefab
1. **Create Parent GameObject:**
   - In Hierarchy, create empty GameObject named "HelicopterExplosionPrefab"
   - Reset its Transform (position 0,0,0, rotation 0,0,0, scale 1,1,1)

2. **Add Main Script:**
   - Add `HelicopterExplosion.cs` component to "HelicopterExplosionPrefab"
   - Configure the explosion parameters in Inspector:
     - Base Explosion Force: 1000
     - Explosion Radius: 8
     - Upward Force Multiplier: 0.3
     - Directional Force Multiplier: 0.4
     - Randomness Multiplier: 0.25

3. **Add the FBX Model:**
   - Drag `enemyHelicopterExplode.fbx` from Assets/Models into the scene
   - Make it a child of "HelicopterExplosionPrefab"
   - Ensure all 51 shards have:
     - ✅ Rigidbody component
     - ✅ Collider component (MeshCollider or simplified)
     - ✅ Physics Material applied

4. **Configure Shards:**
   - The `HelicopterExplosion.cs` script will automatically:
     - Find all Rigidbody components in children
     - Add `ExplosionShard.cs` components if missing
     - Initialize each shard properly

### Step 2: Create Prefab Asset
1. **Save as Prefab:**
   - Drag "HelicopterExplosionPrefab" from Hierarchy to Assets/Prefabs/
   - This creates the prefab asset that can be instantiated

2. **Place in Resources (Optional):**
   - Create folder: Assets/Resources/ (if it doesn't exist)
   - Copy the prefab to Assets/Resources/HelicopterExplosionPrefab.prefab
   - This allows the EnemyAI script to find it automatically

### Step 3: Configure Audio and Visual Effects (Optional)
1. **Explosion Sounds:**
   - Assign explosion sound clips to the HelicopterExplosion component
   - Add metal collision sounds array for shard impacts
   - Add fire crackling sound for ambience

2. **Particle Effects:**
   - Create or assign explosion flash prefab
   - Create or assign fire and smoke prefab
   - Create or assign sparks prefab
   - These will enhance the visual impact

### Step 4: Test the System
1. **Scene Setup:**
   - Ensure you have a helicopter enemy in the scene
   - Ensure the helicopter has EnemyAI component with EnemyTypeData assigned
   - Ensure there's a player GameObject with "Player" tag

2. **Test Explosion:**
   - Play the scene
   - Damage the helicopter until it dies
   - The system should automatically:
     - Try to use the realistic explosion first
     - Fall back to assigned explosion effect if realistic one fails
     - Fall back to simple particle explosion as final fallback

## Phase 4: Troubleshooting

### Common Issues:

#### 1. "HelicopterExplosion could not be found" Error
**Solution:** Unity needs to recompile scripts
- Save all scripts
- Wait for Unity to finish compiling (check bottom-right progress bar)
- If error persists, restart Unity

#### 2. Explosion Prefab Not Found
**Solutions:**
- Ensure prefab is named exactly "HelicopterExplosionPrefab"
- Place prefab in Assets/Resources/ folder, OR
- Assign the prefab directly to EnemyTypeData.ExplosionEffect field

#### 3. Shards Don't Move
**Check:**
- All shards have Rigidbody components
- Rigidbodies are not set to Kinematic initially
- Mass values are reasonable (0.5-2.0)
- Explosion force values are sufficient (1000+)

#### 4. Performance Issues
**Solutions:**
- Reduce shard lifetime (default: 15 seconds)
- Enable distance cleanup (default: 100 units)
- Use simplified colliders (Box/Sphere instead of Mesh)
- Limit number of simultaneous explosions

#### 5. Shards Fall Through Terrain
**Solutions:**
- Ensure terrain has proper colliders
- Set terrain layer to "Terrain" or similar
- Adjust physics timestep in Project Settings > Time

## Phase 5: Customization Options

### Explosion Types:
```csharp
// In HelicopterExplosion.cs, you can use:
var settings = ExplosionForceCalculator.CreateExplosionSettings(ExplosionType.HighExplosive);
// Types: Standard, HighExplosive, Directional, Fragmentation
```

### Force Customization:
```csharp
// Adjust explosion parameters:
explosionComponent.SetExplosionParameters(1500f, 10f, 0.4f); // force, radius, upward bias
explosionComponent.SetDamageDirection(damageDirection); // directional force
```

### Visual Effects:
- Assign particle effect prefabs in HelicopterExplosion Inspector
- Create custom materials for shards
- Add fire trails to specific shards
- Customize ground scorch marks

## Expected Results

When properly set up, you should see:
- ✅ 51 individual helicopter pieces flying in realistic patterns
- ✅ Mixed explosion forces (radial + directional + random)
- ✅ Pieces bouncing off terrain and other objects
- ✅ Sparks and collision effects on impact
- ✅ Realistic physics with energy loss over time
- ✅ Automatic cleanup after 15 seconds
- ✅ Ground scorch marks
- ✅ Audio feedback for explosions and collisions

## Performance Metrics

**Target Performance:**
- Single explosion: <5ms impact on frame time
- 51 shards: ~2-3MB memory usage
- Cleanup: Automatic after 15 seconds
- Distance culling: Beyond 100 units from player

## Next Steps

1. **Test the basic explosion** - Verify all scripts compile and basic explosion works
2. **Add visual effects** - Create or assign particle effect prefabs
3. **Add audio** - Assign explosion and collision sound effects
4. **Fine-tune parameters** - Adjust forces, lifetime, and visual settings
5. **Performance optimization** - Test with multiple simultaneous explosions

The system is designed to work out-of-the-box with sensible defaults, but can be extensively customized for your specific game requirements.
