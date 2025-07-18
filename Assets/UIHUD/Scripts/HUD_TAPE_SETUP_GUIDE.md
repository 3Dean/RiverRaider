# HUD Tape Animation Setup Guide

This guide explains how to set up the new `HUDTapeController` system for animated speed and altitude tapes in your cockpit HUD.

## Overview

The `HUDTapeController` replaces your previous `SpeedTapeScroller` and `TickTapePool` scripts with a unified system that:
- Properly integrates with your existing `FlightData` and `AltimeterUI` systems
- Animates tick marks smoothly based on real flight data
- Updates TextMeshPro labels to show rounded values (nearest 5 MPH/feet)
- Uses object pooling for optimal performance

## Setup Instructions

### 1. Prepare Your Tick Prefab

First, ensure your tick mark prefab (`img_ticks.prefab`) has the correct structure:

```
img_ticks (GameObject)
├── Image Component (for the tick mark visual)
└── txt-speedTick (Child GameObject)
    └── TextMeshPro - Text (UI) Component
```

**Important**: The TextMeshPro component should be on a child object, not the root prefab.

### 2. Setup Speed Tape

For your speed tape (located at `Canvas/rollGraphics/SpeedTape`):

1. **Add the Controller**:
   - Select the `SpeedTape` GameObject
   - Add the `HUDTapeController` component
   - Set `Tape Type` to `Speed`

2. **Configure References**:
   - `Tick Container`: Drag your `TickContainer` GameObject here
   - `Main Value Label`: Drag your main speed text (e.g., `txt_AirSpeed`) here
   - `Tick Prefab`: Drag your `img_ticks` prefab here
   - `Flight Data`: Will auto-find your FlightData component
   - `Altimeter UI`: Leave empty (not needed for speed tape)

3. **Configure Settings**:
   - `Units Per Tick`: 5 (for 5 MPH increments)
   - `Tick Spacing`: 30 (pixels between ticks - adjust to match your design)
   - `Visible Ticks`: 15 (adjust based on your tape height)
   - `Animation Smoothness`: 5 (higher = more responsive)
   - `Speed Unit`: "" (empty, or " MPH" if you want units)

### 3. Setup Altitude Tape

For your altitude tape (if you have one):

1. **Add the Controller**:
   - Select your altitude tape GameObject
   - Add the `HUDTapeController` component
   - Set `Tape Type` to `Altitude`

2. **Configure References**:
   - Similar to speed tape, but set `Altimeter UI` to your AltimeterUI component
   - `Altitude Unit`: "" (empty, or " ft" if you want units)

### 4. Remove Old Scripts

Once the new system is working:
- Remove `SpeedTapeScroller` components
- Remove `TickTapePool` components
- Keep the old scripts as backup until you're satisfied with the new system

## Configuration Options

### Tape Settings
- **Units Per Tick**: Increment between tick marks (5 for 5 MPH/feet)
- **Tick Spacing**: Pixel distance between ticks (adjust to match your UI design)
- **Visible Ticks**: Number of tick marks in the pool (should cover your visible area)
- **Animation Smoothness**: How quickly the tape responds to value changes

### Display Settings
- **Value Format**: "F0" for whole numbers, "F1" for one decimal place
- **Update Interval**: How often to update (0.05 = 20 times per second)

## How It Works

1. **Data Source**: Gets real-time speed from `FlightData.airspeed` or altitude from `AltimeterUI.GetCurrentAltitude()`

2. **Smooth Animation**: Uses `Mathf.Lerp` to smoothly animate between current and target values

3. **Rounded Display**: Main label shows values rounded to nearest 5 MPH/feet

4. **Dynamic Ticks**: Tick marks are positioned and labeled dynamically based on current value

5. **Object Pooling**: Reuses tick mark objects for optimal performance

## Troubleshooting

### Common Issues:

1. **Ticks not appearing**:
   - Check that `tickPrefab` is assigned
   - Ensure tick prefab has TextMeshPro component on child object
   - Verify `tickContainer` is assigned

2. **Animation too fast/slow**:
   - Adjust `animationSmoothness` (higher = faster response)
   - Modify `updateInterval` (lower = more frequent updates)

3. **Wrong spacing**:
   - Adjust `tickSpacing` to match your UI design
   - Ensure `unitsPerTick` matches your desired increments

4. **Main label not updating**:
   - Check that `mainValueLabel` is assigned
   - Verify FlightData or AltimeterUI references are correct

### Debug Tips:

- Use Unity's Inspector to monitor the controller's values in real-time
- Check the Console for error messages during initialization
- Temporarily increase `updateInterval` to see if performance is an issue

## Performance Notes

- The system uses object pooling to minimize garbage collection
- Update intervals are optimized for smooth animation without excessive CPU usage
- Tick marks outside the visible area are automatically hidden

## Integration with Existing Systems

This controller integrates seamlessly with your existing:
- `FlightData` system for speed values
- `AltimeterUI` system for altitude values
- `AirspeedIndicatorUI` for consistent performance patterns

The main labels will show rounded values (nearest 5 units) while the tape animation remains smooth and responsive to actual flight data changes.
