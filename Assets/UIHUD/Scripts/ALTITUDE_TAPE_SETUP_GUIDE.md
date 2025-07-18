# Altitude Tape Tick Marks Setup Guide

This guide explains how to set up animated altitude tick marks using the existing `HUDTapeController` system, similar to the speed indicator tick marks.

## Overview

The `HUDTapeController` can be configured for altitude display by:
- Setting the tape type to `Altitude`
- Connecting to your existing `AltimeterUI` component
- Creating animated tick marks that respond to altitude changes
- Displaying values in feet with appropriate increments

## Prerequisites

Before setting up altitude tick marks, ensure you have:
- ✅ `AltimeterUI.cs` component working in your scene
- ✅ `HUDTapeController.cs` script available
- ✅ Altitude display text showing current altitude (e.g., "109 ft")
- ✅ A container GameObject where tick marks will be created

## Step-by-Step Setup

### 1. Locate Your Altitude UI Elements

In your scene hierarchy, find your altitude display elements. Based on your HUD layout, this might be:
- Main altitude text display (showing values like "109 ft")
- Container for altitude tick marks
- Parent GameObject for the altitude tape system

### 2. Create Altitude Tick Container

If you don't already have a container for altitude tick marks:

1. **Create Container GameObject**:
   - Right-click in your altitude UI area
   - Create Empty GameObject
   - Name it `AltitudeTickContainer`
   - Position it where you want the tick marks to appear

2. **Configure Container**:
   - Add `RectTransform` component if not present
   - Set appropriate anchoring (usually center-left or center-right)
   - Size the container to match your desired tick mark area height

### 3. Add HUDTapeController for Altitude

1. **Select Target GameObject**:
   - Choose the parent GameObject for your altitude display
   - This should be at the same level as your altitude text

2. **Add Component**:
   - Add `HUDTapeController` component
   - Set `Tape Type` to `Altitude`

3. **Configure References**:
   - `Tick Container`: Drag your `AltitudeTickContainer` here
   - `Main Value Label`: Drag your altitude text component here
   - `Tick Prefab`: Leave empty (system will create simple ticks)
   - `Flight Data`: Will auto-find your FlightData component
   - `Altimeter UI`: Will auto-find your AltimeterUI component

### 4. Configure Altitude-Specific Settings

**Tape Settings**:
- `Units Per Tick`: 5 (for 5-foot increments) or 10 (for 10-foot increments)
- `Tick Spacing`: 25-30 pixels (adjust based on your UI design)
- `Visible Ticks`: 20-25 (adjust based on container height)
- `Animation Smoothness`: 5 (responsive but smooth)
- `Major Tick Interval`: 5 (show numbers every 5 units)

**Display Settings**:
- `Value Format`: "F0" (whole numbers)
- `Altitude Unit`: " ft" (or leave empty if your main label already shows units)
- `Update Interval`: 0.05 (20 updates per second)

### 5. Position and Style the Tick Marks

The system will create tick marks automatically, but you may want to adjust:

**Positioning**:
- Tick marks appear relative to the container position
- Major ticks are longer (19x3 pixels)
- Minor ticks are shorter (15x2 pixels)
- **Numbers appear to the right of tick marks** (opposite of speed tape layout)

**Colors**:
- Default: Bright green (0, 1, 0, 1)
- Modify in `HUDTapeController.cs` if different colors needed

### 6. Test the System

1. **Play Mode Testing**:
   - Enter Play mode
   - Change aircraft altitude (fly up/down)
   - Observe tick marks animating smoothly
   - Verify numbers show appropriate altitude values

2. **Debug Information**:
   - Check Console for initialization messages
   - Look for "HUDTapeController (Altitude)" debug logs
   - Verify "isInitialized: true" message

## Configuration Examples

### For 5-Foot Increments:
```
Altitude Tape Settings:
├── Tape Type: Altitude
├── Units Per Tick: 5
├── Tick Spacing: 25
├── Visible Ticks: 25
├── Major Tick Interval: 5
└── Animation Smoothness: 5
```

### For 10-Foot Increments:
```
Altitude Tape Settings:
├── Tape Type: Altitude
├── Units Per Tick: 10
├── Tick Spacing: 30
├── Visible Ticks: 20
├── Major Tick Interval: 5
└── Animation Smoothness: 5
```

## Troubleshooting

### Common Issues:

1. **No Tick Marks Appearing**:
   - Check Console for "AltimeterUI found and assigned" message
   - Verify `Tick Container` is assigned
   - Ensure container has sufficient height
   - Check that GameObject is active in hierarchy

2. **Tick Marks Not Animating**:
   - Verify `AltimeterUI.GetCurrentAltitude()` returns changing values
   - Check that aircraft is actually changing altitude
   - Increase `Animation Smoothness` for more responsive animation

3. **Wrong Spacing or Size**:
   - Adjust `Tick Spacing` to match your UI design
   - Modify `Units Per Tick` for different increments
   - Resize `Tick Container` if needed

4. **Numbers Not Showing**:
   - Check `Major Tick Interval` setting
   - Verify altitude values are positive (negative altitudes hide labels)
   - Look for "Showing MAJOR tick label" messages in Console

### Debug Commands:

Add these to test altitude changes:
```csharp
// In your flight controller or test script
AltimeterUI altimeter = FindObjectOfType<AltimeterUI>();
HUDTapeController altitudeTape = FindObjectOfType<HUDTapeController>();

// Force altitude update
altimeter.ForceUpdate();
altitudeTape.ForceUpdate();
```

## Integration Notes

### Works With:
- ✅ Existing `AltimeterUI` system
- ✅ Ground-relative altitude calculation
- ✅ Absolute altitude mode
- ✅ Your current flight data systems

### Performance:
- Updates 20 times per second (configurable)
- Object pooling prevents garbage collection
- Automatic hiding of off-screen tick marks
- Optimized for smooth animation

## Customization Options

### Visual Customization:
- Modify tick colors in `CreateSimpleTickMark()` method
- Adjust tick sizes by changing `sizeDelta` values
- Change font size in TextMeshPro creation

### Functional Customization:
- Different increments per altitude range
- Custom altitude formatting
- Integration with terrain-following systems
- Multiple altitude reference modes

## Next Steps

1. **Set up the altitude tick container** in your scene
2. **Add and configure HUDTapeController** with Altitude type
3. **Test in Play mode** with altitude changes
4. **Fine-tune spacing and appearance** to match your HUD design
5. **Consider adding UI masking** for professional appearance

The altitude tick marks will work identically to your speed tick marks, providing smooth animation and clear altitude reference for your flight simulation.
