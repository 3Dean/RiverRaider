# SVG Missile Icon Setup Guide

This guide explains how to set up SVG missile icons for your missile system using the new SVG support.

## Overview

The missile system now supports SVG icons through Unity's Vector Graphics package. This allows for:
- ✅ Scalable vector graphics that stay crisp at any resolution
- ✅ Small file sizes
- ✅ Perfect for UI icons
- ✅ Template system for multiple missile types

## Step 1: SVG Import Settings

For your `img_missile_arm30.svg` file (and future missile SVG icons):

1. **Select the SVG file** in `Assets/Images/UIelements/`
2. **Set Import Settings**:
   - **Generated Asset Type**: `UI SVGImage`
   - **Pixels Per Unit**: `100`
   - **Pivot**: `Center`
   - **Target Resolution**: `1080p` (or your preferred resolution)
3. **Click Apply** to generate the UI SVGImage asset

## Step 2: MissileTypeData Setup

Your `MissileTypeData` assets now have two icon fields:

### In your MTD-ARM30 asset:
1. **Missile SVG Icon**: Assign your `img_missile_arm30` SVG asset here
2. **Missile Icon**: (Legacy sprite support - can leave empty if using SVG)
3. **UI Color**: Set the color for this missile type

## Step 3: MissileUI Component Setup

The `MissileUI` script now supports SVG icons:

1. **Missile Icon Field**: Should now show `SVGImage` type
2. **Auto-Detection**: The script will automatically find SVG components in children
3. **Dynamic Updates**: Icons will change when switching missile types

## Step 4: Testing the System

### In Unity Editor:
1. **Apply SVG Import Settings** (Step 1)
2. **Assign SVG to MissileTypeData** (Step 2)
3. **Test in Play Mode**:
   - Fire missiles to see count decrease
   - Switch missile types to see icon change
   - Verify "/ " formatting in missile count display

### Debug Features:
- Right-click MissileUI component → Context Menu:
  - "Test Fire Missile"
  - "Test Resupply Missiles" 
  - "Test Switch Missile Type"

## Step 5: Creating Additional Missile Types

### For ARM-60, ARM-90, etc.:
1. **Create SVG files**: `img_missile_arm60.svg`, `img_missile_arm90.svg`
2. **Import with same settings** as ARM-30
3. **Create new MissileTypeData assets**
4. **Assign respective SVG icons**

## Template Structure

```
Assets/Images/UIelements/
├── img_missile_arm30.svg  ← ARM-30 icon
├── img_missile_arm60.svg  ← ARM-60 icon (future)
├── img_missile_arm90.svg  ← ARM-90 icon (future)

Assets/
├── MTD-ARM30.asset        ← References img_missile_arm30
├── MTD-ARM60.asset        ← References img_missile_arm60 (future)
├── MTD-ARM90.asset        ← References img_missile_arm90 (future)
```

## Features

### Current Implementation:
- ✅ SVG icon support in MissileUI
- ✅ Dynamic icon switching based on missile type
- ✅ "/ " formatting before max capacity (e.g., "5/ 5")
- ✅ Color coding based on ammo levels
- ✅ Backward compatibility with Sprite icons

### Icon Display Logic:
1. **Priority**: SVG icon from MissileTypeData
2. **Fallback**: Regular Sprite icon from MissileTypeData
3. **Color**: Applied from MissileTypeData.uiColor
4. **Dynamic**: Updates when switching missile types

## Troubleshooting

### SVG Not Showing in Selector:
- Verify SVG import settings (Generated Asset Type = UI SVGImage)
- Click Apply after changing settings
- Check that the SVG asset was generated

### Icon Not Updating:
- Verify MissileTypeData has SVG icon assigned
- Check that FlightData is properly connected
- Use debug context menu to test missile type switching

### Performance:
- SVG icons are optimized for UI use
- Update interval is configurable (default: 0.1 seconds)
- Only updates when values actually change

## Next Steps

1. **Apply SVG import settings** to your `img_missile_arm30.svg`
2. **Assign the generated SVG asset** to your MTD-ARM30
3. **Test the system** in play mode
4. **Create additional missile type SVGs** as needed

The system is now ready for your SVG-based missile icon template!
