# Enemy System Setup Guide

This guide will help you set up the new helicopter enemy system in your RiverRaider project.

## 🎯 System Overview

The enemy system consists of three main components:
- **EnemyTypeData** (ScriptableObjects) - Define enemy characteristics
- **EnemyAI** - Enhanced helicopter AI with data-driven behavior
- **EnemyManager** - Central management with pooling and difficulty scaling

## 📋 Setup Steps

### Step 1: Create Enemy Type Data Assets

1. **Right-click in Project window** → Create → RiverRaider → Enemy Type Data
2. **Create these enemy types:**
   - `BasicHelicopter` (Tier 1, machinegun only)
   - `EliteHelicopter` (Tier 2, machinegun + missiles)

### Step 2: Configure Basic Helicopter

**Basic Properties:**
- Enemy Name: `Basic Helicopter`
- Enemy Prefab: Assign your helicopter prefab (with EnemyAI component)
- Difficulty Tier: `1`

**Health & Defense:**
- Max Health: `100`
- Armor: `0` (no damage reduction)

**Movement:**
- Hover Speed: `1.5` (bobbing frequency)
- Hover Height: `2` (bobbing amplitude)
- Rotation Speed: `2` (turn speed to face player)
- Max Distance: `50` (engagement range)

**Weapons:**
- Has Machinegun: ✅ `true`
- Machinegun Damage: `15`
- Machinegun Fire Rate: `1.0` (1 second between shots)
- Machinegun Range: `40`
- Has Missiles: ❌ `false`

**AI Behavior:**
- Detection Range: `50`
- Retreat Health Threshold: `0.3` (retreat at 30% health)
- Aggression Level: `1.0`

**Spawning:**
- Spawn Weight: `1.0`
- Max Simultaneous: `3`

### Step 3: Configure Elite Helicopter

**Basic Properties:**
- Enemy Name: `Elite Helicopter`
- Enemy Prefab: Same helicopter prefab
- Difficulty Tier: `2`

**Health & Defense:**
- Max Health: `150`
- Armor: `0.1` (10% damage reduction)

**Movement:**
- Hover Speed: `2.0`
- Hover Height: `1.5`
- Rotation Speed: `3`
- Max Distance: `60`

**Weapons:**
- Has Machinegun: ✅ `true`
- Machinegun Damage: `20`
- Machinegun Fire Rate: `0.8`
- Machinegun Range: `45`
- Has Missiles: ✅ `true`
- Missile Damage: `35`
- Missile Fire Rate: `4.0`
- Missile Range: `60`
- Max Missiles: `4`

**AI Behavior:**
- Detection Range: `60`
- Retreat Health Threshold: `0.2`
- Aggression Level: `1.3`

**Spawning:**
- Spawn Weight: `0.7` (less common than basic)
- Max Simultaneous: `2`

### Step 4: Setup Enemy Prefab

1. **Take your existing helicopter model** (enemyHelicopter3.blend)
2. **Add/Update components:**
   - EnemyAI (replace old EnemyAI if exists)
   - Rigidbody (if not present)
   - Collider for collision detection
   - AudioSource (auto-added by EnemyAI)

3. **Configure EnemyAI component:**
   - Enemy Data: Assign BasicHelicopter asset
   - Fire Point: Create empty GameObject as child, position at gun location
   - Missile Fire Point: (Optional) separate missile launch point
   - Show Debug Info: Enable for testing

4. **Setup Fire Points:**
   ```
   HelicopterPrefab
   ├── Model (your 3D model)
   ├── FirePoint (Empty GameObject)
   │   └── Position at machinegun location
   └── MissileFirePoint (Empty GameObject)
       └── Position at missile launcher
   ```

### Step 5: Setup Enemy Manager

1. **Create empty GameObject** in scene: "EnemyManager"
2. **Add EnemyManager component**
3. **Configure settings:**

**Enemy Configuration:**
- Available Enemy Types: Drag BasicHelicopter and EliteHelicopter assets
- Max Active Enemies: `5`
- Enemy Cleanup Distance: `100`

