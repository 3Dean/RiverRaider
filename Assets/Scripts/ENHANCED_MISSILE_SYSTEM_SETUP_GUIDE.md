# Enhanced Missile System Setup Guide

## Overview
The enhanced missile system provides dynamic missile count functionality with support for multiple missile types, each with their own capacities and properties.

## Key Components

### 1. MissileTypeData (ScriptableObject)
- Defines individual missile types (ARM-30, ARM-60, etc.)
- Contains capacity, damage, speed, and visual properties
- Create via: Right-click → Create → Weapons → Missile Type Data

### 2. MissileInventory (Class)
- Manages separate ammunition tracking for each missile type
- Handles missile type switching and capacity management
- Integrated into FlightData

### 3. Enhanced FlightData
- Now uses MissileInventory for missile management
- Maintains backward compatibility with existing code
- Supports dynamic capacity changes when switching types

### 4. Enhanced MissileUI
- Displays current missile type and count dynamically
- Updates colors based on missile type properties
- Shows capacity changes when switching types

### 5. EnhancedWeaponInputController
- Handles missile type switching (Tab key)
- Previous type switching (Shift+Tab)
- Missile firing (Space key)

## Setup Instructions

### Step 1: Create Missile Type Data Assets
1. In Project window, right-click in Assets/Data folder
2. Create → Weapons → Missile Type Data
3. Create these missile types:
   - ARM-30: Capacity 5, Standard damage (100), Speed 40
   - ARM-60: Capacity 3, Heavy damage (150), Speed 35
   - ARM-90: Capacity 2, Maximum damage (200), Speed 30

### Step 2: Configure FlightData
1. Select your FlightData GameObject in the scene
2. In the Inspector, find "Missile System" section
3. Set "Available Missile Types" array size to 3
4. Drag your created MissileTypeData assets into the array slots

### Step 3: Update MissileUI
1. Select your MissileUI GameObject
2. Ensure it references the FlightData
3. The UI will automatically update to show dynamic counts

### Step 4: Add Enhanced Input Controller
1. Add EnhancedWeaponInputController to your player GameObject
2. Assign FlightData and MissileController references
3. Configure input keys as desired

### Step 5: Test the System
1. Play the game
2. Press Tab to switch missile types
3. Press Space to fire missiles
4. Observe the UI updating with different capacities

## Features

### Dynamic Capacity Management
- Each missile type has its own maximum capacity
- Switching types changes the displayed max count
- Separate ammunition tracking per type

### Visual Feedback
- UI colors change based on ammunition levels
- Missile type colors can be customized per type
- Smooth transitions when switching types

### Input Controls
- Tab: Switch to next missile type
- Shift+Tab: Switch to previous missile type
- Space: Fire current missile type

### Backward Compatibility
- Existing code continues to work
- Legacy properties maintained
- Gradual migration path

## Troubleshooting

### Compilation Errors
- Ensure all new scripts are in the project
- Check that MissileTypeData assets are created
- Verify FlightData has the missile types assigned

### UI Not Updating
- Check FlightData reference in MissileUI
- Ensure missile type data assets are properly configured
- Verify the missile inventory is initialized

### Input Not Working
- Check EnhancedWeaponInputController is added to player
- Verify input key assignments
- Ensure FlightData reference is set

## Advanced Features

### Custom Missile Types
- Create new MissileTypeData assets for unique missiles
- Configure damage, speed, capacity, and visual properties
- Add to FlightData's available missile types array

### Audio Integration
- Assign missile type switch sounds
- Configure launch sounds per missile type
- Add empty ammunition sounds

### Visual Customization
- Set unique colors per missile type
- Assign different icons for each type
- Customize UI appearance based on missile properties

## Performance Notes
- UI updates are optimized with change detection
- Missile inventory uses efficient data structures
- Minimal performance impact on existing systems
