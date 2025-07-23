# Helicopter Explosion Freezing Issue - FIXED

## Problem Description
The helicopter explosion pieces were initially separating and flying apart, but then freezing in mid-air instead of falling naturally with gravity. This created an unrealistic effect where debris would hang suspended in space.

## Root Cause Analysis
The issue was caused by the `CheckForSticking()` method in the `ExplosionShard` script, which was making rigidbodies kinematic (freezing them) too early when their velocity dropped below the `stickingVelocityThreshold`.

### Original Problems:
1. **High sticking threshold** (1.0f) - Pieces would freeze while still moving at reasonable speeds
2. **No minimum fall time** - Pieces could stick immediately after explosion
3. **No collision requirement** - Pieces could stick while still in mid-air
4. **Missing physics settings** - Gravity and drag weren't explicitly set

## Solution Implemented

### 1. Reduced Sticking Velocity Threshold
```csharp
// OLD: 1.0f (too high)
[SerializeField] private float stickingVelocityThreshold = 0.2f; // Much lower threshold
```

### 2. Added Minimum Fall Time
```csharp
[SerializeField] private float minimumFallTime = 2f; // Don't stick until after falling for a while
```

### 3. Enhanced Sticking Logic
```csharp
private void CheckForSticking()
{
    // Don't allow sticking until minimum fall time has passed
    float timeSinceActivation = Time.time - activationTime;
    if (timeSinceActivation < minimumFallTime)
    {
        return;
    }

    // Only stick if moving VERY slowly AND has had collisions (indicating it's on ground)
    if (velocity < stickingVelocityThreshold && angularVelocity < 0.5f && collisionCount > 0)
    {
        StickToSurface();
    }
}
```

### 4. Forced Physics Settings
```csharp
// CRITICAL FIX: Ensure proper physics settings
shard.Rigidbody.useGravity = true;
shard.Rigidbody.drag = 0.1f; // Low air resistance
shard.Rigidbody.angularDrag = 0.5f; // Some rotational damping
```

## Key Improvements

### Physics Behavior
- **Gravity explicitly enabled** - Ensures pieces fall naturally
- **Low drag values** - Prevents air resistance from stopping pieces mid-flight
- **Proper angular damping** - Realistic rotational physics

### Sticking Logic
- **2-second minimum fall time** - Pieces must fall for at least 2 seconds before they can stick
- **Collision requirement** - Pieces can only stick after hitting something (ground/terrain)
- **Much lower velocity threshold** - Only stick when nearly motionless (0.2f instead of 1.0f)

### Expected Results
1. **Pieces fly apart dramatically** with 8x stronger explosion forces
2. **Fall naturally with gravity** for at least 2 seconds
3. **Bounce and tumble realistically** when hitting terrain
4. **Only stick when truly at rest** and on the ground
5. **No more mid-air freezing** - pieces continue falling until they hit something

## Testing
- **Press T** with ExplosionManualTester for immediate testing
- **Destroy helicopters in-game** to see realistic falling debris
- **Watch console logs** for detailed physics debugging information

The explosion system now provides realistic physics behavior where pieces fly apart, fall with gravity, bounce off terrain, and only stick when they've truly come to rest on the ground.
