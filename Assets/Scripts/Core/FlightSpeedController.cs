using UnityEngine;

/// <summary>
/// Optimized flight speed controller that replaces multiple redundant speed scripts.
/// Handles throttle input and slope effects efficiently.
/// </summary>
public class FlightSpeedController : MonoBehaviour
{
    [Header("Speed Control Settings")]
    [SerializeField] private float throttleRate = 30f; // MPH per second
    [SerializeField] private float slopeMultiplier = 50f; // Slope effect strength
    [SerializeField] private float startingSpeed = 20f; // Starting speed
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = false;
    [SerializeField] private float debugLogInterval = 3f; // Seconds between debug logs
    
    // Cached references (performance optimization)
    private FlightData flightData;
    private Transform aircraftTransform;
    
    // State tracking
    private bool initialized = false;
    private float lastDebugLogTime = 0f;
    
    void Start()
    {
        // DISABLED - UnifiedFlightController is now the primary controller
        Debug.LogWarning("FlightSpeedController: DISABLED - UnifiedFlightController is handling flight control");
        this.enabled = false;
        return;
        
        // Find or create FlightData
        flightData = GetComponent<FlightData>();
        if (flightData == null)
        {
            flightData = FindObjectOfType<FlightData>();
        }
        
        if (flightData != null)
        {
            flightData.airspeed = startingSpeed;
            initialized = true;
            
            if (enableDebugLogging)
                Debug.Log($"FlightSpeedController: Initialized on {gameObject.name} with speed {startingSpeed}");
        }
        else
        {
            Debug.LogError("FlightSpeedController: No FlightData found!", this);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input events
        FlightInputController.OnThrottleChanged -= HandleThrottleInput;
    }
    
    void Update()
    {
        if (!initialized) return;
        
        float deltaTime = Time.deltaTime;
        
        // Handle slope effects
        HandleSlopeEffects(deltaTime);
        
        // Clamp speed using FlightData settings
        flightData.airspeed = Mathf.Clamp(flightData.airspeed, flightData.minSpeed, flightData.maxSpeed);
        
        // Optional debug logging (performance-conscious)
        if (enableDebugLogging && Time.time - lastDebugLogTime >= debugLogInterval)
        {
            LogDebugInfo();
            lastDebugLogTime = Time.time;
        }
    }
    
    private void HandleThrottleInput(float throttleInput)
    {
        if (Mathf.Abs(throttleInput) < 0.01f) return; // Ignore very small inputs
        
        float deltaTime = Time.deltaTime;
        float speedChange = throttleRate * throttleInput * deltaTime;
        flightData.airspeed += speedChange;
        
        if (enableDebugLogging)
        {
            string direction = throttleInput > 0 ? "Up" : "Down";
            Debug.Log($"Throttle {direction} - Speed: {flightData.airspeed:F1} MPH (Change: {speedChange:F1})");
        }
    }
    
    private void HandleSlopeEffects(float deltaTime)
    {
        if (aircraftTransform == null) return;
        
        float slope = Vector3.Dot(aircraftTransform.forward, Vector3.up);
        
        // Only apply slope effects if slope is significant (performance optimization)
        if (Mathf.Abs(slope) > 0.05f)
        {
            float slopeEffect = slopeMultiplier * slope * deltaTime;
            flightData.airspeed -= slopeEffect; // Climbing slows down, diving speeds up
            
            if (enableDebugLogging && Mathf.Abs(slopeEffect) > 1f)
            {
                string direction = slope > 0 ? "CLIMBING" : "DIVING";
                string effect = slopeEffect > 0 ? "SLOWING DOWN" : "SPEEDING UP";
                Debug.Log($"{direction} - {effect} - Speed change: {-slopeEffect:F1}, New speed: {flightData.airspeed:F1}");
            }
        }
    }
    
    private void LogDebugInfo()
    {
        Debug.Log($"FlightSpeedController: Speed={flightData.airspeed:F1} MPH, Throttle Rate={throttleRate}, Slope Multiplier={slopeMultiplier}");
    }
    
    // Public methods for external control (if needed)
    public void SetThrottleRate(float newRate)
    {
        throttleRate = newRate;
    }
    
    public void SetSlopeMultiplier(float newMultiplier)
    {
        slopeMultiplier = newMultiplier;
    }
    
    public void EnableDebugLogging(bool enable)
    {
        enableDebugLogging = enable;
    }
}
