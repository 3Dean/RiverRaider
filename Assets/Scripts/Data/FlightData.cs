using UnityEngine;

public class FlightData : MonoBehaviour
{
    [Header("Speed Settings")]
    public float minSpeed = 10f;
    public float maxSpeed = 200f;
    public float throttleAcceleration = 100f; // INCREASED for testing - was 50f
    public float boostMultiplier = 2f;

    [Header("Runtime")]
    public float airspeed;
    [HideInInspector] public bool isBoosting;
    
    [Header("Health System")]
    public float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    public float healAmount = 25f;
    
    [Header("Fuel System")]
    public float maxFuel = 100f;
    [HideInInspector] public float currentFuel;
    public float baseFuelConsumption = 1f; // Fuel per second at minimum speed
    public float speedFuelMultiplier = 0.02f; // Additional fuel consumption per MPH

    [Header("Drag & Throttle Inertia")]
    [Tooltip("Linear drag coefficient (~0.01 – 0.1)")]
    public float dragCoefficient = 0.02f;
    [Tooltip("How long (sec) it takes to go from 0→full throttle")]
    public float throttleSmoothTime = 0.3f;

    [Header("Slope Speed Effect")]
    [Tooltip("How strongly climbing slows you, and diving speeds you up (try 30-50 for noticeable effect)")]
    public float slopeEffect = 20f; // Reduced for better control balance - was 100f
    [Tooltip("Maximum slope angle (degrees) that affects speed")]
    public float maxSlopeAngle = 45f;

    [Header("Rotation Spring Settings")]
    [Tooltip("Hz of the spring; higher = snappier spring")]
    public float rotationSpringFrequency = 5f;
    [Range(0f,1f)]
    [Tooltip("Damping ratio; <1 = overshoot, 1 = critically damped")]
    public float rotationDampingRatio = 0.7f;

    [Header("Mouse Look & Bank")]
    public float yawSpeed = 15f;      // deg/sec per unit of Mouse X (reduced from 60)
    public float pitchSpeed = 12f;    // deg/sec per unit of Mouse Y (reduced from 45)
    public float maxBankAngle = 45f;  // maximum roll tilt at full turn - increased for dramatic banking
    public float bankLerpSpeed = 4f;  // Increased for even more responsive banking

    [Header("Rotational Inertia")]
    [Tooltip("Higher = smoother (more sluggish)")]
    public float yawSmoothTime = 0.3f;  // Consistent with pitch for uniform feel
    public float pitchSmoothTime = 0.3f; // Matched to yaw for consistent response
    
    [Header("Speed-Dependent Responsiveness")]
    [Tooltip("Base responsiveness multiplier at minimum speed")]
    public float baseResponsiveness = 1.2f; // Increased for more responsive low-speed control
    [Tooltip("Responsiveness multiplier at maximum speed (lower = less responsive)")]
    public float highSpeedResponsiveness = 0.8f; // Less penalty at high speeds
    [Tooltip("How much speed affects control sensitivity")]
    public float speedResponsivenessEffect = 0.6f; // Gentler speed effect curve

    [Header("Fuel Depletion Physics")]
    [Tooltip("Gravity force applied when out of fuel (positive = downward)")]
    public float gravityForce = 9.81f;
    [Tooltip("Time in seconds for engine power to fade when fuel depleted")]
    public float enginePowerFadeTime = 3f;
    [Tooltip("Minimum speed before stall occurs (loss of control)")]
    public float stallSpeed = 15f;
    [Tooltip("Drag coefficient when gliding without power")]
    public float glideDragCoefficient = 0.05f;

    [Header("Current Attitude (auto-updated)")]
    [HideInInspector] public float roll;
    [HideInInspector] public float pitch;
    [HideInInspector] public float yaw;
    
    [Header("Engine State (runtime)")]
    [HideInInspector] public float enginePowerMultiplier = 1f;
    [HideInInspector] public bool isEngineRunning = true;

    void Start()
    {
        // Initialize health and fuel systems
        currentHealth = maxHealth;
        currentFuel = maxFuel; // Start with full fuel
        
        // CRITICAL FIX: Initialize airspeed to a reasonable starting value
        if (airspeed == 0f)
        {
            airspeed = (minSpeed + maxSpeed) * 0.25f; // Start at 25% of speed range
            Debug.Log($"FlightData: Initialized airspeed to {airspeed:F1} MPH");
        }
        
        // Ensure engine is running with full fuel
        enginePowerMultiplier = 1f;
        isEngineRunning = true;
        
        Debug.Log($"FlightData initialized: Health={currentHealth}, Fuel={currentFuel}, Engine=ON, Airspeed={airspeed:F1}");
    }
    
