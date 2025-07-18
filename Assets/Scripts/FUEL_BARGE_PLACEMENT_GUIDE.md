# Fuel Barge Manual Placement Guide

## ✅ STEP-BY-STEP IMPLEMENTATION

### **Step 1: Terrain Prefab Analysis**
- **terrainRiverChunk01**: Length 300 units (Z: 0 to 300)
- **River Center**: X = 0
- **Safe Y Position**: Around 8-10 units (above water surface)

### **Step 2: Remove Old Spawning System**
1. Open each terrain prefab in edit mode
2. Delete the "FuelSpawner" GameObject (contains FuelBargeSpawner script)
3. This removes the random spawning system

### **Step 3: Manual Fuel Barge Placement**

#### **terrainRiverChunk01.prefab:**
**Recommended Positions:**
- **Fuel Barge 1**: Position (0, 8, 75) - Early in chunk
- **Fuel Barge 2**: Position (0, 8, 225) - Later in chunk

#### **terrainRiverChunk02.prefab:**
**Recommended Positions:**
- **Fuel Barge 1**: Position (0, 8, 100) - Middle of chunk
- **Fuel Barge 2**: Position (0, 8, 250) - Near end

#### **terrainRiverChunk03.prefab:**
**Recommended Positions:**
- **Fuel Barge 1**: Position (0, 8, 50) - Early placement
- **Fuel Barge 2**: Position (0, 8, 200) - Later placement

### **Step 4: Placement Process**
1. **Open Prefab**: Double-click terrain prefab in Project window
2. **Delete FuelSpawner**: Remove the FuelSpawner GameObject
3. **Drag Fuel Barge**: Drag fuelbarge1.prefab into terrain hierarchy
4. **Position**: Set Transform position to recommended coordinates
5. **Duplicate**: Ctrl+D to create second fuel barge
6. **Reposition**: Move second barge to different Z position
7. **Save Prefab**: Ctrl+S to save changes

### **Step 5: Verification Checklist**
- ✅ FuelSpawner GameObject removed
- ✅ Fuel barges positioned in river center (X=0)
- ✅ Fuel barges at safe height (Y=8-10)
- ✅ Fuel barges spaced throughout chunk length
- ✅ No collision with terrain/mountains
- ✅ Fuel barges have "FuelBarge" tag
- ✅ Fuel barges have trigger colliders

### **Step 6: Testing**
1. **Play Mode**: Test fuel system in game
2. **Collision Test**: Verify fuel barges are reachable
3. **Refueling Test**: Confirm fuel system works
4. **Visual Check**: Ensure fuel barges are visible from flight altitude

## 🎯 **BENEFITS OF MANUAL PLACEMENT**
- ✅ **No Mountain Collisions**: Guaranteed safe positioning
- ✅ **Predictable Fuel Access**: Always in reachable locations
- ✅ **Gameplay Balance**: Controlled fuel distribution
- ✅ **Performance**: No runtime spawning calculations
- ✅ **Visual Consistency**: Logical river placement

## 🔧 **CUSTOMIZATION OPTIONS**
- **More Fuel Barges**: Add 3rd barge for easier gameplay
- **Fewer Fuel Barges**: Remove one for harder difficulty
- **Different Heights**: Adjust Y position for visual variety
- **Slight X Offset**: Move slightly off-center for realism
- **Rotation**: Rotate fuel barges to face downstream

## 🚨 **TROUBLESHOOTING**
- **Fuel Not Working**: Check "FuelBarge" tag is correct
- **Can't Reach**: Verify Y position isn't too high/low
- **Clipping**: Ensure no collision with terrain mesh
- **Not Visible**: Check fuel barge scale and materials
