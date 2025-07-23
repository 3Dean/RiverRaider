# Helicopter Explosion Debug Guide

## Current Issue
The helicopter explosion system isn't working as expected - helicopters aren't breaking apart into individual shards when destroyed.

## Debug Steps

### Step 1: Test Manual Explosion
1. Add the `ExplosionManualTester` script to any GameObject in your scene
2. Run the game and press **T** to trigger a manual explosion
3. Check the Unity Console for debug messages
4. Watch if shards fly apart properly

**Expected Result:** You should see 51 individual pieces flying apart with physics

### Step 2: Check Resources Folder
1. Verify `HelicopterExplosionPrefab.prefab` exists in `Assets/Resources/`
2. Open the prefab and confirm it has:
   - HelicopterExplosion component on root
   - 51 child objects with Rigidbody + BoxCollider + ExplosionShard components

### Step 3: Check Enemy Data
1. Open `Assets/Enemies/BasicHelicopter.asset`
2. Verify `explosionEffect` is set to `None {fileID: 0}`
3. Open `Assets/Enemies/EliteHelicopter.asset`
4. Verify `explosionEffect` is set to `None {fileID: 0}`

### Step 4: Test Enemy Destruction
1. Enable debug info on EnemyAI script (`showDebugInfo = true`)
2. Destroy a helicopter in-game
3. Check console for these messages:
   - "EnemyAI: 'HelicopterName' destroyed with realistic explosion!"
   - "HelicopterExplosion: Initialized X shards with colliders disabled"
   - "HelicopterExplosion: Applied forces to X shards"

## Common Issues and Solutions

### Issue 1: "Could not load HelicopterExplosionPrefab from Resources!"
**Solution:** 
- Ensure prefab is in `Assets/Resources/` folder
- Prefab must be named exactly `HelicopterExplosionPrefab.prefab`

### Issue 2: "NO FORCES WERE APPLIED! This is why shards aren't moving!"
**Solution:**
- Check that all shard GameObjects have Rigidbody components
- Verify Rigidbodies are not set to `isKinematic = true` permanently
- Check that ExplosionShard components are properly attached

### Issue 3: Shards spawn but don't separate
**Solution:**
- Check Physics settings in Project Settings > Physics
- Verify collision layers are set up correctly
- Ensure shards have proper BoxCollider components

### Issue 4: Explosion happens but uses wrong effect
**Solution:**
- Clear `explosionEffect` in both helicopter asset files
- Ensure EnemyAI script calls `TryCreateRealisticExplosion` first

## Debug Console Messages to Look For

### Good Messages:
```
ExplosionManualTester: Successfully loaded prefab: HelicopterExplosionPrefab
HelicopterExplosion: Initialized 51 shards with colliders disabled
HelicopterExplosion: Applied forces to 51 shards
EnemyAI: 'BasicHelicopter' destroyed with realistic explosion!
```

### Bad Messages:
```
Could not load HelicopterExplosionPrefab from Resources!
NO FORCES WERE APPLIED! This is why shards aren't moving!
Explosion prefab has no HelicopterExplosion component!
```

## Testing Checklist

- [ ] Manual explosion test works (press T)
- [ ] HelicopterExplosionPrefab loads successfully
- [ ] 51 shards are initialized
- [ ] Forces are applied to all shards
- [ ] Helicopter asset files have no explosion effect assigned
- [ ] EnemyAI debug shows "realistic explosion" message
- [ ] Shards fly apart and fall to ground

## If Still Not Working

1. **Check Unity Console** for any error messages
2. **Verify Physics Settings** - ensure gravity is enabled
3. **Check Collision Layers** - make sure shards can collide with terrain
4. **Test with Simple Scene** - create empty scene with just explosion test
5. **Verify Prefab Structure** - ensure all 51 shards have required components

## Manual Force Test

If automatic explosion isn't working, try this manual test in the HelicopterExplosion script:

```csharp
// Add this to Start() method for testing
void Start()
{
    // Force immediate explosion for testing
    StartCoroutine(ExecuteExplosion());
}
```

This will help identify if the issue is with the explosion system itself or the triggering mechanism.