    void Update()
    {
        // Attitude values are now updated directly by UnifiedFlightController
        // This prevents conflicts and ensures UI gets accurate rotation data
    }

    float NormalizeAngle(float a)
    {
        return (a > 180f) ? a - 360f : a;
    }
    
    // Health system methods
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        Debug.Log($"Player took {amount} damage. Health: {currentHealth:F1}/{maxHealth:F1}");
    }
    
    public void RecoverHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        Debug.Log($"Player recovered {amount} health. Health: {currentHealth:F1}/{maxHealth:F1}");
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    // Fuel system methods
    public void ConsumeFuel(float deltaTime)
    {
        if (currentFuel <= 0f) return;
        
        // Calculate speed-based fuel consumption (W key acceleration increases speed, which increases fuel consumption)
        float speedBasedConsumption = baseFuelConsumption + (airspeed * speedFuelMultiplier);
        
        // Consume fuel
        currentFuel = Mathf.Clamp(currentFuel - (speedBasedConsumption * deltaTime), 0f, maxFuel);
        
        // Debug logging for fuel consumption (can be disabled)
        if (Time.frameCount % 180 == 0) // Log every ~3 seconds at 60fps
        {
            Debug.Log($"Fuel consumption: {speedBasedConsumption:F1}/sec at {airspeed:F0} MPH");
        }
    }
    
    public void RefuelToFull()
    {
        currentFuel = maxFuel;
        Debug.Log("Fuel tank refilled to maximum!");
    }
    
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
        Debug.Log($"Added {amount} fuel. Current: {currentFuel:F1}/{maxFuel:F1}");
    }
    
    public bool HasFuel()
    {
        return currentFuel > 0f;
    }
    
    public float GetFuelPercentage()
    {
        return currentFuel / maxFuel;
    }
    
    public float GetCurrentFuelConsumptionRate()
    {
        return baseFuelConsumption + (airspeed * speedFuelMultiplier);
    }
    
    // Speed-dependent responsiveness methods
    public float GetCurrentResponsiveness()
    {
        // Calculate speed ratio (0 = min speed, 1 = max speed)
        float speedRatio = Mathf.InverseLerp(minSpeed, maxSpeed, airspeed);
        
        // Interpolate between base and high-speed responsiveness
        float responsiveness = Mathf.Lerp(baseResponsiveness, highSpeedResponsiveness, speedRatio * speedResponsivenessEffect);
        
        return Mathf.Clamp(responsiveness, 0.1f, 2f); // Prevent extreme values
    }
    
    public float GetSpeedAdjustedYawSpeed()
    {
        return yawSpeed * GetCurrentResponsiveness();
    }
    
    public float GetSpeedAdjustedPitchSpeed()
    {
        return pitchSpeed * GetCurrentResponsiveness();
    }
    
    public float GetSpeedAdjustedSmoothTime(float baseSmoothTime)
    {
        // Higher speed = longer smooth time = less responsive
        float responsiveness = GetCurrentResponsiveness();
        return baseSmoothTime / responsiveness; // Lower responsiveness = higher smooth time
    }
    
    // Engine power management methods
    public void SetEnginePower(float powerMultiplier)
    {
        enginePowerMultiplier = Mathf.Clamp01(powerMultiplier);
        isEngineRunning = enginePowerMultiplier > 0.01f;
        
        if (!isEngineRunning && Time.frameCount % 120 == 0) // Log every ~2 seconds
        {
            Debug.Log("ENGINE FAILURE - No fuel remaining!");
        }
    }
    
    public float GetEffectiveThrottlePower(float rawThrottle)
    {
        return rawThrottle * enginePowerMultiplier;
    }
    
    public bool IsStalling()
    {
        return airspeed < stallSpeed;
    }
    
    public bool IsGliding()
    {
        return !isEngineRunning && !IsStalling();
    }
    
    // Restart engine when fuel is restored
    public void RestartEngine()
    {
        if (HasFuel())
        {
            enginePowerMultiplier = 1f;
            isEngineRunning = true;
            Debug.Log("ENGINE RESTARTED - Fuel restored!");
        }
    }
}
