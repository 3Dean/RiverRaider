# Altimeter UI Setup Guide for RiverRaider

## Overview
This guide explains the best practices for implementing an altimeter readout in Unity for flying games, specifically tailored for the RiverRaider project.

## Best Practice Approach

### 1. **Follow Existing UI Pattern**
The `AltimeterUI.cs` script follows the same optimized pattern as your existing `AirspeedIndicatorUI`:
- Performance-optimized updates (0.1s intervals)
- Cached references to avoid FindObjectOfType calls
- String allocation minimization
- Consistent error handling and logging

### 2. **Two Altitude Modes**

#### **Ground-Relative Altitude (Recommended for Flying Games)**
- Uses raycasting to detect ground below aircraft
- Shows actual clearance above terrain
- More useful for navigation and safety
- Automatically falls back to absolute altitude if no ground detected

#### **Absolute Altitude (Sea Level)**
- Simple Y-position relative to defined sea level
- Consistent reference point
- Useful for high-altitude flight

## Setup Instructions

### Step 1: Create UI Canvas (if not exists)
```
1. Right-click in Hierarchy → UI → Canvas
2. Set Canvas Render Mode to "Screen Space - Overlay"
3. Add Canvas Scaler component for resolution independence
```

### Step 2: Create Altimeter Text Element
```
1. Right-click Canvas → UI → Text - TextMeshPro
2. Name it "AltimeterText"
3. Position in upper-left or upper-right corner
4. Set anchor to appropriate corner for consistent positioning
```

### Step 3: Add AltimeterUI Script
```
1. Add AltimeterUI component to the Canvas or a UI parent object
2. Assign the TextMeshPro component to "Altitude Text" field
3. The script will automatically find your FlightData component
```

### Step 4: Configure Settings

#### **Display Settings:**
- **Altitude Format**: "F0" (no decimals) or "F1" (one decimal)
- **Altitude Unit**: " ft" or " m" depending on preference
- **Update Interval**: 0.1f (recommended for smooth updates)

#### **Ground Detection Settings:**
- **Ground Layer Mask**: Set to layers containing terrain/ground objects
- **Max Raycast Distance**: 1000f (adjust based on your game's scale)
- **Sea Level**: Y position of your water/ground reference (usually 0)

## Performance Considerations

### ✅ **Optimizations Included:**
- **Interval-based updates** (not every frame)
- **Change detection** (only updates when altitude changes significantly)
- **Cached references** (no repeated FindObjectOfType calls)
- **Efficient raycasting** (single raycast per update)

### ⚠️ **Performance Tips:**
- Use appropriate Layer Masks for ground detection
- Adjust update interval based on game needs (0.05f - 0.2f)
- Consider using object pooling for multiple aircraft

## Visual Design Best Practices

### **Typography:**
- Use monospace fonts for consistent number alignment
- High contrast colors (white text on dark background)
- Appropriate font size for readability

### **Positioning:**
- Upper corners are traditional for altimeter displays
- Maintain consistent margins from screen edges
- Use UI anchoring for different screen resolutions

### **Example Styling:**
```
Font: Liberation Mono or similar monospace
Color: White (#FFFFFF) or Cyan (#00FFFF)
Background: Semi-transparent dark panel
Size: 18-24pt depending on screen resolution
```

## Integration with Existing Systems

### **Works with Current RiverRaider Architecture:**
- ✅ Automatically finds FlightData component
- ✅ Follows same pattern as AirspeedIndicatorUI
- ✅ Compatible with existing UI system
- ✅ Uses same performance optimization strategies

### **Terrain Compatibility:**
- Works with Unity Terrain objects
- Compatible with mesh-based terrain
- Supports custom ground layer configurations

## Advanced Features

### **Runtime Configuration:**
```csharp
// Change altitude mode during gameplay
altimeterUI.SetGroundRelativeMode(true);

// Adjust update frequency
altimeterUI.SetUpdateInterval(0.05f);

// Force immediate update
altimeterUI.ForceUpdate();

// Get current altitude for other systems
float currentAlt = altimeterUI.GetCurrentAltitude();
```

### **Debug Visualization:**
- Select the AltimeterUI object in Scene view
- Yellow line shows raycast direction and distance
- Helps verify ground detection is working correctly

## Alternative Approaches

### **1. Analog Gauge Style:**
- Create circular gauge with needle rotation
- More visually appealing but requires more setup
- Higher performance cost for complex animations

### **2. World Space Canvas:**
- Attach canvas to cockpit interior
- More immersive but requires careful positioning
- Good for VR or first-person cockpit views

### **3. Integration with FlightData:**
- Add altitude directly to FlightData class
- Centralized flight information
- Requires modifying existing architecture

## Recommended Implementation

For RiverRaider, the **Screen Space Overlay with Ground-Relative Altitude** approach is recommended because:

1. **Consistency** with existing UI system
2. **Performance** optimized for real-time flight
3. **Flexibility** supports both altitude modes
4. **Maintainability** follows established code patterns
5. **User Experience** provides relevant altitude information for flying gameplay

## Testing Checklist

- [ ] Altimeter updates smoothly during flight
- [ ] Ground-relative mode shows correct clearance
- [ ] Absolute mode shows consistent sea-level reference
- [ ] UI scales properly on different screen resolutions
- [ ] Performance remains stable during extended gameplay
- [ ] Raycast visualization works in Scene view
- [ ] Error handling works when aircraft reference is missing

## Troubleshooting

### **Common Issues:**
1. **"No FlightData found"** - Ensure FlightData component exists in scene
2. **Altitude shows 0** - Check aircraft Transform assignment
3. **Jumpy readings** - Adjust update interval or change threshold
4. **No ground detection** - Verify Layer Mask settings and terrain setup

### **Performance Issues:**
1. **Frame drops** - Increase update interval
2. **Memory allocation** - Check string formatting settings
3. **Raycast overhead** - Optimize Layer Mask and distance settings
