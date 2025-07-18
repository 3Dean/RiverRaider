# 🎮 INPUT SYSTEM SETUP GUIDE

## 🚨 **ISSUE RESOLVED: W/S Keys Not Working**

The throttle input issue has been fixed by prioritizing direct key input over the event system in `RailMovementController.cs`.

---

## 🔧 **CURRENT INPUT SYSTEM STATUS**

### **✅ WORKING CONTROLS**
- **W Key**: Throttle Up (Forward thrust)
- **S Key**: Throttle Down (Reverse thrust)  
- **Mouse X**: Yaw (Left/Right turning)
- **Mouse Y**: Pitch (Up/Down movement)
- **Left Shift**: Boost (if FlightInputController is active)
- **Space**: Fire Weapon (if FlightInputController is active)

### **🔄 DUAL INPUT SYSTEM**
The game now uses a **dual input system** for maximum reliability:

1. **Primary**: Direct key detection in `RailMovementController.cs`
2. **Fallback**: Event system via `FlightInputController.cs`

---

## 🎯 **SETUP INSTRUCTIONS**

### **Option 1: Quick Fix (Current Status)**
The throttle controls (W/S) now work directly without requiring additional setup. The system will automatically use direct input detection.

### **Option 2: Full Event System (Recommended)**
For complete functionality including boost and weapons:

1. **Add FlightInputController to Scene**:
   - Create an empty GameObject in your scene
   - Name it "InputManager" 
   - Add the `FlightInputController` component
   - Enable "Lock Cursor" if desired

2. **Verify Setup**:
   - Press Play
   - Check Console for "Direct throttle input" messages when pressing W/S
   - All controls should work smoothly

---

## 🎮 **CONTROL MAPPING**

### **Flight Controls**
```
W Key           → Throttle Up (Forward)
S Key           → Throttle Down (Reverse)
Mouse Movement  → Look Around (Pitch/Yaw)
Left Shift      → Boost (requires FlightInputController)
Space Bar       → Fire Weapon (requires FlightInputController)
```

### **Debug Controls**
- Console will show throttle input when W/S is pressed
- Speed and fuel information logged every 2 seconds
- Fuel depletion warnings when engine stops

---

## 🔍 **TROUBLESHOOTING**

### **If W/S Keys Still Don't Work**:

1. **Check Unity Input Settings**:
   - Go to Edit → Project Settings → Input Manager
   - Verify "Vertical" axis has W/S keys mapped
   - Ensure no conflicting input bindings

2. **Verify Script Assignment**:
   - Select your PlayerShip GameObject
   - Ensure `RailMovementController` component is attached
   - Verify `FlightData` is assigned in the inspector

3. **Console Debugging**:
   - Open Console window (Window → General → Console)
   - Press W key and look for "Direct throttle input: 1" message
   - If no message appears, there may be a script error

### **If Mouse Look Doesn't Work**:

1. **Check Mouse Sensitivity**:
   - Go to Edit → Project Settings → Input Manager
   - Adjust "Mouse X" and "Mouse Y" sensitivity values

2. **Cursor Lock Issues**:
   - Press Escape to unlock cursor if stuck
   - Disable "Lock Cursor" in FlightInputController if needed

---

## 📋 **TECHNICAL DETAILS**

### **Input Processing Order**:
```
1. Direct Key Detection (Input.GetKey)
   ↓
2. Event System Fallback (FlightInputController)
   ↓  
3. Apply Fuel Depletion Effects
   ↓
4. Smooth Input Processing
   ↓
5. Apply to Aircraft Movement
```

### **Key Code Changes Made**:
- **RailMovementController.cs**: Added reliable direct input detection
- **Priority System**: Direct input overrides event system
- **Debug Logging**: Added throttle input confirmation messages
- **Fallback Logic**: Ensures input works even without FlightInputController

---

## 🚀 **TESTING CHECKLIST**

### **Basic Input Test**:
- [ ] W key increases throttle (check console for "Direct throttle input: 1")
- [ ] S key decreases throttle (check console for "Direct throttle input: -1")
- [ ] Mouse movement controls aircraft direction
- [ ] Aircraft responds smoothly to input changes

### **Fuel System Integration**:
- [ ] Throttle works when fuel is available
- [ ] Throttle becomes ineffective when fuel is depleted
- [ ] Engine stops when fuel reaches zero
- [ ] Refueling restores throttle functionality

### **Advanced Features** (if FlightInputController is active):
- [ ] Left Shift activates boost
- [ ] Space bar fires weapons
- [ ] Cursor locks properly in play mode

---

## 🔮 **FUTURE IMPROVEMENTS**

### **Potential Enhancements**:
- **Gamepad Support**: Add controller input mapping
- **Customizable Controls**: Allow players to remap keys
- **Input Buffering**: Smooth input during frame drops
- **Accessibility Options**: Alternative input methods
- **Mobile Touch Controls**: For mobile deployment

---

## 📞 **SUPPORT**

If you continue to experience input issues:

1. **Check Console**: Look for error messages or missing debug logs
2. **Verify Components**: Ensure all required scripts are attached
3. **Test in Build**: Sometimes editor behavior differs from builds
4. **Reset Input Settings**: Restore default Unity input settings if modified

---

**🎮 The input system is now robust and should handle all flight controls reliably! 🚁**
