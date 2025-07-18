# 🔍 INPUT SYSTEM DIAGNOSTIC

## 🎯 **CURRENT ISSUE**

W/S keys not working and altimeter broken after optimization changes.

## 🔧 **DIAGNOSTIC CHECKLIST**

### **1. Component Presence Check**
In Unity, verify these components exist:

**On riverraid_hero (Player Aircraft):**
- ✅ FlightData component
- ✅ FlightInputController component  
- ✅ FlightSpeedController component
- ✅ AltimeterUI component (or on UI Canvas)

### **2. FlightInputController Settings**
Check FlightInputController component:
```
Input Configuration:
├── Throttle Up Key: W
├── Throttle Down Key: S
└── Fire Key: Space

Debug:
└── Show Input Debug: ☑ (CHECK THIS!)
```

### **3. FlightSpeedController Settings**
Check FlightSpeedController component:
```
Speed Control Settings:
├── Throttle Rate: 30
├── Slope Multiplier: 50
└── Starting Speed: 20

Debug Settings:
└── Enable Debug Logging: ☑ (CHECK THIS!)
```

### **4. Console Debug Test**
1. **Enable debug logging** on both components
2. **Press W/S keys** in Play mode
3. **Check Console** for these messages:
   - "FlightInputController: Throttle UP input detected"
   - "Throttle Up - Speed: XX.X MPH"

### **5. FlightData Verification**
Check FlightData component values:
```
Speed Settings:
├── Airspeed: [Should change when W/S pressed]
├── Min Speed: 10
└── Max Speed: 100
```

## 🚨 **LIKELY PROBLEMS & SOLUTIONS**

### **Problem 1: Missing Components**
**Solution:** Add missing components to riverraid_hero

### **Problem 2: Event System Not Working**
**Solution:** Components might be on different GameObjects
- FlightInputController and FlightSpeedController should be on SAME GameObject

### **Problem 3: FlightData Not Found**
**Solution:** FlightSpeedController can't find FlightData
- Check Console for "No FlightData found in scene!" error

### **Problem 4: Altimeter Issues**
**Solution:** AltimeterUI can't find aircraft
- Make sure FlightData is on riverraid_hero
- Check AltimeterUI "Aircraft Reference" field

## 🛠️ **QUICK FIX STEPS**

1. **Select riverraid_hero** in hierarchy
2. **Add FlightInputController** if missing
3. **Add FlightSpeedController** if missing  
4. **Enable debug logging** on both
5. **Test W/S keys** and check Console
6. **Fix altimeter** by assigning aircraft reference

## 🎮 **EXPECTED DEBUG OUTPUT**

When working correctly, Console should show:
```
FlightInputController: Throttle UP input detected
Throttle Up - Speed: 25.3 MPH (Change: 1.5)
FlightInputController: Throttle DOWN input detected  
Throttle Down - Speed: 23.8 MPH (Change: -1.5)
```

---

**Follow this diagnostic to identify exactly what's broken! 🔍**
