# Machinegun Cooldown UI Setup Guide

This guide explains how to set up the machinegun cooldown meter UI that displays heat level and cooling rate for the machinegun weapon system.

## Overview

The `MachinegunCooldownUI` script connects your existing "CooldownBar" slider to the `MachinegunController`'s overheating system, providing visual feedback for:
- Current heat level (0-100%)
- Heat state with color coding
- Cooldown rate display
- Overheated state with pulsing effects
- Firing status indicators

## Prerequisites

✅ **MachinegunController** - Must be present in scene with overheating enabled
✅ **WeaponManager** - Should be managing the machinegun system
✅ **CooldownBar Slider** - Must exist in your HUD UI

## Setup Instructions

### Step 1: Locate Your CooldownBar Slider

1. In the Unity Hierarchy, find your HUD Canvas
2. Locate the "CooldownBar" GameObject (must be named exactly "CooldownBar")
3. Ensure it has a **Slider** component attached
4. Verify the slider has a **Fill Rect** with an **Image** component for color changes

### Step 2: Add the MachinegunCooldownUI Script

**Option A: Add to CooldownBar GameObject**
1. Select the "CooldownBar" GameObject
2. Click "Add Component"
3. Search for "MachinegunCooldownUI"
4. Add the script

**Option B: Add to a Parent GameObject**
1. Select a parent GameObject (like HUD Manager)
2. Add the "MachinegunCooldownUI" component
3. The script will automatically find the "CooldownBar" by name

### Step 3: Configure the Script (Optional)

The script will auto-configure itself, but you can customize:

#### UI References (Auto-found if not assigned)
- **Cooldown Slider**: Will find "CooldownBar" GameObject automatically
- **Fill Image**: Auto-found from slider's fill rect
- **Heat Percentage Text**: Auto-found by searching for text with "heat" or "percentage" in name
- **Cooldown Rate Text**: Auto-found by searching for text with "cooldown" or "rate" in name

#### Visual Settings
- **Cool Color**: Green (0-60% heat)
- **Warming Color**: Yellow (60-80% heat)
- **Hot Color**: Orange (80-95% heat)
- **Overheated Color**: Red (95-100% heat)
- **Thresholds**: Customize when colors change

#### Overheated Effects
- **Enable Overheated Pulsing**: Makes the bar pulse when overheated
- **Pulse Speed**: How fast the pulsing effect is
- **Pulse Intensity**: How intense the pulsing effect is

### Step 4: Add Text Elements (Optional)

For heat percentage and cooldown rate display:

1. **Heat Percentage Text**:
   - Create a Text GameObject as child of CooldownBar
   - Name it something containing "heat" or "percentage"
   - Position it near the slider

2. **Cooldown Rate Text**:
   - Create another Text GameObject as child of CooldownBar
   - Name it something containing "cooldown" or "rate"
   - Position it below or beside the slider

### Step 5: Enable Overheating in MachinegunController

Ensure your MachinegunController has overheating enabled:

1. Select the GameObject with MachinegunController
2. In the inspector, expand "Overheating (Optional)"
3. Check **Enable Overheating**
4. Configure heat settings:
   - **Max Heat**: 100 (recommended)
   - **Heat Per Shot**: 2-5 (adjust for gameplay)
   - **Cooling Rate**: 10-20 (adjust for gameplay)
   - **Overheat Threshold**: 80-90 (when overheating occurs)

## Visual States

The cooldown meter displays different states:

### Heat Level Colors
- **Green**: Cool (0-60% heat) - Safe to fire
- **Yellow**: Warming (60-80% heat) - Caution
- **Orange**: Hot (80-95% heat) - Danger zone
- **Red**: Overheated (95-100%) - Cannot fire, pulsing effect

### Text Displays
- **Heat Percentage**: Shows current heat % with status
  - "45%" (normal)
  - "67% FIRING" (while firing)
  - "95% OVERHEAT!" (when overheated)

- **Cooldown Rate**: Shows current state
  - "STABLE" (no heat change)
  - "HEATING" (while firing)
  - "COOLING 12.5/s" (cooling rate per second)

## Testing

### In-Game Testing
1. Start the game
2. Fire the machinegun (left mouse button)
3. Watch the cooldown bar fill up as heat increases
4. Stop firing and watch it cool down
5. Fire continuously until overheated to test the overheated state

### Debug Features
Use the context menu options on the MachinegunCooldownUI component:
- **Test Force Update**: Forces an immediate UI update
- **Toggle Debug Logging**: Enables detailed console logging
- **Toggle Heat Percentage Text**: Shows/hides heat percentage display
- **Toggle Cooldown Rate Text**: Shows/hides cooldown rate display

## Troubleshooting

### "No CooldownBar found" Error
- Ensure your slider GameObject is named exactly "CooldownBar"
- Check that it's active in the hierarchy
- Verify it has a Slider component

### "No MachinegunController found" Error
- Ensure MachinegunController exists in the scene
- Check that it's on an active GameObject
- Verify overheating is enabled in MachinegunController

### Colors Not Changing
- Check that the slider has a Fill Rect assigned
- Ensure the Fill Rect has an Image component
- Verify the Fill Image reference is found (check console logs)

### Text Not Updating
- Ensure text GameObjects are named appropriately
- Check that they're children of the CooldownBar or nearby
- Enable debug logging to see if text components are found

### Performance Issues
- Increase the Update Interval (default: 0.05s)
- Disable debug logging in production
- Consider reducing pulse effects if needed

## Customization

### Custom Colors
Adjust the color settings in the Visual Settings section to match your game's theme.

### Custom Thresholds
Modify the threshold values to change when colors transition:
- Lower thresholds = earlier warnings
- Higher thresholds = later warnings

### Custom Text Format
Modify the `UpdateHeatPercentageText` and `UpdateCooldownRateText` methods to customize text display format.

## Integration Notes

- The script automatically finds components to minimize setup
- Works with existing UI layouts without modification
- Performance optimized with update intervals
- Compatible with the existing weapon system architecture
- Follows the same patterns as other UI components (ThrustBarUI, HealthBarUI)

## Script Location

The MachinegunCooldownUI script is located at:
`Assets/Scripts/UI/MachinegunCooldownUI.cs`

This script integrates seamlessly with your existing weapon system and provides comprehensive visual feedback for the machinegun's heat state.
