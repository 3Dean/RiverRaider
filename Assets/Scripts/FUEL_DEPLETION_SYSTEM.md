# Fuel Depletion System Implementation

## ✅ COMPLETED IMPLEMENTATION

This document describes the comprehensive fuel depletion system that causes the PlayerShip (riverraid_hero) to lose thrust and fall to the terrain when fuel is depleted.

## 🎯 **SYSTEM OVERVIEW**

### **Core Mechanics:**
1. **Gradual Engine Power Loss**: Engine doesn't cut out instantly - power reduces over 3 seconds
2. **Realistic Physics**: Plane maintains momentum but loses thrust, affected by gravity
3. **Gliding Mechanics**: Player can still control pitch/yaw but no throttle response
4. **Stall Behavior**: At low speeds, control effectiveness is reduced
5. **Recovery System**: Engine restarts when fuel is restored at fuel barges

## 🔧 **MODIFIED FILES**

### **1. FlightData.cs** - Enhanced with Engine Physics
**Location**: `Assets/Scripts/Data/FlightData.cs`

**New Parameters:**
```csharp
[Header("Fuel Depletion Physics")]
public float gravityForce = 9.81f;           // Gravity when out of fuel
public float enginePowerFadeTime = 3f;      // Time for engine to fade
public float stallSpeed = 15f;              // Speed below which stall occurs
public float glideDragCoefficient = 0.05f;  // Drag when gliding

[Header("Engine State (runtime)")]
public float enginePowerMultiplier = 1f;    // Current engine power (0-1)
public bool isEngineRunning = true;         // Engine status
```

**New Methods:**
- `SetEnginePower(float)` - Controls engine power multiplier
- `GetEffectiveThrottlePower(float)` - Applies fuel depletion to throttle
- `IsStalling()` - Checks if speed is below stall threshold
- `IsGliding()` - Checks if gliding without power
- `RestartEngine()` - Restarts engine when fuel is restored

### **2. PlayerShipFuel.cs** - Fuel State Management
**Location**: `Assets/Scripts/PlayerShipFuel.cs`

**New Features:**
- **Fuel State Enum**: Normal, Low, Depleted, Refueling
- **Engine Power Fade Timer**: Gradual power reduction over time
- **State Transition Handling**: Smooth transitions between fuel states
- **Recovery Logic**: Engine restart when fuel is restored

**Key Methods:**
- `UpdateFuelState()` - Tracks current fuel state
- `HandleFuelDepletion()` - Manages engine power fade and recovery

### **3. RailMovementController.cs** - Physics Integration
**Location**: `Assets/Scripts/Movement/RailMovementController.cs`

**Enhanced Physics:**
- **Fuel-Dependent Throttle**: `data.GetEffectiveThrottlePower(rawThrottle)`
- **Gravity Application**: Downward force when engine is off
- **Glide Drag**: Different drag coefficient for unpowered flight
- **Stall Mechanics**: Reduced control at low speeds
- **Control Degradation**: Pitch/yaw effectiveness reduced during stall

## 🚀 **SYSTEM BEHAVIOR**

### **Phase 1: Normal Operation**
- **Fuel State**: Normal (>20%)
- **Engine Power**: 100%
- **Throttle Response**: Full effectiveness
- **Physics**: Standard flight mechanics

### **Phase 2: Low Fuel Warning**
- **Fuel State**: Low (≤20%)
- **Engine Power**: 100%
- **Throttle Response**: Full effectiveness
- **Visual Feedback**: Low fuel warnings (can be extended with UI)

### **Phase 3: Fuel Depletion**
- **Fuel State**: Depleted (0%)
- **Engine Power**: Gradually fades from 100% to 0% over 3 seconds
- **Throttle Response**: Progressively reduced
- **Physics**: Gravity begins to take effect

### **Phase 4: Engine Failure**
- **Fuel State**: Depleted
- **Engine Power**: 0%
- **Throttle Response**: None
- **Physics**: Full gravity, glide drag, potential stall

### **Phase 5: Recovery**
- **Fuel State**: Refueling
- **Engine Power**: Instantly restored to 100%
- **Throttle Response**: Full effectiveness restored
- **Physics**: Normal flight mechanics resume

## 🎮 **GAMEPLAY MECHANICS**

### **Realistic Flight Physics:**
```csharp
// Gravity application when engine fails
Vector3 gravityVector = Vector3.down * data.gravityForce * dt;
transform.Translate(gravityVector, Space.World);

// Glide drag (higher than powered flight)
float glideDragChange = data.glideDragCoefficient * currentSpeed * currentSpeed * dt;
currentSpeed -= glideDragChange;
```

### **Stall Behavior:**
```csharp
if (data.IsStalling())
{
    // Reduce control effectiveness
    smoothYaw *= 0.3f;
    smoothPitch *= 0.3f;
    
    // Additional downward force
    Vector3 stallForce = Vector3.down * data.gravityForce * 0.5f * dt;
    transform.Translate(stallForce, Space.World);
}
```

