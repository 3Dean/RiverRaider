# 🏗️ MODULAR ARCHITECTURE SETUP GUIDE

## 🎯 **OVERVIEW**

This guide will help you set up the new modular architecture that separates concerns into distinct, organized systems:

- **TerrainManager** - Handles terrain chunk spawning
- **PickupManager** - Coordinates fuel/repair barge spawning  
- **InputManager** - Centralized input handling

---

## 🗂️ **NEW HIERARCHY STRUCTURE**

### **Recommended Scene Organization**:
```
Scene Root
├── Managers/
│   ├── TerrainManager (ChunkSpawner script)
│   ├── PickupManager (PickupManager script)
│   └── InputManager (FlightInputController script)
├── Player/
│   └── riverraid_hero (PlayerShip components)
├── UI/
│   └── HUD elements
└── Environment/
    └── Lighting, etc.
```

---

## 🔧 **STEP-BY-STEP SETUP**

### **Step 1: Create Manager GameObjects**

1. **Create empty GameObjects** in your scene:
   - Right-click in Hierarchy → Create Empty
   - Name them: `TerrainManager`, `PickupManager`, `InputManager`

2. **Optional: Create parent container**:
   - Create empty GameObject named `Managers`
   - Drag the 3 manager objects under it

### **Step 2: Configure TerrainManager**

1. **Select TerrainManager GameObject**
2. **Add ChunkSpawner component** (if not already present)
3. **Configure in Inspector**:
   ```
   Terrain Configuration:
   ├── Player: [Drag riverraid_hero GameObject here]
   ├── Chunk Prefabs: Size = 3
   │   ├── Element 0: terrainRiverChunk01
   │   ├── Element 1: terrainRiverChunk02
   │   └── Element 2: terrainRiverChunk03
   ├── Chunk Length: 300
   └── Chunks Visible: 5
   
   Debug:
   └── Show Debug Logs: ☐ (enable for troubleshooting)
   ```

### **Step 3: Configure PickupManager**

1. **Select PickupManager GameObject**
2. **Add PickupManager component**
3. **Configure in Inspector**:
   ```
   Repair Barge Configuration:
   ├── Repair Barge Prefab: [Assign your repair barge prefab]
   ├── Min Distance Between Pickups: 150
   ├── Max Distance Between Repair Barges: 400
   └── Repair Barge Spawn Chance: 0.15
   
   Spawn Settings:
   ├── Spawn Height: 8
   └── Chunk Length: 300
   
   Debug:
   ├── Show Debug Logs: ☐ (enable for troubleshooting)
   └── Show Gizmos: ☑ (shows pickup positions in Scene view)
   ```

### **Step 4: Configure InputManager**

1. **Select InputManager GameObject**
2. **Add FlightInputController component**
3. **Configure in Inspector**:
   ```
   Input Configuration:
   ├── Throttle Up Key: W
   ├── Throttle Down Key: S
   └── Fire Key: Space
   
   Input Sensitivity:
   ├── Throttle Input Sensitivity: 1
   └── Mouse Input Sensitivity: 2
   
   Debug:
   └── Show Input Debug: ☐ (enable to see input overlay)
   ```

### **Step 5: Clean Up Redundant Components**

1. **Remove duplicate ChunkSpawner components** from other GameObjects
2. **Remove old FuelBargeSpawner and RepairBargeSpawner** components (if using dynamic spawning)
3. **Keep manual fuel barges** in terrain prefabs (as requested)

---

## 🏷️ **REQUIRED TAGS**

Make sure these tags exist in your project:

### **In Tags & Layers (Window → Tags and Layers)**:
- **FuelBarge** - For fuel barge GameObjects
- **RepairBarge** - For repair barge GameObjects

### **How to Create Tags**:
1. Go to **Window → Tags and Layers**
2. Click **+** next to Tags
3. Add: `FuelBarge` and `RepairBarge`
4. **Apply tags to your prefabs**:
   - Select fuel barge prefab → Set Tag to "FuelBarge"
   - Select repair barge prefab → Set Tag to "RepairBarge"

---

## ⚙️ **SYSTEM INTERACTIONS**

### **How the Systems Work Together**:

