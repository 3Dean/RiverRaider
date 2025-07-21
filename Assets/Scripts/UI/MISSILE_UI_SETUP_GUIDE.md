# ARM-30 Missile UI Setup Guide

This guide explains how to set up the ARM-30 missile UI system that integrates with your existing FlightData architecture.

## Overview

The missile system consists of:
- **FlightData**: Extended to include missile ammunition management
- **MissileController**: Handles missile firing and types (already existed)
- **MissileUI**: Displays missile count and status
- **WeaponManager**: Coordinates weapon systems (already existed)

## What's Been Added

### 1. FlightData Extensions
- `maxMissiles = 5` (ARM-30 capacity)
- `currentMissiles` (starts at 5/5)
- `currentMissileType = "ARM-30"` (default type)
- Missile management methods: `ConsumeMissile()`, `ResupplyMissiles()`, `AddMissiles()`, etc.

### 2. MissileUI Component
- Automatically finds FlightData reference
- Updates missile count display (current/max)
- Color-coded status (normal/low ammo/empty)
- Performance optimized with update intervals

### 3. Updated MissileController
- Now uses FlightData for ammunition instead of local variables
- Integrates with centralized data system
- Maintains existing missile type functionality

## Setup Instructions

### Step 1: Locate Your arm30 UI Element
1. In your scene hierarchy, find the "arm30" UI element
2. It should contain:
   - An Image component (missile icon)
   - Two TextMeshPro components (current count and max capacity)

### Step 2: Add MissileUI Component
1. Select the arm30 GameObject
2. Add Component → Scripts → MissileUI
3. The component will auto-find:
   - FlightData reference
   - TextMeshPro components (first = current, second = max)
   - Image component (for icon color changes)

### Step 3: Configure PlayerShip
1. Select your PlayerShip GameObject
2. Ensure it has a WeaponManager component
3. In WeaponManager, assign:
   - MachinegunController reference
   - MissileController reference
4. In MissileController, the FlightData reference will be auto-found

### Step 4: Test the System
1. Play the game
2. Press Space to fire missiles
3. Watch the arm30 UI update from 5/5 → 4/5 → 3/5, etc.
4. Colors should change: White → Yellow (≤2) → Red (0)

## UI Color System

The MissileUI automatically changes colors based on ammunition:
- **White**: Normal ammunition (3+ missiles)
- **Yellow**: Low ammunition (1-2 missiles)
- **Red**: Empty (0 missiles)

## Testing Features

### FlightData Context Menu
Right-click FlightData in inspector:
- "Test Fire Missile" - Consumes 1 missile
- "Test Resupply Missiles" - Refills to maximum

### MissileUI Context Menu
Right-click MissileUI in inspector:
- "Test Fire Missile" - Same as above
- "Test Resupply Missiles" - Same as above
- "Test Switch Missile Type" - Cycles through ARM-30, ARM-60, ARM-90

## Input Controls

- **Space Key**: Fire missile (handled by WeaponManager)
- **Left Mouse**: Machinegun (existing functionality)

## Missile Resupply System

The system is designed to work with fuel barge-style resupply:
- `flightData.ResupplyMissiles()` - Full reload to 5/5
- `flightData.AddMissiles(amount)` - Add specific amount
- Similar to existing fuel resupply mechanics

## Future Expansion

The system is designed for multiple missile types:
- ARM-30 (current default)
- ARM-60, ARM-90 (future types)
- MissileController supports different damage, speed, cooldown per type
- UI will automatically display the current missile type

## Troubleshooting

### UI Not Updating
- Check that FlightData is found (see console logs)
- Verify TextMeshPro components are assigned correctly
- Ensure MissileUI is on the correct GameObject

### Missiles Not Firing
- Check WeaponManager has MissileController assigned
- Verify MissileController has FlightData reference
- Check missile prefabs are assigned in MissileController

### Console Errors
- All components log their initialization status
- Look for "MissileUI: Initialized successfully"
- Look for "MissileController: Found FlightData"

## Integration Notes

This system follows the same patterns as your existing UI:
- Uses FlightData for centralized state management
- Performance optimized with update intervals
- Auto-finds references when possible
- Consistent debug logging
- Context menu testing features

The missile system is now fully integrated with your existing architecture and ready for use!
