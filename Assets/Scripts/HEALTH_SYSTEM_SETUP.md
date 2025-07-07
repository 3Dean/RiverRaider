# Health System Integration Guide

## ✅ COMPLETED INTEGRATION:

### 1. FlightData Enhanced
**Location**: `Assets/Scripts/Data/FlightData.cs`
**Added**:
- ✅ Health system variables (maxHealth, currentHealth, healAmount)
- ✅ Health management methods (TakeDamage, RecoverHealth, IsAlive, GetHealthPercentage)
- ✅ Automatic health initialization

### 2. HealthBarUI Created
**Location**: `Assets/Scripts/UI/HealthBarUI.cs`
**Features**:
- ✅ Optimized UI updates (0.1s intervals)
- ✅ Color-coded health display (Green→Yellow→Red)
- ✅ Performance optimizations
- ✅ Built-in test methods

### 3. PlayerShipHealth Updated
**Location**: `Assets/Scripts/PlayerShipHealth.cs`
**Changes**:
- ✅ Now uses FlightData for centralized health management
- ✅ Maintains damage/healing interface for other systems
- ✅ Enhanced death handling with optional effects

## 🔧 SETUP INSTRUCTIONS:

### Step 1: Configure FlightData
1. **Select riverraid_hero** in the hierarchy
2. **In FlightData component**, configure:
   - Max Health: 100
   - Heal Amount: 25 (health pack healing)

### Step 2: Set Up Health Bar UI
1. **Find your health bar slider** in the Canvas/UI
2. **Add HealthBarUI script** to the health bar GameObject
3. **Configure HealthBarUI**:
   - Health Slider: Drag your slider here
   - Flight Data: Drag riverraid_hero here
   - Colors: Green (healthy), Yellow (damaged), Red (critical)

### Step 3: Update PlayerShipHealth
1. **Select riverraid_hero** (or wherever PlayerShipHealth is)
2. **In PlayerShipHealth component**:
   - Flight Data: Should auto-find, or drag riverraid_hero
   - Death Effect: Optional explosion prefab
   - Disable On Death: Check if you want ship to disappear

### Step 4: Test the System
**Built-in Test Methods** (Right-click component in Inspector):
- **Test Damage (20)**: Removes 20 health
- **Test Heal (25)**: Restores 25 health  
- **Test Kill**: Instantly kills player

## 🎨 VISUAL CONFIGURATION:

### Health Bar Colors:
```csharp
Healthy Color: Green (100% - 60% health)
Damaged Color: Yellow (60% - 25% health)  
Critical Color: Red (25% - 0% health)
```

### Thresholds (Adjustable):
- **Critical Threshold**: 0.25 (25% health)
- **Damaged Threshold**: 0.6 (60% health)

## 🔗 INTEGRATION BENEFITS:

### Centralized Health Management:
- ✅ **Single source of truth** in FlightData
- ✅ **Consistent health values** across all systems
- ✅ **Easy to extend** for new features

### Performance Optimized:
- ✅ **Cached references** (no FindObjectOfType in Update)
- ✅ **Interval-based updates** (not every frame)
- ✅ **Smart update detection** (only when health changes)

### Developer Friendly:
- ✅ **Built-in test methods** for easy debugging
- ✅ **Clear error messages** for troubleshooting
- ✅ **Configurable thresholds** and colors

## 🚀 USAGE EXAMPLES:

### From Other Scripts:
```csharp
// Get FlightData reference
FlightData flightData = FindObjectOfType<FlightData>();

// Damage player
flightData.TakeDamage(25f);

// Heal player  
flightData.RecoverHealth(20f);

// Check if alive
if (flightData.IsAlive()) {
    // Player is still alive
}

// Get health percentage (0.0 to 1.0)
float healthPercent = flightData.GetHealthPercentage();
```

### From PlayerShipHealth:
```csharp
// Get PlayerShipHealth reference
PlayerShipHealth health = GetComponent<PlayerShipHealth>();

// Use existing interface (now connects to FlightData)
health.TakeDamage(30f);
health.RecoverHealth(15f);

// Check status
bool alive = health.IsAlive();
float currentHP = health.GetCurrentHealth();
```

## 🔧 TROUBLESHOOTING:

### Health Bar Not Updating:
1. Check that HealthBarUI has FlightData reference
2. Verify slider is assigned to Health Slider field
3. Try calling ForceUpdate() method

### Health Not Changing:
1. Verify FlightData is on riverraid_hero
2. Check console for initialization errors
3. Use test methods to verify functionality

### Colors Not Changing:
1. Ensure Fill Image is assigned (auto-detected from slider)
2. Check color thresholds are set correctly
3. Verify slider has a fill area with Image component

## ✨ FINAL RESULT:
- **Centralized health system** integrated with FlightData
- **Visual health bar** with color-coded status
- **Performance optimized** UI updates
- **Easy to use** from any script
- **Professional debugging tools** built-in
