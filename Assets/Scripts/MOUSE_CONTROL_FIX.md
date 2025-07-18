# 🖱️ MOUSE CONTROL FIX - TROUBLESHOOTING GUIDE

## 🚨 **ISSUES IDENTIFIED**

Based on your screenshot, there were two main problems:

1. **Mouse Input Not Working** - Mouse X/Y showing 0.00
2. **Plane Dropping Immediately** - Falling to -65ft on start

## ✅ **FIXES APPLIED**

### **Fix 1: Mouse Input**
**Problem:** Cursor might be locked or mouse sensitivity too low
**Solution:** Added cursor unlock and mouse input debugging to UnifiedFlightController

```csharp
// Mouse input for turning - ensure cursor is unlocked for flight games
if (Cursor.lockState != CursorLockMode.None)
{
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

// Debug mouse input
if (enableDebugLogging && (Mathf.Abs(currentMouseX) > 0.01f || Mathf.Abs(currentMouseY) > 0.01f))
{
    Debug.Log($"Mouse Input: X={currentMouseX:F2}, Y={currentMouseY:F2}");
}
```

### **Fix 2: Immediate Dropping**
**Problem:** FuelDepletionExtension was applying gravity even when engine running
**Solution:** Added better engine status debugging and logging

## 🧪 **TESTING STEPS**

### **Step 1: Test Mouse Input**
1. **Press Play**
2. **Move mouse** - you should now see debug messages in Console:
   ```
   Mouse Input: X=1.23, Y=-0.45
   ```
3. **Check debug display** - Mouse X/Y values should change

### **Step 2: Test Flight Controls**
1. **W Key** - Speed should increase
2. **S Key** - Speed should decrease  
3. **Mouse Movement** - Aircraft should turn and pitch
4. **Check Console** for throttle messages:
   ```
   Throttle UP: Speed 25.0 MPH (Δ5.0)
   ```

### **Step 3: Verify Engine Status**
1. **Check Console** for engine status:
   ```
   FuelDepletionExtension: Initialized with 100.0 fuel, Engine: RUNNING
   Engine Status: RUNNING | Fuel: 100.0
   ```

## 🔧 **ADDITIONAL TROUBLESHOOTING**

### **If Mouse Still Not Working:**

**Option A: Increase Mouse Sensitivity**
- Select riverraid_hero in hierarchy
- Find UnifiedFlightController component
- Increase Mouse Yaw Sensitivity to 5-10
- Increase Mouse Pitch Sensitivity to 5-10

**Option B: Check Input Settings**
- Go to Edit → Project Settings → Input Manager
- Verify "Mouse X" and "Mouse Y" axes exist
- Check sensitivity values (should be around 0.1)

**Option C: Alternative Input Method**
- Try arrow keys instead of mouse temporarily
- Add keyboard pitch/yaw controls for testing

### **If Plane Still Drops:**

**Option A: Disable FuelDepletionExtension Temporarily**
- Uncheck the FuelDepletionExtension component
- Test basic flight without fuel effects
- Re-enable once basic flight works

**Option B: Check Initial Position**
- Make sure riverraid_hero starts above ground
- Set Y position to at least 50-100 units above terrain

**Option C: Verify FlightData**
- Check FlightData component has fuel > 0
- Verify minSpeed/maxSpeed are reasonable (10-120)

## 🎮 **EXPECTED BEHAVIOR AFTER FIXES**

### **Mouse Controls:**
- **Move mouse left/right** → Aircraft yaws (turns)
- **Move mouse up/down** → Aircraft pitches (nose up/down)
- **Banking** → Aircraft rolls when turning

### **Keyboard Controls:**
- **W Key** → Speed increases
- **S Key** → Speed decreases
- **Space** → Fires weapons

### **Debug Display:**
- **Mouse values** should change when moving mouse
- **Speed** should change with W/S keys
- **Position** should show positive altitude

## 🚀 **NEXT STEPS**

1. **Test the fixes** - Try the game again
2. **Check Console** for debug messages
3. **Report results** - Let me know what's working/not working
4. **Fine-tune settings** if needed

The mouse control fix should resolve the turning issue, and the engine status improvements should prevent the immediate dropping problem.

---

**If you're still having issues, please share:**
- Console messages when you move the mouse
- Current mouse sensitivity settings
- Whether FuelDepletionExtension is enabled
- Starting position of the aircraft
