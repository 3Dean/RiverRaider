# Aircraft Shake Setup - Step-by-Step Guide

## 🎯 **CURRENT SITUATION:**
Looking at your Unity hierarchy, I can see:
- `riverraid_hero` (main aircraft object)
- It already has some child objects (FirePoint, etc.)
- The aircraft mesh/model is likely directly on the riverraid_hero object

## 🔧 **STEP-BY-STEP SETUP:**

### **Step 1: Create ShakeContainer**
1. **In Unity Hierarchy**, right-click on `riverraid_hero`
2. **Select**: `Create Empty`
3. **Rename** the new empty GameObject to `ShakeContainer`
4. **Reset its transform**: In Inspector, click the gear icon next to Transform → Reset

### **Step 2: Move the Aircraft Visual**
The aircraft visual (mesh) is currently on the main `riverraid_hero` object. We need to move it:

1. **Select `riverraid_hero`** in hierarchy
2. **In the Inspector**, look for these components:
   - `Mesh Filter` (contains the aircraft mesh)
   - `Mesh Renderer` (renders the aircraft)
   - Any `Material` references

3. **Copy these components**:
   - Right-click on `Mesh Filter` → Copy Component
   - Right-click on `Mesh Renderer` → Copy Component

4. **Select `ShakeContainer`** (the empty child you just created)
5. **Paste the components**:
   - Right-click in Inspector → Paste Component As New (for Mesh Filter)
   - Right-click in Inspector → Paste Component As New (for Mesh Renderer)

6. **Remove from main object**:
   - Select `riverraid_hero` again
   - Remove the `Mesh Filter` and `Mesh Renderer` components (right-click → Remove Component)

### **Step 3: Add Shake Script**
1. **Select `ShakeContainer`** in hierarchy
2. **In Inspector**, click `Add Component`
3. **Search for**: `AircraftShakeController`
4. **Add the component**

### **Step 4: Configure the Shake**
The AircraftShakeController should auto-find your FlightData. Set these values:
```
Min Shake Speed: 20
Max Shake Speed: 200
Max Shake Intensity: 0.15
Shake Frequency: 15
Enable Position Shake: ✓
Enable Rotation Shake: ✓
Rotation Shake Multiplier: 2
Shake Smooth Time: 0.1
```

## 🎯 **FINAL HIERARCHY SHOULD LOOK LIKE:**
```
riverraid_hero (main aircraft - has all your existing scripts)
├── FirePoint (your existing child)
├── ShakeContainer (NEW - has AircraftShakeController + Mesh components)
└── [any other existing children]
```

## 🔍 **ALTERNATIVE METHOD (If Above is Confusing):**

### **Simple Approach:**
1. **Select `riverraid_hero`** in hierarchy
2. **Add Component** → `AircraftShakeController` directly to riverraid_hero
3. **Test if it works** - if the aircraft still moves forward, you're good!
4. **If movement stops**, remove the component and use the hierarchy method above

## ⚠️ **TROUBLESHOOTING:**

### **If Aircraft Disappears:**
- Check that `ShakeContainer` has both `Mesh Filter` and `Mesh Renderer`
- Make sure the mesh is assigned in the Mesh Filter
- Check that materials are assigned in Mesh Renderer

### **If Aircraft Doesn't Move:**
- Make sure `RailMovementController` is still on the main `riverraid_hero` object
- Verify that `FlightData` is still on the main `riverraid_hero` object
- Check that no movement scripts were accidentally moved to `ShakeContainer`

### **If No Shake Visible:**
- Use the test methods: Right-click `AircraftShakeController` → `Test Heavy Shake`
- Check that `Max Shake Intensity` > 0
- Verify the component found your `FlightData`

## 🚀 **QUICK TEST:**
1. **Play the game**
2. **Press W** to accelerate
3. **You should see**: Aircraft moves forward AND shakes more as speed increases
4. **Use test method**: Right-click AircraftShakeController → "Test Heavy Shake" to verify shake works

## 💡 **WHY THIS WORKS:**
- **Main riverraid_hero**: Handles all movement, physics, and game logic
- **ShakeContainer**: Only handles the visual shake effect
- **No interference**: Each system controls different aspects
- **Clean separation**: Movement vs visual effects

Let me know if you need help with any of these steps!