1. **TerrainManager** spawns terrain chunks
2. **TerrainManager** fires events when chunks spawn/destroy
3. **PickupManager** listens to terrain events
4. **PickupManager** scans new chunks for manual fuel barges
5. **PickupManager** spawns repair barges avoiding fuel barge positions
6. **InputManager** processes all input and distributes to systems
7. **RailMovementController** receives input events for movement

### **Event Flow**:
```
TerrainManager → OnChunkSpawned → PickupManager
InputManager → OnThrottleChanged → RailMovementController
InputManager → OnFirePressed → WeaponSystem (if implemented)
```

---

## 🔍 **TROUBLESHOOTING**

### **Common Issues**:

1. **"TerrainManager: chunkPrefabs array is not assigned"**
   - Assign all 3 terrain prefabs to TerrainManager
   - Make sure array size is set to 3

2. **"PickupManager: repairBargePrefab is not assigned"**
   - Assign your repair barge prefab to PickupManager
   - Or disable repair barge spawning if not needed

3. **"FlightInputController: No RailMovementController found"**
   - Make sure RailMovementController is on your player GameObject
   - Check that player GameObject is active in scene

4. **Input not working**
   - Verify InputManager GameObject is active
   - Check that FlightInputController component is enabled
   - Enable "Show Input Debug" to see input detection

5. **Repair barges spawning too close to fuel barges**
   - Increase "Min Distance Between Pickups" value
   - Check that fuel barges have "FuelBarge" tag

### **Debug Features**:

- **TerrainManager**: Enable "Show Debug Logs" to see chunk spawning
- **PickupManager**: Enable "Show Debug Logs" and "Show Gizmos" to visualize pickup placement
- **InputManager**: Enable "Show Input Debug" to see input overlay

---

## 🚀 **TESTING CHECKLIST**

### **After Setup**:
- [ ] Game starts without errors
- [ ] Terrain chunks spawn and despawn properly
- [ ] W/S keys control throttle (check Console for input messages)
- [ ] Mouse controls aircraft turning
- [ ] Fuel barges remain in terrain (manual placement preserved)
- [ ] Repair barges spawn dynamically, avoiding fuel barges
- [ ] No duplicate spawner components in scene

### **Expected Console Messages**:
```
✅ "TerrainManager: Initialized with X chunks"
✅ "PickupManager: Initialized and listening for terrain events"  
✅ "FlightInputController: Initialized and ready for input"
✅ "FlightInputController: Throttle UP input detected" (when pressing W)
✅ "PickupManager: Found manual fuel barge at (position)" (when chunks spawn)
```

---

## 📊 **PERFORMANCE BENEFITS**

### **Modular Architecture Advantages**:

1. **Separation of Concerns**: Each system has a single responsibility
2. **Easy Debugging**: Issues are isolated to specific managers
3. **Maintainability**: Changes to one system don't affect others
4. **Scalability**: Easy to add new systems or modify existing ones
5. **Event-Driven**: Loose coupling between systems via events
6. **Organization**: Clean hierarchy makes project navigation easier

---

## 🔮 **FUTURE ENHANCEMENTS**

### **Easy to Add Later**:
- **EnemyManager**: Handle enemy spawning and AI
- **WeaponManager**: Centralize weapon systems
- **AudioManager**: Coordinate all game audio
- **EffectsManager**: Handle particle effects and visual feedback
- **GameManager**: Overall game state management

---

## 📋 **QUICK SETUP CHECKLIST**

1. [ ] Create 3 empty GameObjects: TerrainManager, PickupManager, InputManager
2. [ ] Add ChunkSpawner to TerrainManager
3. [ ] Add PickupManager to PickupManager GameObject  
4. [ ] Add FlightInputController to InputManager
5. [ ] Configure TerrainManager with terrain prefabs and player reference
6. [ ] Configure PickupManager with repair barge prefab
7. [ ] Configure InputManager with input keys
8. [ ] Create FuelBarge and RepairBarge tags
9. [ ] Apply tags to prefabs
10. [ ] Remove duplicate/old spawner components
11. [ ] Test game - W/S keys should work, terrain should spawn
12. [ ] Enable debug options if troubleshooting needed

---

**🏗️ Your project is now organized with a clean, modular architecture! 🚁**
