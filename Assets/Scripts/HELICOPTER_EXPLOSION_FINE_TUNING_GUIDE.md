# Helicopter Explosion Fine-Tuning Guide

## 🎯 Quick Parameter Adjustments

Now that the core physics issues are fixed, you can fine-tune the explosion behavior to match your exact vision!

### Main Parameters to Adjust

#### **Base Explosion Force** (Default: 75)
- **Lower (25-50)**: Gentle explosion, pieces fall close to helicopter
- **Higher (100-150)**: More dramatic explosion, pieces spread wider
- **Sweet Spot**: 75-100 for most realistic helicopter explosions

#### **Upward Force Multiplier** (Default: 0.3)
- **Lower (0.1-0.2)**: Pieces mostly spread horizontally
- **Higher (0.4-0.6)**: More pieces fly upward dramatically
- **Sweet Spot**: 0.2-0.4 for natural arc trajectories

#### **Shard Mass** (Default: 2.0)
- **Lower (1.0-1.5)**: Pieces move faster, more dramatic
- **Higher (3.0-5.0)**: Pieces move slower, more realistic weight
- **Sweet Spot**: 1.5-2.5 depending on helicopter size

#### **Shard Drag** (Default: 1.5)
- **Lower (0.5-1.0)**: Pieces travel farther before stopping
- **Higher (2.0-3.0)**: Pieces slow down quickly, fall closer
- **Sweet Spot**: 1.0-2.0 for realistic air resistance

## 🎨 Visual Style Adjustments

### For Cinematic Drama:
```
Base Explosion Force: 120
Upward Force Multiplier: 0.5
Directional Force Multiplier: 0.3
Randomness Multiplier: 0.2
Shard Mass: 1.5
Shard Drag: 1.0
```

### For Realistic Physics:
```
Base Explosion Force: 75
Upward Force Multiplier: 0.3
Directional Force Multiplier: 0.2
Randomness Multiplier: 0.1
Shard Mass: 2.5
Shard Drag: 1.8
```

### For Gentle Destruction:
```
Base Explosion Force: 50
Upward Force Multiplier: 0.2
Directional Force Multiplier: 0.1
Randomness Multiplier: 0.05
Shard Mass: 3.0
Shard Drag: 2.5
```

### For Violent Explosion:
```
Base Explosion Force: 150
Upward Force Multiplier: 0.6
Directional Force Multiplier: 0.4
Randomness Multiplier: 0.3
Shard Mass: 1.0
Shard Drag: 0.8
```

## 🔧 Advanced Fine-Tuning

### Explosion Timing:
- **Collider Activation Delay**: Increase (1.5-2.0s) if pieces still collide too early
- **Separation Distance**: Adjust in `SeparateShards()` method (0.2f-0.8f range)

### Physics Behavior:
- **Angular Drag**: Higher values (3.0-4.0) reduce spinning, lower (1.0-1.5) increase it
- **Force Variation**: Higher values (0.2-0.3) make each explosion more unique

### Performance Optimization:
- **Max Shards to Track**: Reduce in diagnostic tool if performance is slow
- **Cleanup Distance**: Adjust based on your game world size
- **Shard Lifetime**: Reduce if you want faster cleanup

## 🎮 Game-Specific Adjustments

### For Fast-Paced Action Games:
- Increase explosion force for more drama
- Reduce shard lifetime for faster cleanup
- Higher randomness for more chaos

### For Simulation Games:
- Lower explosion force for realism
- Higher mass and drag for realistic physics
- Longer shard lifetime for persistence

### For Mobile Games:
- Reduce number of tracked shards
- Shorter cleanup times
- Simpler physics calculations

## 🧪 Testing Workflow

1. **Start with default values** in `HelicopterExplosionFixed`
2. **Adjust one parameter at a time** and test
3. **Use the diagnostic tool** to monitor velocities
4. **Fine-tune based on visual feedback**
5. **Test with different helicopter sizes/types**

## 📊 Monitoring Guidelines

### Healthy Velocity Ranges:
- **Initial velocities**: 5-20 units/sec (green in diagnostic)
- **Peak velocities**: 10-30 units/sec (yellow acceptable)
- **Excessive velocities**: 40+ units/sec (red - needs adjustment)

### Expected Behavior Timeline:
- **0-0.5s**: Explosion and initial separation
- **0.5-2s**: Peak movement and arcing
- **2-5s**: Gradual slowdown due to drag
- **5-10s**: Pieces settle on terrain
- **10-15s**: Cleanup begins

## 🎯 Quick Fixes for Common Issues

### "Pieces still fly too far":
- Increase `shardDrag` to 2.0-3.0
- Increase `shardMass` to 3.0-4.0
- Reduce `baseExplosionForce` to 50-60

### "Explosion looks too weak":
- Increase `baseExplosionForce` to 100-120
- Increase `upwardForceMultiplier` to 0.4-0.5
- Reduce `shardMass` to 1.5-2.0

### "Pieces don't spread enough":
- Increase `randomnessMultiplier` to 0.2-0.3
- Increase `explosionRadius` to 6-8
- Adjust separation distance in code

### "Too much spinning":
- Increase `shardAngularDrag` to 3.0-4.0
- Reduce torque multiplier in code (0.05f → 0.02f)

Remember: The diagnostic tool is your best friend for fine-tuning! It will show you exactly what's happening with the physics so you can adjust accordingly.

Happy exploding! 💥
