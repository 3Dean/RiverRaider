# New Weapon System Setup Guide

## Overview
The weapon system has been completely redesigned to fix the fire rate issue and support multiple weapon types.

## Architecture

### 🎯 WeaponManager (Main Controller)
- **Purpose**: Central weapon management
- **Controls**: 
  - Left Mouse = Continuous machinegun
  - Space Bar = Single missile shots
- **Location**: Add to your player GameObject (riverraid_hero)

### 🔫 MachinegunController (Rapid Fire)
- **Purpose**: Handles continuous bullet streams
- **Fire Rate**: Now works properly (0.1 = fast, 1.0 = slow)
- **Features**: Optional overheating, dedicated fire points
- **Location**: Keep on your player GameObject

### 🚀 MissileController (Single Shot)
- **Purpose**: Handles missile firing
- **Features**: Multiple missile types, ammunition management
- **Location**: Add to your player GameObject if you want missiles

### 💥 Missile (Projectile)
- **Purpose**: Individual missile behavior
- **Features**: Movement, collision, explosion, area damage
- **Location**: Attach to missile prefabs

## Setup Steps

### 1. Remove Old System
- Remove `WeaponSystemController` from riverraid_hero GameObject
- Remove `WeaponInputController` from scene (no longer needed)

### 2. Add New System
- Add `WeaponManager` component to riverraid_hero GameObject
- Keep `MachinegunController` on riverraid_hero GameObject
- Optionally add `MissileController` to riverraid_hero GameObject

### 3. Configure WeaponManager
- Assign `MachinegunController` reference in inspector
- Assign `MissileController` reference in inspector (if using missiles)
- Enable/disable mouse and keyboard input as needed

### 4. Configure MachinegunController
- Set fire rate (0.1 = very fast, 1.0 = slow)
- Assign fire points for bullets
- Configure audio and effects

### 5. Configure MissileController (Optional)
- Set max missiles (default: 20)
- Create missile types in the array
- Assign fire points for missiles
- Configure audio and effects

## Controls
- **Left Mouse Button**: Continuous machinegun firing (rate-limited)
- **Space Bar**: Single missile shots (ammo-limited)

## Benefits
✅ Fire rate control works perfectly
✅ No more conflicting weapon systems  
✅ Scalable for multiple weapon types
✅ Clean, maintainable code
✅ Easy to extend with new weapons

## Troubleshooting

### Fire Rate Still Not Working?
- Make sure WeaponSystemController is removed
- Check that WeaponManager is properly assigned
- Verify MachinegunController fire rate setting

### Missiles Not Working?
- Create missile prefabs with Missile component
- Assign missile prefabs to MissileController missile types
- Set up missile fire points

### No Firing at All?
- Check that WeaponManager has component references assigned
- Verify input settings are enabled
- Check console for error messages