**Difficulty Scaling:**
- Enable Difficulty Scaling: ✅ `true`
- Difficulty Scale Distance: `500` (tier up every 500m)
- Health Scale Multiplier: `1.2`
- Damage Scale Multiplier: `1.15`
- Fire Rate Scale Multiplier: `1.1`

**Spawning:**
- Spawn Cooldown: `3.0`
- Spawn Chance Per Chunk: `0.4` (40% chance)
- Max Enemies Per Chunk: `2`

**Debug:**
- Show Debug Info: ✅ `true` (for testing)
- Show Gizmos: ✅ `true` (for visualization)

### Step 6: Update Bullet System

The enemy system uses your existing BulletPool. Make sure:
1. **BulletPool.Instance** is available in scene
2. **Bullet component** has public `Damage` property
3. **Player has "Player" tag** for enemy targeting

### Step 7: Test the System

1. **Play the game**
2. **Check Console** for debug messages
3. **Fly forward** to trigger terrain chunks and enemy spawning
4. **Verify behaviors:**
   - Helicopters spawn and hover with bobbing motion
   - Helicopters face and shoot at player
   - Difficulty increases as you progress
   - Enemies are cleaned up when far behind

## 🎮 Gameplay Features

### Difficulty Progression
- **Tier 1 (0-500m)**: Basic helicopters only, 1-2 active
- **Tier 2 (500-1000m)**: Elite helicopters appear, 2-3 active
- **Tier 3+ (1000m+)**: Stronger enemies, up to 5 active

### Enemy Behaviors
- **Hovering**: Sine wave bobbing motion
- **Player Tracking**: Smooth rotation to face player
- **Engagement**: Attack when in range, retreat when damaged
- **Weapon Switching**: Elite helicopters use both guns and missiles

### Performance Features
- **Object Pooling**: Reuses enemy instances
- **Distance Culling**: Removes distant enemies
- **Update Optimization**: Reduced update frequency
- **Performance Monitoring**: Debug statistics

## 🔧 Customization

### Adding New Enemy Types
1. Create new EnemyTypeData asset
2. Configure stats and behaviors
3. Add to EnemyManager's Available Enemy Types
4. Set appropriate Difficulty Tier

### Adjusting Difficulty
- Modify `difficultyScaleDistance` for faster/slower progression
- Adjust scale multipliers for different difficulty curves
- Change `maxActiveEnemies` for more/less intense combat

### Spawn Tuning
- `spawnChancePerChunk`: Higher = more frequent spawning
- `spawnCooldown`: Lower = faster spawning
- `maxEnemiesPerChunk`: Higher = more enemies per area

## 🐛 Troubleshooting

### Enemies Not Spawning
- Check EnemyManager has enemy types assigned
- Verify ChunkSpawner is working and firing events
- Enable debug info to see spawn attempts

### Enemies Not Shooting
- Verify BulletPool.Instance exists in scene
- Check Fire Point is assigned and positioned correctly
- Ensure player has "Player" tag

### Performance Issues
- Reduce `maxActiveEnemies`
- Increase `updateInterval` in EnemyManager
- Disable `enablePerformanceMonitoring`

### Enemies Not Facing Player
- Check player has "Player" tag
- Verify `rotationSpeed` is > 0
- Ensure enemy prefab can rotate freely

## 📊 Debug Information

Enable debug info to see:
- Enemy spawn/destroy events
- State changes (Patrolling → Engaging → Retreating)
- Difficulty tier changes
- Performance statistics
- Distance and range calculations

## 🚀 Next Steps

Once basic system is working:
1. **Add sound effects** to enemy data assets
2. **Create explosion effects** for enemy destruction
3. **Add more enemy types** (different helicopters, ground units)
4. **Implement boss encounters** at higher difficulty tiers
5. **Add enemy health bars** or damage indicators

The system is designed to be easily extensible - you can add new enemy types, behaviors, and features without modifying core code!
