# 🚁 UNIFIED FLIGHT SYSTEM - SETUP GUIDE

## 🎯 **OVERVIEW**

This guide will help you replace the conflicting flight controllers with a single, unified system that follows Unity best practices.

## 📋 **STEP-BY-STEP SETUP**

### **STEP 1: BACKUP YOUR CURRENT SETUP**
1. **Save your scene** (Ctrl+S)
2. **Make a backup** of your project folder (optional but recommended)

### **STEP 2: CLEAN UP CONFLICTING COMPONENTS**

**On the riverraid_hero GameObject:**
1. **Disable or Remove** these components:
   - ❌ RailMovementController
   - ❌ SimpleFlightController  
   - ❌ FlightSpeedController
   - ❌ FlightInputController
   
2. **Keep these components:**
   - ✅ FlightData (required)
   - ✅ PlayerShipFuel
   - ✅ PlayerShipHealth
   - ✅ WeaponInputController (if present)

### **STEP 3: ADD UNIFIED FLIGHT CONTROLLER**

1. **Select riverraid_hero** in hierarchy
2. **Add Component** → Search "UnifiedFlightController"
3. **Configure settings:**

```
Input Configuration:
├── Throttle Up Key: W
├── Throttle Down Key: S
└── Fire Key: Space

Control Settings:
├── Throttle Rate: 30
├── Mouse Yaw Sensitivity: 2
├── Mouse Pitch Sensitivity: 2
└── Control Smooth Time: 0.1

Physics Settings:
├── Drag Coefficient: 0.02
├── Slope Effect: 20
├── Banking Strength: 30
└── Banking Smooth Time: 0.2

Debug:
├── Enable Debug Logging: ☑
└── Show On Screen Debug: ☑
```

### **STEP 4: FIX ALTIMETER**

**Option A: Replace with CleanAltimeterUI (Recommended)**
1. **Find your current altimeter UI** (usually on Canvas)
2. **Disable the old AltimeterUI** component
3. **Add Component** → Search "CleanAltimeterUI"
4. **Configure:**
   ```
   Aircraft Reference: riverraid_hero (drag from hierarchy)
   Use Ground Relative Altitude: ☑
   Enable Debug Logging: ☑ (for testing)
   ```

**Option B: Keep existing AltimeterUI**
1. **Find AltimeterUI component**
2. **Set Aircraft Reference** to riverraid_hero
3. **Enable debug logging** to troubleshoot

### **STEP 5: TEST THE SYSTEM**

1. **Press Play**
2. **Check Console** for initialization message:
   ```
   UnifiedFlightController: Initialized on riverraid_hero with speed 20.0 MPH
   ```
3. **Test W/S keys** - should see throttle messages
4. **Test mouse movement** - should turn and bank
5. **Check altitude display** - should show positive values

## 🎮 **EXPECTED BEHAVIOR**

### **Controls:**
- **W Key** - Increase speed (throttle up)
- **S Key** - Decrease speed (throttle down)
- **Mouse X** - Yaw (turn left/right)
- **Mouse Y** - Pitch (nose up/down)
- **Space** - Fire weapons (if weapon system present)

### **Physics:**
- **Banking** - Aircraft rolls when turning
- **Drag** - Speed naturally decreases over time
- **Slope Effects** - Climbing slows down, diving speeds up
- **Speed Limits** - Clamped to FlightData min/max values

### **Debug Display:**
- **On-screen info** in top-left corner
- **Console logging** for all major events
- **Real-time speed/control feedback**

## 🔧 **TROUBLESHOOTING**

### **W/S Keys Not Working:**
1. **Check Console** for "UnifiedFlightController: Initialized" message
2. **Verify FlightData** component is on riverraid_hero
3. **Enable debug logging** and check for throttle messages
4. **Make sure no other controllers** are interfering

### **Altitude Showing Negative Values:**
1. **Use CleanAltimeterUI** instead of old AltimeterUI
2. **Check Ground Layer Mask** settings
3. **Enable debug logging** on altimeter
4. **Verify aircraft reference** is set correctly

### **Mouse Controls Not Working:**
1. **Check mouse sensitivity** settings (try higher values)
2. **Verify control smooth time** isn't too high
3. **Look for banking animation** (aircraft should roll when turning)

### **No Movement:**
1. **Check FlightData min/max speed** values
2. **Verify throttle rate** isn't too low
3. **Check for Console errors**

## 🏗️ **SYSTEM ARCHITECTURE**

```
CLEAN ARCHITECTURE:
UnifiedFlightController ──► FlightData ──► UI Systems
         ↑                      ↑              ↓
    Input System          Data Storage    (Speed/Alt/Fuel)
         ↑                      ↑              ↓
   W/S/Mouse/Space        Fuel/Health    Visual Feedback
```

**Benefits:**
- ✅ **Single source of truth** - no conflicts
- ✅ **Easy to debug** - one place to look
- ✅ **Expandable** - clean event system
- ✅ **Performant** - optimized update cycles
- ✅ **Maintainable** - well-documented code

## 🚀 **NEXT STEPS**

Once the basic system works:

1. **Add fuel depletion effects** to UnifiedFlightController
2. **Integrate weapon systems** via events
3. **Add advanced physics** (stalling, engine failure)
4. **Optimize performance** for mobile if needed

## 📝 **VALIDATION CHECKLIST**

- [ ] UnifiedFlightController added to riverraid_hero
- [ ] Old conflicting controllers disabled/removed
- [ ] FlightData component present and configured
- [ ] W/S keys control speed (visible in Console)
- [ ] Mouse controls turning and banking
- [ ] Altimeter shows positive altitude values
- [ ] No Console errors during play
- [ ] On-screen debug display visible
- [ ] Aircraft moves forward smoothly

---

**This unified system provides a solid foundation for your flight game with clean, maintainable code that follows Unity best practices! 🚁✨**
