# 🗺️ CHUNK SPAWNER SETUP GUIDE

## 🚨 **ISSUE IDENTIFIED: ChunkSpawner Configuration Missing**

The W/S key input issue was caused by the `ChunkSpawner` script throwing errors and preventing the game from running properly. The script has been fixed with proper error handling, but it needs to be configured in the Unity Inspector.

---

## 🔧 **REQUIRED SETUP STEPS**

### **Step 1: Locate ChunkSpawner GameObject**
1. In your Unity scene, find the GameObject with the `ChunkSpawner` component
2. Select it in the Hierarchy window
3. Look at the Inspector panel

### **Step 2: Assign Terrain Prefabs**
The `ChunkSpawner` needs the terrain chunk prefabs to spawn the world:

1. **Find the "Chunks" section** in the ChunkSpawner component
2. **Set Array Size** to `3` (for the 3 terrain chunks)
3. **Assign the prefabs**:
   - **Element 0**: `terrainRiverChunk01` (from Assets/Prefabs/)
   - **Element 1**: `terrainRiverChunk02` (from Assets/Prefabs/)
   - **Element 2**: `terrainRiverChunk03` (from Assets/Prefabs/)

### **Step 3: Assign Player Transform**
1. **Find the "Player" field** in the ChunkSpawner component
2. **Drag your PlayerShip GameObject** (the riverraid_hero) into this field
3. This tells the spawner where the player is for chunk management

### **Step 4: Optional - Configure Spawners**
If you have fuel and repair barge spawners:
1. **Fuel Spawner**: Assign if you have a `FuelBargeSpawner` component
2. **Health Spawner**: Assign if you have a `RepairBargeSpawner` component
3. **Leave empty if not using** - the script will handle null references safely

---

## ⚙️ **CONFIGURATION SETTINGS**

### **Recommended Values**:
```
Chunk Length: 300 (matches terrain chunk size)
Chunks Visible: 5 (keeps 5 chunks loaded at once)
```

### **Terrain Prefab Assignment**:
```
chunkPrefabs[0] = terrainRiverChunk01.prefab
chunkPrefabs[1] = terrainRiverChunk02.prefab  
chunkPrefabs[2] = terrainRiverChunk03.prefab
```

---

## 🎯 **VISUAL SETUP GUIDE**

### **Inspector Should Look Like This**:
```
ChunkSpawner (Script)
├── Pickups
│   ├── Fuel Spawner: [None or assigned]
│   └── Health Spawner: [None or assigned]
└── Chunks
    ├── Player: [PlayerShip GameObject]
    ├── Chunk Prefabs: Size = 3
    │   ├── Element 0: terrainRiverChunk01
    │   ├── Element 1: terrainRiverChunk02
    │   └── Element 2: terrainRiverChunk03
    ├── Chunk Length: 300
    └── Chunks Visible: 5
```

---

## 🔍 **TROUBLESHOOTING**

### **If You Still Get Errors**:

1. **"chunkPrefabs array is not assigned"**:
   - Make sure you've set the Array Size to 3
   - Drag all 3 terrain prefabs into the array slots
   - Ensure no array elements are empty (None)

2. **"Player transform is not assigned"**:
   - Find your PlayerShip GameObject in the scene
   - Drag it into the Player field in ChunkSpawner

3. **"Invalid prefab index" or "Prefab is null"**:
   - Check that all 3 prefab slots are filled
   - Verify the prefabs exist in Assets/Prefabs/
   - Make sure prefabs aren't corrupted

### **If W/S Keys Still Don't Work After Setup**:

1. **Check Console for New Errors**: After fixing ChunkSpawner, look for other error messages
2. **Verify Game is Running**: The game should start without errors now
3. **Test Input**: Press W and look for "Direct throttle input: 1" in Console
4. **Check FlightData Assignment**: Ensure RailMovementController has FlightData assigned

---

## 🚀 **TESTING CHECKLIST**

### **After Setup**:
- [ ] Game starts without ChunkSpawner errors
- [ ] Terrain chunks spawn around the player
- [ ] Player can move through the world
- [ ] W/S keys show input messages in Console
- [ ] Aircraft responds to throttle input
- [ ] Fuel system works with refueling

### **Expected Console Messages**:
```
✅ No ChunkSpawner errors
✅ "Direct throttle input: 1" when pressing W
✅ "Direct throttle input: -1" when pressing S
✅ Speed and throttle debug messages every 2 seconds
```

---

## 📋 **QUICK SETUP CHECKLIST**

1. [ ] Find ChunkSpawner GameObject in scene
2. [ ] Set chunkPrefabs array size to 3
3. [ ] Assign terrainRiverChunk01 to Element 0
4. [ ] Assign terrainRiverChunk02 to Element 1  
5. [ ] Assign terrainRiverChunk03 to Element 2
6. [ ] Assign PlayerShip to Player field
7. [ ] Set Chunk Length to 300
8. [ ] Set Chunks Visible to 5
9. [ ] Press Play and test W/S keys

---

## 🔮 **AFTER SETUP**

Once the ChunkSpawner is properly configured:
- **Terrain Generation**: World will generate dynamically as you fly
- **Input System**: W/S keys should work for throttle control
- **Fuel System**: Complete fuel depletion system will be operational
- **Performance**: Chunks will load/unload efficiently

---

**🗺️ Complete this setup and your input system should work perfectly! 🚁**
