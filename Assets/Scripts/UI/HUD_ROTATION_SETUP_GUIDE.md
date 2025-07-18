# HUD Rotation Setup Guide for RiverRaider

## Overview
This guide explains how to make your airspeed indicator and altimeter rotate with the aircraft's banking motion for a more immersive cockpit experience.

## Quick Setup Instructions

### Step 1: Add HUDRotationController to UI Elements

#### For Airspeed Indicator:
1. In the Hierarchy, find your Canvas → txt_AirSpeed (or similar)
2. Add Component → Scripts → HUDRotationController
3. The script will automatically find your FlightData component

#### For Altimeter:
1. In the Hierarchy, find your Canvas → txt_Altitude (or similar)  
2. Add Component → Scripts → HUDRotationController
3. The script will automatically find your FlightData component

### Step 2: Configure Rotation Settings

#### **Recommended Settings for Banking Effect:**
- ✅ **Use Roll Rotation**: True (this creates the banking effect)
- ❌ **Use Yaw Rotation**: False 
- ❌ **Use Pitch Rotation**: False
- **Roll Multiplier**: 1.0 (matches aircraft banking exactly)
- **Max Rotation Angle**: 45° (prevents extreme tilting)
- **Use Smooth Rotation**: True
- **Rotation Smooth Time**: 0.2s (smooth, responsive feel)

## How It Works

### **Banking Effect:**
When your aircraft banks left or right, the HUD elements will rotate to match:
- **Aircraft banks left** → HUD elements tilt left
- **Aircraft banks right** → HUD elements tilt right
- **Aircraft flies level** → HUD elements return to horizontal

### **Data Source:**
The script uses your existing `FlightData.roll` value, which is already calculated and updated in your flight system.

## Advanced Configuration Options

### **Rotation Types:**
- **Roll Rotation** (Banking): ✅ Recommended for immersive effect
- **Yaw Rotation** (Turning): Optional, subtle effect
- **Pitch Rotation** (Climbing/Diving): Optional, can be disorienting

### **Multiplier Settings:**
- **Roll Multiplier = 1.0**: HUD rotates exactly with aircraft
- **Roll Multiplier = 0.5**: HUD rotates half as much (more subtle)
- **Roll Multiplier = 1.5**: HUD rotates more than aircraft (more dramatic)

### **Smoothing Options:**
- **Smooth Rotation = True**: Gradual, realistic movement
- **Smooth Rotation = False**: Instant, snappy movement
- **Smooth Time**: Lower = faster response, Higher = smoother motion

## Visual Effect Examples

### **Banking Left (Roll = -30°):**
```
Normal HUD:     [100 MPH]  [1000 ft]
Rotated HUD:       [100 MPH]
                      [1000 ft]
```

### **Banking Right (Roll = +30°):**
```
Normal HUD:     [100 MPH]  [1000 ft]
Rotated HUD:  [100 MPH]
            [1000 ft]
```

## Setup for Different UI Layouts

### **Individual Elements (Recommended):**
Add HUDRotationController to each UI element separately:
- txt_AirSpeed gets its own HUDRotationController
- txt_Altitude gets its own HUDRotationController
- Each can have different settings if desired

### **Group Rotation:**
Add HUDRotationController to a parent GameObject containing multiple UI elements:
- Create empty GameObject as parent
- Move airspeed and altimeter under this parent
- Add HUDRotationController to parent
- All children rotate together

## Performance Considerations

### **Optimizations Included:**
- Uses existing FlightData.roll (no additional calculations)
- Smooth rotation uses efficient Vector3.SmoothDamp
- No string allocations or expensive operations
- Minimal impact on frame rate

### **Performance Tips:**
- Use smooth rotation for better visual quality
- Avoid extreme rotation angles (stick to ±45°)
- Consider disabling on lower-end devices if needed

## Troubleshooting

### **Common Issues:**

#### **"No FlightData found"**
- Ensure FlightData component exists in scene
- Check that FlightData is on the aircraft GameObject

#### **HUD elements not rotating**
- Verify HUDRotationController is on UI elements (not aircraft)
- Check that UI elements have RectTransform components
- Ensure "Use Roll Rotation" is enabled

#### **Rotation feels wrong/inverted**
- Try negative Roll Multiplier (-1.0) to invert direction
- Check aircraft's roll calculation in FlightData

#### **Rotation too fast/slow**
- Adjust Roll Multiplier (0.5 = slower, 1.5 = faster)
- Modify Rotation Smooth Time for different feel

#### **Jerky/stuttering rotation**
- Enable "Use Smooth Rotation"
- Increase Rotation Smooth Time (try 0.3-0.5)
- Check that FlightData.roll is updating smoothly

## Testing Checklist

- [ ] HUD elements rotate when aircraft banks left
- [ ] HUD elements rotate when aircraft banks right  
- [ ] HUD elements return to level when aircraft levels off
- [ ] Rotation feels smooth and natural
- [ ] Maximum rotation angle is reasonable (not too extreme)
- [ ] Performance remains stable during flight
- [ ] Text remains readable during rotation

## Alternative Configurations

### **Subtle Effect (Recommended for beginners):**
- Roll Multiplier: 0.5
- Max Rotation Angle: 30°
- Smooth Time: 0.3s

### **Dramatic Effect (For experienced players):**
- Roll Multiplier: 1.0
- Max Rotation Angle: 60°
- Smooth Time: 0.1s

### **Cockpit Simulation:**
- Roll Multiplier: 1.0
- Use Pitch Rotation: True (0.2 multiplier)
- Max Rotation Angle: 45°
- Smooth Time: 0.2s

## Integration with Existing Systems

### **Compatible with:**
- ✅ AirspeedIndicatorUI
- ✅ AltimeterUI  
- ✅ FuelGaugeUI
- ✅ HealthBarUI
- ✅ Any UI element with RectTransform

### **Works with your current:**
- ✅ FlightData system
- ✅ UI optimization patterns
- ✅ Performance requirements
- ✅ Existing UI layout

The HUD rotation effect adds significant immersion to your flying game while maintaining the performance and code quality standards of your existing RiverRaider project.
