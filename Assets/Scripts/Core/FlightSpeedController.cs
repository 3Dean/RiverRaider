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
        // Cache component references once (eliminates FindObjectOfType calls)
        flightData = FindObjectOfType<FlightData>();
        
        if (flightData != null)
        {
            aircraftTransform = flightData.transform;
            flightData.airspeed = startingSpeed;
            initialized = true;
            
            if (enableDebugLogging)
                Debug.Log($"FlightSpeedController: Initialized with starting speed {startingSpeed} MPH");
        }
        else
        {
            Debug.LogError("FlightSpeedController: No FlightData found in scene!", this);
        }
    }
    
    void Update()
    {
        if (!initialized) return;
        
        float deltaTime = Time.deltaTime;
        
        // Handle throttle input
        HandleThrottleInput(deltaTime);
        
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
    
    private void HandleThrottleInput(float deltaTime)
    {
        bool throttleChanged = false;
        
        if (Input.GetKey(KeyCode.W))
        {
            flightData.airspeed += throttleRate * deltaTime;
            throttleChanged = true;
            
            if (enableDebugLogging)
                Debug.Log($"Throttle Up - Speed: {flightData.airspeed:F1} MPH");
        }
        else if (Input.GetKey(KeyCode.S))
        {
            flightData.airspeed -= throttleRate * deltaTime;
            throttleChanged = true;
            
            if (enableDebugLogging)
                Debug.Log($"Throttle Down - Speed: {flightData.airspeed:F1} MPH");
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
