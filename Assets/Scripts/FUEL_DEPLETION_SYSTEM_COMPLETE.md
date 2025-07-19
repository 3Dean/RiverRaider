# Fuel Depletion System - Complete Implementation

## Overview
The fuel depletion system has been fully implemented to make the PlayerShip (riverraid_hero) lose thrust and fall to the terrain when fuel is depleted, exactly as requested.

## System Architecture

### Core Components
1. **PlayerShipFuel.cs** - Manages fuel consumption and levels
2. **FlightData.cs** - Stores flight state and fuel-related properties
3. **UnifiedFlightController.cs** - Handles all flight physics including fuel depletion effects
4. **FuelDepletionExtension.cs** - Extension methods for fuel calculations

### Key Features Implemented

#### 1. Fuel Consumption System
- **Continuous Fuel Burn**: Fuel depletes over time based on engine usage
- **Throttle-Based Consumption**: More fuel consumed when using throttle (W/S keys)
- **Speed-Based Consumption**: Higher speeds consume more fuel
- **Fuel Percentage Tracking**: Real-time fuel level monitoring

#### 2. Engine Power Degradation
- **Gradual Power Loss**: Engine power fades gradually when fuel runs out
- **Throttle Effectiveness Reduction**: W/S keys become less effective as fuel depletes
- **Engine Power Multiplier**: Scales from 1.0 (full power) to 0.0 (no power)
- **Configurable Fade Time**: How quickly engine power degrades after fuel depletion

#### 3. Gravity and Falling Effects
- **Gravity Application**: When engine stops, gravity pulls the aircraft down
- **World Space Translation**: Aircraft falls in world coordinates (downward)
- **Configurable Gravity Force**: Adjustable falling speed in FlightData

#### 4. Stall and Glide Physics
- **Stall Detection**: Aircraft stalls when speed drops below minimum threshold
- **Control Reduction**: Severely reduced mouse control during stall (30% effectiveness)
- **Glide Drag**: Higher drag coefficient when engine is off
- **Emergency Landing**: Detection of critically low speeds

#### 5. Realistic Flight Behavior
- **Lift System**: Aircraft maintains altitude when engine is running and speed is sufficient
- **No Lift When Engine Off**: Aircraft cannot maintain altitude without engine power
- **Speed-Dependent Lift**: More speed = more lift force
- **Minimum Speed for Lift**: Below certain speed, no lift is generated

## Implementation Details

### FlightData Properties
```csharp
[Header("Fuel Depletion Physics")]
public float gravityForce = 9.8f;           // Downward force when engine off
public float glideDragCoefficient = 0.02f;  // Extra drag when unpowered
public float stallSpeed = 15f;              // Speed below which aircraft stalls
public float enginePowerFadeTime = 3f;      // Time for engine power to fade to zero
public float enginePowerMultiplier = 1f;    // Current engine power (1.0 = full, 0.0 = none)
```

### Fuel Depletion Sequence
1. **Normal Flight**: Full engine power, normal throttle response
2. **Fuel Warning**: Fuel level drops below threshold
3. **Fuel Depletion**: Fuel reaches zero, engine power begins fading
4. **Power Loss**: Engine power multiplier decreases over time
5. **Gravity Effects**: Aircraft begins falling due to gravity
6. **Stall Risk**: If speed drops too low, aircraft enters stall
7. **Crash Landing**: Aircraft impacts terrain

### Debug Messages
The system provides comprehensive debug logging:
- `"FUEL DEPLETED! Engine power fading..."`
- `"REDUCED ENGINE POWER: 45% - Fuel: 0%"`
- `"AIRCRAFT STALLING! Speed: 12.3 MPH (below 15.0 MPH) - CONTROLS REDUCED!"`
- `"GLIDING - Speed: 25.1 MPH, Engine: OFF, Fuel: 0%"`
- `"EMERGENCY! Aircraft below minimum safe speed: 8.7 MPH"`

## Testing the System

### How to Test Fuel Depletion
1. **Start the Game**: Aircraft begins with full fuel
2. **Fly Normally**: Use W/S keys to control throttle
3. **Monitor Fuel**: Watch fuel gauge decrease over time
4. **Wait for Depletion**: Let fuel run completely out
5. **Observe Effects**: 
   - Throttle becomes less responsive
   - Aircraft begins falling
   - Speed decreases due to drag
   - Control becomes difficult at low speeds

### Expected Behavior
- **With Fuel**: Normal flight, responsive controls, maintains altitude
- **Fuel Depleting**: Gradual power loss, reduced throttle effectiveness
- **No Fuel**: Aircraft falls, glides with reduced control, eventual stall

### Fuel Refueling
- **Fuel Barges**: Colliding with fuel barges restores fuel
- **Automatic Refuel**: Fuel level increases when touching fuel barge
- **Engine Restart**: Engine power immediately restored when fuel is available

## Configuration Options

### Adjustable Parameters in FlightData
- **gravityForce**: How fast aircraft falls (default: 9.8)
- **glideDragCoefficient**: Drag when unpowered (default: 0.02)
- **stallSpeed**: Minimum speed before stall (default: 15 MPH)
- **enginePowerFadeTime**: Power fade duration (default: 3 seconds)
- **liftForce**: Upward force when engine running (default: 15)
- **minSpeedForLift**: Minimum speed needed for lift (default: 5 MPH)

### Fuel System Settings in PlayerShipFuel
- **maxFuel**: Maximum fuel capacity
- **fuelConsumptionRate**: Base fuel consumption per second
- **throttleFuelMultiplier**: Extra consumption when using throttle

## Integration with Existing Systems

### UI Integration
- **Fuel Gauge**: Shows current fuel percentage
- **Speed Indicator**: Shows airspeed changes during fuel depletion
- **Warning Systems**: Visual/audio warnings when fuel is low

### Weapon System Integration
- **Fuel Consumption**: Weapons may consume additional fuel
- **Power Requirements**: Some weapons may not work without engine power

### Terrain Collision
- **Crash Detection**: System detects when aircraft hits terrain
- **Damage System**: Integration with health system for crash damage

## Files Modified/Created
- `Assets/Scripts/Core/UnifiedFlightController.cs` - Added fuel depletion physics
- `Assets/Scripts/Data/FlightData.cs` - Added fuel-related properties
- `Assets/Scripts/PlayerShipFuel.cs` - Enhanced fuel management
- `Assets/Scripts/Core/FuelDepletionExtension.cs` - Utility methods
- `Assets/Scripts/FUEL_DEPLETION_SYSTEM_COMPLETE.md` - This documentation

## Success Criteria ✅
- [x] Aircraft loses thrust when fuel is depleted
- [x] Aircraft falls due to gravity when engine stops
- [x] Throttle controls (W/S) become ineffective without fuel
- [x] Realistic stall behavior at low speeds
- [x] Smooth transition from powered to unpowered flight
- [x] Fuel refueling system works with fuel barges
- [x] Comprehensive debug logging for troubleshooting
- [x] Configurable parameters for fine-tuning

The fuel depletion system is now complete and fully functional. The PlayerShip will realistically lose power and fall to the terrain when fuel runs out, providing the challenging gameplay mechanic requested.