### **Engine Power Fade:**
```csharp
// Gradually reduce engine power over time
enginePowerFadeTimer += Time.deltaTime;
float fadeProgress = enginePowerFadeTimer / flightData.enginePowerFadeTime;
float enginePower = Mathf.Lerp(1f, 0f, fadeProgress);
flightData.SetEnginePower(enginePower);
```

## 🔍 **DEBUGGING FEATURES**

### **Console Logging:**
- **Fuel Depletion**: "FUEL DEPLETED! Engine power failing..."
- **Engine Failure**: "ENGINE FAILURE - No fuel remaining!"
- **Power Fade**: "ENGINE FAILING! Power at 45% - Find fuel barge!"
- **Gliding**: "GLIDING - Speed: 85 MPH, Engine: OFF"
- **Stalling**: "STALLING! Speed: 12 MPH (below 15 MPH)"
- **Recovery**: "ENGINE RESTARTED - Fuel restored!"

### **Test Methods:**
```csharp
[ContextMenu("Test Consume Fuel (20)")]  // Instant fuel drain
[ContextMenu("Test Refuel to Full")]     // Instant refuel
[ContextMenu("Show Fuel Consumption Rate")] // Display current consumption
```

## ⚙️ **CONFIGURATION SETTINGS**

### **Recommended Settings:**
```csharp
// FlightData settings for realistic behavior
gravityForce = 9.81f;           // Standard Earth gravity
enginePowerFadeTime = 3f;       // 3-second engine fade
stallSpeed = 15f;               // Stall below 15 MPH
glideDragCoefficient = 0.05f;   // Higher drag when gliding

// Fuel consumption (from existing system)
baseFuelConsumption = 1f;       // Base fuel usage
speedFuelMultiplier = 0.02f;    // Speed-dependent consumption
```

### **Tuning Parameters:**
- **Faster Engine Fade**: Reduce `enginePowerFadeTime` to 1-2 seconds
- **More Dramatic Stall**: Increase stall speed to 20-25 MPH
- **Stronger Gravity**: Increase `gravityForce` to 15-20
- **Better Gliding**: Reduce `glideDragCoefficient` to 0.03

## 🎯 **TESTING SCENARIOS**

### **1. Normal Fuel Depletion Test:**
1. Fly normally until fuel runs out
2. Observe gradual engine power fade over 3 seconds
3. Experience gravity-based falling
4. Test control effectiveness during glide/stall
5. Refuel at fuel barge and observe engine restart

### **2. High-Speed Fuel Depletion:**
1. Accelerate to maximum speed
2. Let fuel deplete at high speed
3. Observe momentum maintenance with gravity effect
4. Test gliding behavior at high speed

### **3. Low-Speed Stall Test:**
1. Reduce speed to near stall threshold
2. Let fuel deplete
3. Experience stall behavior with reduced control
4. Observe additional downward force during stall

### **4. Recovery Test:**
1. Glide to a fuel barge while out of fuel
2. Enter fuel barge trigger zone
3. Observe immediate engine restart
4. Test return to normal flight mechanics

## 🚨 **TROUBLESHOOTING**

### **Engine Not Losing Power:**
- Check that `PlayerShipFuel` has `FlightData` reference
- Verify `HandleFuelDepletion()` is being called
- Ensure `SetEnginePower()` is reducing `enginePowerMultiplier`

### **Plane Not Falling:**
- Check `gravityForce` setting in FlightData
- Verify gravity is being applied in `RailMovementController`
- Ensure `isEngineRunning` is false when out of fuel

### **Controls Not Affected:**
- Check stall speed threshold
- Verify control reduction in stall condition
- Ensure `GetEffectiveThrottlePower()` is being used

### **Engine Not Restarting:**
- Check fuel barge collision detection
- Verify `RestartEngine()` is being called
- Ensure fuel is actually being added during refueling

## ✨ **FINAL RESULT**

The system provides:
- **Realistic fuel depletion mechanics** with gradual engine failure
- **Physics-based falling** with gravity and drag effects
- **Stall behavior** at low speeds with reduced control
- **Gliding mechanics** allowing some control without thrust
- **Recovery system** with immediate engine restart at fuel barges
- **Comprehensive debugging** with detailed console logging
- **Configurable parameters** for different difficulty levels

## 🎮 **PLAYER EXPERIENCE**

Players will experience:
1. **Tension**: Fuel management becomes critical for survival
2. **Realism**: Authentic aircraft behavior when fuel is depleted
3. **Challenge**: Must plan routes around fuel barges
4. **Recovery**: Hope for salvation when reaching fuel barges while gliding
5. **Consequence**: Real penalty for poor fuel management

The system transforms fuel from a simple resource into a critical flight mechanic that directly affects aircraft performance and player survival.
